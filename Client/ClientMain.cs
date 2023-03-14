using SocketTCP;

//to do: change console width and height
//Console.WindowWidth = 100;

var client = new ClientSocket();

await client.Connect("127.0.0.1", 5050);

var ifNotReceiveNickname = await client.ReceiveNickname();

if (!ifNotReceiveNickname)
    return;

await client.SendMessageLoop();

await client.ReceiveMessagesLoop();
