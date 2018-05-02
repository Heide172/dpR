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
    public partial class trvEdit : Form
    {
        TreeView treeView1;
        public trvEdit(TreeView trv)
        {
            treeView1 = trv;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode tr = new TreeNode();
            tr.Text = Microsoft.VisualBasic.Interaction.InputBox("group name :");
            treeView1.Nodes.Add(tr);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            deleteClient dc = new deleteClient(treeView1);
            dc.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            clientEdit cl = new clientEdit(treeView1);
            cl.Show();
        }
    }
}
