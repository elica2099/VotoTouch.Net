namespace VotoTouch
{
    partial class frmConfig
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("wqwrqwerqw");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConfig));
            this.grbLettore = new System.Windows.Forms.GroupBox();
            this.txtProva = new System.Windows.Forms.TextBox();
            this.btnNoLettore = new System.Windows.Forms.Button();
            this.btnAssegna = new System.Windows.Forms.Button();
            this.btnSalvaDB = new System.Windows.Forms.Button();
            this.btnAggiorna = new System.Windows.Forms.Button();
            this.btnChiudi = new System.Windows.Forms.Button();
            this.lvSeriali = new System.Windows.Forms.ListView();
            this.PortaCom = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Assegnata = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Tipo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.grbSemaforo = new System.Windows.Forms.GroupBox();
            this.btnNoSemaforo = new System.Windows.Forms.Button();
            this.btnSemAssegnaIP = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSemIP = new System.Windows.Forms.TextBox();
            this.btnSErrore = new System.Windows.Forms.Button();
            this.btnSFineOcc = new System.Windows.Forms.Button();
            this.btnSOccupato = new System.Windows.Forms.Button();
            this.btnLibero = new System.Windows.Forms.Button();
            this.btnAssegnaSem = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.grbLettore.SuspendLayout();
            this.grbSemaforo.SuspendLayout();
            this.SuspendLayout();
            // 
            // grbLettore
            // 
            this.grbLettore.Controls.Add(this.txtProva);
            this.grbLettore.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbLettore.Location = new System.Drawing.Point(15, 349);
            this.grbLettore.Name = "grbLettore";
            this.grbLettore.Size = new System.Drawing.Size(478, 75);
            this.grbLettore.TabIndex = 1;
            this.grbLettore.TabStop = false;
            this.grbLettore.Text = " Prova Lettore Barcode ";
            // 
            // txtProva
            // 
            this.txtProva.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProva.Location = new System.Drawing.Point(173, 23);
            this.txtProva.Name = "txtProva";
            this.txtProva.Size = new System.Drawing.Size(267, 40);
            this.txtProva.TabIndex = 8;
            // 
            // btnNoLettore
            // 
            this.btnNoLettore.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNoLettore.Location = new System.Drawing.Point(516, 272);
            this.btnNoLettore.Name = "btnNoLettore";
            this.btnNoLettore.Size = new System.Drawing.Size(192, 57);
            this.btnNoLettore.TabIndex = 12;
            this.btnNoLettore.Text = "Libera Porta";
            this.btnNoLettore.UseVisualStyleBackColor = true;
            this.btnNoLettore.Click += new System.EventHandler(this.btnNoLettore_Click);
            // 
            // btnAssegna
            // 
            this.btnAssegna.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAssegna.Location = new System.Drawing.Point(14, 273);
            this.btnAssegna.Name = "btnAssegna";
            this.btnAssegna.Size = new System.Drawing.Size(192, 57);
            this.btnAssegna.TabIndex = 11;
            this.btnAssegna.Text = "Assegna a Barcode";
            this.btnAssegna.UseVisualStyleBackColor = true;
            this.btnAssegna.Click += new System.EventHandler(this.btnAssegna_Click);
            // 
            // btnSalvaDB
            // 
            this.btnSalvaDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalvaDB.Location = new System.Drawing.Point(14, 631);
            this.btnSalvaDB.Name = "btnSalvaDB";
            this.btnSalvaDB.Size = new System.Drawing.Size(273, 58);
            this.btnSalvaDB.TabIndex = 10;
            this.btnSalvaDB.Text = "Salva Config su DB";
            this.btnSalvaDB.UseVisualStyleBackColor = true;
            this.btnSalvaDB.Click += new System.EventHandler(this.btnSalvaDB_Click);
            // 
            // btnAggiorna
            // 
            this.btnAggiorna.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAggiorna.Location = new System.Drawing.Point(761, 273);
            this.btnAggiorna.Name = "btnAggiorna";
            this.btnAggiorna.Size = new System.Drawing.Size(64, 56);
            this.btnAggiorna.TabIndex = 6;
            this.btnAggiorna.Text = "Aggiorna Com";
            this.btnAggiorna.UseVisualStyleBackColor = true;
            this.btnAggiorna.Click += new System.EventHandler(this.btnAggiorna_Click);
            // 
            // btnChiudi
            // 
            this.btnChiudi.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChiudi.Location = new System.Drawing.Point(583, 632);
            this.btnChiudi.Name = "btnChiudi";
            this.btnChiudi.Size = new System.Drawing.Size(223, 57);
            this.btnChiudi.TabIndex = 2;
            this.btnChiudi.Text = "Chiudi";
            this.btnChiudi.UseVisualStyleBackColor = true;
            this.btnChiudi.Click += new System.EventHandler(this.btnChiudi_Click);
            // 
            // lvSeriali
            // 
            this.lvSeriali.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.PortaCom,
            this.Assegnata,
            this.Tipo});
            this.lvSeriali.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvSeriali.FullRowSelect = true;
            this.lvSeriali.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSeriali.HideSelection = false;
            this.lvSeriali.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.lvSeriali.Location = new System.Drawing.Point(14, 11);
            this.lvSeriali.MultiSelect = false;
            this.lvSeriali.Name = "lvSeriali";
            this.lvSeriali.Size = new System.Drawing.Size(811, 251);
            this.lvSeriali.TabIndex = 18;
            this.lvSeriali.UseCompatibleStateImageBehavior = false;
            this.lvSeriali.View = System.Windows.Forms.View.Details;
            this.lvSeriali.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.lvSeriali_DrawColumnHeader);
            // 
            // PortaCom
            // 
            this.PortaCom.Text = "Com";
            this.PortaCom.Width = 120;
            // 
            // Assegnata
            // 
            this.Assegnata.Text = "Assegnata a ";
            this.Assegnata.Width = 230;
            // 
            // Tipo
            // 
            this.Tipo.Text = "Tipo";
            this.Tipo.Width = 400;
            // 
            // grbSemaforo
            // 
            this.grbSemaforo.Controls.Add(this.btnNoSemaforo);
            this.grbSemaforo.Controls.Add(this.btnSemAssegnaIP);
            this.grbSemaforo.Controls.Add(this.label3);
            this.grbSemaforo.Controls.Add(this.txtSemIP);
            this.grbSemaforo.Controls.Add(this.btnSErrore);
            this.grbSemaforo.Controls.Add(this.btnSFineOcc);
            this.grbSemaforo.Controls.Add(this.btnSOccupato);
            this.grbSemaforo.Controls.Add(this.btnLibero);
            this.grbSemaforo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbSemaforo.Location = new System.Drawing.Point(14, 430);
            this.grbSemaforo.Name = "grbSemaforo";
            this.grbSemaforo.Size = new System.Drawing.Size(811, 179);
            this.grbSemaforo.TabIndex = 3;
            this.grbSemaforo.TabStop = false;
            this.grbSemaforo.Text = "Semaforo";
            // 
            // btnNoSemaforo
            // 
            this.btnNoSemaforo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNoSemaforo.Location = new System.Drawing.Point(174, 89);
            this.btnNoSemaforo.Name = "btnNoSemaforo";
            this.btnNoSemaforo.Size = new System.Drawing.Size(125, 79);
            this.btnNoSemaforo.TabIndex = 21;
            this.btnNoSemaforo.Text = "Nessun Semaforo";
            this.btnNoSemaforo.UseVisualStyleBackColor = true;
            this.btnNoSemaforo.Visible = false;
            // 
            // btnSemAssegnaIP
            // 
            this.btnSemAssegnaIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSemAssegnaIP.Location = new System.Drawing.Point(26, 89);
            this.btnSemAssegnaIP.Name = "btnSemAssegnaIP";
            this.btnSemAssegnaIP.Size = new System.Drawing.Size(125, 79);
            this.btnSemAssegnaIP.TabIndex = 20;
            this.btnSemAssegnaIP.Text = "Assegna IP a Semaforo";
            this.btnSemAssegnaIP.UseVisualStyleBackColor = true;
            this.btnSemAssegnaIP.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(22, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 24);
            this.label3.TabIndex = 10;
            this.label3.Text = "IP:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSemIP
            // 
            this.txtSemIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSemIP.Location = new System.Drawing.Point(64, 31);
            this.txtSemIP.Name = "txtSemIP";
            this.txtSemIP.Size = new System.Drawing.Size(235, 40);
            this.txtSemIP.TabIndex = 9;
            // 
            // btnSErrore
            // 
            this.btnSErrore.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSErrore.Location = new System.Drawing.Point(598, 101);
            this.btnSErrore.Name = "btnSErrore";
            this.btnSErrore.Size = new System.Drawing.Size(187, 61);
            this.btnSErrore.TabIndex = 3;
            this.btnSErrore.Text = "Semaforo Errore";
            this.btnSErrore.UseVisualStyleBackColor = true;
            this.btnSErrore.Click += new System.EventHandler(this.btnSErrore_Click);
            // 
            // btnSFineOcc
            // 
            this.btnSFineOcc.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSFineOcc.Location = new System.Drawing.Point(598, 23);
            this.btnSFineOcc.Name = "btnSFineOcc";
            this.btnSFineOcc.Size = new System.Drawing.Size(187, 61);
            this.btnSFineOcc.TabIndex = 2;
            this.btnSFineOcc.Text = "Semaforo Fine Occupato";
            this.btnSFineOcc.UseVisualStyleBackColor = true;
            this.btnSFineOcc.Click += new System.EventHandler(this.btnSFineOcc_Click);
            // 
            // btnSOccupato
            // 
            this.btnSOccupato.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSOccupato.Location = new System.Drawing.Point(376, 101);
            this.btnSOccupato.Name = "btnSOccupato";
            this.btnSOccupato.Size = new System.Drawing.Size(187, 61);
            this.btnSOccupato.TabIndex = 1;
            this.btnSOccupato.Text = "Semaforo Occupato";
            this.btnSOccupato.UseVisualStyleBackColor = true;
            this.btnSOccupato.Click += new System.EventHandler(this.btnSOccupato_Click);
            // 
            // btnLibero
            // 
            this.btnLibero.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLibero.Location = new System.Drawing.Point(376, 23);
            this.btnLibero.Name = "btnLibero";
            this.btnLibero.Size = new System.Drawing.Size(187, 61);
            this.btnLibero.TabIndex = 0;
            this.btnLibero.Text = "Semaforo Libero";
            this.btnLibero.UseVisualStyleBackColor = true;
            this.btnLibero.Click += new System.EventHandler(this.btnLibero_Click);
            // 
            // btnAssegnaSem
            // 
            this.btnAssegnaSem.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAssegnaSem.Location = new System.Drawing.Point(263, 272);
            this.btnAssegnaSem.Name = "btnAssegnaSem";
            this.btnAssegnaSem.Size = new System.Drawing.Size(192, 57);
            this.btnAssegnaSem.TabIndex = 19;
            this.btnAssegnaSem.Text = "Assegna a Semaforo Seriale";
            this.btnAssegnaSem.UseVisualStyleBackColor = true;
            this.btnAssegnaSem.Click += new System.EventHandler(this.btnAssegnaSem_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(516, 357);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(290, 70);
            this.label2.TabIndex = 20;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // frmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 700);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSalvaDB);
            this.Controls.Add(this.btnAssegnaSem);
            this.Controls.Add(this.btnNoLettore);
            this.Controls.Add(this.lvSeriali);
            this.Controls.Add(this.btnAggiorna);
            this.Controls.Add(this.btnAssegna);
            this.Controls.Add(this.grbSemaforo);
            this.Controls.Add(this.btnChiudi);
            this.Controls.Add(this.grbLettore);
            this.Name = "frmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configurazione Totem";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.Shown += new System.EventHandler(this.frmConfig_Shown);
            this.grbLettore.ResumeLayout(false);
            this.grbLettore.PerformLayout();
            this.grbSemaforo.ResumeLayout(false);
            this.grbSemaforo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grbLettore;
        private System.Windows.Forms.Button btnChiudi;
        private System.Windows.Forms.Button btnAggiorna;
        private System.Windows.Forms.Button btnSalvaDB;
        private System.Windows.Forms.TextBox txtProva;
        private System.Windows.Forms.ListView lvSeriali;
        private System.Windows.Forms.ColumnHeader PortaCom;
        private System.Windows.Forms.ColumnHeader Assegnata;
        private System.Windows.Forms.GroupBox grbSemaforo;
        private System.Windows.Forms.Button btnSErrore;
        private System.Windows.Forms.Button btnSFineOcc;
        private System.Windows.Forms.Button btnSOccupato;
        private System.Windows.Forms.Button btnLibero;
        private System.Windows.Forms.Button btnAssegna;
        private System.Windows.Forms.Button btnNoLettore;
        private System.Windows.Forms.Button btnAssegnaSem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSemIP;
        private System.Windows.Forms.Button btnNoSemaforo;
        private System.Windows.Forms.Button btnSemAssegnaIP;
        private System.Windows.Forms.ColumnHeader Tipo;
    }
}