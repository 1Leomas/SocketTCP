using SocketTCP;

var server = new ServerSocket("127.0.0.1", 5050);

if (!server.BindAndListen(15))
    return;


while (true)
{
    SocketTCP.Client client = new SocketTCP.Client();

    client.Socket = await server.AcceptClient();

    ThreadPool.QueueUserWorkItem(_ => server.Handle(client));
}