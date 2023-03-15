using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketTCP;

public class ServerSocket
{
    private readonly Socket _serverSocket;
    private readonly IPEndPoint _serverEndPoint;

    private List<Client> _clients;

    private static readonly object _locker = new object();

    public ServerSocket(string ip, int port)
    {
        _clients = new List<Client>();

        var ipAddress = IPAddress.Parse(ip);

        _serverSocket = new Socket(AddressFamily.InterNetwork, 
            SocketType.Stream, ProtocolType.Tcp);

        _serverEndPoint = new IPEndPoint(ipAddress, port);
    }

    public void BindAndListen(int queueLimit)
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
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error binding and listening: {0}", e.Message);
            throw;
        }
    }

    public void AcceptAndHandleClients()
    {
        while (true)
        {
            var clientSocket = AcceptClient();

            if (clientSocket == null) continue;

            Task.Run(() =>
            {
                HandleClient(clientSocket);
            });
        }
    }

    public Socket? AcceptClient()
    {
        Socket socket = null!;
        try
        {
            socket = _serverSocket.Accept();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error accepting client: {0}", e.Message);
        }

        return socket;
    }

    private void HandleClient(Socket clientSocket)
    {
        var client = new Client
        {
            Socket = clientSocket
        };

        _clients.Add(client);

        SendNickname(client);

        PrintColoredText(client.NickName, client.ConsoleColor);
        Console.WriteLine(" connected. Total clients: {0}", _clients.Count);
        
        ReceiveMessageLoop(client);
    }

    private void SendNickname(Client client)
    {
        try
        {
            string dataToSend =
                $"n[{client.NickName}][{client.ConsoleColor}]";

            var bytesDataToSend = Encoding.UTF8.GetBytes(dataToSend);

            client.Socket.Send(bytesDataToSend, SocketFlags.None);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Error while sending data to client");
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private void ReceiveMessageLoop(Client client)
    {
        while (true)
        {
            try
            {
                byte[] bytesReceived = new byte[1024];
                int byteCount = client.Socket.Receive(bytesReceived, SocketFlags.None);

                if (byteCount == 0)
                {
                    RemoveClient(client);
                    continue;
                }

                string messageText = Encoding.UTF8.GetString(bytesReceived, 0, byteCount);

                PrintClientMessage(client, messageText);

                SendMessageToOtherClients(client, messageText);
            }
            catch (SocketException)
            {
                RemoveClient(client);
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving data from client {0}", client.Socket.RemoteEndPoint);
                Console.WriteLine(e.Message);
            }
        }
    }

    private void RemoveClient(Client client)
    {
        lock (_locker)
        {
            PrintColoredText(client.NickName, client.ConsoleColor);
            _clients.Remove(client);
            Console.WriteLine(" disconnected. Total clients: {0}", _clients.Count);
        }
    }

    private void SendMessageToOtherClients(Client client, string messageText)
    {
        foreach (var c in _clients)
        {
            if (c == client) continue;

            try
            {
                var data =
                    $"m[{client.NickName}][{client.ConsoleColor}][{messageText}]";

                var byteToSend = Encoding.UTF8.GetBytes(data);
                c.Socket.Send(byteToSend, SocketFlags.None);
            }
            catch (SocketException)
            {
                _clients.Remove(c);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while sending messages to other clients. {0}", e.Message);
            }
        }
    }

    private void PrintColoredText(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void PrintClientMessage(Client client, string messageText)
    {
        PrintColoredText(client.NickName, client.ConsoleColor);
        Console.WriteLine(": {0}", messageText);
    }
}

