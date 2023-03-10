using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketPR;

public class ClientSocket
{
    private readonly Socket _clientSocket;

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
                Console.Write("{0}: ", Nickname);
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

            var nickNameFromServer = Encoding.UTF8.GetString(bufferReceive);

            Console.WriteLine("Your nickname is {0}", nickNameFromServer);
            Nickname = nickNameFromServer;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public void ChoiseNickname()
    {
        try
        {
            Console.WriteLine("Enter your nickname or press ENTER to get a random one");

            Console.Write("Nickname: ");
            //var nickname = Console.ReadLine();



            var sb = new StringBuilder("");
            var key = Console.ReadKey();
            while (key.Key != ConsoleKey.Enter)
            {
                sb.Append(key.KeyChar);
                key = Console.ReadKey();
            }

            var nickname = sb.Length > 0 ? sb.ToString() : "noNickname";

            var bytesData = Encoding.UTF8.GetBytes(nickname);

            _clientSocket.Send(bytesData);

            byte[] bufferReceive = new byte[1024];
            _clientSocket.Receive(bufferReceive);

            var nickNameFromServer = Encoding.UTF8.GetString(bufferReceive);

            if (nickname == nickNameFromServer)
            {
                Console.WriteLine("Nickname {0} accepted.", nickname);
                Nickname = nickname;
            }
            else
            {
                Console.WriteLine("Your nickname is {0}", nickNameFromServer);
                Nickname = nickNameFromServer;
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }


    }
}

