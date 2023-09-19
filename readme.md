# Remote UCI

Daniel José Queraltó 2020

The "Remote UCI" program allows you to remotely run a chess engine based on the UCI (Universal Chess Interface) protocol. This means you can use a chess engine that runs on a remote server as if it were installed locally on your computer.


## How to use "Remote UCI"

Initial Setup: Before using the program, make sure you have a file named remoteuci.ini in the same folder as the remoteuci.exe program. This file can contain configuration parameters that will be loaded at the start of the program.

Server Mode: If you want to run the program in server mode (to offer the chess engine), you will need to specify the UCI engine executable path with the -E argument. For example:

	RemoteUCI.exe -E[path_of_the_engine]

Client Mode: If you want to connect your chess GUI to a remote engine, you will need to specify the IP of the remote server with the -I argument. For example:

	RemoteUCI.exe -I[server_IP]


Additional Parameters:
	-P: Specifies the TCP/IP port to be used (default is 10000).
	-Q: Mutes the console output.
	-N: Starts the engine and never stops it.
	-R: If the client disconnects, the current analysis continues. With a new connection, the client can resume it.


remoteuci.ini File

This file allows for configuring additional behaviors. You can set up:

Replace Commands: If you wish certain commands sent by the client to be replaced before being sent to the engine, use the REPLACE block + original command + command to send to the engine. For example:

REPLACE
setoption name Hash value 65536
setoption name Hash value 128

Initial Commands: If you wish certain commands to be sent to the engine right after it starts, use the START block. For example:

START
uci

## Example

On the computer with ChessBase or another UCI-compatible program installed, create a folder that includes both remoteuci.exe and remoteuci.ini. The contents of remoteuci.ini can be:

	-p2051
	-i192.168.1.33

This indicates that it should establish a connection to the computer with the IP address 192.168.1.33 using port 2051.

On the other computer, which has the IP address 192.168.1.33, create a folder containing remoteuci.exe, stockfish.exe, and remoteuci.ini. The content of this remoteuci.ini might be:

	replace
	quit
	stop

This example configuration ensures that the engine won't halt; it will persistently run. The purpose is to prevent the need to restart the engine every time one analysis stops and another begins. Essentially, it substitutes the UCI command "quit" with the command "stop".

With all configurations set, initiate the engine on the remote computer using:

	remoteuci.exe -p2051 -n -q -estockfish.exe

For convenience, I have this command saved in a .bat file.

Lastly, configure a new engine in ChessBase that points to the local remoteuci.exe file. To ChessBase, it will seem as if it's interfacing with the remote Stockfish engine.

## Considerations

Make sure you have the appropriate ports open in your firewall if you are running the program in server mode.
Keep in mind that communication between the client and server is not encrypted. You might want to consider additional security measures if you are using it on public or insecure networks.


## Prerequisites

.Net framework 4

## License

This project is licensed under the MIT License - see the LICENSE file for details.
