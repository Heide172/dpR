using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using clSpec;
using xmlSettings;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Configuration;
using NLog;

namespace wfServ
{
    public partial class Form1 : Form
    {
        public delegate void ClientSendCmd1(Guid guid);
        public event ClientSendCmd1 ClientSendCmd; 
        
        private TCPClient client = new TCPClient(IPAddress.Parse("127.0.0.1")); // класс tcp
        const string xmlPath = "treeView.xml";// путь сохранения дерева клиентов 
        cmdManage cmdManage = new cmdManage();// сохраненные cmd комманды 
        Logger logger = LogManager.GetCurrentClassLogger();
        List<Cmd> responseList = new List<Cmd>();
        public Form1()
        {
            InitializeComponent();
            ClientSendCmd += clientCmdSent;
            client.GetCmdResponse += onGetCmdResponse;
            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_Selecting);
            clCmdFill();
            servCmdFill();
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
           
            XmlHandler a = new XmlHandler();
            treeView1.ImageList = imageList1;
            treeView1.ShowNodeToolTips = true;
            treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick;
            foreach (command c in cmdManage.cmdList)
            {
                listBox1.Items.Add(c.name);
            }
            a.XmlToTreeView(xmlPath, treeView1);
            //Task t = new Task(checkState);
            //t.Start();
            startupLoad();
           
            a.XmlToTreeView(xmlPath, treeView1);
        }
        private void startupLoad()
        {
            textBox2.Text = ConfigurationManager.AppSettings["serverIp"];
            client = new TCPClient(IPAddress.Parse(textBox2.Text));
            
            

        }
        private void clCmdFill()
        {
            listBox2.Items.Add("Set system checked"); // 0 
        }

        private void servCmdFill()
        {
            listBox3.Items.Add("Restart server"); // 0 

        }
        private void OnApplicationExit(object sender, EventArgs e)
        {
            XmlHandler a = new XmlHandler();
            a.TreeViewToXml(treeView1, xmlPath);
            logger.Trace("application close");

        }


        private void button1_Click(object sender, EventArgs e)
        {
            
            client.newClientConnected += clientConnected;
            client.ClientDisconneted += clientDisconnected;
            client.GetCmdResponse += onGetCmdResponse;
            //client.ClientGetCmd += clientCmdGet;
           
            if (client.tcpClient == null) client.tcpClient = new System.Net.Sockets.TcpClient();
            if (client.tcpClient.Connected)
            {
                MessageBox.Show("Already connected !");
                return;
            }
            client.Connect();
            logger.Trace("Connected to server");
        }

        private void clientConnected(Guid guid, string ip)
        {
            if (guid == Guid.Parse("00000000-0000-0000-0000-000000000000")) return;
            if (guid == Guid.Parse("00000000-0000-0000-0000-000000000001")) return;
            logger.Trace("new client connected");
            TreeNodeCollection tree = treeView1.Nodes;
            bool isNew = true;// перебор узлов
                foreach (TreeNode node in tree)
                {
                    TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                    //if (node.Nodes.Count == 0)
                    //{
                    //    TreeNode tr = new TreeNode();
                    //    tr.Text = guid.ToString();
                    //    treeView1.Nodes[0].Nodes.Add(tr);
                    //    treeView1.Nodes[0].Nodes[0].ImageIndex = 0;
                    //    treeView1.Nodes[0].Nodes[0].SelectedImageIndex = 0;
                    //    isNew = false;
                    //}
                    
                        foreach (TreeNode node1 in collection)
                        {
                            if (node1.ToolTipText == guid.ToString())
                            {
                                node1.ImageIndex = 0;
                                node1.SelectedImageIndex = 0;
                                isNew = false;
                            }
                            
                            
                        }
                        
                    
                }
                if (isNew)
                {
                    TreeNode tr = new TreeNode();
                    tr.ToolTipText = guid.ToString();
                    tr.Text = ip;
                    tr.ImageIndex = 0;
                    tr.SelectedImageIndex = 0;
                    treeView1.Nodes[0].Nodes.Add(tr);

                }
            
        }

        private void clientDisconnected(Guid guid, string ip)
        {
            logger.Trace("client disconnected" + guid.ToString());
            TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
            foreach (TreeNode node in tree)
            {
                TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                foreach (TreeNode node1 in collection)
                {
                    if (node1.ToolTipText == guid.ToString())
                    {
                        
                        node1.ImageIndex = 1;
                        node1.SelectedImageIndex = 1;
                    }

                    
                }
            }
        }

        private void clientCmdSent(Guid guid)
        {
            foreach (ClSpec cl in client.ClientList)
            {
                if (cl.settings.guid == guid) cl.responseState = "Cmd sent";
            }

            TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
            foreach (TreeNode node in tree)
            {
                TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                foreach (TreeNode node1 in collection)
                {
                    if (node1.ToolTipText == guid.ToString())
                    {
                        node1.BackColor = Color.Red;
                    }


                }
            }
        }

        private void clientCmdGet(Guid guid)
        {
            foreach (ClSpec cl in client.ClientList)
            {
                if (cl.settings.guid == guid) cl.responseState = "Cmd response get";
            }
            TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
            foreach (TreeNode node in tree)
            {
                TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                foreach (TreeNode node1 in collection)
                {
                    if (node1.ToolTipText == guid.ToString())
                    {
                        node1.BackColor = Color.LightGreen;
                    }


                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.Connect();
        }
      
        private void button3_Click(object sender, EventArgs e)
        {
            XmlHandler a = new XmlHandler();
            string p = Microsoft.VisualBasic.Interaction.InputBox("Input file name :") + ".xml";
           
            try
            {
                a.TreeViewToXml(treeView1, p);
            }
            catch (Exception ex)
            {
                logger.WarnException("exeption get while trying to saveЫ in xml", ex); 
                MessageBox.Show(ex.ToString());
            }
        }

       

       
        private void button4_Click(object sender, EventArgs e)
        {
            XmlHandler a = new XmlHandler();
            string p = Microsoft.VisualBasic.Interaction.InputBox("Input file name :") + ".xml";
            try
            {
                a.XmlToTreeView(p, treeView1);
            }
            catch (Exception ex)
            {
                logger.WarnException("exeption get while trying to load from xml", ex);
                MessageBox.Show(ex.ToString());
            }

        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trvEdit trv = new trvEdit(treeView1);
            trv.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add("Unallocated clients ");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Cmd cmd = new Cmd();
            cmd.list = new List<Guid>();
            
            cmd.strCmd = textBox1.Text;
            
            TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
            foreach (TreeNode node in tree)
            {
                TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                foreach (TreeNode node1 in collection)
                {
                    if (node1.Checked)
                    {
                        string gd = node1.ToolTipText.Substring(0, 36);
                        Guid guid = Guid.Parse(gd);
                        ClientSendCmd(guid);
                        cmd.list.Add(guid);
                    }
                }
            }
            byte[] data;
            using (MemoryStream Memory = new MemoryStream()) // сериализация перечисления
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(Memory, cmd);
                Memory.Position = 0;
                data = new byte[Memory.Length];
                var r = Memory.Read(data, 0, data.Length);
            }
            client.Send(HeaderPack.ServiceMessage.cmd, client.guid, data);
            
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            foreach (command c in cmdManage.cmdList)
            {
                listBox1.Items.Add(c.name);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            cmdManage.Show();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            Cmd cmd = new Cmd();
            cmd.list = new List<Guid>();

            cmd.cmdIndex = listBox2.SelectedIndex;

            TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
            foreach (TreeNode node in tree)
            {
                TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                foreach (TreeNode node1 in collection)
                {
                    if (node1.Checked)
                    {
                        string gd = node1.ToolTipText.Substring(0, 36);
                        Guid guid = Guid.Parse(gd);
                        cmd.list.Add(guid);
                    }
                }
            }
            byte[] data;
            using (MemoryStream Memory = new MemoryStream()) // сериализация перечисления
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(Memory, cmd);
                Memory.Position = 0;
                data = new byte[Memory.Length];
                var r = Memory.Read(data, 0, data.Length);
            }
            client.Send(HeaderPack.ServiceMessage.clCommand, client.guid, data);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            client.SendCommand(HeaderPack.ServiceMessage.servCommand, client.guid, listBox3.SelectedIndex);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            client.Disonnect();
        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                currentConfig.AppSettings.Settings["serverIp"].Value = textBox2.Text;
                currentConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                client.servAdress = IPAddress.Parse(textBox2.Text);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.ToString());
                MessageBox.Show(ex.ToString());
            
            }

        }

        private void onGetCmdResponse(Cmd cmd)
        {
            int i = 0; 
            foreach (string s in comboBox1.Items)
            {

                if (cmd.destGuid == Guid.Parse(s))
                {
                    comboBox1.SelectedIndex = i;
                    textBox3.Text = cmd.strCmd;
                    textBox4.Text = textBox4.Text + Environment.NewLine + "Get cmd response from " + cmd.destGuid.ToString();
                    clientCmdGet(cmd.destGuid);
                    return;
                }
                i ++;
            }

            comboBox1.Items.Add(cmd.destGuid.ToString());
            comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            responseList.Add(cmd);
            if (textBox3.Text == "")
            {
                textBox3.Text = cmd.strCmd;
            }
            textBox4.Text = textBox4.Text + Environment.NewLine + "Get cmd response from " + cmd.destGuid.ToString();
            clientCmdGet(cmd.destGuid);
        
        }

 //       protected void treeView1_AfterSelect(object sender,
 //System.Windows.Forms.TreeViewEventArgs e)
 //       {
 //           // Determine by checking the Text property.  
 //           MessageBox.Show(e.Node.Text);
 //       }

        void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            textBoxInfoGUID.Text = "";
            textBoxInfoIPADD.Text = "";
            textBoxInfoSTATE.Text = "";
            textBoxINFO_HDD.Text = "";
            textBoxINFO_PROC_CAPTION.Text = "";
            textBoxINFO_PROC_NUMOFCORES.Text = "";
            textBoxINFO_RAM_CAPTION.Text = "";
            textBoxINFO_RAM_CLOCK.Text = "";
            listBoxINFO_GPU_NAME.Items.Clear();
            listBoxINFO_RAM_CAPACITY.Items.Clear();
            textBoxINFO_OS_BUILD.Text = "";
            textBoxINFO_OS_NAME.Text = "";
            textBoxINFO_OS_SERIAL.Text = "";
            textBoxINFO_OS_STATUS.Text = "";
            textBoxINFO_OS_SYS_DIR.Text = "";
            textBoxINFO_OS_VERSION.Text = "";


            try
            {
                textBoxInfoIPADD.Text = e.Node.Text;
                textBoxInfoGUID.Text = e.Node.ToolTipText;
                foreach (ClSpec cl in client.ClientList)
                {
                    if (cl.settings.guid == Guid.Parse(e.Node.ToolTipText))
                    {
                        textBoxInfoSTATE.Text = cl.settings.error;
                        textBoxINFO_RAM_CAPTION.Text = cl.settings.sysinfo1.ramInfo[0].BankLabel;
                        textBoxINFO_RAM_CLOCK.Text = cl.settings.sysinfo1.ramInfo[0].Speed;
                        textBoxINFO_OS_NAME.Text = cl.settings.sysinfo1.OSInfo.Caption;
                        textBoxINFO_OS_VERSION.Text = cl.settings.sysinfo1.OSInfo.Version;
                        textBoxINFO_OS_STATUS.Text = cl.settings.sysinfo1.OSInfo.Status;
                        textBoxINFO_OS_SERIAL.Text = cl.settings.sysinfo1.OSInfo.SerialNumber ;
                        textBoxINFO_OS_BUILD.Text = cl.settings.sysinfo1.OSInfo.BuildNumber;
                        textBoxINFO_OS_SYS_DIR.Text = cl.settings.sysinfo1.OSInfo.SystemDirectory;


                        foreach (ramInfo r in cl.settings.sysinfo1.ramInfo)
                        {
                            listBoxINFO_RAM_CAPACITY.Items.Add(r.Capacity);
                        }
                        foreach (driveInfo d in cl.settings.sysinfo1.driveInfo)
                        {
                            textBoxINFO_HDD.Text = textBoxINFO_HDD.Text +
                                "-/-/-/-/-/-/" + Environment.NewLine +
                                d.Caption + " " + Environment.NewLine +
                                "Total Space : " + d.Capacity + Environment.NewLine +
                                "Free Space : " + d.FreeSpace + Environment.NewLine +
                                "File System : " + d.FileSystem + Environment.NewLine;
                                
                        }
                        textBoxINFO_PROC_CAPTION.Text = cl.settings.sysinfo1.processorInfo.Name;
                        textBoxINFO_PROC_NUMOFCORES.Text = cl.settings.sysinfo1.processorInfo.NumberOfCores;
                        foreach (gpuInfo g in cl.settings.sysinfo1.gpuInfo)
                        {
                            listBoxINFO_GPU_NAME.Items.Add(g.Caption);
                        }

                        tabControl1.SelectedTab = tabPage4;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox3.Text = responseList[comboBox1.SelectedIndex].strCmd;
            Guid guid = Guid.Parse(comboBox1.SelectedItem.ToString());
            foreach (ClSpec cl in client.ClientList)
            {
                if (cl.settings.guid == guid) cl.responseState = "State ok";
            }
            TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
            foreach (TreeNode node in tree)
            {
                TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                foreach (TreeNode node1 in collection)
                {
                    if (node1.ToolTipText == guid.ToString())
                    {
                        node1.BackColor = Color.White;
                    }


                }
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            textBox1.Text = cmdManage.cmdList[listBox1.SelectedIndex].cmd;
        }

       

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            cmdManage.Show();
        }

     

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client.newClientConnected += clientConnected;
            client.ClientDisconneted += clientDisconnected;
            client.GetCmdResponse += onGetCmdResponse;
            if (client.tcpClient == null) client.tcpClient = new System.Net.Sockets.TcpClient();
            if (client.tcpClient.Connected)
            {
                MessageBox.Show("Already connected !");
                return;
            }
            client.Connect();
            logger.Trace("Connected to server");
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client.Disonnect();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XmlHandler a = new XmlHandler();
            string p = Microsoft.VisualBasic.Interaction.InputBox("Input file name :") + ".xml";

            try
            {
                a.TreeViewToXml(treeView1, p);
            }
            catch (Exception ex)
            {
                logger.WarnException("exeption get while trying to saveЫ in xml", ex);
                MessageBox.Show(ex.ToString());
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XmlHandler a = new XmlHandler();
            string p = Microsoft.VisualBasic.Interaction.InputBox("Input file name :") + ".xml";
            try
            {
                a.XmlToTreeView(p, treeView1);
            }
            catch (Exception ex)
            {
                logger.WarnException("exeption get while trying to load from xml", ex);
                MessageBox.Show(ex.ToString());
            }
        }

        private void clearListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add("Unallocated clients ");
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
            foreach (TreeNode node in tree)
            {
                TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                foreach (TreeNode node1 in collection)
                {
                                  
                        node1.BackColor = Color.White;
                }
            }
        }

        void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Text == "Response ")
            {
                TreeNodeCollection tree = treeView1.Nodes;// перебор узлов
                foreach (TreeNode node in tree)
                {
                    TreeNodeCollection collection = node.Nodes;// перебор элементов узла
                    foreach (TreeNode node1 in collection)
                    {
                        if (node1.ToolTipText == comboBox1.SelectedText)
                        {
                            node1.BackColor = Color.White;
                        }


                    }
                }
            }
            // Validate the current page. To cancel the select, use:
            
        }
    }


    public class XmlHandler
    {
        XmlDocument xmlDocument;

        /// <summary>
        /// Initialisiert eine neue Instanz der MultiClipboard Klasse.
        /// </summary>
        public XmlHandler()
        {
        }

        /// <summary>
        /// Den inhalt des TreeViews in eine xml Datei exportieren
        /// </summary>
        /// <param name="treeView">Der TreeView der exportiert werden soll</param>
        /// <param name="path">Ein  Pfad unter dem die Xml Datei entstehen soll</param>
        public void TreeViewToXml(TreeView treeView, String path)
        {
            xmlDocument = new XmlDocument();
            xmlDocument.AppendChild(xmlDocument.CreateElement("ROOT"));
            XmlRekursivExport(xmlDocument.DocumentElement, treeView.Nodes);
            xmlDocument.Save(path);
        }

        /// <summary>
        /// Eine vorher Exportierte Xml Datei wieder in ein TreeView importieren
        /// </summary>
        /// <param name="path">Der Quellpfad der Xml Datei</param>
        /// <param name="treeView">Ein TreeView in dem der Inhalt der Xml Datei wieder angezeigt werden soll</param>
        /// <exception cref="FileNotFoundException">gibt an das die Datei nicht gefunden werden konnte</exception>
        public void XmlToTreeView(String path, TreeView treeView)
        {
            try
            {
                xmlDocument = new XmlDocument();

                xmlDocument.Load(path);
                treeView.Nodes.Clear();
                XmlRekursivImport(treeView.Nodes, xmlDocument.DocumentElement.ChildNodes);
            }
            catch
            { } 
            }

        private XmlNode XmlRekursivExport(XmlNode nodeElement, TreeNodeCollection treeNodeCollection)
        {
            XmlNode xmlNode = null;
            foreach (TreeNode treeNode in treeNodeCollection)
            {
                xmlNode = xmlDocument.CreateElement("TreeViewNode");

                xmlNode.Attributes.Append(xmlDocument.CreateAttribute("value"));
                xmlNode.Attributes["value"].Value = treeNode.Text;
                xmlNode.Attributes.Append(xmlDocument.CreateAttribute("tooltip"));
                xmlNode.Attributes["tooltip"].Value = treeNode.ToolTipText;

                if (nodeElement != null)
                    nodeElement.AppendChild(xmlNode);

                if (treeNode.Nodes.Count > 0)
                {
                    XmlRekursivExport(xmlNode, treeNode.Nodes);
                }
            }
            return xmlNode;
        }

        private void XmlRekursivImport(TreeNodeCollection elem, XmlNodeList xmlNodeList)
        {
            TreeNode treeNode;
            foreach (XmlNode myXmlNode in xmlNodeList)
            {
                treeNode = new TreeNode(myXmlNode.Attributes["value"].Value);
                treeNode.ToolTipText = myXmlNode.Attributes["tooltip"].Value;

                if (myXmlNode.ChildNodes.Count > 0)
                {
                    XmlRekursivImport(treeNode.Nodes, myXmlNode.ChildNodes);
                }
                elem.Add(treeNode);
            }
        }
    }
}
