# Remote UCI

Daniel José Queraltó 2020

The "Remote UCI" program allows you to remotely run a chess engine based on the UCI (Universal Chess Interface) protocol. This means you can use a chess engine that runs on a remote server as if it were installed locally on your computer.


## How to use "Remote UCI"

Initial Setup: Before using the program, make sure you have a file named remoteuci.ini in the same folder as the program. This file can contain configuration parameters that will be loaded at the start of the program.

Server Mode: If you want to run the program in server mode (to offer a chess engine to other users), you will need to specify the UCI engine path with the -E argument. For example:

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


## Considerations

Make sure you have the appropriate ports open in your firewall if you are running the program in server mode.
Keep in mind that communication between the client and server is not encrypted. You might want to consider additional security measures if you are using it on public or insecure networks.


## Prerequisites

.Net framework 4

## License

This project is licensed under the MIT License - see the LICENSE file for details.
