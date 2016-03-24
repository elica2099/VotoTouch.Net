using System;
using System.Collections.ObjectModel;
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
    // TODO: verificare multicandidato e pagine
    // TODO: In caso di Votazione con AbilitaDiritti... mettere sulla videata di inizio lo stato dei diritti espressi
    // TODO: ModoAssemblea, salvare azioni o voti, mostrare azioni o voti
    // TODO: Mettere finestra riepilogo azionista
    // TODO: nelle azioni mettere la formattazione

	/// <summary>
	/// Summary description for Form1.\
	/// </summary>
    public partial class frmMain : Form, IVotoTouchTestInterface
	{
        public delegate void EventDataReceived(object source, string messaggio);
        public event EventDataReceived evtDataReceived;

        // timer di disaccoppiamento
        private System.Windows.Forms.Timer timLetturaBadge;
        private System.Windows.Forms.Timer timCambiaStato;
        private System.Windows.Forms.Timer timConfigura;
        private System.Windows.Forms.Timer timAutoRitorno;

        // Modo Debug
        public bool DebugMode;
        public bool PaintTouch;
        // VersioneDemo
        public bool DemoVersion;
        // oggetti demo
        private Button btnBadgeUnVoto;
        private Button btnBadgePiuVoti;
        private Button btnFineVotoDemo;

        private static Mutex appMutex;

        // finestre e usercontrol
        public FVSMessage frmVSMessage;
        public frmConfig fConfig;
        public SplashScreen splash;
        public FVSStart frmStart;
	    public LabelCandidati lbConferma;

		// oggetti globali
		public  CVotoTouchScreen oVotoTouch;    // classe del touch
        public  CVotoTheme oVotoTheme;          // classe del tema grafico
        public  CVotoBaseDati oDBDati;          // classe del database
        public  CBaseSemaphore oSemaforo;       // classe del semaforo
        public  CNETActiveReader NewReader;
        public  CVotoImages oVotoImg;
        // strutture
        public ConfigDbData DBConfig;           // database
        public TAppStato Stato;                 // macchina a stato
        public string Data_Path;                // path della cartella data
        public string   NomeTotem;              // nome della macchina
        public string   LogVotiNomeFile;        // nome file del log
        public bool CtrlPrimoAvvio;             // serve per chiudere la finestra in modo corretto
        
        // Votazioni
	    public TListaVotazioni Votazioni;
        // Dati dell'azionista e delle deleghe che si porta dietro
        public TListaAzionisti Azionisti;

        public bool IsVotazioneDifferenziata = false;
         // cpontrollo degli eventi di voto
	    private bool AperturaVotoEsterno;
        // flag uscita in votazione
        public bool UscitaInVotazione;

        // Variabile temporanea voti espressi Nuova Versione (Array)
        public ArrayList FVotiExpr;

        // risorse per l'internazionalizzazione
        ResourceManager rm;

        // Variabile temporanea Voti Espressi
        public int VotoEspresso;
        public string VotoEspressoStr;
        public string VotoEspressoStrUp;
		public int Badge_Letto;
        public string Badge_Seriale;

		public frmMain()
		{
			InitializeComponent();

            // inizializzo le risorse
            rm = Properties.Resources.ResourceManager; // new System.Resources.ResourceManager("VotoSegreto", Assembly.GetExecutingAssembly());

            // "magiche paroline" per evitare il flickering
            // You can use SuspendLayout() and ResumeLayout().
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            // variabili demo e debug
            DebugMode = false;
		    PaintTouch = false;
            DemoVersion = false;

            CtrlPrimoAvvio = PrimoAvvio;
            if (!CtrlPrimoAvvio)
			{
				MessageBox.Show(rm.GetString("SAPP_START_CTRL"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				//Application.Exit();
				return;
			}
			
            // finestra di start
            frmStart = new FVSStart();
            this.AddOwnedForm(frmStart);
            if (frmStart.ShowDialog() != System.Windows.Forms.DialogResult.Yes)
            {
                CtrlPrimoAvvio = false;
                return;
            }
            frmStart.Dispose();
            frmStart = null;

            splash = new SplashScreen();
            splash.Show();
            splash.SetSplash(0, rm.GetString("SAPP_START_INIT")); //Inizializzo applicazione...
            splash.Update();

			// Massimizzo la finestra
            this.WindowState = FormWindowState.Maximized;

            // inizializzo il splashscreen
            splash.SetSplash(10, rm.GetString("SAPP_START_IMGSEARCH")); //Ricerco immagini...
           
            // ritrovo il nome della macchina che mi servir� per interrogare il db
			int i;
			NomeTotem = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
			for (i = 0; i< NomeTotem.Length; i++)
				if (NomeTotem[i] == '\\' ) break;
			NomeTotem = NomeTotem.Remove(0, i+1);

            // ok, per prima cosa verifico se c'� la cartella c:\data, se si ok
            // senn� devo considerare la cartella dell'applicazione, se non c'� esco
            oVotoImg = new CVotoImages();
            oVotoImg.MainForm = this;
            CtrlPrimoAvvio = oVotoImg.CheckDataFolder(ref Data_Path);

            btnCancVoti.Visible = System.IO.File.Exists(Data_Path + "VTS_ADMIN.txt");

            // identificazione della versione demo, nella cartella data o nella sua cartella
            if (System.IO.File.Exists(Data_Path + "VTS_DEMO.txt"))
            {
                // Ok � la versione demo
                DemoVersion = true;
                // start the logging
                Logging.generateInternalLogFileName(Data_Path, "VotoTouch_" + NomeTotem);
                Logging.WriteToLog("---- DEMO MODE ----");
                // ok, ora creo la classe che logga i voti
                LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(Data_Path, "VotoT_" + NomeTotem);
            }
            else
            {
                // ok, qua devo vedere i due casi:
                // il primo � VTS_STANDALONE.txt presente il che vuol dire che ho la configurazione
                // in locale, caricando comunque un file GEAS.sql da data
                if (System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt"))
                {
                    Logging.generateInternalLogFileName(Data_Path, "VotoTouch_" + NomeTotem);
                    Logging.WriteToLog("---- STANDALONE MODE ----");
                }
                else
                {
                    // verifica della mappatura
                    if (!System.IO.Directory.Exists(@"M:\"))
                    {
                        MessageBox.Show(rm.GetString("SAPP_START_ERRMAP"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CtrlPrimoAvvio = false;
                        return;
                    }

                    // Inizializzo il log
                    if (!System.IO.Directory.Exists(@"M:\LST\VotoTouch\"))
                        System.IO.Directory.CreateDirectory(@"M:\LST\VotoTouch\");
                    Logging.generateInternalLogFileName(@"M:\LST\VotoTouch\", "VotoTouch_" + NomeTotem);
                }
            }
            // loggo l'inizio dell'applicazione
            Logging.WriteToLog("<start> Inizio Applicazione");
            splash.SetSplash(20, rm.GetString("SAPP_START_INITDB"));    // "Inizializzo database...");

            // identificazione DebugMode
            DebugMode = System.IO.File.Exists(Data_Path + "VTS_DEBUG.txt");
            PaintTouch = System.IO.File.Exists(Data_Path + "VTS_PAINT_TOUCH.txt");

            // classe lbConferma
		    lbConferma = new LabelCandidati();
		    lbConferma.Visible = false;
		    lbConferma.Parent = this;

            // Inizializzo la classe del database, mi servir� prima delle altre classi perch� in
            // questa versione la configurazione � centralizzata sul db
            bool dataloc = System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt");
            if (DemoVersion)
                oDBDati = new CVotoMDBDati(DBConfig, NomeTotem, dataloc, Data_Path);
                //oDBDati = new CVotoFileDati(DBConfig, NomeTotem, dataloc, Data_Path);
            else
                oDBDati = new CVotoDBDati(DBConfig, NomeTotem, dataloc, Data_Path);

            //oDBDati.FDBConfig = DBConfig;
            //oDBDati.NomeTotem = NomeTotem;
            //// se � standalone prende i dati in locale
            //oDBDati.ADataLocal = System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt");
            //oDBDati.AData_path = Data_Path;
            if (!oDBDati.CaricaConfig())
            {
                Logging.WriteToLog("<dberror> Problemi nel caricamento della configurazione DB, mappatura");
                MessageBox.Show(rm.GetString("SAPP_START_ERRCFG"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CtrlPrimoAvvio = false;
                return;
            }

            // vado avanti con il database mi connetto
            splash.SetSplash(30, rm.GetString("SAPP_START_INITCFG"));    //"Carico configurazione..."
            if (oDBDati.DBConnect() != null)
            {
                int DBOk = 0;  // variabile di controllo sul caricamento
                // leggo la configurazione del badge/impianto
                DBOk += oDBDati.CaricaConfigDB(ref VTConfig.BadgeLen, ref VTConfig.CodImpianto);
                splash.SetSplash(40, rm.GetString("SAPP_START_INITPREF"));   //"Carico preferenze..."
                // leggo la configurazione generale
                DBOk += oDBDati.DammiConfigDatabase(); //ref TotCfg);
                // leggo la configurazione del singolo totem
                DBOk += oDBDati.DammiConfigTotem(NomeTotem); //, ref TotCfg);
                splash.SetSplash(50, rm.GetString("SAPP_START_INITVOT"));  //"Carico liste e votazioni..."

                if (VTConfig.VotoAperto) Logging.WriteToLog("Votazione gi� aperta");

                // carica le votazioni, le carica comunque all'inizio
                Rectangle FFormRect = new Rectangle(0, 0, this.Width, this.Height);
                Votazioni = new TListaVotazioni(oDBDati);
                Votazioni.CaricaListeVotazioni(Data_Path, FFormRect, true);

                // ok, finisce
                if (DBOk == 0)
                {
                    // nel log va tutto bene
                    Logging.WriteToLog("<startup> Caricamento dati database OK");
                }
                else
                {
                    Logging.WriteToLog("<dberror> Problemi nel caricamento configurazione db");
                    MessageBox.Show(rm.GetString("SAPP_START_ERRDB"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Logging.WriteToLog("<dberror> Problemi nella connessione al Database");
                MessageBox.Show(rm.GetString("SAPP_START_ERRCONN"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CtrlPrimoAvvio = false;
                return;
            }

            splash.SetSplash(60, rm.GetString("SAPP_START_INITBARCODE"));   //"inizializzo lettore barcode...");
            // A questo punto la configurazione � attiva e caricata centralmente, posso continuare
            // il lettore del badge
            NewReader = new CNETActiveReader();
            NewReader.ADataRead += ObjDataReceived;
            evtDataReceived += new EventDataReceived(onDataReceived);
            // lo attiver� nel load
            if (VTConfig.UsaLettore)
            {
                NewReader.PortName = "COM" + VTConfig.PortaLettore.ToString();
            }

            splash.SetSplash(70, rm.GetString("SAPP_START_INITSEM"));       //"Inizializzo Semaforo..."
            // il semaforo, ora fa tutto lei
            SemaforoOKImg(VTConfig.UsaSemaforo);
            // ok, in funzione del tipo di semaforo faccio
            if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_IP)
                // USARE SEMPRE CIPThreadSemaphore
                oSemaforo = new CIPThreadSemaphore();
            else
                oSemaforo = new CComSemaphore();
            // se � attivato lo setto
            oSemaforo.ConnAddress = VTConfig.IP_Com_Semaforo;  //  deve essere "COM1" o "COMn"
            oSemaforo.ChangeSemaphore += onChangeSemaphore;
            if (VTConfig.UsaSemaforo)
                oSemaforo.AttivaSemaforo(true);

            splash.SetSplash(80, rm.GetString("SAPP_START_INITTOUCH"));       // "Inizializzo Touch..."
            // array dei voti temporanei
            FVotiExpr = new ArrayList();
            // azionisti
            Azionisti = new TListaAzionisti(oDBDati);
            // Classe del TouchScreen
		    oVotoTouch = new CVotoTouchScreen(); //ref TotCfg);
            oVotoTouch.PremutoVotaNormale += new ehPremutoVotaNormale(onPremutoVotaNormale);
            oVotoTouch.PremutoVotaDifferenziato += new ehPremutoVotaDifferenziato(onPremutoVotaDifferenziato);
            oVotoTouch.PremutoConferma += new ehPremutoConferma(onPremutoConferma);
            oVotoTouch.PremutoAnnulla += new ehPremutoAnnulla(onPremutoAnnulla);
            oVotoTouch.PremutoVotoValido += new ehPremutoVotoValido(onPremutoVotoValido);
            oVotoTouch.PremutoSchedaBianca += new ehPremutoSchedaBianca(onPremutoSchedaBianca);
            oVotoTouch.PremutoNonVoto += new ehPremutoNonVoto(onPremutoNonVoto);
            oVotoTouch.PremutoInvalido += new ehPremutoInvalido(onPremutoInvalido);
            oVotoTouch.PremutoTab += new ehPremutoTab(onPremutoTab);
            oVotoTouch.TouchWatchDog += new ehTouchWatchDog(onTouchWatchDog);
            oVotoTouch.PremutoMultiAvanti += new ehPremutoMultiAvanti(onPremutoVotoValidoMulti);
            oVotoTouch.PremutoMulti += new ehPremutoMulti(onPremutoVotoMulti);
            oVotoTouch.PremutoBottoneUscita += new ehPremutoBottoneUscita(onPremutoBottoneUscita);
            oVotoTouch.PremutoContrarioTutti += new ehPremutoContrarioTutti(onPremutoContrarioTutti);
            oVotoTouch.PremutoAstenutoTutti += new ehPremutoAstenutoTutti(onPremutoAstenutoTutti);

            // classe del tema
            oVotoTheme = new CVotoTheme();
            oVotoTheme.CaricaTemaDaXML(oVotoImg.Img_path);

            IsVotazioneDifferenziata = false;               // non � differenziata
            Badge_Letto = 0;
            AperturaVotoEsterno = VTConfig.VotoAperto;  // lo setto uguale cos� in stato badge non carica 2 volte le Liste
            Badge_Seriale = "";
            UscitaInVotazione = false;

            // i timer di disaccoppiamento funzioni (non potendo usare WM_USER!!!!)
            // timer di lettura badge
            timLetturaBadge = new System.Windows.Forms.Timer {Enabled = false, Interval = 30};
		    timLetturaBadge.Tick += timLetturaBadge_Tick;
            // timer di cambio stato
            timCambiaStato = new System.Windows.Forms.Timer {Enabled = false, Interval = 30};
		    timCambiaStato.Tick += timCambiaStato_Tick;
            // timer di configurazione
            timConfigura = new System.Windows.Forms.Timer {Enabled = false, Interval = 30};
		    timConfigura.Tick += timConfigura_Tick;
            // timer di autoritorno
            timAutoRitorno = new System.Windows.Forms.Timer
                {
                    Enabled = false,
                    Interval = VTConfig.TimeAutoRitornoVoto*1000
                };
		    timAutoRitorno.Tick += timAutoRitorno_Tick;

            pnSemaf.BackColor = Color.Transparent;

            splash.SetSplash(90, rm.GetString("SAPP_START_INITVAR"));   //"Inizializzo variabili...");
            // scrive la configurazione nel log
            Logging.WriteToLog(VSDecl.VTS_VERSION);
            Logging.WriteToLog("** Configurazione:");
            Logging.WriteToLog("   Usalettore: " + VTConfig.UsaLettore.ToString());
            Logging.WriteToLog("   Porta: " + VTConfig.PortaLettore.ToString());
            Logging.WriteToLog("   UsaSemaforo: " + VTConfig.UsaSemaforo.ToString());
            Logging.WriteToLog("   IPSemaforo: " + VTConfig.IP_Com_Semaforo.ToString());
            Logging.WriteToLog("   IDSeggio: " + VTConfig.IDSeggio.ToString());
            Logging.WriteToLog("   NomeComputer: " + NomeTotem);
            Logging.WriteToLog("   ControllaPresenze: " + VTConfig.ControllaPresenze.ToString());
            Logging.WriteToLog("** CodiceUscita: " + VTConfig.CodiceUscita);
            Logging.WriteToLog("");
            
            // inizializzo i componenti
			InizializzaControlli();
            // Se � in demo mode metto i controlli
            if (DemoVersion)
                InizializzaControlliDemo();

			// ora inizializzo la macchina a stati
			Stato = TAppStato.ssvBadge;
            
            splash.SetSplash(100);   
            splash.Hide();

            // se sono in debug evidenzio le zone sensibili
            oVotoTouch.PaintTouchOnScreen = PaintTouch;

            // se la votazione � aperta il timer di controllo voto batte di meno
            if (VTConfig.VotoAperto)
                timVotoApero.Interval = VSDecl.TIM_CKVOTO_MAX;
            else
                timVotoApero.Interval = VSDecl.TIM_CKVOTO_MIN;

            // Attivo la macchina a stati (in FMain_MacchinaAStati.cs)
            CambiaStato();
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmMain());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void frmMain_Closed(object sender, System.EventArgs e)
		{
            timVotoApero.Enabled = false;
			// alcune cose sul database
			oDBDati.DBDisconnect();
            NewReader.Close();
		}

		private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            //
        }

		private void frmMain_Load(object sender, System.EventArgs e)
		{
            // testo 
            if (!CtrlPrimoAvvio)
            {
                Application.Exit();
                return;
            }

            // la seconda finestra
            frmVSMessage = new FVSMessage();
            this.AddOwnedForm(frmVSMessage);

            if (VTConfig.UsaLettore)
            {
                if (!NewReader.Open())
                {
                    // ci sono stati errori con la com all'apertura
                    VTConfig.UsaLettore = false;
                    MessageBox.Show(rm.GetString("SAPP_START_ERRCOM1") + VTConfig.PortaLettore + rm.GetString("SAPP_START_ERRCOM2"), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            timVotoApero.Enabled = true;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            // Carico il tema nelle labels, alla fine l'ho messo in resize
            //CaricaTemaInControlli();
        }
        
        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            // ok, questa funzione serve all'oggetto CTouchscreen per evidenziare le zone sensibili
            if (oVotoTouch != null)
            {
                oVotoTouch.PaintTouch(sender, e);

                // controllo che CurrVoteIDX sia nel range giusto
                //if (CurrVoteIDX < NVoti)
                //{
                    // se la votazione corrente � di candidato su pi� pagine disegno i rettangoli
                    if (Stato == TAppStato.ssvVoto && 
                        (Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_CANDIDATO ||
                         Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_CANDIDATO_SING))
                    {
                        // paint delle label Aggiuntive
                        //oVotoTheme.PaintlabelProposteCdaAlt(sender, e, ref Votazioni.VotoCorrente, true);
                        oVotoTheme.PaintlabelProposteCdaAlt(sender, e, Votazioni.VotoCorrente, true);
                        // paint dei Bottoni
                        oVotoTouch.PaintButtonCandidatoPagina(sender, e, false, oVotoTheme.BaseFontCandidato);
                    }
                    // se la votazione corrente � di MULTIcandidato su pi� pagine disegno i rettangoli
                    if (Stato == TAppStato.ssvVoto && Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                    {
                        // paint delle label Aggiuntive
                        //oVotoTheme.PaintlabelProposteCdaAlt(sender, e, ref Votazioni.VotoCorrente, false);
                        oVotoTheme.PaintlabelProposteCdaAlt(sender, e, Votazioni.VotoCorrente, false);
                        // paint dei bottoni
                        oVotoTouch.PaintButtonCandidatoPagina(sender, e, true, oVotoTheme.BaseFontCandidato);
                    }

                    // ******* OBSOLETO ********/
                    // votazione VOTO_CANDIDATO_SING, candidato a singola pagina, disegno i rettangoli
                    //if (Stato == TAppStato.ssvVoto && (FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_CANDIDATO_SING))
                    //    oVotoTouch.PaintButtonCandidatoSingola(sender, e);
                //}
                
                // se sono nello stato di votostart e il n. di voti � > 1
                if (Stato == TAppStato.ssvVotoStart && Azionisti.HaDirittiDiVotoMultipli())
                {
                    // faccio il paint del numero di diritti di voto nel bottone in basso a sx , 
                    // in questo caso uso un paint e non una label per un problema grafico di visibilit�
                    int VVoti = VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP
                                   ? Azionisti.DammiMaxNumeroDirittiDiVotoTotali()
                                   : Azionisti.DammiMaxNumeroAzioniTotali();
                    oVotoTheme.PaintDirittiDiVoto(sender, e, VVoti);
                }
            }

            // se � demo devo stampare una label
            if (DemoVersion)
            {
                try
                {
                    System.Drawing.Drawing2D.GraphicsState gs = e.Graphics.Save();
                    Font fn = new Font("Tahoma", 90, System.Drawing.FontStyle.Bold);
                    string str = rm.GetString("SAPP_DEMO");
                    StringFormat sf = (StringFormat)StringFormat.GenericTypographic.Clone();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Trimming = StringTrimming.EllipsisWord;
                    Color semiTransparentColor = Color.FromArgb(50, Color.DarkBlue);
                    SolidBrush whiteBrush = new SolidBrush(semiTransparentColor);
                    e.Graphics.RotateTransform(-35);
                    e.Graphics.TranslateTransform(-400, 350);

                    e.Graphics.DrawString(str, fn, whiteBrush, new
                        RectangleF(5, 5, this.ClientRectangle.Width - 15, this.ClientRectangle.Height - 10), sf);
                    e.Graphics.Restore(gs);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    // non faccio nulla, non serve, al massimo non apparir� la scritta
                }
            }
        }
         
        private void frmMain_Resize(object sender, EventArgs e)
        {
            // immagine del salvataggio
            pbSalvaDati.Left = (this.Width / 2) - (pbSalvaDati.Width / 2);
            pbSalvaDati.Top = (this.Height / 2) - (pbSalvaDati.Height  / 2);

            Rectangle FFormRect = new Rectangle(0, 0, this.Width, this.Height);

            // devo dire alla nuova touch le dimensioni della finestra
            if (oVotoTouch != null)
            {
                oVotoTouch.CalcolaVotoTouch(FFormRect);
            }
            // lo stesso faccio per la classe del thema che si occupa di disegnare 
            // le label di informazione
            if (oVotoTheme != null)
            {
                oVotoTheme.FFormRect = FFormRect;
                CaricaTemaInControlli();
            }
            // ok ora le votazioni
            if (Votazioni != null)
            {
                Votazioni.CalcolaTouchZoneVotazioni(FFormRect);
            }
            
            // ok, ora se � in demo mode faccio il resize dei controlli
            if (DemoVersion)
            {
                // bottone un voto
                if (btnBadgeUnVoto != null)
                {
                    btnBadgeUnVoto.Left = this.Width / 7;
                    btnBadgeUnVoto.Top = (this.Height / 10) * 6;
                    btnBadgeUnVoto.Width = (this.Width / 7) * 2;
                    btnBadgeUnVoto.Height = (this.Height / 10) *2;
                }
                // bottone pi� voto
                if (btnBadgePiuVoti != null)
                {
                    btnBadgePiuVoti.Left = (this.Width / 7) * 4;
                    btnBadgePiuVoti.Top = (this.Height / 10) * 6;
                    btnBadgePiuVoti.Width = (this.Width / 7) * 2;
                    btnBadgePiuVoti.Height = (this.Height / 10) * 2;
                }
                // bottone finevotodemo
                if (btnFineVotoDemo != null)
                {
                    btnFineVotoDemo.Left = (this.Width / 7) * 2;
                    btnFineVotoDemo.Top = (this.Height / 10) * 6;
                    btnFineVotoDemo.Width = (this.Width / 7) * 3;
                    btnFineVotoDemo.Height = (this.Height / 10) * 2;
                }
            }
        }
       
		//------------------------------------------------------------------------------
		//  SALVATAGGIO DEI DATI DI VOTO COME SCHEDA BIANCA o nulla DA INTERRUZIONE
		//------------------------------------------------------------------------------

		private int MettiSchedeDaInterruzione()
		{
            // prima di tutto vedo se � attivato SalvaVotoNonConfermato
            // se sono nello stato di conferma, confermo il voto espresso e poi metto le altre schede
            if (Stato == TAppStato.ssvVotoConferma && VTConfig.SalvaVotoNonConfermato) 
                Azionisti.ConfermaVoti_VotoCorrente(ref FVotiExpr);

            // Dopodich� segnalo ad azionisti di riempire le votazioni con schede bianche, ma solo  
            // in funzione di AbilitaDirittiNonVoglioVotare:
            //      false - mi comporto normalmente, salvo i non votati con IDSchedaUscitaForzata
            //      true  - non faccio nulla, verranno come non votati e saranno disponibili alla nuova votazione

            if (!VTConfig.AbilitaDirittiNonVoglioVotare)
            {
                TVotoEspresso vz = new TVotoEspresso
                    {
                        NumVotaz = Votazioni.VotoCorrente.IDVoto,
                        VotoExp_IDScheda = VTConfig.IDSchedaUscitaForzata,
                        TipoCarica = 0,
                        //Str_ListaElenco = "",
                        //StrUp_DescrLista = ""
                    };

                Azionisti.ConfermaVotiDaInterruzione(vz);
            }
		    return 0;
		}

        // ******************************************************************
        // ----------------------------------------------------------------
        // CONFIGURAZIONE
        // ----------------------------------------------------------------
        // ******************************************************************

        #region Finestra Configurazione

        private void timConfigura_Tick(object sender, EventArgs e)
        {
            timConfigura.Enabled = false;
            if (Stato == TAppStato.ssvBadge) MostraFinestraConfig();
        }

        private void MostraFinestraConfig()
        {
            fConfig = new frmConfig(); //TotCfg);
            fConfig.ConfiguraLettore += new ehConfiguraLettore(OnConfiguraLettore);
            fConfig.SalvaConfigurazioneLettore += new ehSalvaConfigurazioneLettore(OnSalvaConfigurazioneLettore);
            fConfig.ConfiguraSemaforo += new ehConfiguraSemaforo(OnConfiguraSemaforo);
            fConfig.StatoSemaforo += new ehStatoSemaforo(OnStatoSemaforo);

            fConfig.Configura();
            fConfig.ShowDialog();
            fConfig = null;
 
            // aggiorna il componente (lo faccio comunque)
            CfgLettore(VTConfig.UsaLettore, VTConfig.PortaLettore);
            OnConfiguraSemaforo(this, VTConfig.UsaSemaforo,
                VTConfig.IP_Com_Semaforo, VTConfig.TipoSemaforo);

            // metto il semaforo libero
            oSemaforo.SemaforoLibero();
        }

        public void OnConfiguraLettore(object sender, bool AUsaLettore, int AComPort)
        {
            // aggiorna il componente
            CfgLettore(AUsaLettore, AComPort);
        }

        public void OnSalvaConfigurazioneLettore(object sender, bool AUsaLettore, int AComPort,
                string ASemComPort, bool AUsaSemaforo)
        {
            // aggiorna le variabili
            VTConfig.UsaLettore = AUsaLettore;
            VTConfig.PortaLettore = AComPort;
            if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_COM)
            {
                VTConfig.UsaSemaforo = AUsaSemaforo;
                VTConfig.IP_Com_Semaforo = ASemComPort;
            }
            // salva la configurazione sul database
            if (oDBDati.SalvaConfigurazione(NomeTotem) == 1)
                MessageBox.Show("Configurazione salvata sul database", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            //// aggiorna il componente (non serve)
            //CfgLettore(AUsaLettore, AComPort);
        }

        public void CfgLettore(bool EUsaLettore, int EComPort)
        {
            // aggiorna il componente
            if (EUsaLettore)
            {
                NewReader.Close();
                NewReader.PortName = "COM" + EComPort.ToString();
                NewReader.Open();
            }
            else
            {
                NewReader.Close();
                NewReader.PortName = "COM" + EComPort.ToString();
            }

        }

        public void OnConfiguraSemaforo(object sender, bool AUsaSemaforo, 
            string AComPort, int ATipoSemaforo)
        {
            // cambia port semaforo
            oSemaforo.AttivaSemaforo(false);
            if (AUsaSemaforo)
            {
                oSemaforo.ConnAddress = AComPort;
                oSemaforo.AttivaSemaforo(true);
                oSemaforo.SemaforoLibero();
            }
        }

        public void OnStatoSemaforo(object sender, TStatoSemaforo AStato)
        {
            // ribatto il comando
            switch (AStato)
            {
                case TStatoSemaforo.stsOccupato:
                    oSemaforo.SemaforoOccupato();
                    break;
                case TStatoSemaforo.stsLibero:
                    oSemaforo.SemaforoLibero();
                    break;
                case TStatoSemaforo.stsErrore:
                    oSemaforo.SemaforoErrore();
                    break;
                case TStatoSemaforo.stsFineoccupato:
                    oSemaforo.SemaforoFineOccupato();
                    break;
            }
        }

        #endregion

        // ----------------------------------------------------------------
		//    Varie
		// ----------------------------------------------------------------

        #region Varie

        private void SemaforoOKImg(bool bok)
        {
            if (bok)
                pnSemaf.BackColor = Color.LimeGreen;
            else
                pnSemaf.BackColor = Color.Red;
        }
        
        private static bool PrimoAvvio
        {
            get
            {
                bool primoAvvio = false;
                appMutex = new Mutex(true, "VotoTouch.exe", out primoAvvio);
                return primoAvvio;
            }
        }

        public Screen GetSecondaryScreen()
        {
            if (Screen.AllScreens.Length == 1)
            {
                return null;
            }

            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Primary == false)
                {
                    return screen;
                }
            }

            return null;
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Q : USCITA
            if ((e.Control && e.KeyCode == Keys.Q) || (e.Alt && e.KeyCode == Keys.Q))
            {
                if (MessageBox.Show(rm.GetString("SAPP_CLOSE"), "Question",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    Application.Exit();
            }

            // Ctrl + S : Configurazione
            if (e.Control && e.KeyCode == Keys.S) 
            {
                if (Stato == TAppStato.ssvBadge) MostraFinestraConfig();
            }
            
            // Stato
            if ((e.Alt && e.KeyCode == Keys.S) || (e.Control && e.KeyCode == Keys.W))
            {
                MostraPannelloStato();
            }

            // Stato Azionista
            if (e.Alt && e.KeyCode == Keys.A)
            {
                MostaPannelloStatoAzionista();
            }

            // Unit� di test programma
            if (e.Alt && e.KeyCode == Keys.T)
            {
                FTest formTest = new FTest(oDBDati, this);
                formTest.ShowDialog();
                formTest = null;
            }

            // Ctrl + 2 Va sul secondo schermo
            if (e.Control && e.KeyCode == Keys.F2)
            {
                if (Screen.AllScreens.Length > 1)
                {
                    // Important !
                    this.StartPosition = FormStartPosition.Manual;
                    this.WindowState = FormWindowState.Normal;
                    // Get the second monitor screen
                    Screen screen = GetSecondaryScreen();
                    // set the location to the top left of the second screen
                    this.Location = screen.WorkingArea.Location;
                    // set it fullscreen
                    this.Size = new Size(screen.WorkingArea.Width, screen.WorkingArea.Height);
                }
            }

            // Ctrl + 1 Massimizza la finestra
            if (e.Control && e.KeyCode == Keys.F1)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            // Ctrl + F8 mette la risoluzione a 1280*1024
            if (e.Control && e.KeyCode == Keys.F8)
            {
                this.WindowState = FormWindowState.Normal;
                this.Width = 1280;
                this.Height = 1024;
                //this.
            }
            // Ctrl + F9 mette la risoluzione a 1024*768
            if (e.Control && e.KeyCode == Keys.F9)
            {
                this.WindowState = FormWindowState.Normal;
                this.Width = 1024;
                this.Height = 768;
                //this.
            }

        }

        private void MostraPannelloStato()
        {
            TNewLista a;
            int i, z;
            
            //lbVersion.Visible = true;
            Panel4.Left = this.Width - Panel4.Width - 5;
            Panel4.Top = 5;
            label1.Text = "Informazioni sulla Versione;";

            lbVersion.Items.Clear();
            lbVersion.Items.Add(VSDecl.VTS_VERSION);
#if _DBClose
            lbVersion.Items.Add("DBClose version");
#endif
            lbVersion.Items.Add("Configurazione");
            lbVersion.Items.Add("Usalettore: " + VTConfig.UsaLettore.ToString() + " Porta: " + VTConfig.PortaLettore.ToString());
            lbVersion.Items.Add("UsaSemaforo: " + VTConfig.UsaSemaforo.ToString() + " IP: " + VTConfig.IP_Com_Semaforo.ToString());
            lbVersion.Items.Add("IDSeggio: " + VTConfig.IDSeggio.ToString() + " NomeComputer: " + NomeTotem);
            lbVersion.Items.Add("ControllaPresenze: " + VTConfig.ControllaPresenze.ToString() +
                " CodiceUscita: " + VTConfig.CodiceUscita);
            lbVersion.Items.Add("SalvaLinkVoto: " + VTConfig.SalvaLinkVoto.ToString());
            lbVersion.Items.Add("SalvaVotoNonConfermato: " + VTConfig.SalvaVotoNonConfermato.ToString());
            lbVersion.Items.Add("IDSchedaUscitaForzata: " + VTConfig.IDSchedaUscitaForzata.ToString());
            lbVersion.Items.Add("AbilitaDirittiNonVoglioVotare: " + VTConfig.AbilitaDirittiNonVoglioVotare.ToString());
            lbVersion.Items.Add("");
            // le votazioni
            foreach (TNewVotazione fVoto in Votazioni.Votazioni)
            {
                lbVersion.Items.Add("Voto: " + fVoto.IDVoto.ToString() + ", Tipo: " +
                    fVoto.TipoVoto.ToString() + ", " + fVoto.Descrizione);
                lbVersion.Items.Add("   NListe: " + fVoto.NListe + ", MaxScelte: " +
                    fVoto.MaxScelte);
                lbVersion.Items.Add("   SKBianca: " + fVoto.SkBianca.ToString() +
                    ", SKNonVoto: " + fVoto.SkNonVoto);
                // Le liste
                for (z = 0; z < fVoto.NListe; z++)
                {
                    a = (TNewLista)fVoto.Liste[z];
                    lbVersion.Items.Add("    Lista:" + a.IDLista.ToString() + ", IdSk:" +
                        a.IDScheda.ToString() + ", " + a.DescrLista + ", p" +
                        a.Pag.ToString() + " " + a.PagInd + "  cda: " + a.PresentatodaCDA.ToString());
                }
            }
            Panel4.Visible = true;

        }

        private void MostaPannelloStatoAzionista()
        {
            int i;
            Panel4.Left = this.Width - Panel4.Width - 5;
            Panel4.Top = 5;
            label1.Text = "Informazioni sull'Azionista";

            lbVersion.Items.Clear();
            lbVersion.Items.Add(VSDecl.VTS_VERSION);
#if _DBClose
            lbVersion.Items.Add("DBClose version");
#endif
            foreach (TAzionista c in Azionisti.Azionisti)
            {
                lbVersion.Items.Add("Badge: " + c.IDBadge.ToString() + " " + c.RaSo.Trim());
                lbVersion.Items.Add("   IDazion:" + c.IDAzion.ToString() + " *** IDVotaz: " + c.IDVotaz.ToString());
                lbVersion.Items.Add("   ProgDeleg:" + c.ProgDeleg.ToString() + " Coaz:" + c.CoAz +
                            " AzOrd: " + c.NAzioni.ToString());

            }
            Panel4.Visible = true;
        }

        private void edtBadge_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (DebugMode)
            {
                // il tasto pi� aumenta di uno edtBadge
                if (e.KeyChar == '+')
                {
                    e.KeyChar = (char)0;
                    //int bb = Convert.ToInt32(edtBadge.Text);
                    Badge_Letto++;
                    edtBadge.Text = Badge_Letto.ToString();
                }
            }
        }
        
        private void btnExitVoto_Click(object sender, EventArgs e)
        {
            BadgeLetto("999999");
        }

        private void btnRipetiz_Click(object sender, EventArgs e)
        {
            BadgeLetto("88889999");
        }

        public void onTouchWatchDog(object source, int VParam)
        {
            Logging.WriteToLog("     >> Touch Watchdog intervenuto");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

             // ricarico le liste
            if (Stato == TAppStato.ssvVotoStart)
            {
                if (MessageBox.Show(rm.GetString("SAPP_CLOSE"), "Question",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    Application.Exit();
            }
            else
                MessageBox.Show(rm.GetString("SAPP_CLOSE_ERR"), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnRicaricaListe_Click(object sender, EventArgs e)
        {
            // ricarico le liste
            if (Stato == TAppStato.ssvVotoStart)
            {
                if (MessageBox.Show("Questa operazione ricaricher� le liste/votazioni rileggendole " +
                    "dal database?\n Vuoi veramente continuare?", "Question",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Rectangle FFormRect = new Rectangle(0, 0, this.Width, this.Height);
                    bool pippo = Votazioni.CaricaListeVotazioni(Data_Path, FFormRect, false);
                    if (pippo)
                        MessageBox.Show("Liste/votazioni caricate correttamente.", "information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Problemi nel caricamento Liste/votazioni.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            }
            else
                MessageBox.Show("Impossibile effettuare questa operazione durante la votazione.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnCloseInfo_Click(object sender, EventArgs e)
        {
            Panel4.Visible = false;
        }

        private void edtBadge_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                BadgeLetto(edtBadge.Text);
        }

        private void btnCancVoti_Click(object sender, EventArgs e)
        {
#if DEBUG
            if (MessageBox.Show("Questa operazione canceller� TUTTI i voti " +
                "dal database?\n Vuoi veramente continuare?", "Question",
            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                oDBDati.CancellaTuttiVoti();
            }
#else
            MessageBox.Show("Funzione non disponibile", "Exclamation",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
#endif
        }

        public static string UppercaseWords(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }


        #endregion

		// ----------------------------------------------------------------
		//    PARTE DEMO MODE
		// ----------------------------------------------------------------

        #region Demo mode

        public void InizializzaControlliDemo()
        {
            Font myFont = new Font("Impact", 32, FontStyle.Bold);
            // devo aggiungere due bottoni
            btnBadgeUnVoto = new Button();
            btnBadgeUnVoto.FlatStyle = FlatStyle.Flat;
            btnBadgeUnVoto.Text = rm.GetString("SAPP_DEMO_1DIR");   // "Tocca per provare con 1 diritto di voto";
            btnBadgeUnVoto.Font = myFont;
            btnBadgeUnVoto.Click += new EventHandler(btnBadgeUnVoto_Click);
            btnBadgeUnVoto.Visible = false;
            this.Controls.Add(btnBadgeUnVoto);

            btnBadgePiuVoti = new Button();
            btnBadgePiuVoti.FlatStyle = FlatStyle.Flat;
            btnBadgePiuVoti.Text = rm.GetString("SAPP_DEMO_3DIR");  // "Tocca per provare con 3 diritti di voto";
            btnBadgePiuVoti.Font = myFont;
            btnBadgePiuVoti.Click += new EventHandler(btnBadgePiuVoti_Click);
            btnBadgePiuVoti.Visible = false;
            this.Controls.Add(btnBadgePiuVoti);

            btnFineVotoDemo = new Button();
            btnFineVotoDemo.FlatStyle = FlatStyle.Flat;
            btnFineVotoDemo.Text = rm.GetString("SAPP_DEMO_3END"); // "Tocca per ritornare alla videata iniziale";
            btnFineVotoDemo.Font = myFont;
            btnFineVotoDemo.Click += new EventHandler(btnFineVotoDemo_Click);
            btnFineVotoDemo.Visible = false;
            this.Controls.Add(btnFineVotoDemo);
        }

        public void onChangeSemaphore(object source, TStatoSemaforo ASemStato)
        {
            // evento inutile
        }

        void btnBadgeUnVoto_Click(object sender, EventArgs e)
        {
            //1 voto
            //MessageBox.Show("Un diritto di voto", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            BadgeLetto("1000");

        }

        void btnBadgePiuVoti_Click(object sender, EventArgs e)
        {
            //3 voti
            //MessageBox.Show("Tre diritti di voto", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            BadgeLetto("1001");
        }

        void btnFineVotoDemo_Click(object sender, EventArgs e)
        {
            BadgeLetto("999999");
        }


        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            //StartTest();
            ////TListaAzionisti azio = new TListaAzionisti(oDBDati);
            ////azio.CaricaDirittidiVotoDaDatabase(10005, ref fVoto, NVoti);

            ////List<TAzionista> aziofilt = azio.DammiDirittiDiVotoPerIDVotazione(1, true);

            //TListaVotazioni vot = new TListaVotazioni(oDBDati);
            //vot.CaricaListeVotazioni();
        }



    }  // public class frmMain....
}  // Namespace....
