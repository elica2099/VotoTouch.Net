using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;


namespace VotoTouch
{

    public delegate void ehConfiguraLettore(object source, bool AUsaLettore, int AComPort);
    public delegate void ehConfiguraSemaforo(object source, bool AUsaSemaforo, 
                string AComPort, int ATipoSemaforo);
    public delegate void ehStatoSemaforo(object source, TStatoSemaforo AStato);
    public delegate void ehSalvaConfigurazioneLettore(object source, bool AUsaLettore, 
            int AComPort, string ASemComPort, bool AUsaSemaforo);
   
    
    public partial class frmConfig : Form
    {

        public event ehConfiguraLettore ConfiguraLettore;
        public event ehConfiguraSemaforo ConfiguraSemaforo;
        public event ehStatoSemaforo StatoSemaforo;
        public event ehSalvaConfigurazioneLettore SalvaConfigurazioneLettore;


        public const string BC_ASSIGN = "Pistola Barcode";
        public const string SEM_ASSIGN = "Semaforo Seriale";
        public const string NO_ASSIGN = "-";
        
        //public TTotemConfig TotCfg;
        private bool UsaLettore;
        private int ComPort;
        private bool UsaSemaforo;
        private int TipoSemaforo;
        private string SemComPort;

        private bool NoPorte;


        public frmConfig() //TTotemConfig ATotCfg)
        {
            InitializeComponent();

            //TotCfg = ATotCfg;

            NoPorte = false;

            CaricaSeriali();
        }

        public void Configura()
        {
            // semaforo
            //grbSemaforo.Enabled = TotCfg.UsaSemaforo;
            if (VTConfig.UsaSemaforo)
            {
                // vedo il tipo di semaforo
                if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_IP)
                {
                    grbSemaforo.Text = "Configurazione Database Semaforo: IP " + VTConfig.IP_Com_Semaforo +
                        "  Tipo: " + VTConfig.TipoSemaforo.ToString();
                    txtSemIP.Text = VTConfig.IP_Com_Semaforo;
                }
                if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_COM)
                    grbSemaforo.Text = "Configurazione Database Semaforo: Seriale " + VTConfig.IP_Com_Semaforo +
                        "  Tipo: " + VTConfig.TipoSemaforo.ToString();
            }
            else
            {
                grbSemaforo.Text = "Nessun semaforo collegato - Semaforo ";
                if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_IP)
                    grbSemaforo.Text += "IP";
                else
                    grbSemaforo.Text += "COM";
            }   

            // lettore
            if (VTConfig.UsaLettore)
                grbLettore.Text = "Prova Lettore Barcode collegato su COM" + VTConfig.PortaLettore.ToString();

            else
                grbLettore.Text = "Nessun lettore Barcode collegato";

            // se è il semaforo ip comunque disabilito il pulsante
            if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_IP)
                btnAssegnaSem.Enabled = false;
            else
            {
                txtSemIP.Enabled = false;
                label3.Enabled = false;
                btnSemAssegnaIP.Enabled = false;
                btnNoSemaforo.Enabled = false;
            }
        }

        // ******************************************************************************
        // LIST VIEW SERIALI
        // ******************************************************************************

        private void CaricaSeriali()
        {
            int i;

            // cancello
            lvSeriali.Items.Clear();
            // carico le seriali
            foreach (COMPortInfo comPort in COMPortInfo.GetCOMPortsInfo())
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = comPort.Name;
                lvi.SubItems.Add("-");
                lvi.SubItems.Add(comPort.Description);
                lvSeriali.Items.Add(lvi);
            }

            //foreach (string portname in SerialPort.GetPortNames())
            //{
            //    ListViewItem lvi = new ListViewItem();
            //    lvi.Text = portname;
            //    lvSeriali.Items.Add(lvi);
            //}

            // se non ci sono porte seriali attive inserisco una scritta
            if (lvSeriali.Items.Count == 0)
            {
                ListViewItem lvis = new ListViewItem();
                lvis.Text = "Nessuna ";
                lvis.SubItems.Add("COM nel sistema");
                lvSeriali.Items.Add(lvis);
                NoPorte = true;
            }

            // setto i componenti in funzione delle porte
            //btnSalvaDB.Enabled = !NoPorte;
            btnAssegna.Enabled = !NoPorte;
            btnNoLettore.Enabled = !NoPorte;
            btnAssegnaSem.Enabled = !NoPorte;

            if (!NoPorte)
            {
                // controllo quali sono assegnate 
                for (i = 0; i < lvSeriali.Items.Count; i++)
                {
                    // ne aggiungo comunque uno
                    //lvSeriali.Items[i].SubItems.Add(NO_ASSIGN);

                    // quali sono quelle assegnate alla pistola
                    if (VTConfig.UsaLettore && lvSeriali.Items[i].Text == "COM" + VTConfig.PortaLettore.ToString())
                    {
                        lvSeriali.Items[i].SubItems[1].Text = BC_ASSIGN;
                    }

                    // quali sono assegnate al semaforo seriale
                    if (VTConfig.UsaSemaforo && VTConfig.TipoSemaforo == VSDecl.SEMAFORO_COM)
                    {
                        if (lvSeriali.Items[i].Text == VTConfig.IP_Com_Semaforo)
                        {
                            lvSeriali.Items[i].SubItems[1].Text = SEM_ASSIGN;
                        }
                    }
                        
                }
            }

            UsaLettore = VTConfig.UsaLettore;
            ComPort = VTConfig.PortaLettore;
            UsaSemaforo = VTConfig.UsaSemaforo;
            TipoSemaforo = VTConfig.TipoSemaforo;
            SemComPort = VTConfig.IP_Com_Semaforo;
            
        }

        private void btnAggiorna_Click(object sender, EventArgs e)
        {
            CaricaSeriali();
            // riporto la situazione a prima
            if (ConfiguraLettore != null) { ConfiguraLettore(this, VTConfig.UsaLettore, VTConfig.PortaLettore); }
        }


        // ******************************************************************************
        // PULSANTI ASSEGNAMENTO
        // ******************************************************************************

        private void btnAssegna_Click(object sender, EventArgs e)
        {
            int i;
            // assegna a barcode la seriale corrente
            ListViewItem lvi = lvSeriali.SelectedItems[0];
            string ss = lvi.Text;

            if (lvi.SubItems[1].Text != NO_ASSIGN)
            {
                MessageBox.Show("Porta già assegnata", "Exclamation",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // ora devo trovare la stringa COM
            string removeString = "COM";
            int index = ss.LastIndexOf(removeString);
            if (index >= 0)
            {
                string st = ss.Remove(ss.IndexOf(removeString), removeString.Length);
                //string st = ss.Substring(index +3, ss.Length - index +3); // ss[index + 3].ToString();
                ComPort = Convert.ToInt16(st);
                UsaLettore = true;
                // ok ora apro
                if (ConfiguraLettore != null) { ConfiguraLettore(this, UsaLettore, ComPort); }
            }
            else
            {
                ComPort = VTConfig.PortaLettore;
                UsaLettore = false;
                // devo disabilitare l'oggetto
                if (ConfiguraLettore != null) { ConfiguraLettore(this, UsaLettore, ComPort); }
            }

            // però devo settare anche la lista
            // controllo quali sono assegnate 
            for (i = 0; i < lvSeriali.Items.Count; i++)
            {
                if (lvSeriali.Items[i].SubItems.Count > 0
                    && lvSeriali.Items[i].SubItems[1].Text == BC_ASSIGN)
                {
                    lvSeriali.Items[i].SubItems[1].Text = NO_ASSIGN;
                }
            }
            // ok ora lo scrivo sulla corretta porta com
            lvSeriali.SelectedItems[0].SubItems[1].Text = BC_ASSIGN;

            if (UsaLettore) txtProva.Focus();
        }

        private void btnAssegnaSem_Click(object sender, EventArgs e)
        {
            int i;
            // assegna al semaforo la seriale corrente
            ListViewItem lvi = lvSeriali.SelectedItems[0];
            string ss = lvi.Text;

            if (lvi.SubItems[1].Text != NO_ASSIGN)
            {
                MessageBox.Show("Porta già assegnata", "Exclamation",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // ora devo trovare la stringa COM
            int index = ss.LastIndexOf("COM");
            if (index >= 0)
            {
                UsaSemaforo = true;
                SemComPort = ss;
                TipoSemaforo = VSDecl.SEMAFORO_COM;
                // ok ora apro
                if (ConfiguraSemaforo != null) { ConfiguraSemaforo(this, UsaSemaforo, 
                        SemComPort, TipoSemaforo); }

            }
            else
            {
                UsaSemaforo = false;
                SemComPort = VTConfig.IP_Com_Semaforo;
                // devo disabilitare l'oggetto
                if (ConfiguraSemaforo != null) { ConfiguraSemaforo(this, UsaSemaforo,
                    SemComPort, TipoSemaforo);
                }
            }

            // però devo settare anche la lista
            // controllo quali sono assegnate 
            for (i = 0; i < lvSeriali.Items.Count; i++)
            {
                if (lvSeriali.Items[i].SubItems.Count > 0
                    && lvSeriali.Items[i].SubItems[1].Text == SEM_ASSIGN)
                {
                    lvSeriali.Items[i].SubItems[1].Text = NO_ASSIGN;
                }
            }
            // ok ora lo scrivo sulla corretta porta com
            lvSeriali.SelectedItems[0].SubItems[1].Text = SEM_ASSIGN;
        }

        
        private void btnNoLettore_Click(object sender, EventArgs e)
        {
            // questa procedura libera la porta assegnata
           
            ListViewItem lvi = lvSeriali.SelectedItems[0];
            string ss = lvi.Text;

            txtProva.Text = "";

            // se è già libera esco
            if (lvi.SubItems[1].Text == NO_ASSIGN)
            {
                MessageBox.Show("Porta già libera", "Exclamation",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // controllo se è assegnata al barcode
            if (lvi.SubItems[1].Text == BC_ASSIGN)
            {
                // libero la porta ed esco
                ComPort = VTConfig.PortaLettore;
                UsaLettore = false;
                lvi.SubItems[1].Text = NO_ASSIGN;
                // devo disabilitare l'oggetto
                if (ConfiguraLettore != null) { ConfiguraLettore(this, UsaLettore, ComPort); }
                return;
            }

            // controllo se è assegnata al semaforo
            if (lvi.SubItems[1].Text == SEM_ASSIGN)
            {
                lvi.SubItems[1].Text = NO_ASSIGN;
                UsaSemaforo = false;
                SemComPort = VTConfig.IP_Com_Semaforo;
                // devo disabilitare l'oggetto
                if (ConfiguraSemaforo != null) { ConfiguraSemaforo(this, UsaSemaforo, 
                    SemComPort, TipoSemaforo); }
                return;
            }        
        }

        // ******************************************************************************
        // PULSANTI SALVATAGGIO E CHIUSURA
        // ******************************************************************************


        private void btnChiudi_Click(object sender, EventArgs e)
        {
            // prima testo se ci sono state variazioni non salvate
            if (VTConfig.UsaLettore != UsaLettore || VTConfig.PortaLettore != ComPort ||
                VTConfig.UsaSemaforo != UsaSemaforo || VTConfig.IP_Com_Semaforo != SemComPort)
            {
                if (MessageBox.Show("La configurazione del lettore è cambiata, vuoi chiudere la finestra senza salvarla?", "Question",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    this.Close();

            }
            else
                this.Close();
        }


        private void btnSalvaDB_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Vuoi salvare la configurazione sul database?", "Question",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                VTConfig.UsaLettore = UsaLettore;
                VTConfig.PortaLettore = ComPort;
                // posso solo modificare il semaforo com
                if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_COM)
                {
                    VTConfig.IP_Com_Semaforo = SemComPort;
                    VTConfig.UsaSemaforo = UsaSemaforo;
                }
                if (SalvaConfigurazioneLettore != null) 
                {
                    SalvaConfigurazioneLettore(this, UsaLettore, ComPort, SemComPort, UsaSemaforo); 
                }
                // aggiorno i campi
                Configura();
            }
        }


        public void Semaforo(bool Attivato)
        {
            grbSemaforo.Enabled = Attivato;

        }

        // ******************************************************************************
        // SEMAFORO
        // ******************************************************************************

        private void btnLibero_Click(object sender, EventArgs e)
        {
            if (StatoSemaforo != null) { StatoSemaforo(this, TStatoSemaforo.stsLibero); }
        }

        private void btnSOccupato_Click(object sender, EventArgs e)
        {
            if (StatoSemaforo != null) { StatoSemaforo(this, TStatoSemaforo.stsOccupato); }
        }

        private void btnSFineOcc_Click(object sender, EventArgs e)
        {
            if (StatoSemaforo != null) { StatoSemaforo(this, TStatoSemaforo.stsFineoccupato); }
        }

        private void btnSErrore_Click(object sender, EventArgs e)
        {
            if (StatoSemaforo != null) { StatoSemaforo(this, TStatoSemaforo.stsErrore); }
        }


        // ******************************************************************************
        // LETTORE BARCODE
        // ******************************************************************************


        public void BadgeLetto(string AText)
        {
            //  lettura badge
            txtProva.Text = AText;
        }



        private void frmConfig_Load(object sender, EventArgs e)
        {
            // non serve
            lvSeriali.Focus();
        }

        private void frmConfig_Shown(object sender, EventArgs e)
        {
            lvSeriali.Focus();

        }

        private void lvSeriali_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            //using (StringFormat sf = new StringFormat())
            //{
            //    // Store the column text alignment, letting it default
            //    // to Left if it has not been set to Center or Right.
            //    switch (e.Header.TextAlign)
            //    {
            //        case HorizontalAlignment.Center:
            //            sf.Alignment = StringAlignment.Center;
            //            break;
            //        case HorizontalAlignment.Right:
            //            sf.Alignment = StringAlignment.Far;
            //            break;
            //    }

            //    // Draw the standard header background.
            //    e.DrawBackground();

            //    // Draw the header text.
            //    using (Font headerFont =
            //                new Font("Helvetica", 10, FontStyle.Bold)) //Font size!!!!
            //    {
            //        e.Graphics.DrawString(e.Header.Text, headerFont,
            //            Brushes.Black, e.Bounds, sf);
            //    }
            //}
            //return;
        }









    }
}
