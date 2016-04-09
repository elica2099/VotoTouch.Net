namespace VotoTouch
{
    partial class LabelCandidati
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label_note = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 160);
            this.label1.TabIndex = 0;
            this.label1.Text = "label3_candidati";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(228, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(219, 160);
            this.label2.TabIndex = 1;
            this.label2.Text = "label2_candidati";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(439, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(219, 160);
            this.label3.TabIndex = 2;
            this.label3.Text = "label3_candidati";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label_note
            // 
            this.label_note.Location = new System.Drawing.Point(1, 167);
            this.label_note.Name = "label_note";
            this.label_note.Size = new System.Drawing.Size(660, 35);
            this.label_note.TabIndex = 3;
            this.label_note.Text = "label_note";
            // 
            // LabelCandidati
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label_note);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "LabelCandidati";
            this.Size = new System.Drawing.Size(661, 202);
            this.Resize += new System.EventHandler(this.LabelCandidati_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_note;
    }
}
