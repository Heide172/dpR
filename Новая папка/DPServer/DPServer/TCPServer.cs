using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using HeaderPack;
using System.Runtime.Serialization.Formatters.Binary;
using clSpec;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.InteropServices;

namespace DPServer
{
    internal delegate void SignalHandler(ConsoleSignal consoleSignal);
    internal enum ConsoleSignal
    {
        CtrlC = 0,
        CtrlBreak = 1,
        Close = 2,
        LogOff = 5,
        Shutdown = 6
    }

    internal static class ConsoleHelper
    {
        [DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
        public static extern bool SetSignalHandler(SignalHandler handler, bool add);
    }

    class TCPServer
    {
        
        internal delegate void ClientConnectedEventHandler(newCLient sender);
        internal event ClientConnectedEventHandler ConnectedNewClient;
        
        internal delegate void ClientDisconnectedEventHandler(newCLient sender, string Message);
        internal event ClientDisconnectedEventHandler DisconnectedClient;
        
        internal delegate void ClientDataAvailableEventHandler(newCLient sender, string Message);
        internal event ClientDataAvailableEventHandler DataAvailable;
        
        
        TcpListener tcpListener;
        TcpClient TcpClient;
        public List<newCLient> ClientList = new List<newCLient>();
        private List<DataList> dataList = new List<DataList>(); // буффер для обработки полученных данных
        private bool run;
        public string ErrorMsg;
        Guid guid = Guid.Parse("00000000-0000-0000-0000-000000000000");


        private static SignalHandler signalHandler;
        public bool Start() //обработка подключений 
        {
            signalHandler += onServClose;
            ConsoleHelper.SetSignalHandler(signalHandler, true);
            
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 3155);
                tcpListener.Start();
                Console.WriteLine("//S.Server started\n");
                //while (tcpListener != null)
                Task task = new Task(AcceptClients);
                task.Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.HResult.ToString());
                tcpListener = null;
                //throw;
                return false;
            }
        }

        private  void  AcceptClients()// 
        {
            
            while (tcpListener != null)
                try
                {
                    Console.WriteLine("waiting\n");
                    var AcceptSocket =  tcpListener.AcceptTcpClient();//
                    Console.WriteLine("connetced\n");
                    newCLient User = new newCLient(AcceptSocket);
                    User_Connected(User);
                    User.Listen();
                    User.DataAvailable += User_DataAv;
                    User.DisconnectedClient += User_Disconnected;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " " + ex.HResult.ToString());
                    SendErMessage(ex.ToString());

                }
            

        }

       

        public void Stop()
        {
            if (this.tcpListener != null)
            {
                this.tcpListener.Stop();
                this.tcpListener = null;
            }

        }

        private void User_DataAv(newCLient sender,HeaderDsc header, byte[] data) //обработка данных
        {
            
            Task t = new Task(() => DataProc(sender, header, data));
            t.Start();
            //DataProc(sender, header, data);
            
        }




        private void DataProc(newCLient sender, HeaderDsc header, byte[] data)
        {
            switch (header.Message)
            {
                case ServiceMessage.file: // получение файла

                    FilePr file = new FilePr();
                    string path = "123" + header.ext;
                    File.WriteAllBytes(path, data);
                    if (!(file.CheckSum(path) == header.Hex)) Console.WriteLine("file damaged"); // проверка целостности файла
                    break;

                case ServiceMessage.data:
                    break;
                case ServiceMessage.Authorization: // подключение
                    newUserAv(sender);
                    Console.WriteLine("Authorization message from ");
                    Console.WriteLine(sender.settings.guid);
                    break;
                case ServiceMessage.Server: // подключение серверного приложения
                    sender.clientState = 1;
                    Console.WriteLine("///server app connected\n");
                    List<ClSpec> listCl = new List<ClSpec>();// формирование перечисления описаний клиентов 
                    foreach (newCLient cl in ClientList)
                    {
                        if (cl.clSpec.settings == null)
                        {
                            cl.clSpec.settings = new xmlSettings.XmlSet();
                        }
                        listCl.Add(cl.clSpec);

                    }
                    byte[] clList;
                    using (MemoryStream Memory = new MemoryStream()) // сериализация перечисления
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(Memory, listCl);
                        Memory.Position = 0;
                        clList = new byte[Memory.Length];
                        var r = Memory.Read(clList, 0, clList.Length);
                    }
                    sender.SendCl(ServiceMessage.clList, this.guid, clList, 0);
                    break;
                case ServiceMessage.cmd: // комманда коммандной строки
                    if (sender.clientState == 1) // если сообщение получено от серверного приложения
                    {
                        Console.WriteLine("cmd get from serv app\n");
                        try
                        {
                            Cmd cmd = new Cmd();
                            using (MemoryStream Memory = new MemoryStream(header.DataSize)) // десериализация заголовка
                            {

                                Memory.Write(data, 0, data.Length);
                                Memory.Position = 0;

                                BinaryFormatter bf = new BinaryFormatter();
                                cmd = (Cmd)bf.Deserialize(Memory);
                            }
                            foreach (Guid guid in cmd.list) // поиск выбранных клиентов
                            {
                                foreach (newCLient cl in ClientList)
                                {
                                    if (cl.clSpec.settings == null) ;
                                    else
                                    {
                                        if (cl.clSpec.settings.guid == guid)
                                        {
                                            byte[] str = Encoding.Default.GetBytes(cmd.strCmd);
                                            cl.Send(ServiceMessage.cmd, this.guid, str);

                                        }
                                    }
                                }

                            }
                        }
                        catch (Exception ex)// отправка ошибки на серверное приложение
                        {
                            string erMsg = ex.ToString() + " SERVER EXCEPTION WHILE SENDING CMD";
                            sender.Send(ServiceMessage.error, guid, Encoding.Default.GetBytes(erMsg));
                        }
                    }
                    else // если это ответ от клиента
                    {
                        Console.WriteLine("cmd response get from" + sender.tcpClient.ToString());
                        foreach (newCLient client in ClientList)
                        {
                            if (client.clientState == 1)
                            {
                                client.Send(ServiceMessage.cmd, guid, data);

                            }

                        }


                    }
                    break;
                case ServiceMessage.servCommand:
                    break;
                case ServiceMessage.clCommand:
                    clientSendCommand(sender, header, data);
                    break;
            }
            
        
        }

        private void serverCommandGet(int commandIndex)
        {
            switch (commandIndex)
            { 
                case 0:
                    this.Stop();
                    Thread.Sleep(100);
                    this.Start();
                    break;
            
            
            }
        
        }
        private void clientSendCommand(newCLient sender, HeaderDsc header, byte[] data)
       {
           if (sender.clientState == 1) // если сообщение получено от серверного приложения
           {
               try
               {
                   Cmd cmd = new Cmd();
                   using (MemoryStream Memory = new MemoryStream(header.DataSize)) // десериализация заголовка
                   {

                       Memory.Write(data, 0, data.Length);
                       Memory.Position = 0;

                       BinaryFormatter bf = new BinaryFormatter();
                       cmd = (Cmd)bf.Deserialize(Memory);
                   }
                   foreach (Guid guid in cmd.list) // поиск выбранных клиентов
                   {
                       foreach (newCLient cl in ClientList)
                       {
                           if (cl.clSpec.settings == null) break;
                           if (cl.clSpec.settings.guid == guid)
                           {
                               byte[] index = BitConverter.GetBytes(header.comIndex);
                               cl.Send(ServiceMessage.clCommand, this.guid, index);

                           }

                       }

                   }
               }
               catch (Exception ex)// отправка ошибки на серверное приложение
               {
                   string erMsg = ex.ToString() + " SERVER EXCEPTION WHILE SENDING CLIENT COMMAND";
                   sender.Send(ServiceMessage.error, guid, Encoding.Default.GetBytes(erMsg));
               }
           }
           else // если это ответ от клиента
           {
               foreach (newCLient client in ClientList)
               {
                   if (client.clientState == 1)
                   {
                       client.Send(ServiceMessage.clCommand, guid, data);

                   }

               }


           }
       
       }
       private void SendErMessage(string er)// 
        {
            foreach (newCLient cl in ClientList)
            {
                if (cl.clientState == 1)
                {
                    cl.Send(ServiceMessage.error, Guid.Empty, Encoding.Default.GetBytes(er));
                
                }
            }
        
        
        }
        void User_Disconnected(newCLient sender, string message)
        {
            if (sender.clientState != 1)
            {
                try
                {
                    byte[] data;
                    foreach (newCLient client in ClientList)
                    {
                        if (client.clientState == 1)
                        {
                            using (MemoryStream Memory = new MemoryStream()) // сериализация перечисления
                            {
                                BinaryFormatter bf = new BinaryFormatter();
                                bf.Serialize(Memory, sender.clSpec);
                                Memory.Position = 0;
                                data = new byte[Memory.Length];
                                var r = Memory.Read(data, 0, data.Length);
                            }
                            client.SendCl(ServiceMessage.clList, this.guid, data, 2);


                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " " + ex.HResult.ToString());
                }
            
            }
            ClientList.Remove(sender);
            Console.WriteLine(sender.ipadr);
            Console.WriteLine("//S.User Disconnected\n");
            Console.WriteLine(ClientList.Count);
        }
       
        void User_Connected(newCLient sender)
        {
            ClientList.Add(sender);
            Console.WriteLine(sender.ipadr);
            Console.WriteLine("//S.User Connected\n");
            Console.WriteLine(ClientList.Count);
        }

        void newUserAv(newCLient sender)
        {
            try
            {
                byte[] data;
                foreach (newCLient client in ClientList)
                {
                    if (client.clientState == 1)
                    {
                        using (MemoryStream Memory = new MemoryStream()) // сериализация перечисления
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Serialize(Memory, sender.clSpec);
                            Memory.Position = 0;
                            data = new byte[Memory.Length];
                            var r = Memory.Read(data, 0, data.Length);
                        }
                        client.SendCl(ServiceMessage.clList, this.guid, data, 1);


                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.HResult.ToString());
            }
        }

        void onServClose(ConsoleSignal consoleSignal)
        {
            SendErMessage("001");
        }

    
    }

    struct DataList
    {
        newCLient sender;
        HeaderDsc header;
        byte[] data;
    }
}