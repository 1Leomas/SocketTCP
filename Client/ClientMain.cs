using SocketTCP;

//to do: change console width and height
//Console.WindowWidth = 100;

var client = new ClientSocket();

try
{
    client.Connect("127.0.0.1", 5050);
}
catch (Exception e)
{
    Console.WriteLine("Cannot connect to server.");
    //Console.WriteLine(e);
    return;
}

client.ReceiveNickname();

client.ReceiveMessagesLoop();

client.SendMessageLoop();

