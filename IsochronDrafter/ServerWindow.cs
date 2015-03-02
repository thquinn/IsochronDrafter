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
            int packs, commons, uncommons, rares;
            float mythicPercentage;
            if (!int.TryParse(textBox8.Text, out packs) || packs < 0)
            {
                MessageBox.Show("You must enter a positive integer number of packs.");
                return;
            }
            if (!int.TryParse(textBox4.Text, out commons) || commons < 0)
            {
                MessageBox.Show("You must enter a positive integer number of commons.");
                return;
            }
            if (!int.TryParse(textBox5.Text, out uncommons) || uncommons < 0)
            {
                MessageBox.Show("You must enter a positive integer number of uncommons.");
                return;
            }
            if (!int.TryParse(textBox6.Text, out rares) || rares < 0)
            {
                MessageBox.Show("You must enter a positive integer number of rares.");
                return;
            }
            if (!float.TryParse(textBox7.Text, out mythicPercentage) || mythicPercentage < 0 || mythicPercentage > 1)
            {
                MessageBox.Show("You must enter a mythic percentage between 0 and 1.");
                return;
            }
            Util.imageDirectory = textBox3.Text;
            if (!Util.imageDirectory.EndsWith("/"))
                Util.imageDirectory += "/";
            server = new DraftServer(this, textBox2.Text, packs, commons, uncommons, rares, mythicPercentage);
            if (server.IsValidSet())
            {
                button1.Enabled = false;
                button3.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;
                textBox8.Enabled = false;
                server.PrintServerStartMessage();
            }
            else
                server.server.Close();
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
