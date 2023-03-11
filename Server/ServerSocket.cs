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

    public List<Client> Clients { get; private set; }

    public ServerSocket(string ip, int port)
    {
        Clients = new List<Client>();

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
        Clients.Add(client);

        await SendNickname(client);

        PrintColoredText(client.NickName, client.ConsoleColor);
        Console.WriteLine(" connected. Total clients: {0}", Clients.Count);

        await ReceiveMessageLoop(client);
    }

    public async Task ReceiveMessageLoop(Client client)
    {
        while (true)
        {
            try
            {
                byte[] bytesRecived = new byte[1024];
                int byteCount = await client.Socket.ReceiveAsync(bytesRecived, SocketFlags.None);

                if (byteCount == 0)
                {
                    PrintColoredText(client.NickName, client.ConsoleColor);
                    Clients.Remove(client);
                    Console.WriteLine(" disconnected. Total clients: {0}", Clients.Count);
                    
                    continue;
                }

                string messageText = Encoding.UTF8.GetString(bytesRecived, 0, byteCount);

                PrintColoredText(client.NickName, client.ConsoleColor);

                Console.WriteLine(": {0}", messageText);

                foreach (var c in Clients)
                {
                    if (c != client)
                    {
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
                            Clients.Remove(c);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error while sending messages to other clients. {0}", e.Message);
                        }
                    }
                }
            }
            catch (SocketException)
            {
                PrintColoredText(client.NickName, client.ConsoleColor);
                Clients.Remove(client);
                Console.WriteLine(" disconnected. Total clients: {0}", Clients.Count);

                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving data from client {0}", client.Socket.RemoteEndPoint);
                Console.WriteLine(e.Message);
            }
        }
    }

    public async Task SendNickname(Client client)
    {
        try
        {
            string dataToSend = 
                $"[{client.NickName}][{client.ConsoleColor}]";

            var bytesData = Encoding.UTF8.GetBytes(dataToSend);
            await client.Socket.SendAsync(bytesData, 0);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while sending text");
            Console.WriteLine(e.Message);
        }
    }

    public void PrintColoredText(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

