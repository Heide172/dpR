using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using HeaderPack;
using xmlSettings;
namespace wfServ
{
    class newCLient
    {
        internal event ClientConnectedEventHandler ConnectedNewClient;
        internal delegate void ClientConnectedEventHandler(newCLient sender);
        internal event ClientDisconnectedEventHandler DisconnectedClient;
        internal delegate void ClientDisconnectedEventHandler(newCLient sender, string Message);
        internal event ClientDataAvailableEventHandler DataAvailable;
        internal delegate void ClientDataAvailableEventHandler(newCLient sender, HeaderDsc header, byte[] data);


        private bool run;
        public IPAddress ipadr;
        private TcpClient tcpClient;
        public string ErrorMsg = "";
        public XmlSet settings;
        FilePr file = new FilePr();
        public newCLient(TcpClient client)
        {
            this.tcpClient = client;
            run = true;
            tcpClient.ReceiveBufferSize = TCPPack.BufferSize;
            tcpClient.SendBufferSize = TCPPack.BufferSize;
            ipadr = IPAddress.Parse(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());
            Console.WriteLine("e23e42323d");


        }



        public async void Listen()
        {
            await Task.Run(() => Read());
            
        }

        public void Stop()
        {
            if (this.tcpClient != null)
            {
                run = false;
                this.tcpClient.Client.Close();
                this.tcpClient.Close();
                DisconnectedClient(this, "stopped");
                
            }


        }

        public  void Read()
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

                    int readBytes =  Network.Read(header, 0, header.Length); // считывание заголовка await async
                    if (readBytes == 0)
                    {
                        Console.WriteLine("Remote host dropped connection");
                        break;

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
                            //Console.ReadKey();
                        }
                    }

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
                        
                    }
                    if (headerDsc.Message == ServiceMessage.Authorization)
                    {
                        using (MemoryStream Memory = new MemoryStream(headerDsc.DataSize)) // десериализация заголовка
                        {
                           
                            Memory.Write(data, 0, data.Length);
                            Memory.Position = 0;

                            BinaryFormatter bf = new BinaryFormatter();
                            settings = (XmlSet)bf.Deserialize(Memory);
                        }
                    }
                    DataAvailable(this, headerDsc, data); // СОБЫТИЕ ДОСТУПНОСТИ ДАННЫХ

                    //Send(ServiceMessage.file, ipadr, "123.mov", ".mov");


                }
                catch(Exception ex)
                {
                    ErrorMsg = ex.Message;
                    run = false;
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
                this.Stop();
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
                this.Stop();
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