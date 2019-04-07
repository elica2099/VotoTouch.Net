﻿using System;
using System.Resources;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Threading;
using System.Media;
using System.Reflection;

namespace VotoTouch
{
    partial class frmMain : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        //private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.lbDirittiDiVoto = new System.Windows.Forms.Label();
            this.lbDirittiStart = new System.Windows.Forms.Label();
            this.lbConferma_old2 = new System.Windows.Forms.Label();
            this.lbConfermaNVoti = new System.Windows.Forms.Label();
            this.lbNomeDisgiunto = new System.Windows.Forms.Label();
            this.lbDisgiuntoRimangono = new System.Windows.Forms.Label();
            this.Panel4 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.btnCancVoti = new System.Windows.Forms.Button();
            this.btnRicaricaListe = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCloseInfo = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lbVersion = new System.Windows.Forms.ListBox();
            this.pnBadge = new System.Windows.Forms.Panel();
            this.btnAbilitaDifferenziata = new System.Windows.Forms.Button();
            this.btnRipetiz = new System.Windows.Forms.Button();
            this.btnExitVoto = new System.Windows.Forms.Button();
            this.btmBadge = new System.Windows.Forms.Button();
            this.edtBadge = new System.Windows.Forms.TextBox();
            this.timVotoApero = new System.Windows.Forms.Timer(this.components);
            this.pbSalvaDati = new System.Windows.Forms.PictureBox();
            this.lbConfermaUp = new System.Windows.Forms.Label();
            this.lbNomeAzStart = new System.Windows.Forms.Label();
            this.imgERSemaf = new System.Windows.Forms.PictureBox();
            this.imgERBarcode = new System.Windows.Forms.PictureBox();
            this.labelMousee = new System.Windows.Forms.Label();
            this.pnPopupRed = new System.Windows.Forms.Panel();
            this.lblMsgPopup = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.Panel4.SuspendLayout();
            this.pnBadge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSalvaDati)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgERSemaf)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgERBarcode)).BeginInit();
            this.pnPopupRed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lbDirittiDiVoto
            // 
            this.lbDirittiDiVoto.BackColor = System.Drawing.Color.Transparent;
            this.lbDirittiDiVoto.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDirittiDiVoto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(0)))), ((int)(((byte)(3)))));
            this.lbDirittiDiVoto.Location = new System.Drawing.Point(96, 356);
            this.lbDirittiDiVoto.Name = "lbDirittiDiVoto";
            this.lbDirittiDiVoto.Size = new System.Drawing.Size(104, 48);
            this.lbDirittiDiVoto.TabIndex = 69;
            this.lbDirittiDiVoto.Text = "9";
            this.lbDirittiDiVoto.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbDirittiDiVoto.Visible = false;
            // 
            // lbDirittiStart
            // 
            this.lbDirittiStart.BackColor = System.Drawing.Color.Transparent;
            this.lbDirittiStart.Font = new System.Drawing.Font("Tahoma", 99.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDirittiStart.ForeColor = System.Drawing.Color.Firebrick;
            this.lbDirittiStart.Location = new System.Drawing.Point(26, 150);
            this.lbDirittiStart.Name = "lbDirittiStart";
            this.lbDirittiStart.Size = new System.Drawing.Size(248, 165);
            this.lbDirittiStart.TabIndex = 77;
            this.lbDirittiStart.Text = "10";
            this.lbDirittiStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbDirittiStart.Visible = false;
            // 
            // lbConferma_old2
            // 
            this.lbConferma_old2.BackColor = System.Drawing.Color.Transparent;
            this.lbConferma_old2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConferma_old2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbConferma_old2.Location = new System.Drawing.Point(285, 269);
            this.lbConferma_old2.Name = "lbConferma_old2";
            this.lbConferma_old2.Size = new System.Drawing.Size(197, 46);
            this.lbConferma_old2.TabIndex = 91;
            this.lbConferma_old2.Text = "lbConferma_old";
            this.lbConferma_old2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbConferma_old2.Visible = false;
            // 
            // lbConfermaNVoti
            // 
            this.lbConfermaNVoti.BackColor = System.Drawing.Color.Cyan;
            this.lbConfermaNVoti.Font = new System.Drawing.Font("Arial", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConfermaNVoti.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(0)))), ((int)(((byte)(3)))));
            this.lbConfermaNVoti.Location = new System.Drawing.Point(390, 502);
            this.lbConfermaNVoti.Name = "lbConfermaNVoti";
            this.lbConfermaNVoti.Size = new System.Drawing.Size(239, 56);
            this.lbConfermaNVoti.TabIndex = 92;
            this.lbConfermaNVoti.Text = "9999 voti";
            this.lbConfermaNVoti.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbConfermaNVoti.Visible = false;
            // 
            // lbNomeDisgiunto
            // 
            this.lbNomeDisgiunto.BackColor = System.Drawing.Color.Transparent;
            this.lbNomeDisgiunto.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNomeDisgiunto.ForeColor = System.Drawing.Color.Black;
            this.lbNomeDisgiunto.Location = new System.Drawing.Point(286, 315);
            this.lbNomeDisgiunto.Name = "lbNomeDisgiunto";
            this.lbNomeDisgiunto.Size = new System.Drawing.Size(200, 51);
            this.lbNomeDisgiunto.TabIndex = 107;
            this.lbNomeDisgiunto.Text = "lbNomeDisgiunto";
            this.lbNomeDisgiunto.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbNomeDisgiunto.Visible = false;
            // 
            // lbDisgiuntoRimangono
            // 
            this.lbDisgiuntoRimangono.AutoSize = true;
            this.lbDisgiuntoRimangono.BackColor = System.Drawing.Color.Transparent;
            this.lbDisgiuntoRimangono.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDisgiuntoRimangono.ForeColor = System.Drawing.Color.Black;
            this.lbDisgiuntoRimangono.Location = new System.Drawing.Point(206, 410);
            this.lbDisgiuntoRimangono.Name = "lbDisgiuntoRimangono";
            this.lbDisgiuntoRimangono.Size = new System.Drawing.Size(142, 27);
            this.lbDisgiuntoRimangono.TabIndex = 108;
            this.lbDisgiuntoRimangono.Text = "Rimangono:";
            this.lbDisgiuntoRimangono.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbDisgiuntoRimangono.Visible = false;
            // 
            // Panel4
            // 
            this.Panel4.BackColor = System.Drawing.Color.White;
            this.Panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Panel4.Controls.Add(this.button2);
            this.Panel4.Controls.Add(this.btnCancVoti);
            this.Panel4.Controls.Add(this.btnRicaricaListe);
            this.Panel4.Controls.Add(this.label1);
            this.Panel4.Controls.Add(this.btnCloseInfo);
            this.Panel4.Controls.Add(this.button1);
            this.Panel4.Controls.Add(this.lbVersion);
            this.Panel4.Location = new System.Drawing.Point(519, 12);
            this.Panel4.Name = "Panel4";
            this.Panel4.Size = new System.Drawing.Size(300, 525);
            this.Panel4.TabIndex = 118;
            this.Panel4.Visible = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(112, 493);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 130;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnCancVoti
            // 
            this.btnCancVoti.BackColor = System.Drawing.Color.White;
            this.btnCancVoti.Location = new System.Drawing.Point(117, 450);
            this.btnCancVoti.Name = "btnCancVoti";
            this.btnCancVoti.Size = new System.Drawing.Size(64, 31);
            this.btnCancVoti.TabIndex = 129;
            this.btnCancVoti.Text = "Canc Voti";
            this.btnCancVoti.UseVisualStyleBackColor = false;
            this.btnCancVoti.Click += new System.EventHandler(this.btnCancVoti_Click);
            // 
            // btnRicaricaListe
            // 
            this.btnRicaricaListe.Location = new System.Drawing.Point(9, 447);
            this.btnRicaricaListe.Name = "btnRicaricaListe";
            this.btnRicaricaListe.Size = new System.Drawing.Size(100, 63);
            this.btnRicaricaListe.TabIndex = 4;
            this.btnRicaricaListe.Text = "Ricarica Liste/Votazioni";
            this.btnRicaricaListe.UseVisualStyleBackColor = true;
            this.btnRicaricaListe.Click += new System.EventHandler(this.btnRicaricaListe_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Informazioni sulla Versione";
            // 
            // btnCloseInfo
            // 
            this.btnCloseInfo.Location = new System.Drawing.Point(167, 6);
            this.btnCloseInfo.Name = "btnCloseInfo";
            this.btnCloseInfo.Size = new System.Drawing.Size(114, 26);
            this.btnCloseInfo.TabIndex = 2;
            this.btnCloseInfo.Text = "Chiudi pannello";
            this.btnCloseInfo.UseVisualStyleBackColor = true;
            this.btnCloseInfo.Click += new System.EventHandler(this.btnCloseInfo_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(188, 447);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 65);
            this.button1.TabIndex = 1;
            this.button1.Text = "Chiudi Applicazione";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // lbVersion
            // 
            this.lbVersion.FormattingEnabled = true;
            this.lbVersion.Location = new System.Drawing.Point(1, 33);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(294, 394);
            this.lbVersion.TabIndex = 0;
            // 
            // pnBadge
            // 
            this.pnBadge.BackColor = System.Drawing.Color.Transparent;
            this.pnBadge.Controls.Add(this.btnAbilitaDifferenziata);
            this.pnBadge.Controls.Add(this.btnRipetiz);
            this.pnBadge.Controls.Add(this.btnExitVoto);
            this.pnBadge.Controls.Add(this.btmBadge);
            this.pnBadge.Controls.Add(this.edtBadge);
            this.pnBadge.Location = new System.Drawing.Point(15, 114);
            this.pnBadge.Name = "pnBadge";
            this.pnBadge.Size = new System.Drawing.Size(151, 117);
            this.pnBadge.TabIndex = 124;
            this.pnBadge.Visible = false;
            // 
            // btnAbilitaDifferenziata
            // 
            this.btnAbilitaDifferenziata.BackColor = System.Drawing.Color.White;
            this.btnAbilitaDifferenziata.Location = new System.Drawing.Point(78, 88);
            this.btnAbilitaDifferenziata.Name = "btnAbilitaDifferenziata";
            this.btnAbilitaDifferenziata.Size = new System.Drawing.Size(70, 26);
            this.btnAbilitaDifferenziata.TabIndex = 128;
            this.btnAbilitaDifferenziata.Text = "8888900";
            this.toolTip1.SetToolTip(this.btnAbilitaDifferenziata, "Abilita la votazione differenziata");
            this.btnAbilitaDifferenziata.UseVisualStyleBackColor = false;
            this.btnAbilitaDifferenziata.Click += new System.EventHandler(this.btnAbilitaDifferenziata_Click);
            // 
            // btnRipetiz
            // 
            this.btnRipetiz.BackColor = System.Drawing.Color.White;
            this.btnRipetiz.Location = new System.Drawing.Point(3, 88);
            this.btnRipetiz.Name = "btnRipetiz";
            this.btnRipetiz.Size = new System.Drawing.Size(70, 26);
            this.btnRipetiz.TabIndex = 127;
            this.btnRipetiz.Text = "88889999";
            this.btnRipetiz.UseVisualStyleBackColor = false;
            this.btnRipetiz.Click += new System.EventHandler(this.btnRipetiz_Click);
            // 
            // btnExitVoto
            // 
            this.btnExitVoto.BackColor = System.Drawing.Color.White;
            this.btnExitVoto.Location = new System.Drawing.Point(3, 56);
            this.btnExitVoto.Name = "btnExitVoto";
            this.btnExitVoto.Size = new System.Drawing.Size(70, 26);
            this.btnExitVoto.TabIndex = 126;
            this.btnExitVoto.Text = "999999";
            this.btnExitVoto.UseVisualStyleBackColor = false;
            this.btnExitVoto.Click += new System.EventHandler(this.btnExitVoto_Click);
            // 
            // btmBadge
            // 
            this.btmBadge.BackColor = System.Drawing.Color.White;
            this.btmBadge.Location = new System.Drawing.Point(89, 49);
            this.btmBadge.Name = "btmBadge";
            this.btmBadge.Size = new System.Drawing.Size(58, 26);
            this.btmBadge.TabIndex = 123;
            this.btmBadge.Text = "Badge";
            this.btmBadge.UseVisualStyleBackColor = false;
            this.btmBadge.Click += new System.EventHandler(this.btmBadge_Click);
            // 
            // edtBadge
            // 
            this.edtBadge.Font = new System.Drawing.Font("Arial", 20F);
            this.edtBadge.Location = new System.Drawing.Point(6, 5);
            this.edtBadge.Name = "edtBadge";
            this.edtBadge.Size = new System.Drawing.Size(136, 38);
            this.edtBadge.TabIndex = 50;
            this.edtBadge.KeyDown += new System.Windows.Forms.KeyEventHandler(this.edtBadge_KeyDown);
            this.edtBadge.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.edtBadge_KeyPress);
            // 
            // timVotoApero
            // 
            this.timVotoApero.Interval = 30000;
            this.timVotoApero.Tick += new System.EventHandler(this.timVotoApero_Tick);
            // 
            // pbSalvaDati
            // 
            this.pbSalvaDati.BackColor = System.Drawing.Color.White;
            this.pbSalvaDati.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbSalvaDati.Location = new System.Drawing.Point(405, 114);
            this.pbSalvaDati.Name = "pbSalvaDati";
            this.pbSalvaDati.Size = new System.Drawing.Size(110, 99);
            this.pbSalvaDati.TabIndex = 125;
            this.pbSalvaDati.TabStop = false;
            this.pbSalvaDati.Visible = false;
            // 
            // lbConfermaUp
            // 
            this.lbConfermaUp.BackColor = System.Drawing.Color.Transparent;
            this.lbConfermaUp.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConfermaUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbConfermaUp.Location = new System.Drawing.Point(258, 228);
            this.lbConfermaUp.Name = "lbConfermaUp";
            this.lbConfermaUp.Size = new System.Drawing.Size(255, 45);
            this.lbConfermaUp.TabIndex = 126;
            this.lbConfermaUp.Text = "lbConfermaUp";
            this.lbConfermaUp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbConfermaUp.Visible = false;
            // 
            // lbNomeAzStart
            // 
            this.lbNomeAzStart.BackColor = System.Drawing.Color.Transparent;
            this.lbNomeAzStart.Font = new System.Drawing.Font("Arial", 39.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNomeAzStart.ForeColor = System.Drawing.Color.White;
            this.lbNomeAzStart.Location = new System.Drawing.Point(15, 57);
            this.lbNomeAzStart.Name = "lbNomeAzStart";
            this.lbNomeAzStart.Size = new System.Drawing.Size(498, 59);
            this.lbNomeAzStart.TabIndex = 128;
            this.lbNomeAzStart.Text = "Nome Azionista";
            this.lbNomeAzStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbNomeAzStart.Visible = false;
            // 
            // imgERSemaf
            // 
            this.imgERSemaf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.imgERSemaf.Image = global::VotoTouch.Properties.Resources.Semaforo_red;
            this.imgERSemaf.Location = new System.Drawing.Point(32, 536);
            this.imgERSemaf.Name = "imgERSemaf";
            this.imgERSemaf.Size = new System.Drawing.Size(30, 30);
            this.imgERSemaf.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgERSemaf.TabIndex = 129;
            this.imgERSemaf.TabStop = false;
            this.imgERSemaf.Visible = false;
            // 
            // imgERBarcode
            // 
            this.imgERBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.imgERBarcode.Image = global::VotoTouch.Properties.Resources.barcode_red;
            this.imgERBarcode.Location = new System.Drawing.Point(0, 536);
            this.imgERBarcode.Name = "imgERBarcode";
            this.imgERBarcode.Size = new System.Drawing.Size(30, 30);
            this.imgERBarcode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgERBarcode.TabIndex = 130;
            this.imgERBarcode.TabStop = false;
            this.imgERBarcode.Visible = false;
            // 
            // labelMousee
            // 
            this.labelMousee.AutoSize = true;
            this.labelMousee.Location = new System.Drawing.Point(7, 9);
            this.labelMousee.Name = "labelMousee";
            this.labelMousee.Size = new System.Drawing.Size(35, 13);
            this.labelMousee.TabIndex = 131;
            this.labelMousee.Text = "label2";
            // 
            // pnPopupRed
            // 
            this.pnPopupRed.BackColor = System.Drawing.Color.Transparent;
            this.pnPopupRed.Controls.Add(this.lblMsgPopup);
            this.pnPopupRed.Controls.Add(this.pictureBox1);
            this.pnPopupRed.Location = new System.Drawing.Point(267, 69);
            this.pnPopupRed.Name = "pnPopupRed";
            this.pnPopupRed.Size = new System.Drawing.Size(543, 133);
            this.pnPopupRed.TabIndex = 132;
            this.pnPopupRed.Visible = false;
            // 
            // lblMsgPopup
            // 
            this.lblMsgPopup.BackColor = System.Drawing.Color.White;
            this.lblMsgPopup.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMsgPopup.Location = new System.Drawing.Point(13, 20);
            this.lblMsgPopup.Name = "lblMsgPopup";
            this.lblMsgPopup.Size = new System.Drawing.Size(421, 78);
            this.lblMsgPopup.TabIndex = 1;
            this.lblMsgPopup.Text = "label2";
            this.lblMsgPopup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(537, 120);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // frmMain
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(844, 566);
            this.Controls.Add(this.pnPopupRed);
            this.Controls.Add(this.labelMousee);
            this.Controls.Add(this.imgERBarcode);
            this.Controls.Add(this.imgERSemaf);
            this.Controls.Add(this.lbConfermaUp);
            this.Controls.Add(this.lbConferma_old2);
            this.Controls.Add(this.Panel4);
            this.Controls.Add(this.pnBadge);
            this.Controls.Add(this.lbNomeAzStart);
            this.Controls.Add(this.pbSalvaDati);
            this.Controls.Add(this.lbDisgiuntoRimangono);
            this.Controls.Add(this.lbNomeDisgiunto);
            this.Controls.Add(this.lbConfermaNVoti);
            this.Controls.Add(this.lbDirittiStart);
            this.Controls.Add(this.lbDirittiDiVoto);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Voto Segreto";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
            this.Closed += new System.EventHandler(this.frmMain_Closed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.frmMain_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseUp);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.Panel4.ResumeLayout(false);
            this.Panel4.PerformLayout();
            this.pnBadge.ResumeLayout(false);
            this.pnBadge.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSalvaDati)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgERSemaf)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgERBarcode)).EndInit();
            this.pnPopupRed.ResumeLayout(false);
            this.pnPopupRed.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        #endregion

        private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label lbDirittiDiVoto;
        private System.Windows.Forms.Label lbDirittiStart;
		private System.Windows.Forms.Label lbConferma_old2;
        private System.Windows.Forms.Label lbConfermaNVoti;
		private System.Windows.Forms.Label lbNomeDisgiunto;
        private System.Windows.Forms.Label lbDisgiuntoRimangono;
        private System.Windows.Forms.Panel Panel4;
        private System.Windows.Forms.ListBox lbVersion;
        private Button button1;
        private Panel pnBadge;
        private Button btmBadge;
        private TextBox edtBadge;
        private Button btnExitVoto;
        private Label label1;
        private Button btnCloseInfo;
        private System.Windows.Forms.Timer timVotoApero;
        private Button btnRicaricaListe;

        private PictureBox pbSalvaDati;
        private Label lbConfermaUp;
        private Button btnCancVoti;
        private Label lbNomeAzStart;
        private Button btnRipetiz;
        private Button button2;
        private PictureBox imgERSemaf;
        private PictureBox imgERBarcode;
        private Label labelMousee;
        private Panel pnPopupRed;
        private PictureBox pictureBox1;
        private Label lblMsgPopup;
        private Button btnAbilitaDifferenziata;
        private ToolTip toolTip1;
    }
}