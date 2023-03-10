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
            Console.WriteLine("New connection accepted");
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
                Console.WriteLine("{0}: {1}", ClientNickname, text);
            }
            catch (SocketException e)
            {
                // to do - delete client from client list
                //Console.WriteLine("Socket error: {0}", e.Message);
                Console.WriteLine("{0} disconnected", ClientNickname);
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving data from client {0}", client.RemoteEndPoint);
                Console.WriteLine(e.Message);
            }
        }
    }

    public bool ReceiveNickname(Socket client)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int byteCount = client.Receive(buffer);
            var stringReceive = Encoding.UTF8.GetString(buffer, 0, byteCount);


            if (string.Equals(stringReceive, "noNickname"))
            {
                var nameGenerator = new NameGenerator();
                ClientNickname = nameGenerator.Generate(new Random().Next(4, 8));
            }
            else
            {
                ClientNickname = stringReceive;
            }


            Console.WriteLine("New client {0} with nickname {1}", client.RemoteEndPoint, ClientNickname);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Client with address {0} disconnected", client.RemoteEndPoint);
            return false;

        }
        catch (Exception e)
        {
            Console.WriteLine("Error receiving data from client {0}", client.RemoteEndPoint);
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public void SendNickname(Socket client)
    {
        try
        {
            Console.WriteLine("Sedding nickname");

            ClientNickname = GenerateNickname();

            var bytesData = Encoding.UTF8.GetBytes(ClientNickname);
            client.Send(bytesData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public string GenerateNickname()
    {
        var nameGenerator = new NameGenerator();
        var nickname = nameGenerator.Generate(new Random().Next(4, 8));

        return nickname;
    }
}

