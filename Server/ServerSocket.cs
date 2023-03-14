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

    public async Task AcceptAndHandleClients()
    {
        while (true)
        {
            var clientSocket = await AcceptClient();

            if (clientSocket == null) continue;

            ThreadPool.QueueUserWorkItem(_ => HandleClient(clientSocket));
        }
    }

    public async Task<Socket?> AcceptClient()
    {
        Socket? socket = null;
        try
        {
            socket = await _serverSocket.AcceptAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error accepting client: {0}", e.Message);
        }

        return socket;
    }

    private async Task HandleClient(Socket clientSocket)
    {
        var client = new Client
        {
            Socket = clientSocket
        };

        _clients.Add(client);

        await SendNickname(client);

        PrintColoredText(client.NickName, client.ConsoleColor);
        Console.WriteLine(" connected. Total clients: {0}", _clients.Count);

        await ReceiveMessageLoop(client);
    }

    public async Task SendNickname(Client client)
    {
        try
        {
            string dataToSend =
                $"n[{client.NickName}][{client.ConsoleColor}]";

            var bytesDataToSend = Encoding.UTF8.GetBytes(dataToSend);
            await client.Socket.SendAsync(bytesDataToSend, 0);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Error while sending text");
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async Task ReceiveMessageLoop(Client client)
    {
        while (true)
        {
            try
            {
                byte[] bytesReceived = new byte[1024];
                int byteCount = await client.Socket.ReceiveAsync(bytesReceived, SocketFlags.None);

                if (byteCount == 0)
                {
                    RemoveClient(client);
                    continue;
                }

                string messageText = Encoding.UTF8.GetString(bytesReceived, 0, byteCount);

                PrintClientMessage(client, messageText);

                await SendMessageToOtherClients(client, messageText);
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
        PrintColoredText(client.NickName, client.ConsoleColor);
        _clients.Remove(client);
        Console.WriteLine(" disconnected. Total clients: {0}", _clients.Count);
    }

    private async Task SendMessageToOtherClients(Client client, string messageText)
    {
        foreach (var c in _clients)
        {
            if (c == client) continue;

            try
            {
                //send message to other connected clients
                var data =
                    $"m[{client.NickName}][{client.ConsoleColor}][{messageText}]";

                var byteToSend = Encoding.UTF8.GetBytes(data);
                await c.Socket.SendAsync(byteToSend, SocketFlags.None);
            }
            catch (SocketException)
            {
                //to do: sa adaug o metoda de remove cu look
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

