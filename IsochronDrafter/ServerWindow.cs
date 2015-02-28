using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace IsochronDrafter
{
    public partial class ServerWindow : Form
    {
        private DraftServer server;

        public ServerWindow()
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitializeComponent();
            MaximizeBox = false;
            #if DEBUG
            textBox2.Text = "C:\\Users\\Tom\\Desktop\\Kavanagh.txt";
            textBox3.Text = "https://dl.dropboxusercontent.com/u/1377551/IsochronDrafter/";
            #endif
        }

        public void PrintLine(string text)
        {
            textBox1.Invoke(new MethodInvoker(delegate
            {
                if (textBox1.Text.Length != 0)
                    textBox1.Text += "\r\n";
                textBox1.Text += text;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }));
        }
        public void DraftButtonEnabled(bool enabled)
        {
            button2.Invoke(new MethodInvoker(delegate
            {
                button2.Enabled = enabled;
            }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
            {
                MessageBox.Show("You must choose a set file.");
                return;
            }
            if (textBox3.Text.Length == 0)
            {
                MessageBox.Show("You must enter a remote image directory.");
                return;
            }

            Util.imageDirectory = textBox3.Text;
            if (!Util.imageDirectory.EndsWith("/"))
                Util.imageDirectory += "/";
            server = new DraftServer(this, textBox2.Text);
            if (server.IsValidSet())
            {
                button1.Enabled = false;
                button3.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                server.PrintServerStartMessage();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PrintLine("Starting draft with " + server.aliases.Count + " players.");
            server.StartNextPack();
            button2.Enabled = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }
    }
}
