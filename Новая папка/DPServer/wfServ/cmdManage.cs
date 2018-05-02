using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace wfServ
{
    public partial class cmdManage : Form
    {
        public List<command> cmdList = new List<command>();
        public const string path = "cmdList.xml";

        
        public cmdManage()
        {
            InitializeComponent();
            this.FormClosing += cmdManage_FormClosing;
            if (!(System.IO.File.Exists(path))) serializeXml(cmdList);
            else 
            
                cmdList = deserializeXml(path);

            foreach (command c in cmdList)
            {
                listBox1.Items.Add(c.name);
            }
            
        }
       

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            command a = new command();
            a.name = textBox1.Text;
            a.cmd = textBox2.Text;
            foreach (command c in cmdList)
            {
                int i = 0;
                if (c.name == a.name)
                {
                    replaceElem(a.cmd, i);
                    return;
                }
                    
            }
            cmdList.Add(a);
            listBox1.Items.Add(a.name);
            textBox1.Text = null;
            textBox2.Text = null;

           
        }
        private void replaceElem(string c, int i)
        {
            command cmd = new command();
            cmd = cmdList[i];
            cmd.cmd = c;
            cmdList[i] = cmd;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            serializeXml(cmdList);
        }
        private void serializeXml(List<command> cmdL)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(List<command>));

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, cmdL);
            }
        }

        private List<command> deserializeXml(string file)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(List<command>));

            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
            {
                List<command> cmdL = (List<command>)formatter.Deserialize(fs);

               return cmdL;
            }
            

        }
        private void OnApplicationExit(object sender, EventArgs e)
        {
            serializeXml(cmdList);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool elemSaved = false;

            if (textBox2.Text != "")
            {
                foreach (command c in cmdList)
                {
                    if (textBox2.Text == c.cmd) elemSaved = true;
                }
                if (!elemSaved)
                {
                    MessageBox.Show("Save macros first");
                    return;
                }
            }
            textBox1.Text = cmdList[listBox1.SelectedIndex].name;
            textBox2.Text = cmdList[listBox1.SelectedIndex].cmd;
        }

        private void cmdManage_FormClosing(object sender, FormClosingEventArgs e)
        {
             e.Cancel=true;
             this.Hide();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
           
        }
    }

    [Serializable]
    public struct command
    {
        public string name;
        public string cmd;
    }
}
