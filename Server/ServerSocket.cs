using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SocketPR;

public class ServerSocket
{
    private readonly Socket _serverSocket;
    private readonly IPEndPoint _serverEndPoint;
    private Dictionary<string, string> _clients;
    public ConsoleColor NicknameColor;

    public string ClientNickname{ get; set; }

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

    public void AcceptAndReceive()
    {
        Socket? client;

        client = AcceptClient();

        if (client == null)
            return;

        ReceiveMessageLoop(client);
    }

    public Socket AcceptClient()
    {
        Socket? client = null;
        try
        {
            client = _serverSocket.Accept();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error accepting client: {0}", e.Message);
        }

        return client;
    }

    public void ReceiveMessageLoop(Socket client)
    {

        while (true)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int byteCount = client.Receive(buffer);

                if (byteCount == 0)
                {
                    Console.WriteLine("Client disconnected");
                    return;
                }

                string text = Encoding.UTF8.GetString(buffer, 0, byteCount);

                Console.ForegroundColor = NicknameColor;
                Console.Write("{0}", ClientNickname, text);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(": {0}", text);
            }
            catch (SocketException e)
            {
                // to do - delete client from client list
                //Console.WriteLine("Socket error: {0}", e.Message);
                PrintNickname(ClientNickname, NicknameColor);
                Console.WriteLine(" disconnected");
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving data from client {0}", client.RemoteEndPoint);
                Console.WriteLine(e.Message);
            }
        }
    }

    public void GenerateAndSendNickname(Socket client)
    {
        try
        {
            ClientNickname = GenerateNickname();
            NicknameColor = GenerateColor();

            string dataToSend = $"[{ClientNickname}][{NicknameColor}]";

            var bytesData = Encoding.UTF8.GetBytes(dataToSend);
            client.Send(bytesData);
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

