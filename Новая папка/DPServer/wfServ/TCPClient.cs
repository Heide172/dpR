using System;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using HeaderPack;
using System.Threading.Tasks;
using System.Collections.Generic;
using clSpec;
using System.Xml.Serialization;
using System.Text;
using xmlSettings;
namespace wfServ
{
    class TCPClient
    {
        private const string filename = "cllist.xml";


        public delegate void ClientListUpdatedAddCl(Guid guid, string ip);
        public event ClientListUpdatedAddCl newClientConnected;
        public delegate void ClientListUpdatedRemoveCl(Guid guid, string ip);
        public event ClientListUpdatedRemoveCl ClientDisconneted;
        public delegate void SocketDisconnectedEventHandler();
        public event SocketDisconnectedEventHandler Disconnected;
        public delegate void GetCmdResponse1(Cmd cmd);
        public event GetCmdResponse1 GetCmdResponse;
        public delegate void onServerClose(string reason);
        public event onServerClose servClosed;
        public delegate void errorGet(int errID);
        public event errorGet exeptionGet;


        public IPAddress servAdress = IPAddress.Parse("127.0.0.1");
        public IPAddress thisAdress;
        public List<ClSpec> ClientList = new List<ClSpec>();
        private List<ClSpec> newClientList = new List<ClSpec>();
        public Guid guid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public TcpClient tcpClient = new TcpClient();
        private FilePr file = new FilePr();
        private bool run;
        public XmlSet settings = new XmlSet();
        public string ErrorMsg;
        public TCPClient(IPAddress servAdd)
        {
            Application.ApplicationExit += OnApplicationExit;
            servAdress = servAdd;
            settings.guid = guid;
            if (!(System.IO.File.Exists(filename)))
            {
                listToXml();
            }
            listFromXml();

        }
        public bool Connect()
        {
            try
            {
               


                if (!tcpClient.Connected)
                {
                    run = true;
                    tcpClient.Connect(servAdress, 3155);
                    //Task.Run(() => Reading_of_transport_stream());
                }
                Check_in();
                return tcpClient.Connected;
            }

            catch (Exception ex)
            {
                this.Disonnect();
                exeptionGet(ex.HResult);
                MessageBox.Show(ex.HResult.ToString());
                return false;
                
               // throw new Exception(ex.Message);
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
                
            }
        }
        public void Check_in()
        {
            String host = Dns.GetHostName();
            // Получение ip-адреса.
            IPAddress ip = Dns.GetHostByName(host).AddressList[0];
            thisAdress = ip;
            this.Send(ServiceMessage.Server, guid, new byte[] { });
            //Task t = new Task(Read);
            //t.Start();
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

                    int readBytes = await Network.ReadAsync(header, 0, header.Length); // считывание заголовка

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
                catch (SocketException ex)
                {
                    exeptionGet(ex.ErrorCode);
                    MessageBox.Show(ex.ErrorCode.ToString());

                }
                catch (IOException ex)
                {
                    exeptionGet(ex.HResult);
                        MessageBox.Show(ex.HResult.ToString());
                        this.Disonnect();
                
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message;
                    if (ex.Message != "Ссылка на объект не указывает на экземпляр объекта.")
                        MessageBox.Show(ex.HResult.ToString());
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
                case ServiceMessage.clList:
                    switch (headerDsc.comIndex)
                    {
                        case 0:
                            deSerializeList(data, headerDsc);
                            if (ClientList.Count == 0)
                            {
                                foreach (ClSpec c2 in newClientList)
                                {
                                    
                                    
                                    ClientList.Add(c2);
                                    newClientConnected(c2.settings.guid, c2.ipAdr);
                                }
                            }
                            else
                            {
                                
                                foreach (ClSpec c in newClientList)
                                {
                                    bool isNew = false;
                                    foreach (ClSpec c1 in ClientList)
                                    {
                                        if (c1.settings == null)
                                        {
                                            c1.settings = new XmlSet();
                                        }
                                        if (c.settings.guid == c1.settings.guid)
                                        {
                                            newClientConnected(c.settings.guid, c.ipAdr);
                                            isNew = false;
                                            break;
                                        }
                                        else isNew = true;

                                    }
                                    if (isNew)
                                    {
                                        newClientConnected(c.settings.guid, c.ipAdr);
                                        ClientList.Add(c);
                                    }
                                }
                            }
                        break;
                        case 1:
                             ClSpec cl = new ClSpec();
                             using (MemoryStream Memory = new MemoryStream(headerDsc.DataSize)) // десериализация заголовка
                            {
                                 Memory.Write(data, 0, data.Length);
                                 Memory.Position = 0;
                                 BinaryFormatter bf = new BinaryFormatter();
                                 cl = (ClSpec)bf.Deserialize(Memory);
                            }
                            newClientConnected(cl.settings.guid, cl.ipAdr);
                             bool isNew1 = false;
                             foreach (ClSpec c in ClientList)
                             {
                                 if (c.settings.guid != cl.settings.guid) isNew1 = true;
                             
                             }
                             if (isNew1) ClientList.Add(cl);
                             break;
                        case 2:
                             ClSpec cl1 = new ClSpec();
                             using (MemoryStream Memory = new MemoryStream(headerDsc.DataSize)) // десериализация заголовка
                            {
                                 Memory.Write(data, 0, data.Length);
                                 Memory.Position = 0;
                                 BinaryFormatter bf = new BinaryFormatter();
                                 cl1 = (ClSpec)bf.Deserialize(Memory);
                            }
                             ClientDisconneted(cl1.settings.guid, cl1.ipAdr);
                             break;

                    }
                    break;
                case ServiceMessage.cmd:
                    
                    Form2 formAnsw = new Form2();
                    Cmd cmd = new Cmd();
                            using (MemoryStream Memory = new MemoryStream(headerDsc.DataSize)) // десериализация заголовка
                            {

                                Memory.Write(data, 0, data.Length);
                                Memory.Position = 0;

                                BinaryFormatter bf = new BinaryFormatter();
                                cmd = (Cmd)bf.Deserialize(Memory);
                            }
                            GetCmdResponse(cmd);
                    break;
                case ServiceMessage.clCommand:
                    MessageBox.Show(Encoding.Default.GetString(data));
                    break;
                case ServiceMessage.servCommand:
                    MessageBox.Show(Encoding.Default.GetString(data));
                    break;
                case ServiceMessage.error:
                    string er = Encoding.Default.GetString(data);
                    if (er == "001")
                        servClosed("Server was closed manualy");
                    else
                        MessageBox.Show(er);
                    break;
               
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
                MessageBox.Show(ex.Message + " " + ex.HResult.ToString());
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

        public void SendCommand(ServiceMessage message, Guid guid, int comIndex)
        {
            byte[] buffer;
            byte[] header;
            byte[] infobuffer; //???
            byte[] data = new byte[0];
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
                headerDsc.comIndex = comIndex;
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
                MessageBox.Show(ex.Message + " " + ex.HResult.ToString());
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
                MessageBox.Show(ex.Message + " " + ex.HResult.ToString());
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
        private void deSerializeList(byte[] data, HeaderDsc headerDsc)
        {
            using (MemoryStream Memory = new MemoryStream(headerDsc.DataSize)) // десериализация заголовка
            {

                Memory.Write(data, 0, data.Length);
                Memory.Position = 0;

                BinaryFormatter bf = new BinaryFormatter();
                newClientList = (List<ClSpec>)bf.Deserialize(Memory);
            }

        }
        private void OnApplicationExit(object sender, EventArgs e)
        {
            listToXml();
        }
        private void listToXml()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<ClSpec>));
            System.IO.TextWriter writer = new System.IO.StreamWriter(filename);
            ser.Serialize(writer, ClientList);
            writer.Close();
        }

        private void listFromXml()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<ClSpec>));
            System.IO.TextReader writer = new System.IO.StreamReader(filename);
            ClientList = (List<ClSpec>)ser.Deserialize(writer);
            writer.Close();
        }
    }

}
