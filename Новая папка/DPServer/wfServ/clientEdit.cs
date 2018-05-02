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
    public partial class clientEdit : Form
    {
        TreeView treeView1;
        public clientEdit(TreeView trv)
        {
            treeView1 = trv;
            InitializeComponent();

            foreach (TreeNode node in treeView1.Nodes)
            {
                comboBox1.Items.Add(node.Text);

            }

            foreach (TreeNode node in treeView1.Nodes)
            {
                comboBox2.Items.Add(node.Text);

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeView1.Nodes[comboBox1.SelectedIndex].Nodes)
            {
                checkedListBox1.Items.Add(node.Text);

            }
        }
        private void button1_Click(object sender, EventArgs e) // перенос клиентов из одной ветви в другую
        {
            foreach (int indexChecked in checkedListBox1.CheckedIndices)
            {
               treeView1.Nodes[comboBox2.SelectedIndex].Nodes.Add( 
                   treeView1.Nodes[comboBox1.SelectedIndex].Nodes[indexChecked].Text);
               treeView1.Nodes[comboBox1.SelectedIndex].Nodes[indexChecked].Remove();
            }
            MessageBox.Show("Done");
        }
        private void clientEdit_Load(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        
    }
}
