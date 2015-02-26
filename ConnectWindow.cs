using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IsochronDrafter
{
    public partial class ConnectWindow : Form
    {
        public ConnectWindow()
        {
            InitializeComponent();
            MaximizeBox = false;
            #if DEBUG
            textBox1.Text = "isochron.us.to";
            ActiveControl = textBox2;
            #endif
        }

        // Connect.
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
                MessageBox.Show("You must enter a server.");
            else if (textBox2.Text.Length == 0)
                MessageBox.Show("You must enter an alias.");
            else if (textBox2.Text.Length > 16)
                MessageBox.Show("Please use an alias with sixteen or fewer characters.");
            else if (textBox2.Text.Contains('|') || textBox2.Text.Contains(';'))
                MessageBox.Show("Your alias contains disallowed characters.");
            else
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        // Start Server.
        private void button2_Click(object sender, EventArgs e)
        {
            ServerWindow serverWindow = new ServerWindow();
            DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.Hide();
            Owner.Owner = serverWindow;
            serverWindow.Show();
        }

        public string GetHostname()
        {
            return textBox1.Text;
        }

        public string GetAlias()
        {
            return textBox2.Text;
        }
    }
}
