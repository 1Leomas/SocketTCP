using Client;
using System.Net.Sockets;

namespace SocketTCP;

public class Client
{
    public string NickName { get; set; } = String.Empty;

    public ConsoleColor ConsoleColor { get; set; }

    public Socket Socket { get; set; }

    public Client()
    {
        NickName = GenerateNickname();
        ConsoleColor = GenerateColor();
    }

    public string GenerateNickname()
    {
        var nameGenerator = new NameGenerator();
        var nickname = nameGenerator.Generate(new Random().Next(4, 8));

        return nickname;
    }

    public ConsoleColor GenerateColor()
    {
        var random = new Random().Next(1, 14);
        var color = (ConsoleColor)Enum.ToObject(typeof(ConsoleColor), random);

        return color;
    }
}