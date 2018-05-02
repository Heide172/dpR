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
    public partial class deleteClient : Form
    {
        TreeView treeView1;
        public deleteClient(TreeView trv)
        {
            treeView1 = trv;
            InitializeComponent();
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (int indexChecked in checkedListBox1.CheckedIndices)
            {
                if (indexChecked == 0)
                {
                    MessageBox.Show("You cannot delete this group ");
                }
                else
                {
                    treeView1.Nodes.RemoveAt(indexChecked);
                    MessageBox.Show("Deleted ");
                }
            }
           
        }

        private void deleteClient_Load(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                checkedListBox1.Items.Add(node.Text);

            }
        }

       

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
