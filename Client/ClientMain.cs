using SocketTCP;

var client = new ClientSocket();

await client.Connect("127.0.0.1", 5050);

var ifNotReceiveNickname = await client.ReceiveNickname();

if (!ifNotReceiveNickname)
    return;

await client.SendMessageLoop();

await client.ReceiveMessages();
