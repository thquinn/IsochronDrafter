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
    public partial class CardWindow : Form
    {
        public CardWindow()
        {
            InitializeComponent();
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        public void SetImage(Image image)
        {
            Width = image.Width;
            Height = image.Height;
            pictureBox1.Image = image;
        }
    }
}
