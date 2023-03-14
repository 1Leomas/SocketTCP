using SocketTCP;

var server = new ServerSocket("127.0.0.1", 5050);

try
{
    server.BindAndListen(15);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    return;
}


await server.AcceptAndHandleClients();
