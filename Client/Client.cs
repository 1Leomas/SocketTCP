using System.Net.Sockets;

namespace SocketTCP;

public class Client
{
    public string NickName { get; set; } = String.Empty;

    public ConsoleColor ConsoleColor { get; set; }

    public Socket Socket { get; set; }
}