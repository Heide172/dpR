using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wfServ
{
    public partial class Form2 : Form
    {
        public string str = " ";
        public Form2()
        {
            InitializeComponent();
            //textBox1.Text = str;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = str;
        }
    }
}
