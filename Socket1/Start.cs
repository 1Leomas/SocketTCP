using System.Diagnostics;


var serverFullPath = 
    "D:\\Coding\\C#\\Retea\\SocketTCP\\Server\\bin\\Debug\\net6.0\\Server.exe";

var clientFullPath = 
    "D:\\Coding\\C#\\Retea\\SocketTCP\\Client\\bin\\Debug\\net6.0\\Client.exe";

ProcessStartInfo infoServer = new ProcessStartInfo(serverFullPath);
infoServer.UseShellExecute = true;

ProcessStartInfo infoClient = new ProcessStartInfo(clientFullPath);
infoClient.UseShellExecute = true;

Process serverProcess = Process.Start(infoServer);


for (int i = 0; i < 2; i++)
{
    Process clientProcess = Process.Start(infoClient);
}