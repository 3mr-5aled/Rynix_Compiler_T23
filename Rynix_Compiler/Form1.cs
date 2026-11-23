using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rynix_Compiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
            treeView1.Nodes.Clear();
            dataGridView1.Rows.Clear();
            Rynix_Compiler.TokenStream.Clear();
            Errors.Error_List.Clear();

            string Code = textBox1.Text;
            Rynix_Compiler.Start_Compiling(Code);
            PrintTokens();
            treeView1.Nodes.Add(Parser.PrintParseTree(Rynix_Compiler.treeroot));
            PrintErrors();
        }
        void PrintTokens()
        {
            for (int i = 0; i < Rynix_Compiler.Rynix_Scanner.Tokens.Count; i++)
            {
               dataGridView1.Rows.Add(Rynix_Compiler.Rynix_Scanner.Tokens.ElementAt(i).lex, Rynix_Compiler.Rynix_Scanner.Tokens.ElementAt(i).token_type);
            }
        }

        void PrintErrors()
        {
            for(int i=0; i<Errors.Error_List.Count; i++)
            {
                textBox2.Text += Errors.Error_List[i];
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            Rynix_Compiler.TokenStream.Clear();
            dataGridView1.Rows.Clear();
            treeView1.Nodes.Clear();
            Errors.Error_List.Clear();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
