namespace VotoTouch
{
    partial class FTest
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
            this.btnTestVideate = new System.Windows.Forms.Button();
            this.btnTestAssemblea = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTestVideate
            // 
            this.btnTestVideate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestVideate.Location = new System.Drawing.Point(13, 13);
            this.btnTestVideate.Name = "btnTestVideate";
            this.btnTestVideate.Size = new System.Drawing.Size(155, 77);
            this.btnTestVideate.TabIndex = 0;
            this.btnTestVideate.Text = "Test Videate";
            this.btnTestVideate.UseVisualStyleBackColor = true;
            this.btnTestVideate.Click += new System.EventHandler(this.btnTestVideate_Click);
            // 
            // btnTestAssemblea
            // 
            this.btnTestAssemblea.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestAssemblea.Location = new System.Drawing.Point(221, 13);
            this.btnTestAssemblea.Name = "btnTestAssemblea";
            this.btnTestAssemblea.Size = new System.Drawing.Size(155, 77);
            this.btnTestAssemblea.TabIndex = 1;
            this.btnTestAssemblea.Text = "Test Assemblea";
            this.btnTestAssemblea.UseVisualStyleBackColor = true;
            this.btnTestAssemblea.Click += new System.EventHandler(this.btnTestAssemblea_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(429, 13);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(155, 77);
            this.button2.TabIndex = 2;
            this.button2.Text = "Test Videate";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            // 
            // FTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 102);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnTestAssemblea);
            this.Controls.Add(this.btnTestVideate);
            this.Name = "FTest";
            this.Text = "FTest";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTestVideate;
        private System.Windows.Forms.Button btnTestAssemblea;
        private System.Windows.Forms.Button button2;
    }
}