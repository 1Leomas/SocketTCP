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

    public ClientSocket()
    {
        _clientSocket = new Socket(
            AddressFamily.InterNetwork, 
            SocketType.Stream, 
            ProtocolType.Tcp);
    }

    public async Task Connect(string remoteIP, int remotePort)
    {
        var ipAddress = IPAddress.Parse(remoteIP);
        var remotEndPoint = new IPEndPoint(ipAddress, remotePort);

        try
        {
            await _clientSocket.ConnectAsync(remotEndPoint);

            Console.WriteLine("Client connected to {0}", remotEndPoint);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while connecting to server. {0}", e.Message);
        }
    }

    public async Task SendMessageLoop()
    {
        var thread =  new Thread(async () =>
        {
            while (true)
            {
                try
                {
                    printNickname(Nickname, _consoleColor);
                    Console.Write(": ");

                    var text = Console.ReadLine() ?? "";

                    if (text == "x") break;

                    var bytesData = Encoding.UTF8.GetBytes(text);

                    await _clientSocket.SendAsync(bytesData, SocketFlags.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            _clientSocket.Close();
        });

        thread.Start();
    }

    public async Task ReceiveMessagesLoop()
    {
        var thread = new Thread(async () =>
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int byteCount = await _clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                string dataFromServer = Encoding.UTF8.GetString(buffer, 0, byteCount);

                if (dataFromServer.First() != 'm')
                    continue;

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

                printNickname(Nickname, _consoleColor);
                Console.Write(": ");
            }
        });

        thread.Start();

    }

    public async Task<bool> ReceiveNickname()
    {
        try
        {
            byte[] bufferReceive = new byte[1024];
            await _clientSocket.ReceiveAsync(bufferReceive, SocketFlags.None);

            var dataFromServer = Encoding.UTF8.GetString(bufferReceive);

            if (dataFromServer.First() != 'n')
                return false;

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
            printNickname(Nickname, _consoleColor);
            Console.WriteLine();
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.Message);
            return false;
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

