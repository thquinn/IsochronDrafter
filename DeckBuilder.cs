using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace IsochronDrafter
{
    public class DeckBuilder : Panel
    {
        public static readonly float SPACING_PERCENTAGE = .05f;
        public static readonly float CARD_HEADER_PERCENTAGE = .11f;
        public static readonly int CARD_WIDTH = 375;
        public static readonly int CARD_HEIGHT = 523;
        public static readonly int NUM_INITIAL_COLUMNS = 8;
        public static readonly int SIDEBOARD_SPACING_MULTIPLIER = 3;
        public static readonly int INTER_ROW_SPACING_MULTIPLIER = 3;
        public static readonly Color INDICATOR_COLOR = Color.Gold;
        public static readonly Color DRAGGED_STROKE_COLOR = Color.Gold;
        public static readonly int DRAGGED_STROKE_THICKNESS = 5;

        public DraftWindow draftWindow;
        public CardWindow cardWindow;
        private List<List<DeckBuilderCard>[]> columns;
        private DeckBuilderCard draggedCard = null;
        private int[] hoveredColumnRowNum = null;
        public PictureBox indicator;

        public DeckBuilder()
            : base()
        {
            AutoScroll = true;

            columns = new List<List<DeckBuilderCard>[]>();
            for (int i = 0; i < NUM_INITIAL_COLUMNS; i++)
            {
                columns.Add(new List<DeckBuilderCard>[2]);
                columns[i][0] = new List<DeckBuilderCard>();
                columns[i][1] = new List<DeckBuilderCard>();
            }

            // Make indicator.
            indicator = new PictureBox();
            indicator.BackColor = INDICATOR_COLOR;
            Controls.Add(indicator);
            indicator.Hide();
        }

        public void AddCard(String cardName)
        {
            DeckBuilderCard card = new DeckBuilderCard();
            card.SizeMode = PictureBoxSizeMode.Zoom;
            card.Image = DraftWindow.GetImage(cardName);
            card.cardName = cardName;

            columns[columns.Count - 1][0].Add(card);
            Controls.Add(card);
            LayoutControls();
        }
        public void Clear()
        {
            Controls.Clear();
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i] = new List<DeckBuilderCard>[2];
                columns[i][0] = new List<DeckBuilderCard>();
                columns[i][1] = new List<DeckBuilderCard>();
            }
        }
        public void SetNumColumns(int numColumns)
        {
            while (columns.Count > numColumns)
            {
                // Remove second-to-last column.
                List<DeckBuilderCard>[] column = columns[columns.Count - 2];
                columns.RemoveAt(columns.Count - 2);
                columns[columns.Count - 2][0].AddRange(column[0]);
                columns[columns.Count - 2][1].AddRange(column[1]);
            }
            while (columns.Count < numColumns)
            {
                // Add new second-to-last column.
                List<DeckBuilderCard>[] column = new List<DeckBuilderCard>[2];
                column[0] = new List<DeckBuilderCard>();
                column[1] = new List<DeckBuilderCard>();
                columns.Insert(columns.Count - 1, column);
            }
            LayoutControls();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            LayoutControls();
            base.OnResize(eventargs);
        }
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            LayoutControls();
            base.OnInvalidated(e);
        }
        private void LayoutControls()
        {
            if (VerticalScroll.Visible)
                AutoScrollMargin = new Size(0, System.Windows.Forms.SystemInformation.HorizontalScrollBarHeight);
            else
                AutoScrollMargin = new Size(0, 0);

            DeckBuilderLayout layout = new DeckBuilderLayout(this);

            for (int column = 0; column < columns.Count; column++)
                for (int row = 0; row < 2; row++)
                    for (int cardNum = 0; cardNum < columns[column][row].Count; cardNum++)
                    {
                        // Set location and size.
                        DeckBuilderCard card = columns[column][row][cardNum];
                        float x = layout.spacing * (column + 1) + (CARD_WIDTH * layout.scale * column);
                        float y = layout.spacing + (layout.headerSize * cardNum);
                        if (column == columns.Count - 1)
                        {
                            x += layout.spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1);
                            y += layout.spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1);
                        }
                        if (row == 1)
                            y += layout.secondRowY;
                        y -= VerticalScroll.Value;
                        card.Left = (int)Math.Round(x);
                        card.Top = (int)Math.Round(y);
                        card.Width = (int)Math.Round(CARD_WIDTH * layout.scale);
                        card.Height = (int)Math.Round(CARD_HEIGHT * layout.scale);

                        // Set child index.
                        Controls.SetChildIndex(card, columns[column][row].Count - cardNum);
                    }
            indicator.BringToFront();
            SetCardCounts();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DeckBuilderCard card = GetCardFromCoor(e.X, e.Y);
                if (card == null)
                    return;
                draggedCard = card;
                draggedCard.selected = true;
                draggedCard.Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle || e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                DeckBuilderCard card = GetCardFromCoor(e.X, e.Y);
                if (card == null)
                    return;

                // Reposition card form and draw.
                cardWindow.SetImage(DraftWindow.GetImage(card.cardName));
                float x = card.Left + card.Width / 2f;
                float y = card.Top + card.Height / 2f;
                Point point = PointToScreen(new Point((int)Math.Round(x), (int)Math.Round(y)));
                cardWindow.SetLocation(point);
                cardWindow.Show();
                Focus();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (draggedCard == null)
                return;
            int[] columnRowNum = GetColumnRowNumFromCoor(e.X, e.Y);
            // Check if the hovered area has changed.
            if (columnRowNum == null && hoveredColumnRowNum == null)
                return;
            if (columnRowNum != null && hoveredColumnRowNum != null && columnRowNum.SequenceEqual(hoveredColumnRowNum))
                return;
            // Toggle visibility of indicator or change position.
            hoveredColumnRowNum = columnRowNum;
            if (hoveredColumnRowNum == null)
                indicator.Hide();
            else
            {
                int column = hoveredColumnRowNum[0];
                int row = hoveredColumnRowNum[1];
                int cardNum = hoveredColumnRowNum[2];
                // If the hovered position is immediately before or after the dragged card, don't draw the indicator.
                int[] draggedColumnRowNum = GetColumnRowNumFromCard(draggedCard);
                if (column == draggedColumnRowNum[0] && row == draggedColumnRowNum[1] && (cardNum == draggedColumnRowNum[2] || cardNum == draggedColumnRowNum[2] + 1))
                {
                    indicator.Hide();
                }
                // Otherwise, draw the indicator.
                else
                {
                    DeckBuilderLayout layout = new DeckBuilderLayout(this);
                    // TODO: COPIED FROM LayoutControls()! Bad!
                    float x = layout.spacing * (column + 1) + (CARD_WIDTH * layout.scale * column);
                    float y = layout.spacing + (layout.headerSize * cardNum);
                    if (column == columns.Count - 1)
                    {
                        x += layout.spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1);
                        y += layout.spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1);
                    }
                    if (row == 1)
                        y += layout.secondRowY;
                    if (cardNum != 0 && cardNum == columns[column][row].Count)
                        y += CARD_HEIGHT * layout.scale - layout.headerSize;
                    y -= VerticalScroll.Value;
                    // END COPY
                    indicator.Left = (int)Math.Round(x - 2);
                    indicator.Top = (int)Math.Round(y - 1);
                    indicator.Width = (int)Math.Round(CARD_WIDTH * layout.scale + 4);
                    indicator.Height = 2;
                    indicator.Show();
                }
            }
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (draggedCard == null)
                    return;
                if (hoveredColumnRowNum != null)
                {
                    // Move dragged card in columns.
                    int[] draggedColumnRowNum = GetColumnRowNumFromCard(draggedCard);
                    if (draggedColumnRowNum[0] != hoveredColumnRowNum[0] || draggedColumnRowNum[1] != hoveredColumnRowNum[1])
                    {
                        columns[draggedColumnRowNum[0]][draggedColumnRowNum[1]].RemoveAt(draggedColumnRowNum[2]);
                        columns[hoveredColumnRowNum[0]][hoveredColumnRowNum[1]].Insert(hoveredColumnRowNum[2], draggedCard);
                    }
                    else
                    {
                        if (hoveredColumnRowNum[2] <= draggedColumnRowNum[2])
                        {
                            columns[draggedColumnRowNum[0]][draggedColumnRowNum[1]].RemoveAt(draggedColumnRowNum[2]);
                            columns[draggedColumnRowNum[0]][draggedColumnRowNum[1]].Insert(hoveredColumnRowNum[2], draggedCard);
                        }
                        else
                        {
                            columns[draggedColumnRowNum[0]][draggedColumnRowNum[1]].Insert(hoveredColumnRowNum[2], draggedCard);
                            columns[draggedColumnRowNum[0]][draggedColumnRowNum[1]].RemoveAt(draggedColumnRowNum[2]);
                        }
                    }
                }
                draggedCard.selected = false;
                draggedCard.Invalidate();
                draggedCard = null;
                hoveredColumnRowNum = null;
                indicator.Hide();
                Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle || e.Button == System.Windows.Forms.MouseButtons.Right)
                cardWindow.Hide();
        }
        private int[] GetColumnRowNumFromCoor(int x, int y)
        {
            DeckBuilderLayout layout = new DeckBuilderLayout(this);
            if (Math.Floor(x / (layout.scale * CARD_WIDTH + layout.spacing)) > columns.Count - 1)
                x -= (int)Math.Round(layout.spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1));
            if (x % (layout.scale * CARD_WIDTH + layout.spacing) < layout.spacing)
                return null;
            int column = (int)Math.Floor(x / (layout.scale * CARD_WIDTH + layout.spacing));
            if (column >= columns.Count)
                return null;
            bool isEmpty = GetMaxFirstRowLength() == 0;
            y += VerticalScroll.Value;
            int row = y < layout.secondRowY || isEmpty || column == columns.Count - 1 ? 0 : 1;
            int cardNum;
            if (columns[column][row].Count == 0) // Dragged card should get put as the first element in the now-empty column.
                cardNum = 0;
            else
            {
                if (column == columns.Count - 1)
                    y -= (int)Math.Round(layout.spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1));
                if (row == 1)
                    y -= (int)Math.Round(layout.secondRowY);
                y -= (int)Math.Round(layout.spacing);
                int count = columns[column][row].Count;
                if (y > (count - 1) * CARD_HEIGHT * layout.scale * CARD_HEADER_PERCENTAGE + CARD_HEIGHT * layout.scale)
                    return null;
                cardNum = (int)Math.Floor(y / (CARD_HEIGHT * layout.scale * CARD_HEADER_PERCENTAGE));
                if (cardNum < 0)
                    return null;
                if (cardNum > count)
                    cardNum = count;
            }
            return new int[] { column, row, cardNum };
        }
        private DeckBuilderCard GetCardFromCoor(int x, int y)
        {
            // TODO: This should be done with calculation instead of checking every card, but since there will usually be under 50 children of this control, it's fine for now.
            for (int column = 0; column < columns.Count; column++)
                for (int row = 0; row < 2; row++)
                    for (int cardNum = columns[column][row].Count - 1; cardNum >= 0; cardNum--)
                    {
                        DeckBuilderCard card = columns[column][row][cardNum];
                        if (x >= card.Left && x <= card.Right && y >= card.Top && y <= card.Bottom)
                            return card;
                    }
            return null;
        }
        private int[] GetColumnRowNumFromCard(DeckBuilderCard card)
        {
            for (int column = 0; column < columns.Count; column++)
                for (int row = 0; row < 2; row++)
                    for (int cardNum = 0; cardNum < columns[column][row].Count; cardNum++)
                        if (columns[column][row][cardNum] == card)
                            return new int[] { column, row, cardNum };
            return null;
        }

        public int ColumnCount()
        {
            return columns.Count;
        }
        public int GetMaxFirstRowLength()
        {
            int output = 0;
            for (int i = 0; i < columns.Count - 1; i++)
                if (columns[i][0].Count > output)
                    output = columns[i][0].Count;
            return output;
        }

        public void SetCardCounts()
        {
            int maindeck = 0;
            for (int column = 0; column < columns.Count - 1; column++)
                for (int row = 0; row < 2; row++)
                    for (int cardNum = 0; cardNum < columns[column][row].Count; cardNum++)
                        maindeck++;
            int sideboard = 0;
            for (int row = 0; row < 2; row++)
                for (int cardNum = 0; cardNum < columns[columns.Count - 1][row].Count; cardNum++)
                    sideboard++;
            if (draftWindow != null && maindeck + sideboard > 0)
                draftWindow.SetCardCounts(maindeck, sideboard);
        }

        public String GetCockatriceDeck()
        {
            Dictionary<string, int> quantities = new Dictionary<string, int>();
            Dictionary<string, int> sideboardQuantities = new Dictionary<string, int>();
            for (int column = 0; column < columns.Count; column++)
                for (int row = 0; row < 2; row++)
                    for (int cardNum = 0; cardNum < columns[column][row].Count; cardNum++)
                    {
                        Dictionary<string, int> dictionary = column == columns.Count - 1 ? sideboardQuantities : quantities;
                        string cardName = columns[column][row][cardNum].cardName;
                        if (dictionary.ContainsKey(cardName))
                            dictionary[cardName]++;
                        else
                            dictionary.Add(cardName, 1);
                    }
            string output = "";
            foreach (KeyValuePair<string, int> kvp in quantities)
                output += "\r\n" + kvp.Value + " " + kvp.Key;
            foreach (KeyValuePair<string, int> kvp in sideboardQuantities)
                output += "\r\nSB: " + kvp.Value + " " + kvp.Key;
            return output.Trim();
        }
    }

    internal class DeckBuilderCard : PictureBox
    {
        public string cardName;
        public bool selected = false;

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            base.OnPaint(paintEventArgs);
            if (selected)
            {
                ControlPaint.DrawBorder(paintEventArgs.Graphics, ClientRectangle,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = (-1);

            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }

    internal class DeckBuilderLayout
    {
        public float scale, spacing, headerSize, secondRowY;

        public DeckBuilderLayout(DeckBuilder deckBuilder)
        {
            int columnCount = deckBuilder.ColumnCount();
            float usableWidth = deckBuilder.ClientSize.Width;
            scale = (usableWidth * (1 - DeckBuilder.SPACING_PERCENTAGE) / columnCount) / DeckBuilder.CARD_WIDTH;
            spacing = (usableWidth * DeckBuilder.SPACING_PERCENTAGE) / (columnCount + 1 + (DeckBuilder.SIDEBOARD_SPACING_MULTIPLIER - 1) * 2);
            headerSize = DeckBuilder.CARD_HEIGHT * scale * DeckBuilder.CARD_HEADER_PERCENTAGE;
            int maxFirstRowLength = deckBuilder.GetMaxFirstRowLength();
            secondRowY = (spacing * DeckBuilder.INTER_ROW_SPACING_MULTIPLIER - 1) + (headerSize * (maxFirstRowLength - 1)) + (DeckBuilder.CARD_HEIGHT * scale);
        }
    }
}
