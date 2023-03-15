using SocketTCP;

//to do: change console width and height
//Console.WindowWidth = 100;

var client = new ClientSocket();

client.Connect("127.0.0.1", 5050);

client.ReceiveNickname();

client.ReceiveMessagesLoop();

client.SendMessageLoop();

