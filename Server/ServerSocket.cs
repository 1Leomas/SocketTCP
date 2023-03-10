using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Client = SocketTCP.Client;

namespace SocketTCP;

public class ServerSocket
{
    private readonly Socket _serverSocket;
    private readonly IPEndPoint _serverEndPoint;

    public List<Client> Clients = new List<Client>();

    //public ConcurrentDictionary<string, string> ClientsList { get; set; }

    public ServerSocket(string ip, int port)
    {
        var ipAddress = IPAddress.Parse(ip);

        _serverSocket = new Socket(AddressFamily.InterNetwork, 
            SocketType.Stream, ProtocolType.Tcp);

        _serverEndPoint = new IPEndPoint(ipAddress, port);

    }

    public bool BindAndListen(int queueLimit)
    {
        try
        {
            _serverSocket.Bind(_serverEndPoint);
            _serverSocket.Listen(queueLimit);

            Console.WriteLine("Server listening on: {0}", _serverEndPoint);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket error: {0}", ex.Message);
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error binding and listening: {0}", e.Message);
            return false;
        }
        return true;
    }

    public async Task<Socket> AcceptClient()
    {
        Socket? client = null;
        try
        {
            client = await _serverSocket.AcceptAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error accepting client: {0}", e.Message);
        }

        return client;
    }

    public async void Handle(Client client)
    {
        await GenerateAndSendNickname(client);

        PrintNickname(client.NickName, client.ConsoleColor);
        Console.WriteLine(" connected");

        await ReceiveMessageLoop(client);
    }

    public async Task ReceiveMessageLoop(Client client)
    {

        while (true)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int byteCount = await client.Socket.ReceiveAsync(buffer, SocketFlags.None);

                if (byteCount == 0)
                {
                    PrintNickname(client.NickName, client.ConsoleColor);
                    Console.WriteLine(" disconnected");
                    return;
                }

                string text = Encoding.UTF8.GetString(buffer, 0, byteCount);

                Console.ForegroundColor = client.ConsoleColor;
                Console.Write("{0}", client.NickName, text);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(": {0}", text);
            }
            catch (SocketException)
            {
                Clients.Remove(client);

                PrintNickname(client.NickName, client.ConsoleColor);
                Console.WriteLine(" disconnected");

                Console.WriteLine("{0} clients connected",Clients.Count);
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving data from client {0}", client.Socket.RemoteEndPoint);
                Console.WriteLine(e.Message);
            }
        }
    }

    public async Task GenerateAndSendNickname(Client client)
    {
        try
        {
            client.NickName = GenerateNickname();
            client.ConsoleColor = GenerateColor();

            string dataToSend = 
                $"[{client.NickName}][{client.ConsoleColor}]";

            var bytesData = Encoding.UTF8.GetBytes(dataToSend);
            await client.Socket.SendAsync(bytesData, 0);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while sending nickname");
            Console.WriteLine(e.Message);
        }
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

    public void PrintNickname(string nickname, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(nickname);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

