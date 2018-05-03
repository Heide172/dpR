using System.Net;
using DPClient;
using System;
using System.IO;
using System.Text;
using System.Management;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
namespace DPClient
{
    class Program
    {
        static IPAddress servAdress = IPAddress.Parse("127.0.0.1");
        static string settingsPath = "settings.xml";
        static TCPClient client = new TCPClient();
        static DPClient.xmlSave xml = new xmlSave();
        
        static void Main(string[] args)
        {
            client.Disconnected += onDisconnect;
            xml.Save(settingsPath);
           
            while (true)
            {
                if (client.tcpClient == null) onDisconnect();
                if(!client.tcpClient.Connected)
                    client.Connect(servAdress, xml.settings);
                if (client.sysWasChecked == true)
                {
                    xml.sysWasChecked = true;
                    xml.Save(settingsPath);
                    xml.sysWasChecked = false;
                    client.sysWasChecked = false;
                }
                Thread.Sleep(5000);

            }
        }

        static void onDisconnect()
        {
            client.tcpClient = new System.Net.Sockets.TcpClient();
            client.Connect(servAdress, xml.settings);
        }
        static void runCommand(string cmd)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = cmd;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.Start();
            process.BeginErrorReadLine();
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            process.WaitForExit();
        }



        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
        }


       
       
       
    }

    
}
