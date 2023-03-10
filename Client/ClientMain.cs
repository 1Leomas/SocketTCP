using SocketPR;

var client = new ClientSocket();

client.Connect("127.0.0.1", 5050);

if (!client.ReceiveNickname())
    return;

client.SendMessageLoop();
