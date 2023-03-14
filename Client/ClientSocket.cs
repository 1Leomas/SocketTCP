using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SocketTCP;

public class ClientSocket
{
    private readonly Socket _clientSocket;
    private ConsoleColor _consoleColor = ConsoleColor.White;

    public string Nickname { get; set; } = string.Empty;

    private string _input { get; set; } = "";

    public ClientSocket()
    {
        _clientSocket = new Socket(
            AddressFamily.InterNetwork, 
            SocketType.Stream, 
            ProtocolType.Tcp);
    }

    public void Connect(string remoteIp, int remotePort)
    {
        var ipAddress = IPAddress.Parse(remoteIp);
        var remoteEndPoint = new IPEndPoint(ipAddress, remotePort);

        try
        {
            _clientSocket.Connect(remoteEndPoint);

            Console.WriteLine("Client connected to {0}", remoteEndPoint);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while connecting to server.");
            Console.WriteLine("Error message: {0}", e.Message);
        }
    }

    public void SendMessageLoop()
    {
        Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    PrintNickname(Nickname, _consoleColor);
                    Console.Write(": ");

                    while (true)
                    {

                        var inputKey = Console.ReadKey();

                        if (inputKey.Key == ConsoleKey.Enter) break;

                        _input += inputKey.KeyChar;
                    }

                    var bytesData = Encoding.UTF8.GetBytes(_input);

                    _clientSocket.Send(bytesData);

                    PrintNickname(Nickname, _consoleColor);
                    Console.WriteLine(": {0}", _input);

                    _input = "";
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            _clientSocket.Close();
        });
    }

    public void ReceiveMessagesLoop()
    {
        Task.Run(() =>
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int byteCount = 0;

                try
                {
                    byteCount = _clientSocket.Receive(buffer);
                }
                catch (Exception)
                {
                    Console.WriteLine();
                    //Console.WriteLine("Server disconnected");
                    System.Environment.Exit(0);

                    break;
                }

                string dataFromServer = Encoding.UTF8.GetString(buffer, 0, byteCount);

                if (dataFromServer.First() != 'm')
                    continue;

                ProcessAndPrintMessage(dataFromServer);
            }
        });
    }

    private void ProcessAndPrintMessage(string dataFromServer)
    {
        var pattern = @"\[([\w\s]+)\]";
        var replace = @"$1";

        var dataList = Regex
            .Matches(dataFromServer, pattern)
            .Select(m => m
                .Result(replace)
                .ToString())
            .ToList();

        var nickName = dataList.First();

        var consoleColor = Enum.Parse<ConsoleColor>(dataList[1]);

        var message = dataList.Last();

        var cp = Console.GetCursorPosition();
        Console.SetCursorPosition(0, cp.Top);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, cp.Top);

        Console.ForegroundColor = consoleColor;
        Console.Write("{0}", nickName);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(": {0}", message);

        PrintNickname(Nickname, _consoleColor);
        Console.Write(": {0}", _input);
    }

    public void ReceiveNickname()
    {
        try
        {
            byte[] bufferReceive = new byte[1024];
            _clientSocket.Receive(bufferReceive, SocketFlags.None);

            var dataFromServer = Encoding.UTF8.GetString(bufferReceive);

            if (dataFromServer.First() != 'n')
                throw new Exception("Received wrong data");

            var pattern = @"\[(\w+)\]";
            var replace = @"$1";

            var dataList = Regex
                .Matches(dataFromServer, pattern)
                .Select(m => m
                    .Result(replace)
                    .ToString())
                .ToList();

            Nickname = dataList.First();

            _consoleColor = Enum.Parse<ConsoleColor>(dataList.Last());

            Console.Write("Welcome to server, your nickname is ");
            PrintNickname(Nickname, _consoleColor);
            Console.WriteLine();
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private void PrintNickname(string nickname, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(nickname);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

