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
    // TODO: Bottone uscita
    // TODO: Votazione con contrari a tutte e astenuti a tutte
    // TODO: In caso di Votazione con AbilitaDiritti... mettere sulla videata di inizio lo stato dei diritti espressi
    // TODO: VERIFICARE DEMO MODE

	/// <summary>
	/// Summary description for Form1.\
	/// </summary>
    public partial class frmMain : Form
	{

        public delegate void EventDataReceived(object source, string messaggio);
        public event EventDataReceived evtDataReceived;

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
        public string Data_Path;                // path della cartella data
        public string   NomeTotem;              // nome della macchina
        public string   LogVotiNomeFile;        // nome file del log
        public bool CtrlPrimoAvvio;             // serve per chiudere la finestra in modo corretto
        
        // Votazioni
	    public TListaVotazioni Votazioni;
        //public TVotazione[] fVoto;        // array delle votazioni
        //public int NVoti;                       // ntot di votazioni

        // Dati dell'azionista e delle deleghe che si porta dietro
        public TListaAzionisti Azionisti;
        //public int FNAzionisti;				// n. di azionisti	
        //public ArrayList FAzionisti;        // Collection di clsAzionisti

        public bool IsVotazioneDifferenziata = false;
	    //public bool IsStartingVoto = false;             // potrebbe non servire

         // cpontrollo degli eventi di voto
	    private bool AperturaVotoEsterno;
        // flag uscita in votazione
        public bool UscitaInVotazione;

        // Variabile temporanea voti espressi Nuova Versione (Array)
        public ArrayList FVotiExpr;

        // risorse per l'internazionalizzazione
        ResourceManager rm;

        // ********************DA VEDERE************************************
        // Voti da salvare
        //public ArrayList FVotiDaSalvare ;        // array del salvataggio finale di TVotiDaSalvare
        // ciclo della votazione
        //public int CurrVoteIDX;
        // votazioni differenziate
        // Variabile temporanea Voti Espressi
        public int VotoEspresso;
        //public int VotoEspressoCarica;
        public string VotoEspressoStr;
        public string VotoEspressoStrUp;
        // Variabile temporanea voti espressi Nuova Versione (Array)
        //public ArrayList FVotiExpr;
        //public int CurrIdAzionDelega;		// Indice alla delega in voto corrente di FAzionisti nel differenziato
        //public DatiUtente DatiUsr;
        //public int utente_voti_bak;
        //public int utente_voti_diff;
		public int Badge_Letto;
        public string Badge_Seriale;
        // ********************DA VEDERE************************************

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
                // start the logging
                Logging.generateInternalLogFileName(Data_Path, "VotoTouch_" + NomeTotem);
                Logging.WriteToLog("---- DEMO MODE ----");
                // ok, ora creo la classe che logga i voti
                LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(Data_Path, "VotoT_" + NomeTotem);
            }
            else
            {
                // ok, qua devo vedere i due casi:
                // il primo è VTS_STANDALONE.txt presente il che vuol dire che ho la configurazione
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

            // Inizializzo la classe del database, mi servirà prima delle altre classi perché in
            // questa versione la configurazione è centralizzata sul db
            if (DemoVersion)
                oDBDati = new CVotoFileDati();
            else
                oDBDati = new CVotoDBDati();

            oDBDati.FDBConfig = DBConfig;
            //oDBDati.LogNomeFile = LogNomeFile;
            oDBDati.NomeTotem = NomeTotem;
            //oDBDati.ProgressoSalvaTutto += new ehProgressoSalvaTutto(onProgressoSalvaTutto);
            // se è standalone prende i dati in locale
            oDBDati.ADataLocal = System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt");
            oDBDati.AData_path = Data_Path;
            if (!oDBDati.CaricaConfig())
            {
                Logging.WriteToLog("<dberror> Problemi nel caricamento della configurazione DB, mappatura");
                MessageBox.Show(rm.GetString("SAPP_START_ERRCFG"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CtrlPrimoAvvio = false;
                return;
            }

            // vado avanti con il database
            // mi connetto
            //dbr = oDBDati.DBConnect();
            splash.SetSplash(30, rm.GetString("SAPP_START_INITCFG"));    //"Carico configurazione..."
            if (oDBDati.DBConnect() != null)
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

                if (TotCfg.VotoAperto) Logging.WriteToLog("Votazione già aperta");

                // carica le votazioni, le carica comunque all'inizio
                Votazioni = new TListaVotazioni(oDBDati, DemoVersion);
                Votazioni.CaricaListeVotazioni(Data_Path);
                //DBOk += CaricaListeVotazioni();

                // ok, finisce
                if (DBOk == 0)
                {
                    // nel log va tutto bene
                    Logging.WriteToLog("<startup> Caricamento dati database OK");
                    // ora secondo TotCfg.AbilitalogV creo il file del log per le versioni Standalone e normale
                    if (TotCfg.AbilitaLogV)
                    {
                        // Inizializzo il log dei voti
                        if (System.IO.File.Exists(Data_Path + "VTS_STANDALONE.txt"))
                            LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(Data_Path, "VotoT_" + NomeTotem);
                        else
                            LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(@"M:\LST\VotoTouch\",
                                    "VotoT_" + NomeTotem);
                    } 
                    if (TotCfg.AbilitaLogV) LogVote.WriteToLog((LogVotiNomeFile), "------ Inizio Applicazione ------");
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
            //oSemaforo.LogNomeFile = LogNomeFile;
            oSemaforo.ConnAddress = TotCfg.IP_Com_Semaforo;  //  deve essere "COM1" o "COMn"
            oSemaforo.ChangeSemaphore += onChangeSemaphore;
            if (TotCfg.UsaSemaforo)
                oSemaforo.AttivaSemaforo(true);

            splash.SetSplash(80, rm.GetString("SAPP_START_INITTOUCH"));       // "Inizializzo Touch..."
            // array dei voti da salvare
            //FVotiDaSalvare = new ArrayList();
            // array dei voti temporanei
            FVotiExpr = new ArrayList();
            // azionisti
		    Azionisti = new TListaAzionisti(oDBDati);
            //FAzionisti = new ArrayList();
            //FNAzionisti = 0;
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

            //CurrVoteIDX = 0;                // parte alla prima votazione
            IsVotazioneDifferenziata = false;               // non è differenziata
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
            Logging.WriteToLog(VSDecl.VTS_VERSION);
            Logging.WriteToLog("** Configurazione:");
            Logging.WriteToLog("   Usalettore: " + TotCfg.UsaLettore.ToString());
            Logging.WriteToLog("   Porta: " + TotCfg.PortaLettore.ToString());
            Logging.WriteToLog("   UsaSemaforo: " + TotCfg.UsaSemaforo.ToString());
            Logging.WriteToLog("   IPSemaforo: " + TotCfg.IP_Com_Semaforo.ToString());
            Logging.WriteToLog("   IDSeggio: " + TotCfg.IDSeggio.ToString());
            Logging.WriteToLog("   NomeComputer: " + NomeTotem);
            Logging.WriteToLog("   ControllaPresenze: " + TotCfg.ControllaPresenze.ToString());
            Logging.WriteToLog("** CodiceUscita: " + TotCfg.CodiceUscita);
            Logging.WriteToLog("");
            
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
            // ok, questa funzione serve all'oggetto CTouchscreen per evidenziare le zone sensibili
            if (oVotoTouch != null)
            {
                oVotoTouch.PaintTouch(sender, e);

                // controllo che CurrVoteIDX sia nel range giusto
                //if (CurrVoteIDX < NVoti)
                //{
                    // se la votazione corrente è di candidato su più pagine disegno i rettangoli
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
                    // se la votazione corrente è di MULTIcandidato su più pagine disegno i rettangoli
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
                
                // se sono nello stato di votostart e il n. di voti è > 1
                if (Stato == TAppStato.ssvVotoStart && Azionisti.HaDirittiDiVotoMultipli())
                {
                    // faccio il paint del numero di diritti di voto nel bottone in basso a sx , 
                    // in questo caso uso un paint e non una label per un problema grafico di visibilità
                    oVotoTheme.PaintDirittiDiVoto(sender, e, Azionisti.DammiMaxNumeroDirittiDiVotoTotali());
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
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    // non faccio nulla, non serve, al massimo non apparirà la scritta
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
                //oVotoTouch.FFormRect = FFormRect;
                //oVotoTouch.FFormRect.X = 0; // this.Top;
                //oVotoTouch.FFormRect.Y = 0; // this.Left;
                //oVotoTouch.FFormRect.Height = this.Height;
                //oVotoTouch.FFormRect.Width = this.Width;
            }
            // lo stesso faccio per la classe del thema che si occupa di disegnare 
            // le label di informazione
            if (oVotoTheme != null)
            {
                oVotoTheme.FFormRect = FFormRect;
                //oVotoTheme.FFormRect.X = 0; // this.Top;
                //oVotoTheme.FFormRect.Y = 0; // this.Left;
                //oVotoTheme.FFormRect.Height = this.Height;
                //oVotoTheme.FFormRect.Width = this.Width;
                CaricaTemaInControlli();
            }
            // ok ora le votazioni
            if (Votazioni != null)
            {
                Votazioni.CalcolaTouchZoneVotazioni(FFormRect);
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

        // *************** OBSOLETO CON I NUOVI OGGETTI *************************
        /*
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
            Logging.WriteToLog("Caricamento Liste/Votazioni");
 
            // testa se ci sono dati già caricati, se si li cancella
            if (fVoto != null)
            {
                // cancello le collection
                for (int i = 0; i < fVoto.Length; i++)
                {
                    fVoto[i].Liste.Clear();
                    fVoto[i].Pagine.Clear();
                }
            }
            // metto a nullo l'array
            fVoto = null;
            
            // carica le votazioni
            DBOk += oDBDati.CaricaDatiVotazioni(ref NVoti, ref fVoto);
            // carica i dettagli delle votazioni
            DBOk += oDBDati.CaricaDatiListe(ref NVoti, ref fVoto);
            // Calcolo l'area di voto per Candidati e multicandidati
            CalcolaAreaDiVoto();
            // ok, ora ordino le liste nel caso in cui siano di candidato
            OrdinaListeInPagine();

            // NOTA: Nelle liste il nome può contenere anche la data di nascita, inserita
            // come token tra ( e ). Serve nel caso di omonimia. La routine di disegno riconoscerà
            // questo e lo tratterà come scritta piccola a lato

            return DBOk;
        }
        */

        //private void onProgressoSalvaTutto(object source, int ATot, int AProg)
        //{
        //    if (prbSalvaTutto.Maximum != ATot) prbSalvaTutto.Maximum = ATot;
        //    prbSalvaTutto.Value = AProg;
        //}

      

		//------------------------------------------------------------------------------
		//  SALVATAGGIO DEI DATI DI VOTO COME SCHEDA BIANCA o nulla DA INTERRUZIONE
		//------------------------------------------------------------------------------

		private int MettiSchedeDaInterruzione()
		{
            // prima di tutto vedo se è attivato SalvaVotoNonConfermato
            // se sono nello stato di conferma, confermo il voto espresso e poi metto le altre schede
            if (Stato == TAppStato.ssvVotoConferma && TotCfg.SalvaVotoNonConfermato) 
                Azionisti.ConfermaVoti_VotoCorrente(ref FVotiExpr);

            // Dopodichè segnalo ad azionisti di riempire le votazioni con schede bianche, ma solo  
            // in funzione di AbilitaDirittiNonVoglioVotare:
            //      false - mi comporto normalmente, salvo i non votati con IDSchedaUscitaForzata
            //      true  - non faccio nulla, verranno come non votati e saranno disponibili alla nuova votazione

            if (!TotCfg.AbilitaDirittiNonVoglioVotare)
            {
                TVotoEspresso vz = new TVotoEspresso
                    {
                        NumVotaz = Votazioni.VotoCorrente.IDVoto,
                        VotoExp_IDScheda = TotCfg.IDSchedaUscitaForzata,
                        TipoCarica = 0,
                        Str_ListaElenco = "",
                        StrUp_DescrLista = ""
                    };

                Azionisti.ConfermaVotiDaInterruzione(vz);
            }



            //int i, z, NVotaz, NSKSalvate;
            ////TVotiDaSalvare v;
            //TAzionista c;

            
            // procedura chiamata dall'interruzione 999999 durante il voto
            // oppure al salvataggio se il n. di sk è minore x qualche motivo
			// qua devo scrivere tante schede bianche/nulle quante sono la differenza
			//
			// Ok, la cosa migliore è quella di ciclare sul progdeleghe e testare se c'è
			// un record di voto, se non c'è inserirlo
            // il parametro viene preso da TotCfg.IDSchedaUscitaForzata

            // prima di tutto vedo se è attivato SalvaVotoNonConfermato
            // se sono nello stato di conferma, confermo il voto espresso e poi metto le altre schede

            //if (Stato == TAppStato.ssvVotoConferma && TotCfg.SalvaVotoNonConfermato)
            //    ConfermaVotiEspressi();



            /*
             
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
                NVotaz = fVoto[z].IDVoto;

                for (i = 0; i < FNAzionisti; i++)
                {
                    c = (TAzionista)FAzionisti[i];
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
             */

		    return 0;
		}

        //public Boolean HaVotato(int NVotaz, int FProgDeleg)
        //{
        //    // DR11 Ok
        //    Boolean res;
        //    TVotiDaSalvare v;
        //    int i;

        //    // trovo almeno un voto del progdelega
        //    res = false;

        //    for (i = 0; i < FVotiDaSalvare.Count; i++)
        //    {
        //        v = (TVotiDaSalvare)FVotiDaSalvare[i];
        //        if (v.NumVotaz_1 == NVotaz && v.ProgDelega_5 == FProgDeleg)
        //        {
        //            res = true;
        //            break;
        //        }
        //    }
        //    return res;
        //}

        //------------------------------------------------------------------------------
        //  SALVATAGGIO DEI DATI DI VOTO su log
        //------------------------------------------------------------------------------

        //public void SalvaTuttoSuLog()
        //{
        //    string salva = Badge_Letto.ToString() + ";";
        //    TVotiDaSalvare v;

        //    try
        //    {
        //        for (int i = 0; i < FVotiDaSalvare.Count; i++)
        //        {
        //            v = (TVotiDaSalvare)FVotiDaSalvare[i];

        //            salva += v.NumVotaz_1.ToString() +
        //                    v.ProgDelega_5.ToString() + "-" +
        //                    v.AScheda_2.ToString() + ";";
        //            //v.NVoti_3.ToString() + ";" +
        //            //v.AScheda_2.ToString() + ";" +
        //        }

        //        if (TotCfg.AbilitaLogV) LogVote.WriteToLogCrypt(LogVotiNomeFile, salva);

        //    }
        //    catch (Exception ex)
        //    {
        //        // non faccio nulla
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //    }
        //}

        //------------------------------------------------------------------------------
        //  Controllo dei voti
        //------------------------------------------------------------------------------

        //private bool ControllaVoti()
        //{
        //   // int z, i;

        //    // questa routine serve a verificare l'integrità dei voti.
        //    // in pratica il n. di records che ci sono in Fvoti da salvare è:
        //    //      NVotazioni x NAzionisti
        //    // non solo, bisogna controllare che tutte le votazioni abbiano lo stesso
        //    // n. di voti e che tutte le deleghe siano state espresse
        //    // non è semplice, ma si fa.

        //    // per prima cosa controllo il numero totale, se ci sono differenze
        //    // ci sono sicuramente problemi
        //    int NRecVoti = NVoti * FNAzionisti;

        //    if (NRecVoti == FVotiDaSalvare.Count)
        //    {
        //        // in realtà dovrei controllare la congruità

        //        return true;
        //    }
        //    else
        //    {
        //        Logging.WriteToLog("<error> Anomalia in ControllaVoti - exp:" +
        //           NRecVoti.ToString() + " found:" + FVotiDaSalvare.Count + " badge:" + Badge_Letto.ToString());
        //        // se sono di meno devo fare il check
        //        if (NRecVoti < FVotiDaSalvare.Count)
        //        {
        //            // non faccio nulla
        //        }
        //        return false;
        //    }

        //    //return true;
        //}

        //public void ControllaSalvaLinkVoto()
        //{
        //    // questa routine serve a mantenere o a distruggere il link voto->badge
        //    // vedi situazione bpm

        //    /*
        //    // ok ora, se è false, distruggo il link
        //    if (!TotCfg.SalvaLinkVoto)
        //    {
        //        Random random = new Random();
        //        TVotiDaSalvare v;
        //        int TopRand = VSDecl.MAX_ID_RANDOM;

        //        for (int i = 0; i < FVotiDaSalvare.Count; i++)
        //        {
        //            // trovo
        //            v = (TVotiDaSalvare)FVotiDaSalvare[i];
        //            // randomizzo il badge
        //            v.AIDBadge_4 = random.Next(1, TopRand); 
        //            //salvo
        //            FVotiDaSalvare[i] = v;
        //        }
        //    }
        //     * */
        //}


        // ******************************************************************
        // ----------------------------------------------------------------
        // ORDINAMENTO ED ELABORAZIONE LISTE (AL CARICAMENTO)
        // ----------------------------------------------------------------
        // ******************************************************************

        #region AREA DI VOTO ORDINAMENTO ED ELABORAZIONE LISTE ***** OBSOLETO E VUOTO *********
/*
        // *************** OBSOLETO CON I NUOVI OGGETTI *************************
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
                if (fVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO ||
                    fVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                    fVoto[i].TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
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
                    CandAlt = fVoto[i].NListe - fVoto[i].NPresentatoCDA; 

                    switch (fVoto[i].NPresentatoCDA)
                    {
                        case 0:
                            // vedo se mi servono i tabs
                            fVoto[i].AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_10);
                            // ok, ora setto l'area in pixel dei Alt
                            fVoto[i].AreaVoto.XAlt = 3; //40px;
                            fVoto[i].AreaVoto.YAlt = 25; //265px;
                            if (fVoto[i].AreaVoto.NeedTabs)
                                fVoto[i].AreaVoto.WAlt = 72; //930px;
                            else
                                fVoto[i].AreaVoto.WAlt = 94; //1200px;
                            fVoto[i].AreaVoto.HAlt = 52; //535px;
                            fVoto[i].AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_10;
                            if (CandAlt < fVoto[i].AreaVoto.CandidatiPerPagina)
                            {
                                fVoto[i].AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x10 = new int[] { 0, 6, 6, 4, 4, 2, 2, 0, 0, 0, 0 };
                                fVoto[i].AreaVoto.YAlt = fVoto[i].AreaVoto.YAlt + x10[CandAlt];
                                fVoto[i].AreaVoto.HAlt = fVoto[i].AreaVoto.HAlt - (x10[CandAlt] * 2);
                            }
                            break;

                        case 1:
                        case 2:
                        case 3:
                            // vedo se mi servono i tabs
                            fVoto[i].AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_8);
                            // ok, ora setto l'area in pixel
                            fVoto[i].AreaVoto.XCda = 3; //40px;
                            fVoto[i].AreaVoto.YCda = 25; //265px;
                            fVoto[i].AreaVoto.WCda = 94; //1200px;
                            fVoto[i].AreaVoto.HCda = 8; //80px;
                            // ok, ora setto l'area in pixel dei Alt
                            fVoto[i].AreaVoto.XAlt = 3; //40px;
                            fVoto[i].AreaVoto.YAlt = 42; //430px;
                            if (fVoto[i].AreaVoto.NeedTabs)
                                fVoto[i].AreaVoto.WAlt = 72; //930px;
                            else
                                fVoto[i].AreaVoto.WAlt = 94; //1200px;
                            fVoto[i].AreaVoto.HAlt = 36; //370px;
                            fVoto[i].AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_8;
                            if (CandAlt < fVoto[i].AreaVoto.CandidatiPerPagina)
                            {
                                fVoto[i].AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x8 = new int[] { 0, 6, 6, 4, 4, 2, 2, 0, 0, 0, 0 };
                                fVoto[i].AreaVoto.YAlt = fVoto[i].AreaVoto.YAlt + x8[CandAlt];
                                fVoto[i].AreaVoto.HAlt = fVoto[i].AreaVoto.HAlt - (x8[CandAlt] * 2);
                            }
                            break;

                        case 4:
                        case 5:
                        case 6:
                            // vedo se mi servono i tabs
                            fVoto[i].AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_6);
                            // ok, ora setto l'area in pixel dei CDA
                            fVoto[i].AreaVoto.XCda = 3; //40px;
                            fVoto[i].AreaVoto.YCda = 25; //265px;
                            fVoto[i].AreaVoto.WCda = 94; //1200px;
                            fVoto[i].AreaVoto.HCda = 17; //178px;
                            // ok, ora setto l'area in pixel dei Alt
                            fVoto[i].AreaVoto.XAlt = 3; //40px;
                            fVoto[i].AreaVoto.YAlt = 51; //520px;
                            if (fVoto[i].AreaVoto.NeedTabs)
                                fVoto[i].AreaVoto.WAlt = 72; //930px;
                            else
                                fVoto[i].AreaVoto.WAlt = 94; //1200px;
                            fVoto[i].AreaVoto.HAlt = 27; //280px;
                            fVoto[i].AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_6;
                            if (CandAlt < fVoto[i].AreaVoto.CandidatiPerPagina)
                            {
                                fVoto[i].AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x6 = new int[] { 0, 4, 4, 2, 2, 0, 0, 0, 0, 0, 0 };
                                fVoto[i].AreaVoto.YAlt = fVoto[i].AreaVoto.YAlt + x6[CandAlt];
                                fVoto[i].AreaVoto.HAlt = fVoto[i].AreaVoto.HAlt - ( x6[CandAlt] * 2);
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

        // *************** OBSOLETO CON I NUOVI OGGETTI *************************
        public void OrdinaListeInPagine()
        {
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
            TLista li;
            TIndiceListe idx; //, idx1;

            // innanzitutto ciclo sulle votazioni
            for (i = 0; i < NVoti; i++)
            {
                // solo se il voto è di candidato continuo
                if (fVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO ||
                    fVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                    fVoto[i].TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                {
                    // comunque cancello la collection delle pagine
                    fVoto[i].Pagine.Clear();
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
                    fVoto[i].Pagine.Add(idx);
                    // ok, ora ciclo
                    for (z = 0; z < fVoto[i].Liste.Count; z++)
                    {
                        // prelevo la lista che dovrebbe già essere ordinata in modo alfabetico
                        li = (TLista)fVoto[i].Liste[z];
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
                            if (pgind > fVoto[i].AreaVoto.CandidatiPerPagina ||
                                z == (fVoto[i].Liste.Count - 1))
                            {
                                // cognome di fine e aggiungo pagina
                                idx = new TIndiceListe();
                                idx.pag = pg;
                                idx.sp = sp + "    ";  // metto gli spazi per il substring dopo
                                idx.ep = li.DescrLista + "    "; // come sopra, brutta ma efficace
                                fVoto[i].Pagine.Add(idx);

                                // setto le variabili per la pagina successiva
                                sp = "";
                                pg++;
                                pgind = 1;
                            }
                        }
                        // aggiorno
                        fVoto[i].Liste[z] = li;
                    } //for (z = 0; z < FParVoto[i].Liste.Count; z++)

                    // ok ora devo creare l'indice nella collection
                    for (z = 1; z < fVoto[i].Pagine.Count; z++)
                    {
                        idx = (TIndiceListe)fVoto[i].Pagine[z];

                        if (z == 1) idx.sp = "A  ";
                        if (z == (fVoto[i].Pagine.Count - 1)) idx.ep = "Z  ";
                        idx.indice = idx.sp.Substring(0, 3).Trim() + "-" +
                                idx.ep.Substring(0, 3).Trim();
                        idx.indice = idx.indice.Trim();
                        fVoto[i].Pagine[z] = idx;
                    }

                    // ok, ora metto le informazioni nelle liste
                    for (z = 0; z < fVoto[i].Liste.Count; z++)
                    {
                        // prelevo la lista che dovrebbe già essere ordinata in modo alfabetico
                        li = (TLista)fVoto[i].Liste[z];
                        // controllo per scrupolo l'indice
                        if (li.Pag < fVoto[i].Liste.Count)
                        {
                            idx = (TIndiceListe)fVoto[i].Pagine[li.Pag];
                            li.PagInd = idx.indice.ToLower();
                        }
                        fVoto[i].Liste[z] = li;
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

*/
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

            // Unità di test programma
            if (e.Alt && e.KeyCode == Keys.U)
            {
                FTest formTest = new FTest(oDBDati);
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
            //            for (i = 0; i < NVoti; i++)
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

            //TAzionista c;

            lbVersion.Items.Clear();
            lbVersion.Items.Add(VSDecl.VTS_VERSION);
#if _DBClose
            lbVersion.Items.Add("DBClose version");
#endif
            //for (i = 0; i < FAzionisti.Count; i++)
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
                if (MessageBox.Show("Questa operazione ricaricherà le liste/votazioni rileggendole " +
                    "dal database?\n Vuoi veramente continuare?", "Question",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    bool pippo = Votazioni.CaricaListeVotazioni(Data_Path);
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

        private void button2_Click(object sender, EventArgs e)
        {
            ////TListaAzionisti azio = new TListaAzionisti(oDBDati);
            ////azio.CaricaDirittidiVotoDaDatabase(10005, ref fVoto, NVoti);

            ////List<TAzionista> aziofilt = azio.DammiDirittiDiVotoPerIDVotazione(1, true);

            //TListaVotazioni vot = new TListaVotazioni(oDBDati);
            //vot.CaricaListeVotazioni();
        }



    }  // public class frmMain....
}  // Namespace....
