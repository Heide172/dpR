using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace DPServer
{
    class Program
    {
       
        static void Main(string[] args)
        {
            Form Form1 = new Form1();
            
            TCPServer server = new TCPServer();
            
            string Command = " ";
            while (Command != "exit")
            {
                Command = Console.ReadLine();
                switch (Command)
                {
                    case "Start":
                        server.Start();
                        break;
                    case "Stop":
                        server.Stop();
                        break;
                    case "Restart":
                        server.Stop();
                        Thread.Sleep(100);
                        server.Start();
                        break;
                    case "Help":
                        Console.WriteLine(
                            " Start - server start\n Stop - server stop\n Restart - server restart");
                        break;
                }
            }
        }

       
    }
}
