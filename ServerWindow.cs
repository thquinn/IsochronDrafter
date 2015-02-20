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
            InitializeComponent();
            MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                server = new DraftServer(this, openFileDialog1.FileName);
                button1.Enabled = false;
            }
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
