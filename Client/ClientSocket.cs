using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SocketPR;

public class ClientSocket
{
    private readonly Socket _clientSocket;
    private ConsoleColor _myColor = ConsoleColor.White;

    public string Nickname { get; set; } = string.Empty;

    public ClientSocket()
    {
        _clientSocket = new Socket(AddressFamily.InterNetwork, 
            SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect(string remoteIP, int remotePort)
    {
        var ipAddress = IPAddress.Parse(remoteIP);
        var remotEndPoint = new IPEndPoint(ipAddress, remotePort);

        try
        {
            _clientSocket.Connect(remotEndPoint);

            Console.WriteLine("Client connected to {0}", remotEndPoint);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while connecting to server. {0}", e.Message);
        }
    }

    public void SendMessageLoop()
    {
        while (true)
        {
            try
            {
                printNickname(Nickname, _myColor);
                Console.Write(": ");

                var text = Console.ReadLine() ?? "";

                if (text == "x") break;

                var bytesData = Encoding.UTF8.GetBytes(text);

                _clientSocket.Send(bytesData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                break;
            }
        }

        _clientSocket.Close();
    }

    public bool ReceiveNickname()
    {
        try
        {
            byte[] bufferReceive = new byte[1024];
            _clientSocket.Receive(bufferReceive);

            var dataFromServer = Encoding.UTF8.GetString(bufferReceive);

            var nickNameFromServer = Regex.Matches(dataFromServer, @"\w+").First().ToString();
            Nickname = nickNameFromServer;

            var color = Regex.Matches(dataFromServer, @"\w+").Last().ToString();
            _myColor = Enum.Parse<ConsoleColor>(color);

            Console.Write("Welcome to server, your nickname is ");
            printNickname(Nickname, _myColor);
            Console.WriteLine();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    private void printNickname(string nickname, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(nickname);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

