namespace IsochronDrafter
{
    partial class DraftWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusTextBox = new System.Windows.Forms.TextBox();
            this.draftPicker = new IsochronDrafter.DraftPicker();
            this.deckBuilder = new IsochronDrafter.DeckBuilder();
            this.SuspendLayout();
            // 
            // statusTextBox
            // 
            this.statusTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.statusTextBox.Location = new System.Drawing.Point(918, 518);
            this.statusTextBox.Multiline = true;
            this.statusTextBox.Name = "statusTextBox";
            this.statusTextBox.ReadOnly = true;
            this.statusTextBox.Size = new System.Drawing.Size(344, 466);
            this.statusTextBox.TabIndex = 5;
            // 
            // draftPicker
            // 
            this.draftPicker.BackColor = System.Drawing.SystemColors.Window;
            this.draftPicker.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.draftPicker.Location = new System.Drawing.Point(12, 12);
            this.draftPicker.Name = "draftPicker";
            this.draftPicker.Size = new System.Drawing.Size(1250, 500);
            this.draftPicker.TabIndex = 3;
            this.draftPicker.DoubleClick += new System.EventHandler(this.draftPicker1_DoubleClick);
            // 
            // deckBuilder
            // 
            this.deckBuilder.AutoScroll = true;
            this.deckBuilder.BackColor = System.Drawing.SystemColors.Window;
            this.deckBuilder.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.deckBuilder.Location = new System.Drawing.Point(12, 518);
            this.deckBuilder.Name = "deckBuilder";
            this.deckBuilder.Size = new System.Drawing.Size(900, 466);
            this.deckBuilder.TabIndex = 6;
            // 
            // DraftWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1274, 996);
            this.Controls.Add(this.deckBuilder);
            this.Controls.Add(this.statusTextBox);
            this.Controls.Add(this.draftPicker);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DraftWindow";
            this.Text = "Isochron Drafter";
            this.Load += new System.EventHandler(this.DraftWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox statusTextBox;
        private DraftPicker draftPicker;
        private DeckBuilder deckBuilder;
    }
}

