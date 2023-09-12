'Remote uci - Daniel José Queraltó
'11/10/2020
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Module Module1

    Dim port As Integer = 10000
    Dim engine As String = ""
    Dim remoteip As String = ""
    Dim startandneverstopengine As Boolean = False
    Dim retainanalysisondisconnect As Boolean = False
    Dim inifile As String = "remoteuci.ini"

    Dim enginestdin As StreamWriter
    Dim engineprocess As Process
    Dim clientipaddress As String

    Dim clientstream As NetworkStream  'the opened connection from the client
    Dim serverstream As NetworkStream  'the opened connection to the server

    Dim lintoreplace As New List(Of String)
    Dim linreplace As New List(Of String)
    Dim linstart As New List(Of String)
    Dim consoleoutput As Boolean = True

    Sub Main()
        For Each arg As String In Environment.GetCommandLineArgs()
            applyargument(arg)
        Next

        'if configuration file exists
        If My.Computer.FileSystem.FileExists(inifile) Then
            Dim fileReader As String
            fileReader = My.Computer.FileSystem.ReadAllText(inifile)
            Dim lines As String() = fileReader.Split(New String() {vbCrLf, vbCr, vbLf},
                                   StringSplitOptions.None)
            Dim linenum As Integer = 0
            Do
                If linenum > lines.GetUpperBound(0) Then
                    Exit Do
                End If
                If lines(linenum).ToUpper = "REPLACE" Then
                    'replace first line with the second when sending it to the engine
                    linenum += 1
                    lintoreplace.Add(lines(linenum))
                    linenum += 1
                    linreplace.Add(lines(linenum))
                Else
                    If lines(linenum).ToUpper = "START" Then
                        linenum += 1
                        linstart.Add(lines(linenum))
                    Else
                        applyargument(lines(linenum))
                    End If
                End If
                linenum += 1
            Loop
        End If

        start()
    End Sub

    Sub applyargument(arg As String)
        arg = arg.ToUpper
        'tcp/ip port
        If arg.StartsWith("-P") Then
            port = arg.Substring(2)
        End If
        'engine
        If arg.StartsWith("-E") Then
            engine = arg.Substring(2)
        End If
        'remote ip
        If arg.StartsWith("-I") Then
            remoteip = arg.Substring(2)
        End If
        'quiet, no console output
        If arg.StartsWith("-Q") Then
            consoleoutput = False
        End If
        'start and never stop engine
        If arg.StartsWith("-N") Then
            startandneverstopengine = True
        End If
        'if the client disconnects, maintain the current analysis on. With a new connection the client will be able to take it back
        If arg.StartsWith("-R") Then
            retainanalysisondisconnect = True
        End If
    End Sub

    Sub start()
        If engine <> "" Then
            'server mode
            'listener for the clients
            CreateServerListener()
            'Dim serverThread As Threading.Thread
            'serverThread = New Thread(AddressOf CreateServerListener)
            'serverThread.IsBackground = True
            'serverThread.Start()
        Else
            'client mode
            'connect to the server. This will start the remote engine
            Dim serverconnection As New TcpClient
            Dim result = serverconnection.BeginConnect(remoteip, port, Nothing, Nothing)
            Dim success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1))
            If Not success Then
                End
            End If
            serverstream = serverconnection.GetStream()

            Dim clientThread As Threading.Thread
            'send stdin to server
            clientThread = New Thread(AddressOf SendToServer)
            clientThread.IsBackground = True
            clientThread.Start()

            'receive stdout from the remote engine and send it to stdout
            ReceiveFromServer()
        End If
    End Sub

    'receiving standard output from the engine, send to the client
    Private Sub OutputHandler(sendingProcess As Object,
        outLine As DataReceivedEventArgs)

        If IsNothing(clientstream) Then
            'can happen when -n option is on (startandneverstopengine)
            Return
        End If

        ' Collect the sort command output.
        If Not String.IsNullOrEmpty(outLine.Data) Then
            Dim WriteBuffer As Byte() = Encoding.ASCII.GetBytes(outLine.Data + vbCrLf)
            Try
                clientstream.Write(WriteBuffer, 0, WriteBuffer.Length)
                If consoleoutput Then
                    Console.WriteLine(outLine.Data + vbCrLf)  'info for the console viewer
                End If
            Catch ex As Exception
                'on connection abruptly closed can except
            End Try
        End If
    End Sub

    'server listener for the clients
    Public Sub CreateServerListener()
        Dim s As String
        Dim tcpListener As TcpListener = New TcpListener(System.Net.IPAddress.Parse("0.0.0.0"), port)  'Listen from all the ip addresses
        tcpListener.Start()

        If startandneverstopengine Then
            startengine()
        End If

        While True
            'Thread.Sleep(10)
            Dim tcpClient As TcpClient = tcpListener.AcceptTcpClient()
            clientstream = tcpClient.GetStream()
            Dim sr As New StreamReader(clientstream, Encoding.UTF8)

            'client connection received

            If Not startandneverstopengine Then
                startengine()
            End If

            Do
                Try
                    If Not tcpClient.Client.Connected Then
                        If Not retainanalysisondisconnect Then
                            enginestdin.WriteLine("stop")
                            If consoleoutput Then
                                Console.WriteLine("stop") 'info for the console viewer
                            End If
                        End If
                        Exit Do
                    End If
                    s = sr.ReadLine
                    For i = 0 To lintoreplace.Count - 1
                        If s = lintoreplace(i) Then
                            s = linreplace(i)
                            Exit For
                        End If
                        If lintoreplace(i).EndsWith("*") AndAlso s.StartsWith(lintoreplace(i).Substring(0, lintoreplace(i).Length - 1)) Then
                            s = linreplace(i)
                            Exit For
                        End If
                    Next
                    If s.Trim <> "" Then
                        enginestdin.WriteLine(s)
                        If consoleoutput Then
                            Console.WriteLine(s) 'info for the console viewer
                        End If
                    End If
                Catch ex As Exception
                    If Not startandneverstopengine Then
                        Try
                            engineprocess.Kill()
                        Catch ex2 As Exception

                        End Try
                        Exit Do
                    End If
                End Try
            Loop
        End While
    End Sub

    Sub startengine()
        'start the engine
        engineprocess = New Process()
        engineprocess.StartInfo.FileName = engine
        engineprocess.StartInfo.Arguments = ""
        engineprocess.StartInfo.UseShellExecute = False
        engineprocess.StartInfo.CreateNoWindow = True
        engineprocess.StartInfo.RedirectStandardInput = True
        engineprocess.StartInfo.RedirectStandardOutput = True
        'process.StartInfo.WorkingDirectory =

        AddHandler engineprocess.OutputDataReceived,
               AddressOf OutputHandler

        AddHandler engineprocess.Exited, AddressOf Process_Exited

        engineprocess.Start()
        ' Use a stream writer to synchronously write the input.
        enginestdin = engineprocess.StandardInput
        ' Start the asynchronous read of the output stream.
        engineprocess.BeginOutputReadLine()

        'send instructions on engine startup
        For Each s In linstart
            enginestdin.WriteLine(s)
            If consoleoutput Then
                Console.WriteLine(s) 'info for the console viewer
            End If
        Next
    End Sub

    Private Sub Process_Exited(ByVal sender As Object, ByVal e As System.EventArgs)
        If startandneverstopengine Then
            startengine()
        End If
    End Sub

    'Send stdin to the server
    Public Sub SendToServer()

        Dim inputStream As Stream = Console.OpenStandardInput()
        Dim bytes As Byte() = New Byte(100000) {}
        Dim s As String

        Do
            Dim outputLength As Integer = inputStream.Read(bytes, 0, 100000)
            serverstream.Write(bytes, 0, outputLength)
            s = Encoding.ASCII.GetString(bytes, 0, outputLength)
            If s.Trim.ToLower.StartsWith("quit") Then
                End
            End If
        Loop
    End Sub

    'receive stdrout from server and send it to the gui
    Sub ReceiveFromServer()
        Do
            If serverstream.DataAvailable Then
                Dim myReadBuffer As Byte() = New Byte(1023) {}
                Dim Complete As StringBuilder = New StringBuilder()
                Dim numberOfBytesRead As Integer = 0

                Do
                    numberOfBytesRead = serverstream.Read(myReadBuffer, 0, myReadBuffer.Length)
                    Complete.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead))
                Loop While serverstream.DataAvailable

                Console.Write(Complete)  'send to gui
            End If
            Thread.Sleep(10)
        Loop
    End Sub
End Module
