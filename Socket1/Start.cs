using System.Diagnostics;
using System.Reflection;
using System.Text;

var curentPath = Assembly.GetEntryAssembly().Location;

var arr = curentPath.Split('\\');
var clientFullPath = new StringBuilder();
var serverFullPath = new StringBuilder();
for (var i = 0; i < arr.Length; i++)
{
    var item = arr[i];
    if (i == arr.Length - 5)
    {
        clientFullPath.Append("Client\\\\");
        serverFullPath.Append("Server\\\\");
    }
    else if (i < arr.Length - 1)
    {
        clientFullPath.Append($"{item}\\\\");
        serverFullPath.Append($"{item}\\\\");
    }
    else
    {
        clientFullPath.Append("Client.exe");
        serverFullPath.Append("Server.exe");
    }
}

ProcessStartInfo infoServer = new ProcessStartInfo(serverFullPath.ToString());
infoServer.UseShellExecute = true;

ProcessStartInfo infoClient = new ProcessStartInfo(clientFullPath.ToString());
infoClient.UseShellExecute = true;

Process serverProcess = Process.Start(infoServer);

Thread.Sleep(2000);

for (int i = 0; i < 2; i++)
{
    Process clientProcess = Process.Start(infoClient);
}

