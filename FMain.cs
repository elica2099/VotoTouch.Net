using System;
using System.Resources;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace VotoTouch
{
    // TODO: verificare multicandidato e pagine
    // TODO: In caso di Votazione con AbilitaDiritti... mettere sulla videata di inizio lo stato dei diritti espressi
    // TODO: ModoAssemblea, salvare azioni o voti, mostrare azioni o voti
    // TODO: Mettere finestra riepilogo azionista
    // TODO: Unificare pop e agm in TListaAzionisti

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
        private System.Windows.Forms.Timer timPopup;

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
        public string   LogVotiNomeFile;        // nome file del log
        public bool CtrlPrimoAvvio;             // serve per chiudere la finestra in modo corretto

        // start del voto
	    public TStartVoteMode ModoStart = TStartVoteMode.vszNormal;
        // Votazioni
        public TListaVotazioni Votazioni;
        // Dati dell'azionista e delle deleghe che si porta dietro
        public TListaAzionisti Azionisti;
        // variabili relative alla votazione
        public bool IsVotazioneDifferenziata = false;
        public bool LocalAbilitaVotazDifferenziataSuRichiesta = false;
        // cpontrollo degli eventi di voto
	    private bool AperturaVotoEsterno;
        // flag uscita in votazione
        public bool UscitaInVotazione;
        // public bool 
	    public bool RitornaDaAnnulla = false;

        // Variabile temporanea voti espressi Nuova Versione (Array)
        public ArrayList FVotiExpr;
        // risorse per l'internazionalizzazione
        ResourceManager rm;

        // Variabile temporanea Voti Espressi
        public int VotoEspresso;
        public string VotoEspressoStr;
        public string VotoEspressoStrUp;
	    public string VotoEspressoStrNote;
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
            VTConfig.IsDebugMode = false;
            VTConfig.IsPaintTouch = false;
            VTConfig.IsDemoMode = false;

            CtrlPrimoAvvio = PrimoAvvio;
            if (!CtrlPrimoAvvio)
			{
				MessageBox.Show(rm.GetString("SAPP_START_CTRL"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				//Application.Exit();
				return;
			}
			
            // finestra di start
            frmStart = new FVSStart();
            AddOwnedForm(frmStart);
            if (frmStart.ShowDialog() != DialogResult.Yes)
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
#if DEBUG
            this.WindowState = FormWindowState.Normal;
		    this.Left = 0;
		    this.Height = 0;
            this.Width = 1280;
            this.Height = 1024;
            labelMousee.Visible = true;
#else      
            labelMousee.Visible = false;
            WindowState = FormWindowState.Maximized;
#endif
            // inizializzo il splashscreen
            splash.SetSplash(10, rm.GetString("SAPP_START_IMGSEARCH")); //Ricerco immagini...
           
            // ritrovo il nome della macchina che mi servirà per interrogare il db
			int i;
            VTConfig.NomeTotem = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
            for (i = 0; i < VTConfig.NomeTotem.Length; i++)
                if (VTConfig.NomeTotem[i] == '\\') break;
            VTConfig.NomeTotem = VTConfig.NomeTotem.Remove(0, i + 1);

            // ok, per prima cosa verifico se c'è la cartella c:\data, se si ok
            // sennò devo considerare la cartella dell'applicazione, se non c'è esco
            oVotoImg = new CVotoImages();
            oVotoImg.MainForm = this;
            CtrlPrimoAvvio = oVotoImg.CheckDataFolder(ref Data_Path);
		    pnPopupRed.Left = 5;
            pnPopupRed.Top = 5;

            btnCancVoti.Visible = File.Exists(Data_Path + "VTS_ADMIN.txt");

            // identificazione della versione demo, nella cartella data o nella sua cartella
            if (File.Exists(Data_Path + "VTS_DEMO.txt"))
            {
                // Ok è la versione demo
                VTConfig.IsDemoMode = true;
                // start the logging
                Logging.generateInternalLogFileName(Data_Path, "VotoTouch_" + VTConfig.NomeTotem);
                Logging.WriteToLog("---- DEMO MODE ----");
                // ok, ora creo la classe che logga i voti
                LogVotiNomeFile = LogVote.GenerateDefaultLogFileName(Data_Path, "VotoT_" + VTConfig.NomeTotem);
            }
            else
            {
                // ok, qua devo vedere i due casi:
                // il primo è VTS_STANDALONE.txt presente il che vuol dire che ho la configurazione
                // in locale, caricando comunque un file GEAS.sql da data
                if (File.Exists(Data_Path + "VTS_STANDALONE.txt"))
                {
                    Logging.generateInternalLogFileName(Data_Path, "VotoTouch_" + VTConfig.NomeTotem);
                    Logging.WriteToLog("---- STANDALONE MODE ----");
                }
                else
                {
                    // verifica della mappatura
                    if (!Directory.Exists(@"M:\"))
                    {
                        MessageBox.Show(rm.GetString("SAPP_START_ERRMAP"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CtrlPrimoAvvio = false;
                        return;
                    }

                    // Inizializzo il log
                    if (!Directory.Exists(@"M:\LST\VotoTouch\"))
                        Directory.CreateDirectory(@"M:\LST\VotoTouch\");
                    Logging.generateInternalLogFileName(@"M:\LST\VotoTouch\", "VotoTouch_" + VTConfig.NomeTotem);
                }
            }
            // loggo l'inizio dell'applicazione
            Logging.WriteToLog("<start> Inizio Applicazione");
            splash.SetSplash(20, rm.GetString("SAPP_START_INITDB"));    // "Inizializzo database...");

            // identificazione DebugMode
            VTConfig.IsDebugMode = File.Exists(Data_Path + "VTS_DEBUG.txt");
            VTConfig.IsPaintTouch = File.Exists(Data_Path + "VTS_PAINT_TOUCH.txt");

            // classe lbConferma
		    lbConferma = new LabelCandidati {Visible = false, Parent = this};

		    // Inizializzo la classe del database, mi servirà prima delle altre classi perché in
            // questa versione la configurazione è centralizzata sul db
            bool dataloc = File.Exists(Data_Path + "VTS_STANDALONE.txt");
            if (VTConfig.IsDemoMode)
                //oDBDati = new CVotoMDBDati(DBConfig, dataloc, Data_Path);
                oDBDati = new CVotoFileDati(DBConfig, dataloc, Data_Path);
            else
                oDBDati = new CVotoDBDati(DBConfig, dataloc, Data_Path);

            //oDBDati.FDBConfig = DBConfig;
            //oDBDati.NomeTotem = NomeTotem;
            //// se è standalone prende i dati in locale
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
                DBOk += oDBDati.DammiConfigTotem(); //, ref TotCfg);
                splash.SetSplash(50, rm.GetString("SAPP_START_INITVOT"));  //"Carico liste e votazioni..."

                if (VTConfig.VotoAperto) Logging.WriteToLog("Votazione già aperta");

                // carica le votazioni, le carica comunque all'inizio
                Rectangle FFormRect = new Rectangle(0, 0, Width, Height);
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
            // A questo punto la configurazione è attiva e caricata centralmente, posso continuare
            //// il lettore del badge
            //NewReader = new CNETActiveReader();
            //NewReader.ADataRead += ObjDataReceived;
            //evtDataReceived += new EventDataReceived(onDataReceived);
            //// ora cerco se c'è qualche porta che va bene
            //string ComPort = ""; 
            //string ComDescr = "";
            //int ComPortInt = 0;
            //bool foundsomething = NewReader.AutodiscoverBarcode(ref ComPort, ref ComDescr, ref ComPortInt);
            //// lo attiverà nel load
            //if (VTConfig.UsaLettore)
            //{
            //    NewReader.PortName = "COM" + VTConfig.PortaLettore.ToString();
            //}
            //else
            //{
            //    // se ho trovato qualcosa allora lo salvo
            //}

            splash.SetSplash(70, rm.GetString("SAPP_START_INITSEM"));       //"Inizializzo Semaforo..."
            // il semaforo, ora fa tutto lei
            //SemaforoOKImg(VTConfig.UsaSemaforo);
            // ok, in funzione del tipo di semaforo faccio
            // USARE SEMPRE CIPThreadSemaphore
            if (VTConfig.TipoSemaforo == VSDecl.SEMAFORO_IP)
                oSemaforo = new CIPThreadSemaphore();
            else
                oSemaforo = new CComSemaphore();
            // se è attivato lo setto
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
            oVotoTouch.ShowPopup += new ehShowPopup(oVotoTouch_ShowPopup);
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
		    oVotoTouch.PremutoGruppiAvanti += new ehPremutoGruppiAvanti(onPremutoVotoValidoGruppo);
		    oVotoTouch.Premuto_AK_Avanti += new ehPremuto_AK_Avanti(onPremuto_AK_Avanti);

            // classe del tema
            oVotoTheme = new CVotoTheme();
            oVotoTheme.CaricaTemaDaXML(oVotoImg.Img_path);

            IsVotazioneDifferenziata = false;               // non è differenziata
            Badge_Letto = 0;
            AperturaVotoEsterno = VTConfig.VotoAperto;  // lo setto uguale così in stato badge non carica 2 volte le Liste
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
            timAutoRitorno = new System.Windows.Forms.Timer {Enabled = false, Interval = VTConfig.TimeAutoRitornoVoto };
		    timAutoRitorno.Tick += timAutoRitorno_Tick;
            // popup multicandidati
            timPopup = new System.Windows.Forms.Timer { Enabled = false, Interval = 6000 };
            timPopup.Tick += timPopup_Tick;

            //pnSemaf.BackColor = Color.Transparent;

            splash.SetSplash(90, rm.GetString("SAPP_START_INITVAR"));   //"Inizializzo variabili...");
            // scrive la configurazione nel log
            Logging.WriteToLog(VSDecl.VTS_VERSION);
            Logging.WriteToLog("** Configurazione:");
            Logging.WriteToLog("   Usalettore: " + VTConfig.UsaLettore.ToString());
            Logging.WriteToLog("   Porta: " + VTConfig.PortaLettore.ToString());
            Logging.WriteToLog("   UsaSemaforo: " + VTConfig.UsaSemaforo.ToString());
            Logging.WriteToLog("   IPSemaforo: " + VTConfig.IP_Com_Semaforo);
            Logging.WriteToLog("   IDSeggio: " + VTConfig.IDSeggio.ToString());
            Logging.WriteToLog("   NomeComputer: " + VTConfig.NomeTotem);
            Logging.WriteToLog("   ControllaPresenze: " + VTConfig.ControllaPresenze.ToString());
            Logging.WriteToLog("** CodiceUscita: " + VTConfig.CodiceUscita);
            Logging.WriteToLog("");
            
            // inizializzo i componenti
			InizializzaControlli();
            // Se è in demo mode metto i controlli
            if (VTConfig.IsDemoMode)
                InizializzaControlliDemo();

			// ora inizializzo la macchina a stati
			Stato = TAppStato.ssvBadge;
            
            splash.SetSplash(100);   
            splash.Hide();

            // se sono in debug evidenzio le zone sensibili
            oVotoTouch.PaintTouchOnScreen = VTConfig.IsPaintTouch;

            // se la votazione è aperta il timer di controllo voto batte di meno
            timVotoApero.Interval = VTConfig.VotoAperto ? VSDecl.TIM_CKVOTO_MAX : VSDecl.TIM_CKVOTO_MIN;

            // Attivo la macchina a stati (in FMain_MacchinaAStati.cs)
            CambiaStato();
        }

        void oVotoTouch_ShowPopup(object source, string messaggio)
        {
            lblMsgPopup.Text = messaggio;
            pnPopupRed.Visible = true;
            timPopup.Enabled = true;
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmMain());
		}

	    private void frmMain_Closed(object sender, EventArgs e)
		{
            timVotoApero.Enabled = false;
			// alcune cose sul database
			oDBDati.DBDisconnect();
            NewReader.Close();
		}

		private void frmMain_Closing(object sender, CancelEventArgs e)
		{
            //
        }

		private void frmMain_Load(object sender, EventArgs e)
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

            //if (VTConfig.UsaLettore)
            //{
            //    if (!NewReader.Open())
            //    {
            //        // ci sono stati errori con la com all'apertura
            //        VTConfig.UsaLettore = false;
            //        MessageBox.Show(
            //            rm.GetString("SAPP_START_ERRCOM1") + VTConfig.PortaLettore + rm.GetString("SAPP_START_ERRCOM2"),
            //            "Error",
            //            MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        imgERBarcode.Visible = true;
            //    }
            //    else
            //        imgERSemaf.Visible = false;
            //}
            timVotoApero.Enabled = true;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            // in shown attivo il barcode reader
            NewReader = new CNETActiveReader();
            NewReader.ADataRead += ObjDataReceived;
            evtDataReceived += new EventDataReceived(onDataReceived);
            // ora cerco se c'è qualche porta che va bene
            string ComPort = "";
            string ComDescr = "";
            int ComPortInt = 0;
            bool foundsomething = NewReader.AutodiscoverBarcode(ref ComPort, ref ComDescr, ref ComPortInt);
            // Attivo        
            if (VTConfig.UsaLettore)
            {
                if (foundsomething && VTConfig.PortaLettore != ComPortInt)
                {
                    // todo: ha perso la configurazione della com, non sa cosa fare
                }
                NewReader.PortName = "COM" + VTConfig.PortaLettore.ToString();
            }
            else
            {
                // se ho trovato qualcosa e non c'era ancora la configurazione allora lo salvo comunque
                if (foundsomething)
                {
                    VTConfig.UsaLettore = true;
                    VTConfig.PortaLettore = ComPortInt;
                    NewReader.PortName = "COM" + VTConfig.PortaLettore.ToString();
                    // salvo nel db
                    oDBDati.SalvaConfigurazionePistolaBarcode();
                }
            } 
            
            if (VTConfig.UsaLettore)
            {
                if (!NewReader.Open())
                {
                    // ci sono stati errori con la com all'apertura
                    VTConfig.UsaLettore = false;
                    MessageBox.Show(
                        rm.GetString("SAPP_START_ERRCOM1") + VTConfig.PortaLettore + rm.GetString("SAPP_START_ERRCOM2"),
                        "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    imgERBarcode.Visible = true;
                }
                else
                    imgERSemaf.Visible = false;
            }

        }
        
        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            // ok, questa funzione serve all'oggetto CTouchscreen per evidenziare le zone sensibili
            if (oVotoTouch != null)
            {
                oVotoTouch.PaintTouch(sender, e);

                // se la votazione corrente è di candidato su più pagine disegno i rettangoli
                if (Stato == TAppStato.ssvVoto &&
                    (Votazioni.VotoCorrente.TipoVoto == TTipoVoto.stvCandidato ||
                     Votazioni.VotoCorrente.TipoVoto == TTipoVoto.stvCandidatoSing))
                    //(Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_CANDIDATO ||
                    //    Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_CANDIDATO_SING))
                {
                    // paint delle label Aggiuntive
                    //oVotoTheme.PaintlabelProposteCdaAlt(sender, e, ref Votazioni.VotoCorrente, true);
                    oVotoTheme.PaintlabelProposteCdaAlt(sender, e, Votazioni.VotoCorrente, true);
                    // paint dei Bottoni
                    oVotoTouch.PaintButtonCandidatoPagina(sender, e, false, oVotoTheme.BaseFontCandidato, 
                        oVotoTheme.BaseFontCandidatoBold, oVotoTheme.BaseColorCandidato);
                }
                // se la votazione corrente è di MULTIcandidato su più pagine disegno i rettangoli
                if (Stato == TAppStato.ssvVoto && 
                    (Votazioni.VotoCorrente.TipoVoto == TTipoVoto.stvMultiCandidato || 
                        Votazioni.VotoCorrente.TipoVoto == TTipoVoto.stvGruppo)) // VSDecl.VOTO_MULTICANDIDATO)
                {
                    // paint delle label Aggiuntive
                    oVotoTheme.PaintlabelProposteCdaAlt(sender, e, Votazioni.VotoCorrente, false);
                    oVotoTheme.PaintlabelNSelezioni(sender, e, Votazioni.VotoCorrente, false);
                    // paint dei bottoni
                    oVotoTouch.PaintButtonCandidatoPagina(sender, e, true, oVotoTheme.BaseFontCandidato,
                        oVotoTheme.BaseFontCandidatoBold, oVotoTheme.BaseColorCandidato);
                }

                // ******* OBSOLETO ********/
                // votazione VOTO_CANDIDATO_SING, candidato a singola pagina, disegno i rettangoli
                //if (Stato == TAppStato.ssvVoto && (FParVoto[CurrVoteIDX].TipoVoto == VSDecl.VOTO_CANDIDATO_SING))
                //    oVotoTouch.PaintButtonCandidatoSingola(sender, e);
                
                // se sono nello stato di votostart e il n. di voti è > 1
                if (Stato == TAppStato.ssvVotoStart) // && Azionisti.HaDirittiDiVotoMultipli())
                {

                    // faccio il paint del numero di diritti di voto nel bottone in basso a sx , 
                    // in questo caso uso un paint e non una label per un problema grafico di visibilità
                    int VVoti = VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP
                        ? Azionisti.DammiMaxNumeroDirittiDiVotoTotali()
                        : Azionisti.DammiMaxNumeroVotiTotali();
                    string ssVoti = string.Format("{0:N0}", VVoti.ToString());

                    switch (ModoStart)
                    {
                        case TStartVoteMode.vszNormal:
                            // non stampo nessuna etichetta
                            break;
                        case TStartVoteMode.vszMixedDiffer:
                            // stampo l'etichetta
                            ssVoti += "(d)";
                            oVotoTheme.PaintDirittiDiVoto(sender, e, ssVoti);
                            break;
                        case TStartVoteMode.vszOnlyDiffer:
                            // non stampo nessuna etichetta
                            break;
                    }
                }
            }

            // se è demo devo stampare una label
            if (VTConfig.IsDemoMode)
            {
                try
                {
                    System.Drawing.Drawing2D.GraphicsState gs = e.Graphics.Save();
                    Font fn = new Font("Tahoma", 90, FontStyle.Bold);
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
            //if (oVotoTouch != null)
            //{
            //    oVotoTouch.CalcolaVotoTouch(FFormRect);
            //}
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
                Votazioni.ResizeZoneVotazioni(FFormRect);
                //Votazioni.CalcolaTouchZoneVotazioni(FFormRect);
            }
            
            // ok, ora se è in demo mode faccio il resize dei controlli
            if (VTConfig.IsDemoMode)
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
       
		//------------------------------------------------------------------------------
		//  SALVATAGGIO DEI DATI DI VOTO COME SCHEDA BIANCA o nulla DA INTERRUZIONE
		//------------------------------------------------------------------------------

		private int MettiSchedeDaInterruzione()
		{
            // prima di tutto vedo se è attivato SalvaVotoNonConfermato
            // se sono nello stato di conferma, confermo il voto espresso e poi metto le altre schede
            if (Stato == TAppStato.ssvVotoConferma && VTConfig.SalvaVotoNonConfermato) 
                Azionisti.ConfermaVoti_VotoCorrente(ref FVotiExpr);

            // Dopodichè segnalo ad azionisti di riempire le votazioni con schede bianche, ma solo  
            // in funzione di AbilitaDirittiNonVoglioVotare:
            //      false - mi comporto normalmente, salvo i non votati con IDSchedaUscitaForzata
            //      true  - non faccio nulla, verranno come non votati e saranno disponibili alla nuova votazione

            if (!VTConfig.AbilitaDirittiNonVoglioVotare)
            {
                TVotoEspresso2 vz = new TVotoEspresso2
                    {
                        NumVotaz = Votazioni.VotoCorrente.IDVoto,
                        NumSubVotaz = 0,
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
            if (oDBDati.SalvaConfigurazione() == 1)
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
            imgERSemaf.Visible = !bok;
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

            // Unità di test programma
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
            int z;
            
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
            lbVersion.Items.Add("IDSeggio: " + VTConfig.IDSeggio.ToString() + " NomeComputer: " + VTConfig.NomeTotem);
            lbVersion.Items.Add("ControllaPresenze: " + VTConfig.ControllaPresenze.ToString());
            lbVersion.Items.Add("     0: Non controllare, 1: Blocca");
            lbVersion.Items.Add("     2: Forza Ingresso (ora) 3: Forza ingresso Geas");
            lbVersion.Items.Add("MaxDeleghe: " + VTConfig.MaxDeleghe);
            lbVersion.Items.Add("AbilitaDifferenziataSuRichiesta: " + VTConfig.AbilitaDifferenziatoSuRichiesta.ToString());
            lbVersion.Items.Add("CodiceUscita: " + VTConfig.CodiceUscita);
            lbVersion.Items.Add("SalvaLinkVoto: " + VTConfig.SalvaLinkVoto.ToString());
            lbVersion.Items.Add("SalvaVotoNonConfermato: " + VTConfig.SalvaVotoNonConfermato.ToString());
            lbVersion.Items.Add("SalvaVoto In geas: " + VTConfig.SalvaVotoInGeas.ToString());
            lbVersion.Items.Add("IDSchedaUscitaForzata: " + VTConfig.IDSchedaUscitaForzata.ToString());
            lbVersion.Items.Add("AbilitaDirittiNonVoglioVotare: " + VTConfig.AbilitaDirittiNonVoglioVotare.ToString());
            lbVersion.Items.Add("TimerAutoritorno: " + VTConfig.AttivaAutoRitornoVoto.ToString());
            lbVersion.Items.Add("Tempo TimerAutoritorno (ms): " + VTConfig.TimeAutoRitornoVoto.ToString());
            lbVersion.Items.Add("AK_CheckVote: " + VTConfig.AKCheckVote.ToString());
            lbVersion.Items.Add("");
            // le votazioni
            foreach (TNewVotazione fVoto in Votazioni.Votazioni)
            {
                lbVersion.Items.Add("Voto: " + fVoto.IDVoto.ToString() +
                    ", Tipo: " +
                    fVoto.TipoVoto.ToString() + ", " + fVoto.Descrizione);
                lbVersion.Items.Add("   NListe: " + fVoto.NListe + ", MaxScelte: " +
                    fVoto.MaxScelte);
                lbVersion.Items.Add("   SKBianca: " + fVoto.SkBianca.ToString() +
                    ", SKNonVoto: " + fVoto.SkNonVoto);
                // Le liste
                for (z = 0; z < fVoto.NListe; z++)
                {
                    a = (TNewLista)fVoto.Liste[z];
                    lbVersion.Items.Add("    Lista:" + a.IDLista.ToString() + 
                        ", idSubV: " + a.NumSubVotaz + 
                        ", IdSk:" +
                        a.IDScheda.ToString() + ", " + a.DescrLista + ", p" +
                        a.Pag.ToString() + " " + a.PagInd + "  cda: " + a.PresentatodaCDA.ToString());
                }
            }
            Panel4.Visible = true;

        }

        private void MostaPannelloStatoAzionista()
        {
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
                            " AzOrd: " + c.NVoti.ToString() + " AKPV:" + c.AK_PrevVote.ToString());

            }
            Panel4.Visible = true;
        }

        private void edtBadge_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (VTConfig.IsDebugMode)
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

        private void btnAbilitaDifferenziata_Click(object sender, EventArgs e)
        {
            BadgeLetto("88889900");
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
            //StartTest();
            ////TListaAzionisti azio = new TListaAzionisti(oDBDati);
            ////azio.CaricaDirittidiVotoDaDatabase(10005, ref fVoto, NVoti);

            ////List<TAzionista> aziofilt = azio.DammiDirittiDiVotoPerIDVotazione(1, true);

            //TListaVotazioni vot = new TListaVotazioni(oDBDati);
            //vot.CaricaListeVotazioni();
        }

        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
#if DEBUG
            float vx = (VSDecl.VOTESCREEN_DIVIDE_WIDTH / this.Width) * Cursor.Position.X;
            float vy = (VSDecl.VOTESCREEN_DIVIDE_HEIGHT / this.Height) * Cursor.Position.Y;
            float lx = (100 / this.Width) * Cursor.Position.X;
            float ly = (100 / this.Height) * Cursor.Position.Y;

            labelMousee.Text = "Local: " + e.X + ", " + e.Y + ". \n"
               + "Vote: " + (int)vx + ", " + (int)vy + ". \n"
                +"Label: " + (int)lx + ", " + (int)ly + ". \n"
               + "Globalis " + Cursor.Position.X + ", " + Cursor.Position.Y + ".";
#endif
        }




    }  // public class frmMain....
}  // Namespace....
