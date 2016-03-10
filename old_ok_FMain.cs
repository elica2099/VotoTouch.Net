using System;
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

namespace VotoSegreto
{

	/// <summary>
	/// Summary description for Form1.\
	/// </summary>
    public partial class frmMain : Form
	{

		//
		// VERSIONE CON TEST DELEGHE (CORRETTA)
		//

        public delegate void EventDataReceived(object source, string messaggio);
        public event EventDataReceived evtDataReceived;

        // ----------------------------------------
        // LE COSTANTI SONO IN VSDecl.
        // ----------------------------------------
        
        private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label lbDirittiDiVoto;
        private System.Windows.Forms.Label lbDirittiStart;
		private System.Windows.Forms.Label lbConferma;
        private System.Windows.Forms.Label lbConfermaNVoti;
		private System.Windows.Forms.Label lbNome;
		private System.Windows.Forms.Label lbNomeDisgiunto;
        private System.Windows.Forms.Label lbDisgiuntoRimangono;
        private System.Windows.Forms.Panel Panel4;
        private System.Windows.Forms.ListBox lbVersion;
        private System.Windows.Forms.PictureBox imgSemOk;
        private System.Windows.Forms.PictureBox imgSemNo;
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
        private ProgressBar prbSalvaTutto;
        

        // timer di disaccoppiamento
        private System.Windows.Forms.Timer timLetturaBadge;
        private System.Windows.Forms.Timer timCambiaStato;
        private System.Windows.Forms.Timer timConfigura;

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

        // finestre
        public FVSMessage frmVSMessage;
        //public FVSMessageExit frmVSMessageExit;
        public frmConfig fConfig;
        public SplashScreen splash;
        public FVSStart frmStart;

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
        public TTotemConfig TotCfg;             // configurazione
        // Variabili normali
        public string Data_Path;                // path della cartella data
        //public string Img_path;                 // path delle immagini
        // altri
        public string   NomeTotem;              // nome della macchina
        public string   LogNomeFile;            // nome file del log
        public string   LogVotiNomeFile;            // nome file del log
        public bool CtrlPrimoAvvio;         // serve per chiudere la finestra in modo corretto
        // votazione
        public TParVotazione[] FParVoto;        // array delle votazioni
        public int NVoti;                       // ntot di votazioni
        // Voti da salvare
        public ArrayList FVotiDaSalvare;        // array del salvataggio finale di TVotiDaSalvare
         // cpontrollo degli eventi di voto
        private bool AperturaVotoEsterno;
      
        // ciclo della votazione
        public int CurrVoteIDX;
        // votazioni differenziate
        public bool VoteDiff;
        // Variabile temporanea Voti Espressi
        public int VotoEspresso;
        public int VotoEspressoCarica;
        public string VotoEspressoStr;
        public string VotoEspressoStrUp;
        // Variabile temporanea voti espressi Nuova Versione (Array)
        public ArrayList FVotiExpr; 

        // flag uscita in votazione
        public bool UscitaInVotazione;

        // Dati dell'azionista e delle deleghe che si porta dietro
        public int FNAzionisti;				// n. di azionisti	
        public ArrayList FAzionisti;        // Collection di clsAzionisti

        // risorse per l'internazionalizzazione
        ResourceManager rm;

        // ********************DA VEDERE************************************
		public int CurrIdAzionDelega;		// Indice alla delega in voto corrente di FAzionisti nel differenziato
		public DatiUtente DatiUsr;
		public int utente_voti_bak;
		public int utente_voti_diff;
		public int Badge_Letto;
        public string Badge_Seriale;
        // ********************DA VEDERE************************************

		public frmMain()
		{
            // DR11 OK
			int dbr;

			//
			// Required for Windows Form Designer support
			//
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
           
            // ritrovo il nome della macchina che mi servirà per interrogare il db
			int i;
			NomeTotem = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
			for (i = 0; i< NomeTotem.Length; i++)
				if (NomeTotem[i] == '\\' ) break;
			NomeTotem = NomeTotem.Remove(0, i+1);

            // ok, per prima cosa verifico se c'è la cartella c:\data, se si ok
            // sennò devo considerare la cartella dell'applicazione, se non c'è esco
            oVotoImg = new CVotoImages();
            oVotoImg.MainForm = this;
            CtrlPrimoAvvio = oVotoImg.CheckDataFolder(ref Data_Path);

            btnCancVoti.Visible = System.IO.File.Exists(Data_Path + "VTS_ADMIN.txt");

            // identificazione della versione demo, nella cartella data o nella sua cartella
            if (System.IO.File.Exists(Data_Path + "VTS_DEMO.txt"))
            {
                // Ok è la versione demo
                DemoVersion = true;
                LogNomeFile = Logging.GenerateDefaultLogFileName(Data_Path, "VotoSegreto_" + NomeTotem);
                Logging.WriteToLog((LogNomeFile), "---- DEMO MODE ----");
                // ok, ora creo la classe che logga i voti
                LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(Data_Path, "VotoS_" + NomeTotem);
            }
            else
            {
                // ok, qua devo vedere i due casi:
                // il primo è VTS_STANDALONE.txt presente il che vuol dire che ho la configurazione
                // in locale, caricando comunque un file GEAS.sql da data
                if (System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt"))
                {

                    LogNomeFile = Logging.GenerateDefaultLogFileName(Data_Path, "VotoSegreto_" + NomeTotem);
                    Logging.WriteToLog((LogNomeFile), "---- STANDALONE MODE ----");
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
                    if (!System.IO.Directory.Exists(@"M:\LST\VotoSegreto\"))
                        System.IO.Directory.CreateDirectory(@"M:\LST\VotoSegreto\");
                    LogNomeFile = Logging.GenerateDefaultLogFileName(@"M:\LST\VotoSegreto\",
                            "VotoSegreto_" + NomeTotem);
                }
            }
            // loggo l'inizio dell'applicazione
            Logging.WriteToLog((LogNomeFile), "<start> Inizio Applicazione");
            splash.SetSplash(20, rm.GetString("SAPP_START_INITDB"));    // "Inizializzo database...");

            // identificazione DebugMode
            DebugMode = System.IO.File.Exists(Data_Path + "VTS_DEBUG.txt");
            PaintTouch = System.IO.File.Exists(Data_Path + "VTS_PAINT_TOUCH.txt");

            // Inizializzo la classe del database, mi servirà prima delle altre classi perché in
            // questa versione la configurazione è centralizzata sul db
            if (DemoVersion)
                oDBDati = new CVotoFileDati();
            else
                oDBDati = new CVotoDBDati();

            oDBDati.FDBConfig = DBConfig;
            oDBDati.LogNomeFile = LogNomeFile;
            oDBDati.NomeTotem = NomeTotem;
            oDBDati.ProgressoSalvaTutto += new ehProgressoSalvaTutto(onProgressoSalvaTutto);
            // se è standalone prende i dati in locale
            oDBDati.ADataLocal = System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt");
            oDBDati.AData_path = Data_Path;
            if (!oDBDati.CaricaConfig())
            {
                Logging.WriteToLog((LogNomeFile), "<dberror> Problemi nel caricamento della configurazione DB, mappatura");
                MessageBox.Show(rm.GetString("SAPP_START_ERRCFG"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CtrlPrimoAvvio = false;
                return;
            }

            // vado avanti con il database
            // mi connetto
            dbr = oDBDati.DBConnect();
            splash.SetSplash(30, rm.GetString("SAPP_START_INITCFG"));    //"Carico configurazione..."
            if (dbr == 1)
            {
                int DBOk = 0;  // variabile di controllo sul caricamento
                // leggo la configurazione del badge/impianto
                DBOk += oDBDati.CaricaConfigDB(ref TotCfg.BadgeLen, ref TotCfg.CodImpianto);
                splash.SetSplash(40, rm.GetString("SAPP_START_INITPREF"));   //"Carico preferenze..."
                // leggo la configurazione generale
                DBOk += oDBDati.DammiConfigDatabase(ref TotCfg);
                // leggo la configurazione del singolo totem
                DBOk += oDBDati.DammiConfigTotem(NomeTotem, ref TotCfg);
                splash.SetSplash(50, rm.GetString("SAPP_START_INITVOT"));  //"Carico liste e votazioni..."

                if (TotCfg.VotoAperto) Logging.WriteToLog((LogNomeFile), "Votazione già aperta");

                // carica le votazioni, le carica comunque all'inizio
                DBOk += CaricaListeVotazioni();

                // ok, finisce
                if (DBOk == 0)
                {
                    // nel log va tutto bene
                    Logging.WriteToLog((LogNomeFile), "<startup> Caricamento dati database OK");
                    // ora secondo TotCfg.AbilitalogV creo il file del log per le versioni Standalone e normale
                    if (TotCfg.AbilitaLogV)
                    {
                        // Inizializzo il log dei voti
                        if (System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt"))
                            LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(Data_Path, "VotoS_" + NomeTotem);
                        else
                            LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(@"M:\LST\VotoSegreto\",
                                    "VotoS_" + NomeTotem);
                    } 
                    if (TotCfg.AbilitaLogV) LogVote.WriteToLog((LogVotiNomeFile), "------ Inizio Applicazione ------");
                }
                else
                {
                    Logging.WriteToLog((LogNomeFile), "<dberror> Problemi nel caricamento configurazione db");
                    MessageBox.Show(rm.GetString("SAPP_START_ERRDB"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Logging.WriteToLog((LogNomeFile), "<dberror> Problemi nella connessione al Database");
                MessageBox.Show(rm.GetString("SAPP_START_ERRCONN"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CtrlPrimoAvvio = false;
                return;
            }

            splash.SetSplash(60, rm.GetString("SAPP_START_INITBARCODE"));   //"inizializzo lettore barcode...");
            // A questo punto la configurazione è attiva e caricata centralmente, posso continuare
            // il lettore del badge
            NewReader = new CNETActiveReader();
            NewReader.ADataRead += ObjDataReceived;
            evtDataReceived += new EventDataReceived(onDataReceived);
            // lo attiverà nel load
            if (TotCfg.UsaLettore)
            {
                NewReader.PortName = "COM" + TotCfg.PortaLettore.ToString();
            }

            splash.SetSplash(70, rm.GetString("SAPP_START_INITSEM"));       //"Inizializzo Semaforo..."
            // il semaforo, ora fa tutto lei
            SemaforoOKImg(TotCfg.UsaSemaforo);
            // ok, in funzione del tipo di semaforo faccio
            if (TotCfg.TipoSemaforo == VSDecl.SEMAFORO_IP)
                // USARE SEMPRE CIPThreadSemaphore
                oSemaforo = new CIPThreadSemaphore();
            else
                oSemaforo = new CComSemaphore();
            // se è attivato lo setto
            oSemaforo.LogNomeFile = LogNomeFile;
            oSemaforo.ConnAddress = TotCfg.IP_Com_Semaforo;  //  deve essere "COM1" o "COMn"
            oSemaforo.ChangeSemaphore += onChangeSemaphore;
            if (TotCfg.UsaSemaforo)
                oSemaforo.AttivaSemaforo(true);

            splash.SetSplash(80, rm.GetString("SAPP_START_INITTOUCH"));       // "Inizializzo Touch..."
            // array dei voti da salvare
            FVotiDaSalvare = new ArrayList();
            // array dei voti temporanei
            FVotiExpr = new ArrayList();
            // azionisti
            FAzionisti = new ArrayList();
            FNAzionisti = 0;
            // Classe del TouchScreen
            oVotoTouch = new CVotoTouchScreen();
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
            // multivotazione
            oVotoTouch.PremutoMultiAvanti += new ehPremutoMultiAvanti(onPremutoVotoValidoMulti);
            oVotoTouch.PremutoMulti += new ehPremutoMulti(onPremutoVotoMulti);

            // classe del tema
            oVotoTheme = new CVotoTheme();
            oVotoTheme.CaricaTemaDaXML(oVotoImg.Img_path);

            // ok, ora preparo alcune variabili
            CurrVoteIDX = 0;                // parte alla prima votazione
            VoteDiff = false;               // non è differenziata
            Badge_Letto = 0;
            AperturaVotoEsterno = TotCfg.VotoAperto;  // lo setto uguale così in stato badge non carica 2 volte le Liste
            Badge_Seriale = "";
            UscitaInVotazione = false;

            // i timer di disaccoppiamento funzioni (non potendo usare WM_USER!!!!)
            // timer di lettura badge
            timLetturaBadge = new System.Windows.Forms.Timer();
            timLetturaBadge.Enabled = false;
            timLetturaBadge.Interval = 30;
            timLetturaBadge.Tick += timLetturaBadge_Tick;
            // timer di cambio stato
            timCambiaStato = new System.Windows.Forms.Timer();
            timCambiaStato.Enabled = false;
            timCambiaStato.Interval = 30;
            timCambiaStato.Tick += timCambiaStato_Tick;
            // timer di configurazione
            timConfigura = new System.Windows.Forms.Timer();
            timConfigura.Enabled = false;
            timConfigura.Interval = 30;
            timConfigura.Tick += timConfigura_Tick;

            splash.SetSplash(90, rm.GetString("SAPP_START_INITVAR"));   //"Inizializzo variabili...");
            // scrive la configurazione nel log
            Logging.WriteToLog((LogNomeFile), VSDecl.VTS_VERSION);
            Logging.WriteToLog((LogNomeFile), "** Configurazione:");
            Logging.WriteToLog((LogNomeFile), "   Usalettore: " + TotCfg.UsaLettore.ToString());
            Logging.WriteToLog((LogNomeFile), "   Porta: " + TotCfg.PortaLettore.ToString());
            Logging.WriteToLog((LogNomeFile), "   UsaSemaforo: " + TotCfg.UsaSemaforo.ToString());
            Logging.WriteToLog((LogNomeFile), "   IPSemaforo: " + TotCfg.IP_Com_Semaforo.ToString());
            Logging.WriteToLog((LogNomeFile), "   IDSeggio: " + TotCfg.IDSeggio.ToString());
            Logging.WriteToLog((LogNomeFile), "   NomeComputer: " + NomeTotem);
            Logging.WriteToLog((LogNomeFile), "   ControllaPresenze: " + TotCfg.ControllaPresenze.ToString());
            Logging.WriteToLog((LogNomeFile), "** CodiceUscita: " + TotCfg.CodiceUscita);
            Logging.WriteToLog((LogNomeFile), "");
            
            // inizializzo i componenti
			InizializzaControlli();
            // Se è in demo mode metto i controlli
            if (DemoVersion)
                InizializzaControlliDemo();

			// ora inizializzo la macchina a stati
			Stato = TAppStato.ssvBadge;
            
            splash.SetSplash(100);   
            splash.Hide();

            // se sono in debug evidenzio le zone sensibili
            oVotoTouch.PaintTouchOnScreen = PaintTouch;

            // se la votazione è aperta il timer di controllo voto batte di meno
            if (TotCfg.VotoAperto)
                timVotoApero.Interval = VSDecl.TIM_CKVOTO_MAX;
            else
                timVotoApero.Interval = VSDecl.TIM_CKVOTO_MIN;
           
            // Attivo la macchina a stati
            CambiaStato();

        }

        
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
            this.lbConferma = new System.Windows.Forms.Label();
            this.lbConfermaNVoti = new System.Windows.Forms.Label();
            this.lbNome = new System.Windows.Forms.Label();
            this.lbNomeDisgiunto = new System.Windows.Forms.Label();
            this.lbDisgiuntoRimangono = new System.Windows.Forms.Label();
            this.Panel4 = new System.Windows.Forms.Panel();
            this.btnCancVoti = new System.Windows.Forms.Button();
            this.btnRicaricaListe = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCloseInfo = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lbVersion = new System.Windows.Forms.ListBox();
            this.pnBadge = new System.Windows.Forms.Panel();
            this.btnRipetiz = new System.Windows.Forms.Button();
            this.btnExitVoto = new System.Windows.Forms.Button();
            this.btmBadge = new System.Windows.Forms.Button();
            this.edtBadge = new System.Windows.Forms.TextBox();
            this.timVotoApero = new System.Windows.Forms.Timer(this.components);
            this.pbSalvaDati = new System.Windows.Forms.PictureBox();
            this.imgSemNo = new System.Windows.Forms.PictureBox();
            this.imgSemOk = new System.Windows.Forms.PictureBox();
            this.lbConfermaUp = new System.Windows.Forms.Label();
            this.prbSalvaTutto = new System.Windows.Forms.ProgressBar();
            this.lbNomeAzStart = new System.Windows.Forms.Label();
            this.Panel4.SuspendLayout();
            this.pnBadge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSalvaDati)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSemNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSemOk)).BeginInit();
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
            // lbConferma
            // 
            this.lbConferma.BackColor = System.Drawing.Color.Transparent;
            this.lbConferma.Font = new System.Drawing.Font("Arial", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConferma.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbConferma.Location = new System.Drawing.Point(578, 455);
            this.lbConferma.Name = "lbConferma";
            this.lbConferma.Size = new System.Drawing.Size(308, 136);
            this.lbConferma.TabIndex = 91;
            this.lbConferma.Text = "dsfsd";
            this.lbConferma.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbConferma.Visible = false;
            // 
            // lbConfermaNVoti
            // 
            this.lbConfermaNVoti.BackColor = System.Drawing.Color.Transparent;
            this.lbConfermaNVoti.Font = new System.Drawing.Font("Arial", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConfermaNVoti.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(0)))), ((int)(((byte)(3)))));
            this.lbConfermaNVoti.Location = new System.Drawing.Point(390, 502);
            this.lbConfermaNVoti.Name = "lbConfermaNVoti";
            this.lbConfermaNVoti.Size = new System.Drawing.Size(180, 56);
            this.lbConfermaNVoti.TabIndex = 92;
            this.lbConfermaNVoti.Text = "12 voti";
            this.lbConfermaNVoti.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbConfermaNVoti.Visible = false;
            // 
            // lbNome
            // 
            this.lbNome.BackColor = System.Drawing.Color.Transparent;
            this.lbNome.Font = new System.Drawing.Font("Century Gothic", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNome.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(0)))), ((int)(((byte)(3)))));
            this.lbNome.Location = new System.Drawing.Point(13, 429);
            this.lbNome.Name = "lbNome";
            this.lbNome.Size = new System.Drawing.Size(144, 56);
            this.lbNome.TabIndex = 106;
            this.lbNome.Text = "12 voti";
            this.lbNome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbNome.Visible = false;
            // 
            // lbNomeDisgiunto
            // 
            this.lbNomeDisgiunto.BackColor = System.Drawing.Color.Transparent;
            this.lbNomeDisgiunto.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNomeDisgiunto.ForeColor = System.Drawing.Color.Black;
            this.lbNomeDisgiunto.Location = new System.Drawing.Point(206, 498);
            this.lbNomeDisgiunto.Name = "lbNomeDisgiunto";
            this.lbNomeDisgiunto.Size = new System.Drawing.Size(200, 51);
            this.lbNomeDisgiunto.TabIndex = 107;
            this.lbNomeDisgiunto.Text = "label3";
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
            this.Panel4.Controls.Add(this.btnCancVoti);
            this.Panel4.Controls.Add(this.btnRicaricaListe);
            this.Panel4.Controls.Add(this.label1);
            this.Panel4.Controls.Add(this.btnCloseInfo);
            this.Panel4.Controls.Add(this.button1);
            this.Panel4.Controls.Add(this.lbVersion);
            this.Panel4.Location = new System.Drawing.Point(519, 12);
            this.Panel4.Name = "Panel4";
            this.Panel4.Size = new System.Drawing.Size(300, 440);
            this.Panel4.TabIndex = 118;
            this.Panel4.Visible = false;
            // 
            // btnCancVoti
            // 
            this.btnCancVoti.BackColor = System.Drawing.Color.White;
            this.btnCancVoti.Location = new System.Drawing.Point(117, 364);
            this.btnCancVoti.Name = "btnCancVoti";
            this.btnCancVoti.Size = new System.Drawing.Size(64, 31);
            this.btnCancVoti.TabIndex = 129;
            this.btnCancVoti.Text = "Canc Voti";
            this.btnCancVoti.UseVisualStyleBackColor = false;
            // 
            // btnRicaricaListe
            // 
            this.btnRicaricaListe.Location = new System.Drawing.Point(9, 361);
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
            this.button1.Location = new System.Drawing.Point(188, 361);
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
            this.lbVersion.Size = new System.Drawing.Size(294, 316);
            this.lbVersion.TabIndex = 0;
            // 
            // pnBadge
            // 
            this.pnBadge.BackColor = System.Drawing.Color.Transparent;
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
            // btnRipetiz
            // 
            this.btnRipetiz.BackColor = System.Drawing.Color.White;
            this.btnRipetiz.Location = new System.Drawing.Point(76, 84);
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
            this.btnExitVoto.Location = new System.Drawing.Point(3, 84);
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
            this.btmBadge.Location = new System.Drawing.Point(72, 49);
            this.btmBadge.Name = "btmBadge";
            this.btmBadge.Size = new System.Drawing.Size(70, 26);
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
            // imgSemNo
            // 
            this.imgSemNo.Image = ((System.Drawing.Image)(resources.GetObject("imgSemNo.Image")));
            this.imgSemNo.Location = new System.Drawing.Point(6, 4);
            this.imgSemNo.Name = "imgSemNo";
            this.imgSemNo.Size = new System.Drawing.Size(12, 12);
            this.imgSemNo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgSemNo.TabIndex = 120;
            this.imgSemNo.TabStop = false;
            this.imgSemNo.Visible = false;
            // 
            // imgSemOk
            // 
            this.imgSemOk.Image = ((System.Drawing.Image)(resources.GetObject("imgSemOk.Image")));
            this.imgSemOk.Location = new System.Drawing.Point(4, 2);
            this.imgSemOk.Name = "imgSemOk";
            this.imgSemOk.Size = new System.Drawing.Size(12, 12);
            this.imgSemOk.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgSemOk.TabIndex = 119;
            this.imgSemOk.TabStop = false;
            this.imgSemOk.Visible = false;
            // 
            // lbConfermaUp
            // 
            this.lbConfermaUp.BackColor = System.Drawing.Color.Transparent;
            this.lbConfermaUp.Font = new System.Drawing.Font("Arial", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConfermaUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbConfermaUp.Location = new System.Drawing.Point(241, 288);
            this.lbConfermaUp.Name = "lbConfermaUp";
            this.lbConfermaUp.Size = new System.Drawing.Size(255, 45);
            this.lbConfermaUp.TabIndex = 126;
            this.lbConfermaUp.Text = "up";
            this.lbConfermaUp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbConfermaUp.Visible = false;
            // 
            // prbSalvaTutto
            // 
            this.prbSalvaTutto.Location = new System.Drawing.Point(53, 12);
            this.prbSalvaTutto.Name = "prbSalvaTutto";
            this.prbSalvaTutto.Size = new System.Drawing.Size(463, 23);
            this.prbSalvaTutto.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prbSalvaTutto.TabIndex = 127;
            this.prbSalvaTutto.Visible = false;
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
            // frmMain
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(844, 566);
            this.Controls.Add(this.pnBadge);
            this.Controls.Add(this.lbNomeAzStart);
            this.Controls.Add(this.prbSalvaTutto);
            this.Controls.Add(this.lbConfermaUp);
            this.Controls.Add(this.pbSalvaDati);
            this.Controls.Add(this.imgSemNo);
            this.Controls.Add(this.imgSemOk);
            this.Controls.Add(this.Panel4);
            this.Controls.Add(this.lbDisgiuntoRimangono);
            this.Controls.Add(this.lbNomeDisgiunto);
            this.Controls.Add(this.lbNome);
            this.Controls.Add(this.lbConfermaNVoti);
            this.Controls.Add(this.lbConferma);
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
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseUp);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.Panel4.ResumeLayout(false);
            this.Panel4.PerformLayout();
            this.pnBadge.ResumeLayout(false);
            this.pnBadge.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSalvaDati)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSemNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgSemOk)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
      
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

            if (TotCfg.UsaLettore)
            {
                if (!NewReader.Open())
                {
                    // ci sono stati errori con la com all'apertura
                    TotCfg.UsaLettore = false;
                    MessageBox.Show(rm.GetString("SAPP_START_ERRCOM1") + TotCfg.PortaLettore + rm.GetString("SAPP_START_ERRCOM2"), "Error",
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
            // DR11 OK
            // ok, questa funzione serve all'oggetto CTouchscreen per evidenziare le zone sensibili
            if (oVotoTouch != null)
            {
                oVotoTouch.PaintTouch(sender, e);

                // controllo che CurrVoteIDX sia nel range giusto
                if (CurrVoteIDX < NVoti)
                {
                    // se la votazione corrente è di candidato su più pagine disegno i rettangoli
                    if (Stato == TAppStato.ssvVoto && 
                        (FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_CANDIDATO ||
                         FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_CANDIDATO_SING))
                    {
                        // paint delle label Aggiuntive
                        oVotoTheme.PaintlabelProposteCdaAlt(sender, e, ref FParVoto[CurrVoteIDX], true);
                        // paint dei Bottoni
                        oVotoTouch.PaintButtonCandidatoPagina(sender, e, false, oVotoTheme.BaseFontCandidato);
                    }
                    // se la votazione corrente è di MULTIcandidato su più pagine disegno i rettangoli
                    if (Stato == TAppStato.ssvVoto && FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                    {
                        // paint delle label Aggiuntive
                        oVotoTheme.PaintlabelProposteCdaAlt(sender, e, ref FParVoto[CurrVoteIDX], false);
                        // paint dei bottoni
                        oVotoTouch.PaintButtonCandidatoPagina(sender, e, true, oVotoTheme.BaseFontCandidato);
                    }

                    // ******* OBSOLETO ********/
                    // votazione VOTO_CANDIDATO_SING, candidato a singola pagina, disegno i rettangoli
                    //if (Stato == TAppStato.ssvVoto && (FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_CANDIDATO_SING))
                    //    oVotoTouch.PaintButtonCandidatoSingola(sender, e);
                }
                
                // se sono nello stato di votostart e il n. di voti è > 1
                if (Stato == TAppStato.ssvVotoStart && DatiUsr.utente_voti > 1)
                {
                    // faccio il paint del numero di diritti di voto nel bottone in basso a sx , 
                    // in questo caso uso un paint e non una label per un problema grafico di visibilità
                    oVotoTheme.PaintDirittiDiVoto(sender, e, DatiUsr.utente_voti);
                }
            }

            // se è demo devo stampare una label
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
                    // non faccio nulla, non serve, al massimo non apparirà la scritta
                }
            }
        }
         
        private void frmMain_Resize(object sender, EventArgs e)
        {
            // immagine del salvataggio
            pbSalvaDati.Left = (this.Width / 2) - (pbSalvaDati.Width / 2);
            pbSalvaDati.Top = (this.Height / 2) - (pbSalvaDati.Height  / 2);
            // devo dire alla nuova touch le dimensioni della finestra
            if (oVotoTouch != null)
            {
                oVotoTouch.FFormRect.X = 0; // this.Top;
                oVotoTouch.FFormRect.Y = 0; // this.Left;
                oVotoTouch.FFormRect.Height = this.Height;
                oVotoTouch.FFormRect.Width = this.Width;
            }
            // lo stesso faccio per la classe del thema che si occupa di disegnare 
            // le label di informazione
            if (oVotoTheme != null)
            {
                oVotoTheme.FFormRect.X = 0; // this.Top;
                oVotoTheme.FFormRect.Y = 0; // this.Left;
                oVotoTheme.FFormRect.Height = this.Height;
                oVotoTheme.FFormRect.Width = this.Width;

                CaricaTemaInControlli();
            }
            
            // ok, ora se è in demo mode faccio il resize dei controlli
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
                // bottone più voto
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
       
		// ----------------------------------------------------------------
		//		ROUTINE DI CARICAMENTO CONFIGURAZIONE
		// ----------------------------------------------------------------

        public int CaricaListeVotazioni()
        {
           //DR12 OK
            // questa routine serve a caricara/ricaricare le votazioni / liste
            // dal database ai file
            // è disegnata per essere richiamata in qualsiasi momento durante
            // l'esecuzione senza creare problemi
            // In realtà viene richhiamata in funzione del votoaperto
            // - durante il loading della finestra se il voto è già aperto
            // - all'apertura della votazione
            int DBOk = 0;
            Logging.WriteToLog((LogNomeFile), "Caricamento Liste/Votazioni");
 
            // testa se ci sono dati già caricati, se si li cancella
            if (FParVoto != null)
            {
                // cancello le collection
                for (int i = 0; i < FParVoto.Length; i++)
                {
                    FParVoto[i].Liste.Clear();
                    FParVoto[i].Pagine.Clear();
                }
            }
            // metto a nullo l'array
            FParVoto = null;
            
            // carica le votazioni
            DBOk += oDBDati.CaricaDatiVotazioni(ref NVoti, ref FParVoto);
            // carica i dettagli delle votazioni
            DBOk += oDBDati.CaricaDatiListe(ref NVoti, ref FParVoto);
            // Calcolo l'area di voto per Candidati e multicandidati
            CalcolaAreaDiVoto();
            // ok, ora ordino le liste nel caso in cui siano di candidato
            OrdinaListeInPagine();

            // NOTA: Nelle liste il nome può contenere anche la data di nascita, inserita
            // come token tra ( e ). Serve nel caso di omonimia. La routine di disegno riconoscerà
            // questo e lo tratterà come scritta piccola a lato

            return DBOk;
        }

        private void onProgressoSalvaTutto(object source, int ATot, int AProg)
        {
            if (prbSalvaTutto.Maximum != ATot) prbSalvaTutto.Maximum = ATot;
            prbSalvaTutto.Value = AProg;
        }


        //******************************************************************************
		// ----------------------------------------------------------------
		//		ROUTINE DI GESTIONE DEGLI STATI
		// ----------------------------------------------------------------
		//******************************************************************************

        #region Macchina A Stati

        private void CambiaStato()
        {
            // disaccoppia la funzione attraverso un timer che chiama un evento
            timCambiaStato.Enabled = true;
        }

        private void timCambiaStato_Tick(object sender, EventArgs e)
        {
            timCambiaStato.Enabled = false;
            CambiaStatoDaTimer();
        }
        
        private void CambiaStatoDaTimer()
        {
            clsAzionisti c;
            // gestione degli stati della votazione
            // Touchscreen
            oVotoTouch.CalcolaTouch(this, Stato, ref FParVoto[CurrVoteIDX], DatiUsr.utente_voti > 1);
            //
            switch (Stato)
            {
                case TAppStato.ssvBadge:

                    SettaComponenti(false);
                    // resetto la votazione, lo faccio sempre
                    CurrVoteIDX = 0;
                    UscitaInVotazione = false;
                    // labels
                    lbDirittiDiVoto.Text = "";
                    lbNomeAzStart.Text = "";
                    // ok ora testo eventuali eventi di apertura o chisura votazione dall'esterno
                    // questo mi viene dalla variabile AperturaVotoEsterno, che viene settata nell'evento
                    // di controllo del timer che testa la variabile votoaperto relativo alla postazione
                    // sul db. 
                    // All'avvio dell'applicazione AperturaVotoEsterno viene settato come la lettura
                    // sul db, quindi quando qui cambia vuol dire (in funzione del valore)
                    // che un evento di apertura/chiusura votazione è avvenuto
                    if (AperturaVotoEsterno != TotCfg.VotoAperto)
                    {
                        if (AperturaVotoEsterno)
                        {
                            Logging.WriteToLog((LogNomeFile), "Evento Apertura votazione");
                            CaricaListeVotazioni();
                        }
                        else
                            Logging.WriteToLog((LogNomeFile), "Evento Chiusura votazione");
                        // ok, ora setto la variabile locale di configurazione
                        TotCfg.VotoAperto = AperturaVotoEsterno;
                        // se la votazione è aperta il timer di controllo voto batte di meno
                        if (TotCfg.VotoAperto)
                            timVotoApero.Interval = VSDecl.TIM_CKVOTO_MAX;
                        else
                            timVotoApero.Interval = VSDecl.TIM_CKVOTO_MIN;
                    }

                    // a seconda dello stato, mostro il semaforo e metto l'immagine corretta
                    if (TotCfg.VotoAperto)
                    {
                        oSemaforo.SemaforoLibero();
                        oVotoImg.LoadImages(VSDecl.IMG_Badge);
                    }
                    else
                    {
                        oSemaforo.SemaforoChiusoVoto();
                        oVotoImg.LoadImages(VSDecl.IMG_Votochiuso);
                    }
                    break;

                case TAppStato.ssvVotoStart:
                    oSemaforo.SemaforoOccupato();
                    // quà metto il voto differenziato
                    MettiComponentiStartVoto();
                    // resetto comunque la delega che è sempre la prima anche nel caso di un voto solo
                    // nota: l'array degli azionisti parte da 1
                    CurrIdAzionDelega = 0;
                    break;

                // DR->OK10
                case TAppStato.ssvVoto:
                    lbDirittiDiVoto.Visible = true;
                    // ora devo capire che votazione è e mettere i componenti, attenzione che posso tornare
                    // da un'annulla
                    SettaComponenti(false);
                    // cancello i voti temporanei correnti 
                    CancellaTempVotiCorrenti(); 
                    // ora metto in quadro l'immagine, che deve essere presa da un file composto da
                    oVotoImg.LoadImages(VSDecl.IMG_voto + FParVoto[CurrVoteIDX].IDVoto.ToString());
                    // se il voto è differenziato
                    if (VoteDiff || DatiUsr.utente_voti == 1)
                    {
                        // ??????????????????????????????????????????????
                        if (TotCfg.SalvaLinkVoto) lbNomeDisgiunto.Visible = true;
                        // TODO: e se CurrIdAzionDelega è >= di FAzionisti.count ??
                        c = (clsAzionisti)FAzionisti[CurrIdAzionDelega];
                        lbNomeDisgiunto.Text = rm.GetString("SAPP_VOTE_D_RASO") + "\n" + c.RaSo;    // "Si sta votando per:\n"
                        if (VoteDiff)
                            lbDisgiuntoRimangono.Visible = true;
                        lbDirittiDiVoto.Text = utente_voti_bak.ToString() + rm.GetString("SAPP_VOTE_D_DIRITTI");   //" Diritti di voto"
                    }
                    break;

                case TAppStato.ssvVotoConferma:
                    // conferma
                    MettiComponentiConferma();
                    // ora metto in quadro l'immagine, che deve essere presa da un file composto da
                    oVotoImg.LoadImages(VSDecl.IMG_voto + FParVoto[CurrVoteIDX].IDVoto.ToString() + VSDecl.IMG_voto_c);
                    // Differenziato
                    if (VoteDiff || DatiUsr.utente_voti == 1)
                    {
                        if (TotCfg.SalvaLinkVoto) lbNomeDisgiunto.Visible = true;
                        c = (clsAzionisti)FAzionisti[CurrIdAzionDelega];
                        lbNomeDisgiunto.Text = rm.GetString("SAPP_VOTE_D_RASO") + "\n" + c.RaSo; // "Si sta votando per:\n"
                        if (VoteDiff)
                            lbDisgiuntoRimangono.Visible = true;
                    }
                    break;

                case TAppStato.ssvVotoContinua:
                    break;

                case TAppStato.ssvVotoFinito:
                    if (prbSalvaTutto.Visible) 
                        prbSalvaTutto.Visible = false;
                    lbDirittiDiVoto.Visible = false;
                    SettaComponenti(false);
                    // labels
                    lbDirittiDiVoto.Text = "";
                    // messaggio di arrivederci
                    if (TotCfg.UsaLettore)
                    {
                        NewReader.Flush();
                    }
                    // se è uscito in votazione con il 999999
                    if (UscitaInVotazione)
                    {
                        UscitaInVotazione = false;
                        TornaInizio();
                    }
                    else
                        oVotoImg.LoadImages(VSDecl.IMG_fine);
                        //CaricaImmagini(VSDecl.IMG_fine);
                    break;

                case TAppStato.ssvSalvaVoto:
                    // salva i voti e le presenze in un one shot sul database
                    if (FVotiDaSalvare.Count > VSDecl.MINVOTI_PROGRESSIVO)
                    {
                        // devo mettere la videata di salvataggio
                        oVotoImg.LoadImages(VSDecl.IMG_Salva);
                        // devo settare le dimensioni della barra
                        prbSalvaTutto.Left = 200;
                        prbSalvaTutto.Width = this.Width - 400;
                        prbSalvaTutto.Top = this.Height / 2;
                        prbSalvaTutto.Visible = true;
                        lbDirittiDiVoto.Visible = false;
                        SettaComponenti(false);
                        this.Refresh();  
                    }

                    // controllo i voti, alla ricerca di problemi, ma ora non posso più farlo
                    // perché ci sono i multicandidati
                    //ControllaVoti();

                    // salvataggio dati su log
                    if (TotCfg.AbilitaLogV) SalvaTuttoSuLog();

                    // ok, ora prima di salvare controllo il parametro SalvaLinkVoto
                    // se è true non faccio nulla, altrimenti distruggo il link voto-badge
                    ControllaSalvaLinkVoto();

                    // TODO: Salvataggio voti, vedere valore di ritorno e comportarsi di conseguenza
                    oDBDati.SalvaTutto(Badge_Letto, NVoti, ref FParVoto, 
                            FNAzionisti, TotCfg, ref FAzionisti, ref FVotiDaSalvare); 

                    pbSalvaDati.Visible = false;
                  
                    oSemaforo.SemaforoFineOccupato();
                    CurrVoteIDX = 0;
                    Stato = TAppStato.ssvVotoFinito;
                    CambiaStato();        

                    break;
            }
        }

        private void timVotoApero_Tick(object sender, EventArgs e)
        {
            // dr11 ok
            bool vtaperto;
            int getvtaperto;
            // devo verificare sul database se il voto per questa postazione è aperto
            getvtaperto = oDBDati.CheckStatoVoto(NomeTotem);
            // se sono in una condizione di errore (es db non risponde) lascio il valore precedente
            if (getvtaperto != -1)
            {
                if (getvtaperto == 1) vtaperto = true; else vtaperto = false;
                // se sono diversi e sono all'inizio allora cambio lo stato
                if (TotCfg.VotoAperto != vtaperto)
                {
                    // segnalo che c'è stato un evento e setto una variabile che sarà controllata
                    // appena lo stato sarà su Badge, quindi sarà finita l'eventuale votazione 
                    // in corso
                    AperturaVotoEsterno = vtaperto;
                    // ma se per caso sono in badge devo forzare
                    if (Stato == TAppStato.ssvBadge)
                        CambiaStato();
                }
            }
            // chiaramente se non sono diversi non faccio nulla
        }

        #endregion

		//******************************************************************************
		// ----------------------------------------------------------------
		//	  STATO BADGE E LETTURA (DA RIVEDERE)
		// ----------------------------------------------------------------
		//******************************************************************************

        #region StatoBadge E Lettura Dati Utente

        private void btmBadge_Click(object sender, System.EventArgs e)
        {
            BadgeLetto(edtBadge.Text);
        }
 
        private void ObjDataReceived(object sender, string data)
        {
            this.BeginInvoke(evtDataReceived, new object[] { this, data });
        }

        public void onDataReceived(object source, string dato)
        {
            Badge_Seriale = dato;
            timLetturaBadge.Enabled = true;
         }

        private void timLetturaBadge_Tick(object sender, EventArgs e)
        {
            timLetturaBadge.Enabled = false;
            // ora chiamo l'evento
            Serial_NewRead(Badge_Seriale);
        }

        private void Serial_NewRead(string dato)
        {
            // arriva dal timer che "disaccoppia" la funzione
            // può esser di due tipi
            // - codimpianto + badge
            // - com. particolari "999999"
            // faccio un unico test per vedere se è valido, non può superare la lunghezza totale 
            if (dato.Length <= (TotCfg.BadgeLen + TotCfg.CodImpianto.Length))
            {
                if (fConfig != null)
                    fConfig.BadgeLetto(dato);
                else
                    BadgeLetto(dato);
            }
        }
        
        // ----------------------------------------------------------------
        //	  VERIFICA DATI UTENTE, EVENTUALE SCRITTURA DEL VOTANTE TOTEM E
        //    DELLA CONSEGNA SCHEDE
        // ----------------------------------------------------------------

        private void BadgeLetto(string AText)
        {
            //DR12 OK, aggiunto solo controllo badge->999999
            int Badge_Lettura;
            string codimp, bbadge;
            int ErrorFlag = 0;
 
            // allora prima di tutto controllo se c'è stato un comando di Reset Votazione
            // cioè 88889999
            if (AText == VSDecl.RIPETIZ_VOTO && TotCfg.VotoAperto)
            {
                CodiceRipetizioneVoto(AText);
                return;
            } // if (AText == VSDecl.RIPETIZ_VOTO && TotCfg.VotoAperto)

            // COMANDI SPECIALI
            // poi verifico se è stato premuto
            if (AText == VSDecl.CONFIGURA)
            {
                timConfigura.Enabled = true;
                return;
            } 
            // pannello stato
            if (AText == VSDecl.PANNELLO_STATO)
            {
                MostraPannelloStato();
                return;
            }
            // pannello stato azionista
            if (AText == VSDecl.PANNELLO_AZION)
            {
                MostaPannelloStatoAzionista();
                return;
            } 

            // CONTINUO
            // ok, per prima cosa, se il voto è chiuso o la postazione non è attiva
            // esco direttamente
            if (!TotCfg.VotoAperto && Stato == TAppStato.ssvBadge)
            {
                FVSTest test = new FVSTest(AText);
                test.ShowDialog();
                return;
            }

            // Devo considerare il caso in cui le finestre messaggio sono visibili ed uscire
            if ((frmVSMessage != null) && frmVSMessage.Visible) return;

            // se ho il badge il 999999 non esce fuori (doppia lettura)
            if (Stato == TAppStato.ssvBadge && AText == TotCfg.CodiceUscita) return;

            // stato iniziale, ho già filtrato le finestre
            if (Stato == TAppStato.ssvBadge)
            {
                // metto una variabile globale non si sa mai
                Badge_Lettura = -1;
                codimp = "00";  // codice impianto universale
                // ok, qua devo fare dei controlli sul codice impianto e sul badge
                // se la lunghezza è giusta allora estraggo le due parti e controllo
                if (AText.Length >= (TotCfg.BadgeLen + TotCfg.CodImpianto.Length))
                {
                    // estraggo il badge, parto sempre da sinistra
                    bbadge = AText.Substring(AText.Length - TotCfg.BadgeLen, TotCfg.BadgeLen);

                    // estraggo il cod impianto
                    codimp = AText.Substring(AText.Length - TotCfg.BadgeLen -  
                        TotCfg.CodImpianto.Length, TotCfg.CodImpianto.Length);                       
                 }
                else
                    bbadge = AText.Trim();

                // NOTA: in questa routine uso codici di errore bitwise, in pratica nella variabile 
                // ErrorFlag ci sono tutti gli errori che la procedura ha trovato e così posso
                // elaborarli alla fine
                // lo converto in intero
                try
                {
                    Badge_Lettura = Convert.ToInt32(bbadge);
                }
                catch
                {
                    ErrorFlag = ErrorFlag | 0x20;  // setto l'errore                }
                }

                // controllo il codice impianto, uso 00 come codice normale
                if ((codimp != "00") && (codimp != TotCfg.CodImpianto)) 
                        ErrorFlag = ErrorFlag | 0x10;

                // se non ho trovato errori continuo, testo anche il codice uscita così mi
                // evito un inutile lettura del db
                // nota: quando avrò più codici mi bastera fare una funzione
                if ((ErrorFlag == 0) && (AText != TotCfg.CodiceUscita))
                {
                    // variabile
                    Badge_Letto = Badge_Lettura;
                    // ok ora iniziano i test
                    bool Controllato = oDBDati.ControllaBadge(Badge_Lettura, TotCfg, ref ErrorFlag);

                    // NOTA
                    // per creval c'è il problema del non voto, quindi devo caricare tutti i diritti
                    // non espressi, usiamo il trucco:
                    // se non ha votato va bene come prima
                    // se ha votato, dopo bisogna controllare se ha espresso almeno un non voglio votare -2
                    //  se è così si deve:
                    //     cancellare la riga su votanti totem,
                    //     cancellare conschede e intonse che hanno voto -2
                    //     andare avanti, dovrebbe entrare nel giro badge banana
                   
                    // mettere flag su db che caricato dalla configurazione

                    // if ((ErrorFlag & 0x04) == 0x04 && AbilitaDirittiNonVoglioVotare == true)
                    //{
                        
                    //}


                    // separo così mi evito un controllo in più
                    if (Controllato)
                    {
                        if (DammiUtente())
                        {
                            // resetto alcune variabili
                            CurrVoteIDX = 0;                    // resetto alla 1° votazione
                            FVotiDaSalvare.Clear();             // cancello i voti
                            VoteDiff = false;                   // dico che è un voto normale
                            CancellaTempVotiCorrenti();         // cancello i voti temporanei
                            CurrIdAzionDelega = 0;              // la prima delega
                            // cambio lo stato
                            Logging.WriteToLog((LogNomeFile), "** Inizio Voto : " + Badge_Letto.ToString() +
                                " nvoti:" + FNAzionisti.ToString());

                            Stato = TAppStato.ssvVotoStart;

                            CambiaStato();
                        }
                        else  // if (DammiUtente())
                            ErrorFlag = ErrorFlag | 0x08;  // setto l'errore
                    }  // if (Controllato)
                } // if (ErrorFlag == 0)

                // ok, se ora qualcosa è andato storto ResultFlag è > 0
                if (ErrorFlag > 0 || AText == TotCfg.CodiceUscita)
                {
                    string messaggio = rm.GetString("SAPP_ERR_BDG") + Badge_Lettura.ToString();     //"Errore sul badge : "
                    // compongo il messaggio della finestra di errore
                    // 0x01 : Badge Annullato o mai esistito
                    if ((ErrorFlag & 0x01) == 0x01) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGANN");   // "\n - Badge Annullato";
                    // 0x40 : Il Badge non esiste
                    if ((ErrorFlag & 0x40) == 0x40) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGEST");   // "\n - Il Badge non esiste";
                    // 0x02 : Badge non presente (controllo disabilitato)
                    if (TotCfg.ControllaPresenze == VSDecl.PRES_CONTROLLA && (ErrorFlag & 0x01) != 0x01)
                    {
                        if ((ErrorFlag & 0x02) == 0x02) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGPRES");   // "\n - Badge non presente.";
                    }
                    // 0x04 : Badge ha già votato
                    if ((ErrorFlag & 0x04) == 0x04) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGVOT");   // "\n - Il Badge ha già votato";
                    // 0x08 : Tutti i soci hanno già votato
                    if ((ErrorFlag & 0x08) == 0x08) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGZERO");   // "\n - Socio con azioni zero\tutti i soci hanno già votato";
                    // 0x10 : Codice Impianto diverso
                    if ((ErrorFlag & 0x10) == 0x10) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGIMP");   // "\n - Codice Impianto diverso";
                    // 0x20 : Errore nella conversione
                    if ((ErrorFlag & 0x20) == 0x20) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGCONV");   // "\n - Errore nella conversione Badge";

                    // se il badge è 999999, metto un codice a parte
                    if (AText == TotCfg.CodiceUscita)
                    {
                        messaggio = rm.GetString("SAPP_ERR_BDGUSCITA") + TotCfg.CodiceUscita + ")";
                    }

                    // evidenzio
                    oSemaforo.SemaforoErrore();
                    Logging.WriteToLog((LogNomeFile), messaggio);

                    // non so se è cancellata o no, x sicurezza la ricreo
                    if (frmVSMessage == null)
                    {
                        frmVSMessage = new FVSMessage();
                        this.AddOwnedForm(frmVSMessage);
                    }
                    frmVSMessage.Show(messaggio);
                    
                    this.Focus();
                    oSemaforo.SemaforoLibero();
                    return;
                }

                edtBadge.Text = "";
                return;
            }

            // la conferma di uscita
            if (Stato == TAppStato.ssvVotoFinito)
            {
                if (AText == TotCfg.CodiceUscita) //"999999")
                {
                    Logging.WriteToLog((LogNomeFile), "--> Voto " + Badge_Letto.ToString() + " terminato.");
                    TornaInizio();
                }
                edtBadge.Text = "";
                return;
            }

            // ora il codice di uscita in "mezzo" al voto
            if (AText == TotCfg.CodiceUscita && 
                Stato != TAppStato.ssvVotoStart &&
                Stato != TAppStato.ssvBadge)
            {
                // ok, è proprio l'uscita dalla votazione
                // il problema è che qua devo far votare scheda bianca/nulla, ma non so a che punto sono arrivato
                // qundi devo fare un po di eculubrazioni
                int NSKSalvate = MettiSchedeDaInterruzione();
                // loggo
                Logging.WriteToLog((LogNomeFile), "--> USCITA IN VOTO (999999) id:" + Badge_Letto.ToString() +
                                                  " (" + NSKSalvate.ToString() + ")");
                // resetto il tutto
                lbDirittiDiVoto.Visible = false;
                SettaComponenti(false);
                // labels
                lbDirittiDiVoto.Text = "";
                // messaggio di arrivederci
                CurrVoteIDX = 0;
                Stato = TAppStato.ssvSalvaVoto;
                UscitaInVotazione = true;
                CambiaStato();
                edtBadge.Text = "";
 
            }  // if (AText == TotCfg.CodiceUscita && Stato
        }

        private void CodiceRipetizioneVoto(string AText)
        {
            // qua è un casino, perché ho due casi:
            // 1. è stato digitato un badge, quindi c'è il messaggio "ha già votato"
            //    devo cancellare i voti
            if (Stato == TAppStato.ssvBadge)
            {
                if (MessageBox.Show(VSDecl.MSG_CANC_VOTO + Badge_Letto, "Cancellazione Voto", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    // ulteriore conferma
                    if (MessageBox.Show(VSDecl.MSG_CANC_VOTO_C + Badge_Letto, "Conferma Cancellazione Voto", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Logging.WriteToLog((LogNomeFile), "--> Voto " + Badge_Letto.ToString() + " Cancellati voti (88889999).");
                        oDBDati.CancellaBadgeVotazioni(Badge_Letto);
                    }
                }
                return;
            }
            // 2. sono durante la votazione , esco senza salvare
            if (Stato != TAppStato.ssvVotoFinito)
            {
                if (MessageBox.Show(VSDecl.MSG_RIPETIZ_VOTO + Badge_Letto.ToString(), "Annullamento Voto", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    // ritorna all'inizio
                    // ulteriore conferma
                    if (MessageBox.Show(VSDecl.MSG_RIPETIZ_VOTO_C + Badge_Letto, "Conferma Annullamento Voto", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Logging.WriteToLog((LogNomeFile), "--> Voto " + Badge_Letto.ToString() + " Annullato (88889999).");
                        TornaInizio();
                        edtBadge.Text = "";
                    }
                }
                return;
            }
        }

        // ----------------------------------------------------------------
        //   RITROVO DATI UTENTE DA DB
        // ----------------------------------------------------------------

        private bool DammiUtente()
        {
            //DR12 OK, non è stato cambiato
            bool result;
            clsAzionisti c;

            FNAzionisti = 0;
            FAzionisti.Clear();
            // ok, ora dovrei capire le cose leggendole dal database
            if (oDBDati.DammiDatiAzionista(Badge_Letto, ref FNAzionisti, ref FAzionisti) == 1)
            {
                // dati
                utente_voti_bak = FNAzionisti;
                utente_voti_diff = FNAzionisti;
                // metto i dati dell'utente principale, il primo
                DatiUsr.utente_badge = Badge_Letto;
                if (FAzionisti.Count > 0)
                {
                    c = (clsAzionisti)FAzionisti[0];
                    DatiUsr.utente_id = c.IDAzion;
                    DatiUsr.utente_nome = c.RaSo;
                    DatiUsr.utente_sesso = c.Sesso;
                }
                else
                {
                    DatiUsr.utente_id = 0;
                    DatiUsr.utente_nome = "NONE";
                    DatiUsr.utente_sesso = "N";
                }
                DatiUsr.utente_voti = FNAzionisti;
                DatiUsr.utente_voti_bak = FNAzionisti;
                result = true;
            }
            else
                result = false;

            return result;
        }

        #endregion

       
        //******************************************************************************
        // ----------------------------------------------------------------
        //	GESTIONE DELL'AREA SENSIBILE
        // ----------------------------------------------------------------
        //******************************************************************************

        private void frmMain_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // chiamo il metodo in CTouchscreen che mi ritornerà eventi diversi a seconda del caso
            oVotoTouch.TastoPremuto(sender, e, Stato);
        }

        //******************************************************************************
        // ----------------------------------------------------------------
        //	EVENTI DI PRESSIONE SCHERMO DA CTOUCHSCREEN
        // ----------------------------------------------------------------
        //******************************************************************************

        #region Eventi pressione schermo da CTouchscreen

        // ATTENZIONE, mettere una modifica di un qualsiasi controllo in queste routine, 
        // può causare refresh e paint indesiderati (caso a.Text) fare attenzione

        public void onPremutoTab(object source, int VParam)
        {
            // arriva l'evento cambio pagina (ora lo faccio all'interno di Ctouchscreen)
            //oVotoTouch.CurrPag = VParam;
            // ricalcolo, non serve più con la nuova gestione pagine
            //oVotoTouch.CalcolaTouch(this, Stato, ref FParVoto[CurrVoteIDX], DatiUsr.utente_voti > 1);
            // devo ridisegnare tutto, qui lo voglio
            this.Invalidate();
        }

        public void onPremutoVotaNormale(object source, int VParam)
        {
            // DR12 Ok
            // ok, questo evento arriva all'inizio votazione quando è stato premuto l'avvio del voto normale
            // o nel caso di scelta differenziato/normale, evidenzia il voto in un unica soluzione
            VoteDiff = false;
            Logging.WriteToLog((LogNomeFile), "Voto normale");
            Stato = TAppStato.ssvVoto;
            CambiaStato();        
        }

        public  void onPremutoVotaDifferenziato(object source, int VParam)
        {
            // ok, questo evento arriva all'inizio votazione 
            // nel caso di scelta differenziato/normale, evidenzia il voto in soluzioni separate
            VoteDiff = true;
            Logging.WriteToLog((LogNomeFile), "Voto differenziato");
            Stato = TAppStato.ssvVoto;
            CambiaStato();
        }


        public void onPremutoVotoValido(object source, int VParam, bool ZParam)
        {
            // DR12 Ok
            // TODO: UNificare votiespressi con collection
            // TODO: Usare IdScheda invece di indice

            // ok, questo evento arriva quando, nella selezione del voto, è stata
            // premuna una zona valida
            // devo veder in funzione della lista selezionata
            TListe a;
            // questo controllo dell'indice è inutile, però è meglio farlo,
            // in caso di problemi, indici scassati, mette una scheda bianca

            // todo: etruria
            if (VParam == VSDecl.VOTO_ETRURIA)
            {
                VotoEspressoCarica = 0;
                VotoEspresso = VSDecl.VOTO_ETRURIA;
                VotoEspressoStr = "";
                VotoEspressoStrUp = "Bugno Claudia    Nannipieri Luigi";
                Stato = TAppStato.ssvVotoConferma;
                CambiaStato();
                return;
            }

            int ct = FParVoto[CurrVoteIDX].Liste.Count;
            if (VParam >= 0 && VParam < ct)
            {
                a = (TListe)FParVoto[CurrVoteIDX].Liste[VParam];
                VotoEspressoCarica = a.TipoCarica;
                VotoEspresso = a.IDScheda;
                VotoEspressoStr = a.ListaElenco;
                VotoEspressoStrUp    = a.DescrLista;
                // da aggiungere successivamente:
                //VExp = new TVotoEspresso();
                //VExp.NumVotaz = FParVoto[CurrVoteIDX].IDVoto;
                //VExp.VotoExp_IDScheda = VSDecl.VOTO_NONVOTO;
                //VExp.TipoCarica = 0;
                //VExp.Str_ListaElenco = "";
                //VExp.StrUp_DescrLista = "Non Voglio Votare";
                //FVotiExpr.Add(VExp);
            }
            else
            {
                // se succede qualcosa di strano mette sk bianca
                Logging.WriteToLog((LogNomeFile), "<error> onPremutoVotoValido Indice voto non valido");
                VotoEspresso = VSDecl.VOTO_SCHEDABIANCA;
                VotoEspressoCarica = 0;
                VotoEspressoStr = "";
                VotoEspressoStrUp = rm.GetString("SAPP_SKBIANCA");      // "Scheda Bianca";
                // da aggiungere successivamente:
                //VExp = new TVotoEspresso();
                //VExp.NumVotaz = FParVoto[CurrVoteIDX].IDVoto;
                //VExp.VotoExp_IDScheda = VSDecl.VOTO_NONVOTO;
                //VExp.TipoCarica = 0;
                //VExp.Str_ListaElenco = "";
                //VExp.StrUp_DescrLista = "Non Voglio Votare";
                //FVotiExpr.Add(VExp);
            }
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        // ORA E' MULTICANDIDATO, MA DIVENTERA' STANDARD
        public void onPremutoVotoValidoMulti(object source, int VParam, ref List<int> voti)
        {
            // in realtà corrisponde all'AVANTI
            if (voti == null) return;  // in teoria non serve
            TListe a;
            TVotoEspresso vt;
            int ct = FParVoto[CurrVoteIDX].Liste.Count;
            // ok, ora riempio la collection di voti
            for (int i = 0; i < voti.Count; i++)
            {
                if (voti[i] >= 0 && voti[i] < ct)
                {
                    a = (TListe)FParVoto[CurrVoteIDX].Liste[voti[i]];
                    vt = new TVotoEspresso();
                    vt.NumVotaz = a.NumVotaz;
                    vt.TipoCarica = a.TipoCarica;
                    vt.VotoExp_IDScheda = a.IDScheda;
                    vt.Str_ListaElenco = a.ListaElenco;
                    vt.StrUp_DescrLista = a.DescrLista;
                    FVotiExpr.Add(vt);
                }
            }            
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        public void onPremutoVotoMulti(object source, int VParam)
        {
            // mi serve per il repaint per settare o meno i tasti verdi
            this.Invalidate();
        }

        public void onPremutoSchedaBianca(object source, int VParam)
        {
            TVotoEspresso VExp;

            // scheda bianca
            VotoEspresso = VSDecl.VOTO_SCHEDABIANCA;
            VotoEspressoCarica = 0;
            VotoEspressoStr = "";
            VotoEspressoStrUp = rm.GetString("SAPP_SKBIANCA");      // "Scheda Bianca";
            // nuova versione array
            VExp = new TVotoEspresso();
            VExp.NumVotaz = FParVoto[CurrVoteIDX].IDVoto;
            VExp.VotoExp_IDScheda = VSDecl.VOTO_SCHEDABIANCA;
            VExp.TipoCarica = 0;
            VExp.Str_ListaElenco = "";
            VExp.StrUp_DescrLista = rm.GetString("SAPP_SKBIANCA");      // "Scheda Bianca";
            FVotiExpr.Add(VExp);
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        public void onPremutoNonVoto(object source, int VParam)
        {
            TVotoEspresso VExp;

            // NonVotante (caso BPM)
            VotoEspresso = VSDecl.VOTO_NONVOTO;
            VotoEspressoCarica = 0;
            VotoEspressoStr = "";
            VotoEspressoStrUp = rm.GetString("SAPP_NOVOTO");      // "Non Voglio Votare";
            // nuova versione array
            VExp = new TVotoEspresso();
            VExp.NumVotaz = FParVoto[CurrVoteIDX].IDVoto;
            VExp.VotoExp_IDScheda = VSDecl.VOTO_NONVOTO;
            VExp.TipoCarica = 0;
            VExp.Str_ListaElenco = "";
            VExp.StrUp_DescrLista = rm.GetString("SAPP_NOVOTO");      // "Non Voglio Votare";
            FVotiExpr.Add(VExp);
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        public void onPremutoInvalido(object source, int VParam)
        {
            // ok, questo evento arriva quando, nella selezione del voto, è stata
            // premuna una zona invalida, quindi nulla, potrebbe essere un beep
            SystemSounds.Beep.Play();
        }

        // ----------------------------------------------------------------
        //	 CONFERMA - ANNULLA VOTI
        //      versione con SalvaVotoNonConfermato
        // ----------------------------------------------------------------

        public void onPremutoConferma(object source, int VParam)
        {
            // DR12 OK
            // ok, questo evento arriva quando, nella conferma del voto, è stata scelta l'opzione
            //  conferma, cioè il salvataggio del voto

            // CHiamo la funzione di Conferma Voti
            ConfermaVotiEspressi();
            
            // testo la votazione
            if (!VoteDiff)     // VOTAZIONE NORMALE
            {
                // passo alla votazione successiva
                CurrVoteIDX++;

                // testo se ho finito
                if (CurrVoteIDX == NVoti)
                {
                    // per precauzione metto CurrVoteIDX all'ultima votazione 
                    CurrVoteIDX = NVoti - 1;
                    Stato = TAppStato.ssvSalvaVoto;         // finito, salvo i voti e poi chiudo
                }
                else
                    Stato = TAppStato.ssvVoto;              // passo al voto successivo
                CambiaStato();
            }
            else
            {
                // punto all'indice della delega corrente
                CurrIdAzionDelega++;
                // controllo se ho finito la singola multi-votazione
                if (utente_voti_bak == 0)
                {
                    // poi passo alla votazione successiva
                    CurrVoteIDX++;
                    // resetto le deleghe
                    CurrIdAzionDelega = 0;
                    // rimetto a posto i valori
                    utente_voti_bak = DatiUsr.utente_voti;
                    utente_voti_diff = DatiUsr.utente_voti;
                }
                // testo se ho finito tutte le votazioni
                if (CurrVoteIDX == NVoti)
                {
                    // per precauzione metto CurrVoteIDX all'ultima votazione 
                    CurrVoteIDX = NVoti - 1;
                    Stato = TAppStato.ssvSalvaVoto;             // finito, salvo i voti e poi chiudo
                }
                else
                    // passo al voto della votazione successiva (o successiva delega)
                    Stato = TAppStato.ssvVoto;
                // cambio
                CambiaStato();
            }

        }
        
        public void ConfermaVotiEspressi()
        {
            // DR12 Ok
            int i, k;
            TVotiDaSalvare v;
            TVotoEspresso vt;
            clsAzionisti c;

            // TODO: Unificare i voti espressi e salvati con la collection

            // non è il massimo, ma setta a 1 la pagina del touch quando preme conferma
            oVotoTouch.CurrPag = 1;
            // ok, questo evento arriva quando, nella conferma del voto, è stata scelta l'opzione
            //  conferma, cioè il salvataggio del voto
            if (!VoteDiff)     // VOTAZIONE NORMALE
            {
                // Votazione Normale, In VParam c'è l'idx del voto
                // Ok, ora salvo i voti espressi:
                // 1. Nell'arrayList
                for (i = 0; i < FNAzionisti; i++)
                {
                    // prendo l'azionista
                    c = (clsAzionisti)FAzionisti[i];
                    // ok qui distinguo i multicandidati
                    // NOTA: unificare con il metodo collection
                    if (FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                    {
                        // ok, ora riempio la collection di voti
                        for (k = 0; k < FVotiExpr.Count; k++)
                        {
                            vt = (TVotoEspresso)FVotiExpr[k];
                            v = new TVotiDaSalvare();
                            // parte da 1
                            v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                            v.AScheda_2 = vt.VotoExp_IDScheda;
                            v.NVoti_3 = 1;
                            v.AIDBadge_4 = Badge_Letto;
                            v.ProgDelega_5 = c.ProgDeleg;
                            v.IdCarica_6 = vt.TipoCarica;
                            v.IDazion = c.IDAzion;
                            // carico
                            FVotiDaSalvare.Add(v);
                        }
                    }
                    else
                    {

                        //// todo: etruria
                        //if (VotoEspresso == VSDecl.VOTO_ETRURIA)
                        //{
                        //    v = new TVotiDaSalvare();
                        //    // parte da 1
                        //    //c = (clsAzionisti)FAzionisti[i];
                        //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                        //    v.AScheda_2 = 201;
                        //    v.NVoti_3 = 1;
                        //    v.AIDBadge_4 = Badge_Letto;
                        //    v.ProgDelega_5 = c.ProgDeleg;
                        //    v.IdCarica_6 = VotoEspressoCarica;
                        //    v.IDazion = c.IDAzion;
                        //    // carico
                        //    FVotiDaSalvare.Add(v);
                        //    v = new TVotiDaSalvare();
                        //    // parte da 1
                        //    //c = (clsAzionisti)FAzionisti[i];
                        //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                        //    v.AScheda_2 = 202;
                        //    v.NVoti_3 = 1;
                        //    v.AIDBadge_4 = Badge_Letto;
                        //    v.ProgDelega_5 = c.ProgDeleg;
                        //    v.IdCarica_6 = VotoEspressoCarica;
                        //    v.IDazion = c.IDAzion;
                        //    // carico
                        //    FVotiDaSalvare.Add(v);
                        //}
                        //else
                        //{
                            // -------------------------------------------
                            // in obsolescenza, si dovrà usare la collection
                            //
                            v = new TVotiDaSalvare();
                            // parte da 1
                            //c = (clsAzionisti)FAzionisti[i];
                            v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                            v.AScheda_2 = VotoEspresso;
                            v.NVoti_3 = 1;
                            v.AIDBadge_4 = Badge_Letto;
                            v.ProgDelega_5 = c.ProgDeleg;
                            v.IdCarica_6 = VotoEspressoCarica;
                            v.IDazion = c.IDAzion;
                            // carico
                            FVotiDaSalvare.Add(v);
                            // -------------------------------------------
                        //}

                    }
                }
                // alla fine saranno da unificare i metodi
            }
            else
            {
                // Votazione differenziata
                utente_voti_bak --;
                utente_voti_diff --;
                // trovo l'azionista
                c = (clsAzionisti)FAzionisti[CurrIdAzionDelega];
                // Ok, ora salvo i voti espressi Nell'arrayList
                // ok qui distinguo i multicandidati
                // NOTA: unificare con il metodo collection
                if (FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_MULTICANDIDATO )
                {
                    // ok, ora riempio la collection di voti
                    for (k = 0; k < FVotiExpr.Count; k++)
                    {
                        vt = (TVotoEspresso)FVotiExpr[k];
                        v = new TVotiDaSalvare();
                        // parte da 1
                        v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                        v.AScheda_2 = vt.VotoExp_IDScheda;
                        v.NVoti_3 = 1;
                        v.AIDBadge_4 = Badge_Letto;
                        v.ProgDelega_5 = c.ProgDeleg;
                        v.IdCarica_6 = vt.TipoCarica;
                        v.IDazion = c.IDAzion;
                        // carico
                        FVotiDaSalvare.Add(v);
                    }
                }
                else
                {
                    //     // todo: etruria
                    //if (VotoEspresso == VSDecl.VOTO_ETRURIA)
                    //{
                    //    v = new TVotiDaSalvare();
                    //    // parte da 1
                    //    //c = (clsAzionisti)FAzionisti[i];
                    //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                    //    v.AScheda_2 = 201;
                    //    v.NVoti_3 = 1;
                    //    v.AIDBadge_4 = Badge_Letto;
                    //    v.ProgDelega_5 = c.ProgDeleg;
                    //    v.IdCarica_6 = VotoEspressoCarica;
                    //    v.IDazion = c.IDAzion;
                    //    // carico
                    //    FVotiDaSalvare.Add(v);
                    //    v = new TVotiDaSalvare();
                    //    // parte da 1
                    //    //c = (clsAzionisti)FAzionisti[i];
                    //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                    //    v.AScheda_2 = 202;
                    //    v.NVoti_3 = 1;
                    //    v.AIDBadge_4 = Badge_Letto;
                    //    v.ProgDelega_5 = c.ProgDeleg;
                    //    v.IdCarica_6 = VotoEspressoCarica;
                    //    v.IDazion = c.IDAzion;
                    //    // carico
                    //    FVotiDaSalvare.Add(v);
                    //}
                    //else
                    //{
                        // -------------------------------------------
                        // in obsolescenza, si dovrà usare la collection
                        //
                        //c = (clsAzionisti)FAzionisti[CurrIdAzionDelega];
                        v = new TVotiDaSalvare();
                        // parte da 1
                        v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                        v.AScheda_2 = VotoEspresso;
                        v.NVoti_3 = 1;
                        v.AIDBadge_4 = Badge_Letto;
                        v.ProgDelega_5 = c.ProgDeleg;
                        v.IdCarica_6 = VotoEspressoCarica;
                        v.IDazion = c.IDAzion;
                        // carico
                        FVotiDaSalvare.Add(v);
                        // -------------------------------------------
                    //}
                }
            }
        }

        public void onPremutoAnnulla(object source, int VParam)
        {
            // ok, questo evento arriva quando, nella conferma del voto, è stata scelta l'opzione
            //  annulla, cioè il ritorno all'espressione del voto
            CancellaTempVotiCorrenti();
            // Annulla del voto (torno dove ero prima)
            Stato = TAppStato.ssvVoto;
            CambiaStato();
        }

        public void CancellaTempVotiCorrenti()
        {
            // cancella i voti correnti
            FVotiExpr.Clear();
            VotoEspresso = -1;
            VotoEspressoStr = "";
            VotoEspressoStrUp = "";
            VotoEspressoCarica = 0;
        }

        #endregion

        //******************************************************************************
		// ----------------------------------------------------------------
		//	 STATO DI START VOTE
		// ----------------------------------------------------------------
		//******************************************************************************

		private void MettiComponentiStartVoto()
		{
		    string PrefNomeAz = "";
			// DR->OK11	
			// start del voto
			SettaComponenti(false);
			edtBadge.Text = "";
			// le labels
            // nome azionista
		    PrefNomeAz = oDBDati.DammiNomeAzionista(Badge_Letto);

            PrefNomeAz = UppercaseWords(PrefNomeAz.ToLower());          
            //PrefNomeAz += UppercaseWords(DatiUsr.utente_nome.ToLower());
            lbNomeAzStart.Text = PrefNomeAz; 
            lbNomeAzStart.Visible = true;
            // diritti di voto
            lbDirittiDiVoto.Text = DatiUsr.utente_voti.ToString() + rm.GetString("SAPP_VOTE_D_DIRITTI");      // " Diritti di voto";
			lbDirittiStart.Text = DatiUsr.utente_voti.ToString();
 			// in funzione del n. di deleghe metto
			if (DatiUsr.utente_voti > 1)
			{
                oVotoImg.LoadImages(VSDecl.IMG_VotostartD);
                //CaricaImmagini(VSDecl.IMG_VotostartD);
				// sono le label del differenziato
                lbDirittiStart.Visible = true;
			}
			else
			{
				// immagine di 1 voto
                oVotoImg.LoadImages(VSDecl.IMG_Votostart1);
                //CaricaImmagini(VSDecl.IMG_Votostart1);
			}
		}


		// ************************************************************************
		// ----------------------------------------------------------------
		//    CONFERMA/SALVATAGGIO DEL VOTO
		// ----------------------------------------------------------------
		// ************************************************************************

		private void MettiComponentiConferma()
		{
			// DR->OK12
		    bool NODirittiLabel = false;

			// crea la pagina di conferma
			SettaComponenti(false);
			lbDirittiDiVoto.Visible = true;			
			// Sistemo la label dei diritti di voto
			if (VoteDiff)
                lbConfermaNVoti.Text = rm.GetString("SAPP_VOTE_1DIRITTOPER");      //"1 diritto di voto per";
			else
			{
				if (DatiUsr.utente_voti == 1)
                    lbConfermaNVoti.Text = rm.GetString("SAPP_VOTE_1DIRITTOPER");      //"1 diritto di voto per";
				else
                    lbConfermaNVoti.Text = DatiUsr.utente_voti.ToString() + rm.GetString("SAPP_VOTE_DIRITTIPER"); //" diritti di voto per";
			}

            // ok, per ora distinguiamo tra i due metodi di voto, quello normale e quello multicandidato
            // che ha i voti salvati in una collection
            // in un secondo tempo dovrà essere unificato
            if (FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
            {
                // ciclo e metto i candidati
                TVotoEspresso vt;
                bool acapo = false;
                lbConfermaUp.Text = "";
                lbConferma.Text = "";
                int cnt = FVotiExpr.Count;
                // ok, ora riempio la collection di voti
                for (int i = 0; i < cnt; i++)
                {
                    vt = (TVotoEspresso)FVotiExpr[i];

                    // se è sk bianca o non voto non metto i diritti
                    NODirittiLabel = (vt.VotoExp_IDScheda == VSDecl.VOTO_SCHEDABIANCA || vt.VotoExp_IDScheda == VSDecl.VOTO_NONVOTO);

                    lbConfermaUp.Text += vt.StrUp_DescrLista;
                    if (acapo)
                        lbConfermaUp.Text += "\n";
                    else
                    {
                        if (i < (cnt-1))   // per evitarmi l'ultimo " - "
                           lbConfermaUp.Text += "  -  ";
                    }
                    acapo = !acapo;
                }
                oVotoTheme.SetTheme_lbConfermaUp_Cand(ref lbConfermaUp);
            }
            else
            {
                // se è sk bianca o non voto non metto i diritti
                NODirittiLabel = (VotoEspresso == VSDecl.VOTO_SCHEDABIANCA || VotoEspresso == VSDecl.VOTO_NONVOTO);
                // voto di lista/candidato              
                lbConfermaUp.Text = VotoEspressoStrUp;
                lbConferma.Text = VotoEspressoStr;
                oVotoTheme.SetTheme_lbConfermaUp(ref lbConfermaUp);
            }

            // attenzione, se ho una sk bianca o non voto non metto i diritii
            if (NODirittiLabel)
            {
                lbConfermaNVoti.Text = "";
            }

            // ok, ora le mostro
            lbConferma.Visible = true;
            lbConfermaNVoti.Visible = true;
            lbConfermaUp.Visible = true;
        }

		//------------------------------------------------------------------------------
		//  SALVATAGGIO DEI DATI DI VOTO COME SCHEDA BIANCA o nulla DA INTERRUZIONE
		//------------------------------------------------------------------------------

		private int MettiSchedeDaInterruzione()
		{
			// DR->OK11	
            int i, z, NVotaz, NSKSalvate;
            TVotiDaSalvare v;
            clsAzionisti c;

			// procedura chiamata dall'interruzione 999999 durante il voto
            // oppure al salvataggio se il n. di sk è minore x qualche motivo
			// qua devo scrivere tante schede bianche/nulle quante sono la differenza
			//
			// Ok, la cosa migliore è quella di ciclare sul progdeleghe e testare se c'è
			// un record di voto, se non c'è inserirlo
            // il parametro viene preso da TotCfg.IDSchedaUscitaForzata

            // prima di tutto vedo se è attivato SalvaVotoNonConfermato
            // se sono nello stato di conferma, confermo il voto espresso e poi metto le altre schede
            if (Stato == TAppStato.ssvVotoConferma && TotCfg.SalvaVotoNonConfermato)
                ConfermaVotiEspressi();

            // TODO: Possibile BACO MettiSchedeBiancheDaInterruzione MULTIVOTAZIONI
            // ATTENZIONE, C'è UN POSSIBILE BACO :
            // LA PROCEDURA NON TIENE CONTO DELLE MULTIVOTAZIONIU, QUINDI NON FUNZIONA
            // IN CASO DI + CANDIDATI SELEZIONATI
            // 24.03.12 Ad un'analisi successiva non sembra ci possano essere problemi o bachi anche nel caso della
            // multivotazione, più precisamente nelle differenziate, in realtà salva i dati espressi prima, 
            // in ConfermaVotiEspressi. Poi testa per ogni singolo azionista se c'è ALMENO un voto, se no mette
            // scheda bianca. Da testare ma dovrebbe funzionare


            NSKSalvate = 0;
			z = 0;
            for (z = 0; z < NVoti; z++)
            {
                NVotaz = FParVoto[z].IDVoto;

                for (i = 0; i < FNAzionisti; i++)
                {
                    c = (clsAzionisti)FAzionisti[i];
                    // cerco nella collection se c'è un voto 
                    if (!HaVotato(NVotaz, c.ProgDeleg))
                    {
                        // devo scrivere la sk bianca
                        v = new TVotiDaSalvare();
                        // parte da 1
                        v.NumVotaz_1 = NVotaz;
                        v.AScheda_2 = TotCfg.IDSchedaUscitaForzata;
                        v.NVoti_3 = 1;
                        v.AIDBadge_4 = Badge_Letto;
                        v.ProgDelega_5 = c.ProgDeleg;
                        v.IdCarica_6 = 0;
                        v.IDazion = c.IDAzion;
                        // carico
                        FVotiDaSalvare.Add(v);
                        NSKSalvate++;

                    }  //if (!HaVotato(NVotaz, FAzionisti[i].ProgDeleg))
                }  // for (i = 1; i <= FNAzionisti; i++)
            }  // for (z = 0; z < NVoti; z++)

            return NSKSalvate;
		}

        public Boolean HaVotato(int NVotaz, int FProgDeleg)
        {
            // DR11 Ok
            Boolean res;
            TVotiDaSalvare v;
            int i;

            // trovo almeno un voto del progdelega
            res = false;

            for (i = 0; i < FVotiDaSalvare.Count; i++)
            {
                v = (TVotiDaSalvare)FVotiDaSalvare[i];
                if (v.NumVotaz_1 == NVotaz && v.ProgDelega_5 == FProgDeleg)
                {
                    res = true;
                    break;
                }
            }
            return res;
        }

        //------------------------------------------------------------------------------
        //  SALVATAGGIO DEI DATI DI VOTO su log
        //------------------------------------------------------------------------------

        public void SalvaTuttoSuLog()
        {
            string salva = Badge_Letto.ToString() + ";";
            TVotiDaSalvare v;

            try
            {
                for (int i = 0; i < FVotiDaSalvare.Count; i++)
                {
                    v = (TVotiDaSalvare)FVotiDaSalvare[i];

                    salva += v.NumVotaz_1.ToString() +
                            v.ProgDelega_5.ToString() + "-" +
                            v.AScheda_2.ToString() + ";";
                    //v.NVoti_3.ToString() + ";" +
                    //v.AScheda_2.ToString() + ";" +
                }

                if (TotCfg.AbilitaLogV) LogVote.WriteToLogCrypt(LogVotiNomeFile, salva);

            }
            catch (Exception ex)
            {
                // non faccio nulla
            }
        }

        //------------------------------------------------------------------------------
        //  Controllo dei voti
        //------------------------------------------------------------------------------

        private bool ControllaVoti()
        {
           // int z, i;

            // questa routine serve a verificare l'integrità dei voti.
            // in pratica il n. di records che ci sono in Fvoti da salvare è:
            //      NVotazioni x NAzionisti
            // non solo, bisogna controllare che tutte le votazioni abbiano lo stesso
            // n. di voti e che tutte le deleghe siano state espresse
            // non è semplice, ma si fa.

            // per prima cosa controllo il numero totale, se ci sono differenze
            // ci sono sicuramente problemi
            int NRecVoti = NVoti * FNAzionisti;

            if (NRecVoti == FVotiDaSalvare.Count)
            {
                // in realtà dovrei controllare la congruità

                return true;
            }
            else
            {
                Logging.WriteToLog((LogNomeFile), "<error> Anomalia in ControllaVoti - exp:" +
                   NRecVoti.ToString() + " found:" + FVotiDaSalvare.Count + " badge:" + Badge_Letto.ToString());
                // se sono di meno devo fare il check
                if (NRecVoti < FVotiDaSalvare.Count)
                {
                    // non faccio nulla
                }
                return false;
            }

            //return true;
        }

        public void ControllaSalvaLinkVoto()
        {
            // questa routine serve a mantenere o a distruggere il link voto->badge
            // vedi situazione bpm

            // ok ora, se è false, distruggo il link
            if (!TotCfg.SalvaLinkVoto)
            {
                Random random = new Random();
                TVotiDaSalvare v;
                int TopRand = VSDecl.MAX_ID_RANDOM;

                for (int i = 0; i < FVotiDaSalvare.Count; i++)
                {
                    // trovo
                    v = (TVotiDaSalvare)FVotiDaSalvare[i];
                    // randomizzo il badge
                    v.AIDBadge_4 = random.Next(1, TopRand); 
                    //salvo
                    FVotiDaSalvare[i] = v;
                }
            }
        }

        // *************************************************************************
        // ----------------------------------------------------------------
        //   INTERFACCIA GRAFICA
        // ----------------------------------------------------------------
        // *************************************************************************

        // ----------------------------------------------------------------
        //   Caricamento Tema da VotoTheme
        // ----------------------------------------------------------------

        private void CaricaTemaInControlli()
        {
            // carico il tema da vototouch, semplicemente richiamando le singole label
            oVotoTheme.SetTheme_lbNomeDisgiunto(ref lbNomeDisgiunto);
            oVotoTheme.SetTheme_lbDisgiuntoRimangono(ref lbDisgiuntoRimangono);
            oVotoTheme.SetTheme_lbDirittiStart(ref lbDirittiStart);
            oVotoTheme.SetTheme_lbDirittiDiVoto(ref lbDirittiDiVoto);
            oVotoTheme.SetTheme_lbConferma(ref lbConferma);
            oVotoTheme.SetTheme_lbConfermaUp(ref lbConfermaUp);
            oVotoTheme.SetTheme_lbConfermaNVoti(ref lbConfermaNVoti);
            oVotoTheme.SetTheme_lbNomeAzStart(ref lbNomeAzStart);
        }

        // ----------------------------------------------------------------
        // Inizializzazione dei controlli
        // ----------------------------------------------------------------

        private void InizializzaControlli()
		{
			// DR->OK11
            Font MyFont = new Font(VSDecl.BTN_FONT_NAME, VSDecl.BTN_FONT_SIZE, FontStyle.Bold);
 
			// il pannello della conferma
			lbConferma.BackColor = Color.Transparent;
            lbConfermaUp.BackColor = Color.Transparent;

            if (DebugMode) pnBadge.Visible = true;     

		}

		// ----------------------------------------------------------------
		//  SETTAGGIO DEI COMPONENTI A INIZIO CAMBIO STATO
		// ----------------------------------------------------------------

		private void SettaComponenti(bool AVisibile)
		{
			// DR->OK				
			lbConferma.Visible = AVisibile;
            lbConfermaUp.Visible = AVisibile;
            lbConfermaNVoti.Visible = AVisibile;
			// label del differenziato
			lbDirittiStart.Visible = AVisibile;

			lbNome.Visible = AVisibile;
			lbNomeDisgiunto.Visible = AVisibile;
			lbDisgiuntoRimangono.Visible = AVisibile;
            lbNomeAzStart.Visible = AVisibile;

            if (DemoVersion)
            {
                if (btnBadgeUnVoto != null)
                    btnBadgeUnVoto.Visible = (Stato == TAppStato.ssvBadge);
                if (btnBadgePiuVoti != null)
                    btnBadgePiuVoti.Visible = (Stato == TAppStato.ssvBadge);
                if (btnFineVotoDemo != null)
                    btnFineVotoDemo.Visible = (Stato == TAppStato.ssvVotoFinito);

            }
		}
	
		private void TornaInizio()
		{
			// dall'inizio
			CurrVoteIDX = 0;
			Stato = TAppStato.ssvBadge;
			CambiaStato();
		}


        // ******************************************************************
        // ----------------------------------------------------------------
        // ORDINAMENTO ED ELABORAZIONE LISTE (AL CARICAMENTO)
        // ----------------------------------------------------------------
        // ******************************************************************

        #region AREA DI VOTO ORDINAMENTO ED ELABORAZIONE LISTE

        public void CalcolaAreaDiVoto()
        {
            // questa routine effettua calcoli preventivi per ogni singola votazione di tipo Candidato
            // o Multi candidato che riguarda l'area di lavoro dinamicamente in funzione del numero e caratteristiche 
            // dei candidati, più precisamente:
            // - Sapendo se e quanti candidati CDA ci sono, setta le aree di voto CDA e NORMALI
            // - in funzione del numero di candidati definisce i CandidatiPerPagina
            // - in funzione del numero di candidati setta NeedTabs, cioè sceglie se usare o no gli indirizzamenti
            //   alfabetici con le linguette (caso con pochi candidati)
            //
            // Il tutto viene messo nella struttura AreaVoto (TAreaVotazione) per ogni singola votazione.
            // Questo (per ora) non viene usato per le liste
            // innanzitutto ciclo sulle votazioni
            int i, CandAlt;
            
            // area di voto standard x Candidati è:
            // x: 20 y:180 ax:980 (w:960) ay:810 (h:630)  

            for (i = 0; i < NVoti; i++)
            {
                // solo se il voto è di candidato continuo
                if (FParVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO ||
                    FParVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                    FParVoto[i].TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                {
                    // 1° step aree di voto
                    // ok , verifico quanti candidati CDA. So che nell'area di voto c'è spazio per 6x2 righe
                    // in realtà devo lasciare uno spazio in mezzo tra i cda e i normali.
                    // i casi sono:
                    // CDA 0       :  5x2 Righe Alt = Candidati x pagina 10, 14 Linguette x 140 Candidati Totale
                    // CDA da 1 a 3:  1 Riga CDA e 4x2 Righe Alt = Candidati x pagina 8, 12 Linguette x 96 Candidati Totale
                    // CDA da 4 a 6:  2 Righe CDA e 3x2 Righe alt = Candidati Pagina 6, 10 Linguette x 60 Candidati Totale
                    // ma deve essere dinamico in funzione dei candidati
                    // calcolo i candidati alternativi
                    CandAlt = FParVoto[i].NListe - FParVoto[i].NPresentatoCDA; 

                    switch (FParVoto[i].NPresentatoCDA)
                    {
                        case 0:
                            // vedo se mi servono i tabs
                            FParVoto[i].AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_10);
                            // ok, ora setto l'area in pixel dei Alt
                            FParVoto[i].AreaVoto.XAlt = 3; //40px;
                            FParVoto[i].AreaVoto.YAlt = 25; //265px;
                            if (FParVoto[i].AreaVoto.NeedTabs)
                                FParVoto[i].AreaVoto.WAlt = 72; //930px;
                            else
                                FParVoto[i].AreaVoto.WAlt = 94; //1200px;
                            FParVoto[i].AreaVoto.HAlt = 52; //535px;
                            FParVoto[i].AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_10;
                            if (CandAlt < FParVoto[i].AreaVoto.CandidatiPerPagina)
                            {
                                FParVoto[i].AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x10 = new int[] { 0, 6, 6, 4, 4, 2, 2, 0, 0, 0, 0 };
                                FParVoto[i].AreaVoto.YAlt = FParVoto[i].AreaVoto.YAlt + x10[CandAlt];
                                FParVoto[i].AreaVoto.HAlt = FParVoto[i].AreaVoto.HAlt - (x10[CandAlt] * 2);
                            }
                            break;

                        case 1:
                        case 2:
                        case 3:
                            // vedo se mi servono i tabs
                            FParVoto[i].AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_8);
                            // ok, ora setto l'area in pixel
                            FParVoto[i].AreaVoto.XCda = 3; //40px;
                            FParVoto[i].AreaVoto.YCda = 25; //265px;
                            FParVoto[i].AreaVoto.WCda = 94; //1200px;
                            FParVoto[i].AreaVoto.HCda = 8; //80px;
                            // ok, ora setto l'area in pixel dei Alt
                            FParVoto[i].AreaVoto.XAlt = 3; //40px;
                            FParVoto[i].AreaVoto.YAlt = 42; //430px;
                            if (FParVoto[i].AreaVoto.NeedTabs)
                                FParVoto[i].AreaVoto.WAlt = 72; //930px;
                            else
                                FParVoto[i].AreaVoto.WAlt = 94; //1200px;
                            FParVoto[i].AreaVoto.HAlt = 36; //370px;
                            FParVoto[i].AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_8;
                            if (CandAlt < FParVoto[i].AreaVoto.CandidatiPerPagina)
                            {
                                FParVoto[i].AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x8 = new int[] { 0, 6, 6, 4, 4, 2, 2, 0, 0, 0, 0 };
                                FParVoto[i].AreaVoto.YAlt = FParVoto[i].AreaVoto.YAlt + x8[CandAlt];
                                FParVoto[i].AreaVoto.HAlt = FParVoto[i].AreaVoto.HAlt - (x8[CandAlt] * 2);
                            }
                            break;

                        case 4:
                        case 5:
                        case 6:
                            // vedo se mi servono i tabs
                            FParVoto[i].AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_6);
                            // ok, ora setto l'area in pixel dei CDA
                            FParVoto[i].AreaVoto.XCda = 3; //40px;
                            FParVoto[i].AreaVoto.YCda = 25; //265px;
                            FParVoto[i].AreaVoto.WCda = 94; //1200px;
                            FParVoto[i].AreaVoto.HCda = 17; //178px;
                            // ok, ora setto l'area in pixel dei Alt
                            FParVoto[i].AreaVoto.XAlt = 3; //40px;
                            FParVoto[i].AreaVoto.YAlt = 51; //520px;
                            if (FParVoto[i].AreaVoto.NeedTabs)
                                FParVoto[i].AreaVoto.WAlt = 72; //930px;
                            else
                                FParVoto[i].AreaVoto.WAlt = 94; //1200px;
                            FParVoto[i].AreaVoto.HAlt = 27; //280px;
                            FParVoto[i].AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_6;
                            if (CandAlt < FParVoto[i].AreaVoto.CandidatiPerPagina)
                            {
                                FParVoto[i].AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x6 = new int[] { 0, 4, 4, 2, 2, 0, 0, 0, 0, 0, 0 };
                                FParVoto[i].AreaVoto.YAlt = FParVoto[i].AreaVoto.YAlt + x6[CandAlt];
                                FParVoto[i].AreaVoto.HAlt = FParVoto[i].AreaVoto.HAlt - ( x6[CandAlt] * 2);
                            }
                            break;
                    }
                
                    // DA TOGLIERE SE FUNZIONA IL PEZZO NUOVO
                    // pezzo compatibilità vecchia                    
                    //if (FParVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO_SING) //!= VSDecl.VOTO_MULTICANDIDATO)
                    //{
                    //    FParVoto[i].AreaVoto.CandidatiPerPagina = VSDecl.CANDIDATI_PER_PAGINA;
                    //    FParVoto[i].AreaVoto.NeedTabs = true;
                    //}
                    // FINE DA TOGLIERE

                }
            }
        }

        public void OrdinaListeInPagine()
        {
            // TODO: da rivedere, se non servono i tabs è inutile fare sto casino

            // DR11 OK
            // questa routine interviene solamente nel caso di votazione candidato
            // o candidato singolo o multicandidato e serve per:
            // - creare il numero di pagine necessarie al totale delle liste
            // - creare un indice enciclopedico delle liste stesse
            // per far questo si usa:
            // - costante CANDIDATI_PER_PAGINA che ci dice quanti candidati ci stanno x pagina
            // - campo Pag in Tliste che contiene il n. di pagina associato al candidato
            // - campo Pagind che contiene l'indice enciclopedico della pagina 
            //   (es A - CG, CH - TF, TG - Z)

            int i, z, pg, pgind;
            string  sp;
            TListe li;
            TIndiceListe idx; //, idx1;

            // innanzitutto ciclo sulle votazioni
            for (i = 0; i < NVoti; i++)
            {
                // solo se il voto è di candidato continuo
                if (FParVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO ||
                    FParVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                    FParVoto[i].TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                {
                    // comunque cancello la collection delle pagine
                    FParVoto[i].Pagine.Clear();
                    // ok ora faccio una prima scansione per crearmi l'indice alfabetico
                    // e settare le pagine
                    // NOTA : i candidati presentati dal cda sono SEMPRE in pagina 0
                    // in più mi creo un array dei range di cognomi
                    pg = 1;
                    pgind = 1;
                    sp = "";
                    // la prima pagina, quella del cda la metto sempre, anche se non c'è il candidato
                    idx = new TIndiceListe();
                    idx.pag = 0;
                    idx.indice = "A - Z";
                    FParVoto[i].Pagine.Add(idx);
                    // ok, ora ciclo
                    for (z = 0; z < FParVoto[i].Liste.Count; z++)
                    {
                        // prelevo la lista che dovrebbe già essere ordinata in modo alfabetico
                        li = (TListe)FParVoto[i].Liste[z];
                        // testo se è presentato dal cda
                        if (li.PresentatodaCDA)
                        {
                            li.Pag = 0;
                            li.PagInd = "CdA";
                        }
                        else
                        {
                            // setto la pagina
                            li.Pag = pg;
                            // cognome di inizio
                            if (sp == "") sp = li.DescrLista;
                            // controllo ed eventualmente cambio pagina
                            pgind++;
                            // se sono arrivato ai 10 oppure sono arrivato alla fine
                            //if (pgind > VSDecl.CANDIDATI_PER_PAGINA ||
                            if (pgind > FParVoto[i].AreaVoto.CandidatiPerPagina ||
                                z == (FParVoto[i].Liste.Count - 1))
                            {
                                // cognome di fine e aggiungo pagina
                                idx = new TIndiceListe();
                                idx.pag = pg;
                                idx.sp = sp + "    ";  // metto gli spazi per il substring dopo
                                idx.ep = li.DescrLista + "    "; // come sopra, brutta ma efficace
                                FParVoto[i].Pagine.Add(idx);

                                // setto le variabili per la pagina successiva
                                sp = "";
                                pg++;
                                pgind = 1;
                            }
                        }
                        // aggiorno
                        FParVoto[i].Liste[z] = li;
                    } //for (z = 0; z < FParVoto[i].Liste.Count; z++)

                    // ok ora devo creare l'indice nella collection
                    for (z = 1; z < FParVoto[i].Pagine.Count; z++)
                    {
                        idx = (TIndiceListe)FParVoto[i].Pagine[z];

                        if (z == 1) idx.sp = "A  ";
                        if (z == (FParVoto[i].Pagine.Count - 1)) idx.ep = "Z  ";
                        idx.indice = idx.sp.Substring(0, 3).Trim() + "-" +
                                idx.ep.Substring(0, 3).Trim();
                        idx.indice = idx.indice.Trim();
                        FParVoto[i].Pagine[z] = idx;
                    }

                    // ok, ora metto le informazioni nelle liste
                    for (z = 0; z < FParVoto[i].Liste.Count; z++)
                    {
                        // prelevo la lista che dovrebbe già essere ordinata in modo alfabetico
                        li = (TListe)FParVoto[i].Liste[z];
                        // controllo per scrupolo l'indice
                        if (li.Pag < FParVoto[i].Liste.Count)
                        {
                            idx = (TIndiceListe)FParVoto[i].Pagine[li.Pag];
                            li.PagInd = idx.indice.ToLower();
                        }
                        FParVoto[i].Liste[z] = li;
                    }

                }  //if (FParVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO
            }  // for (i = 0; i < NVoti; i++)

        }

        private int StrChCompare(string st1, string st2)
        {
            int result, l, i;

            // ritorna la posizione del primo carattere diverso
            result = 0;
            // prendo la lunghezza più corta
            l = st1.Length;
            if (st2.Length < l) l = st2.Length;

            for (i = 0; i < l; i++)
            {
                // se sono diversi esci
                if (st1[i] != st2[i])
                {
                    break;
                }
                else
                    result++;
            }

            return result;
        }


        #endregion
        
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
            fConfig = new frmConfig(TotCfg);
            fConfig.ConfiguraLettore += new ehConfiguraLettore(OnConfiguraLettore);
            fConfig.SalvaConfigurazioneLettore += new ehSalvaConfigurazioneLettore(OnSalvaConfigurazioneLettore);
            fConfig.ConfiguraSemaforo += new ehConfiguraSemaforo(OnConfiguraSemaforo);
            fConfig.StatoSemaforo += new ehStatoSemaforo(OnStatoSemaforo);

            fConfig.Configura();
            fConfig.ShowDialog();
            fConfig = null;
 
            // aggiorna il componente (lo faccio comunque)
            CfgLettore(TotCfg.UsaLettore, TotCfg.PortaLettore);
            OnConfiguraSemaforo(this, TotCfg.UsaSemaforo, 
                TotCfg.IP_Com_Semaforo, TotCfg.TipoSemaforo);

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
            TotCfg.UsaLettore = AUsaLettore;
            TotCfg.PortaLettore = AComPort;
            if (TotCfg.TipoSemaforo == VSDecl.SEMAFORO_COM)
            {
                TotCfg.UsaSemaforo = AUsaSemaforo;
                TotCfg.IP_Com_Semaforo = ASemComPort;
            }
            // salva la configurazione sul database
            if (oDBDati.SalvaConfigurazione(NomeTotem, ref TotCfg) == 1)
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
            imgSemNo.Visible = !bok;
            imgSemOk.Visible = bok;
        }
        
        //public bool CopiaImmagini()
        //{
        //    // DR11 OK
        //    string SourcePath, dstName;

        //    try
        //    {
        //        // mi trovo il path dell'eseguibile
        //        SourcePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
        //        // e poi aggiungo il path immagini \\Data\\VtsNETImg\\ da dove copierò le immagini
        //        SourcePath = SourcePath + VSDecl.SOURCE_IMG_PATH;

        //        // allora, devo testare se c'è la cartella data in locale, se non c'è devo crearla
        //        // e copiare le immagini
        //        if (!System.IO.Directory.Exists("c:" + VSDecl.IMG_PATH_ABS) && System.IO.Directory.Exists(SourcePath))
        //        {
        //            // creao la cartella in locale, siccome sono in loading mi setto la splash screen
        //            if (splash != null) splash.SetSplash(12, "Creazione cartella immagini...");
        //            // creo la sotto cartella
        //            System.IO.Directory.CreateDirectory("c:" + VSDecl.IMG_PATH_ABS);
        //        }
        //        // copia immagini
        //        if (splash != null) splash.SetSplash(12, "Copia immagini...");

        //        // Process the list of files found in the directory.
        //        string[] fileEntries = System.IO.Directory.GetFiles(SourcePath);
        //        foreach (string fileName in fileEntries)
        //        {
        //             dstName = "c:" + VSDecl.IMG_PATH_ABS + System.IO.Path.GetFileName(fileName);
        //             if (splash != null) splash.SetSplash(12, "Copio " + dstName + "...");
        //             System.IO.File.Copy(fileName, dstName);
        //        }
        //        // ok, tutto a posto
        //        if (fileEntries.Length == 0)
        //            return false;
        //        else
        //            return true;
        //        //}
        //        //else
        //        //    return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //public void CaricaImmagini(string Immagine)
        //{
        //    // questa funzione tenta di caricare l'immagine
        //    // selezionata in background

        //    // cancello l'immagine prima perché sennò aumenta la memoria a palla;
        //    if (this.BackgroundImage != null)
        //        this.BackgroundImage.Dispose();


        //    // prima la cerco nella cartella data
        //    if (System.IO.File.Exists(Img_path + Immagine + ".png"))
        //    {
        //        this.BackgroundImage = Image.FromFile(Img_path + Immagine + ".png");
        //        return;
        //    }
        //}

        private static bool PrimoAvvio
        {
            get
            {
                bool primoAvvio = false;
                appMutex = new Mutex(true, "VotoSegreto.exe", out primoAvvio);
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
            TListe a;
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
            lbVersion.Items.Add("Usalettore: " + TotCfg.UsaLettore.ToString() + " Porta: " + TotCfg.PortaLettore.ToString());
            lbVersion.Items.Add("UsaSemaforo: " + TotCfg.UsaSemaforo.ToString() + " IP: " + TotCfg.IP_Com_Semaforo.ToString());
            lbVersion.Items.Add("IDSeggio: " + TotCfg.IDSeggio.ToString() + " NomeComputer: " + NomeTotem);
            lbVersion.Items.Add("ControllaPresenze: " + TotCfg.ControllaPresenze.ToString() +
                " CodiceUscita: " + TotCfg.CodiceUscita);
            lbVersion.Items.Add("SalvaLinkVoto: " + TotCfg.SalvaLinkVoto.ToString());
            lbVersion.Items.Add("SalvaVotoNonConfermato: " + TotCfg.SalvaVotoNonConfermato.ToString());
            lbVersion.Items.Add("IDSchedaUscitaForzata: " + TotCfg.IDSchedaUscitaForzata.ToString());
            lbVersion.Items.Add("AbilitaDirittiNonVoglioVotare: " + TotCfg.AbilitaDirittiNonVoglioVotare.ToString());
            lbVersion.Items.Add("");
            // le votazioni
            for (i = 0; i < NVoti; i++)
            {
                lbVersion.Items.Add("Voto: " + FParVoto[i].IDVoto.ToString() + ", Tipo: " +
                    FParVoto[i].TipoVoto.ToString() + ", " + FParVoto[i].Descrizione);
                lbVersion.Items.Add("   NListe: " + FParVoto[i].NListe + ", MaxScelte: " +
                    FParVoto[i].MaxScelte);
                lbVersion.Items.Add("   SKBianca: " + FParVoto[i].SkBianca.ToString() +
                    ", SKNonVoto: " + FParVoto[i].SkNonVoto);
                // Le liste
                for (z = 0; z < FParVoto[i].NListe; z++)
                {
                    a = (TListe)FParVoto[i].Liste[z];
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

            clsAzionisti c;

            lbVersion.Items.Clear();
            lbVersion.Items.Add(VSDecl.VTS_VERSION);
#if _DBClose
            lbVersion.Items.Add("DBClose version");
#endif
            for (i = 0; i < FAzionisti.Count; i++)
            {
                c = (clsAzionisti)FAzionisti[i];
                lbVersion.Items.Add("Badge: " + c.IDBadge.ToString() + " " + c.RaSo.Trim() +
                            " IDazion:" + c.IDAzion.ToString());
                lbVersion.Items.Add("   ProgDeleg:" + c.ProgDeleg.ToString() + " Coaz:" + c.CoAz +
                            " AzOrd: " + c.NAzioni.ToString());

            }
            Panel4.Visible = true;
        }

        private void edtBadge_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (DebugMode)
            {
                // il tasto più aumenta di uno edtBadge
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
            Logging.WriteToLog((LogNomeFile), "     >> Touch Watchdog intervenuto");
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
                if (MessageBox.Show("Questa operazione ricaricherà le liste/votazioni rileggendole " +
                    "dal database?\n Vuoi veramente continuare?", "Question",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    int pippo = CaricaListeVotazioni();
                    if (pippo == 0)
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
            if (MessageBox.Show("Questa operazione cancellerà TUTTI i voti " +
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



    }  // public class frmMain....
}  // Namespace....
