using SocketTCP;

var server = new ServerSocket("127.0.0.1", 5050);

if (!server.BindAndListen(15))
    return;

await server.AcceptAndHandleClients();
