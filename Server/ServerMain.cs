using System.Net.Sockets;
using SocketPR;

var server = new ServerSocket("127.0.0.1", 5050);

if (!server.BindAndListen(15))
    return;

//server.AcceptAndReceive();

Socket client;

while (true)
{
    client = server.AcceptClient();

    server.SendNickname(client);

    server.ReceiveMessageLoop(client);
}


