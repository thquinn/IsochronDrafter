using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IsochronDrafter
{
    public partial class DraftPicker : UserControl
    {
        private static readonly float SPACING_PERCENTAGE = .05f;
        private static readonly int CARD_WIDTH = 375;
        private static readonly int CARD_HEIGHT = 523;
        public List<String> cardNames = new List<string>();
        public CardWindow cardWindow;
        private float scale, spacing;
        private int perRow;

        public DraftPicker()
        {
            InitializeComponent();
        }

        public void Populate(List<String> cardNames)
        {
            this.cardNames = cardNames;
            foreach (String cardName in cardNames)
                DraftWindow.LoadImage(cardName);
            Invalidate();
        }
        public void Clear()
        {
            cardNames = new List<string>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (cardNames.Count == 0)
                return;

            // Calculate size of each card.
            float usableWidth = ClientSize.Width * (1 - SPACING_PERCENTAGE);
            float usableHeight = ClientSize.Height * (1 - SPACING_PERCENTAGE);
            float currentMaxScale = 0, currentTestScale = 1;
            for (int i = 0; i < 20; i++)
            {
                int rows = (int)Math.Floor(usableWidth / (CARD_WIDTH * currentTestScale));
                int cols = (int)Math.Floor(usableHeight / (CARD_HEIGHT * currentTestScale));
                if (rows * cols < cardNames.Count())
                    currentTestScale = (currentMaxScale + currentTestScale) / 2;
                else
                {
                    float nextTestScale = currentTestScale + (currentTestScale - currentMaxScale) / 2;
                    currentMaxScale = currentTestScale;
                    currentTestScale = nextTestScale;
                }
            }
            scale = currentMaxScale;

            perRow = (int)Math.Floor(usableWidth / (CARD_WIDTH * scale));
            spacing = (ClientSize.Width * SPACING_PERCENTAGE) / (perRow + 1);
            for (int i = 0; i < cardNames.Count; i++)
            {
                int row = i / perRow, col = i % perRow;
                float x = col * CARD_WIDTH * scale + (col + 1) * spacing;
                float y = row * CARD_HEIGHT * scale + (row + 1) * spacing;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(DraftWindow.GetImage(cardNames[i]), new RectangleF(x, y, CARD_WIDTH * scale, CARD_HEIGHT * scale));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Middle || e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Find which card was clicked.
                int i = GetIndexFromCoor(e.X, e.Y);
                if (i == -1)
                    return;

                // Reposition card form and draw.
                cardWindow.SetImage(DraftWindow.GetImage(cardNames[i]));
                float x = (i % perRow) * (spacing + CARD_WIDTH * scale) + spacing + (CARD_WIDTH * scale / 2);
                float y = (i / perRow) * (spacing + CARD_HEIGHT * scale) + spacing + (CARD_HEIGHT * scale / 2);
                Point point = PointToScreen(new Point((int)Math.Round(x), (int)Math.Round(y)));
                cardWindow.SetLocation(point);
                cardWindow.Show();
                Focus();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Middle || e.Button == System.Windows.Forms.MouseButtons.Right)
                cardWindow.Hide();
        }

        public int GetIndexFromCoor(int x, int y)
        {
            if (x % (spacing + CARD_WIDTH * scale) < spacing)
                return -1;
            if (y % (spacing + CARD_HEIGHT * scale) < spacing)
                return -1;
            int col = (int)Math.Floor(x / (spacing + CARD_WIDTH * scale));
            int row = (int)Math.Floor(y / (spacing + CARD_HEIGHT * scale));
            int i = row * perRow + col;
            if (i < 0 || i >= cardNames.Count)
                return -1;
            return i;
        }
    }
}
