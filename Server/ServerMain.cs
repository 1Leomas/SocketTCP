using System.Net.Sockets;
using SocketPR;

var server = new ServerSocket("127.0.0.1", 5050);

if (!server.BindAndListen(15))
    return;

Socket client;

while (true)
{
    client = server.AcceptClient();

    server.GenerateAndSendNickname(client);

    server.PrintNickname(server.ClientNickname, server.NicknameColor);
    Console.WriteLine(" connected");

    server.ReceiveMessageLoop(client);
}