using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Media;


namespace VotoTouch
{
    // DR16 - Classe intera

    // eventi di pressione scheda
    public enum TTEvento : int { steVotaNormale, steVotaDiffer, steConferma, 
        steAnnulla, steVotoValido, steInvalido, steTabs, steSkBianca, steSkNonVoto,
        steMultiValido, steMultiAvanti, steMultiSelezTuttiCDA, steSelezTuttiCDAAvanti,
        steBottoneUscita, steSkContrarioTutti, steSkAstenutoTutti
    };

    // struttura zone dello schermo
    public class TTZone
    {
        public int id;

        public int x;   
        public int y;
        public int r;       // right
        public int b;       // bottom
        public int expr;
        public int idscheda;
        public TTEvento ev;
        public string Text;
        public int pag;     // importante, gli item con pag=0 vengono sempre visualizzati/considerati
        public bool cda;
        // Paint Mode
        public int PaintMode;
        // per la multivotazione
        public bool MultiNoPrint;
        public int Multi;
        public Rectangle CKRect;
        // per il gruppo
        public int IDVotaz;

        public TTZone()
        {
            MultiNoPrint = false;
            PaintMode = VSDecl.PM_NONE;
            IDVotaz = 0;
        }
    }

    //Dichiaro il delegate
    public delegate void testEventHandler(object source, string messaggio);
    public delegate void ehShowPopup(object source, string messaggio);

    public delegate void ehPremutoVotaNormale(object source, int VParam);
    public delegate void ehPremutoVotaDifferenziato(object source, int VParam);
    public delegate void ehPremutoConferma(object source, int VParam);
    public delegate void ehPremutoAnnulla(object source, int VParam);
    public delegate void ehPremutoVotoValido(object source, int VParam, bool ZParam);
    public delegate void ehPremutoInvalido(object source, int VParam);
    public delegate void ehPremutoTab(object source, int VParam);
    // sk bianca + non voto (v. 3.1)
    public delegate void ehPremutoSchedaBianca(object source, int VParam);
    public delegate void ehPremutoNonVoto(object source, int VParam);
    // multivotazione (v. 3.2)
    public delegate void ehPremutoMultiAvanti(object source, int VParam, ref List<int> voti);
    public delegate void ehPremutoMulti(object source, int VParam);
    // (v. 4.0) btnUscita contraritutti astenutitutti
    public delegate void ehPremutoBottoneUscita(object source, int VParam);
    public delegate void ehPremutoContrarioTutti(object source, int VParam);
    public delegate void ehPremutoAstenutoTutti(object source, int VParam);

    public delegate void ehTouchWatchDog(object source, int VParam);

    /// <summary>
	/// Summary description for CVotoTouchScreen.
	/// </summary>
	public class CVotoTouchScreen
	{
        public const int TIMER_TOUCH_INTERVAL = 250;
        public const int TIMER_TOUCHWATCH_INTERVAL = 1000;

        public event ehShowPopup ShowPopup;

        public event ehPremutoVotaNormale PremutoVotaNormale;
        public event ehPremutoVotaDifferenziato PremutoVotaDifferenziato;
        public event ehPremutoConferma PremutoConferma;
        public event ehPremutoAnnulla PremutoAnnulla;
        public event ehPremutoVotoValido PremutoVotoValido;
        public event ehPremutoInvalido PremutoInvalido;
        public event ehPremutoTab PremutoTab;
        // sk bianca + non voto (v. 3.1)
        public event ehPremutoSchedaBianca PremutoSchedaBianca;
        public event ehPremutoNonVoto PremutoNonVoto;
        // multivotazione (v. 3.2)
        public event ehPremutoMultiAvanti PremutoMultiAvanti;
        public event ehPremutoMulti PremutoMulti;               // serve per il repaint
        // (v. 4.0) btnUscita contraritutti astenutitutti
        public event ehPremutoBottoneUscita PremutoBottoneUscita;
        public event ehPremutoContrarioTutti PremutoContrarioTutti;
        public event ehPremutoAstenutoTutti PremutoAstenutoTutti;

        public event ehTouchWatchDog TouchWatchDog;

        public bool PaintTouchOnScreen;
        public Rectangle FFormRect;         
        private ArrayList Tz;

        //// oggetti conferma e inizio voto
        //CBaseTipoVoto ClasseTipoVotoStartNorm = null;
        //CBaseTipoVoto ClasseTipoVotoStartDiff = null;
        //CBaseTipoVoto ClasseTipoVotoConferma = null;

        public Bitmap btnBmpCand;
        public Bitmap btnBmpCandCda;
        //public Bitmap btnBmpCandArancio; 
        public Bitmap btnBmpCandSing;
        public Bitmap btnBmpCandSelez;
        public Bitmap btnBmpCandSelezCda;
        public Bitmap btnBmpTab;
        public Bitmap btnBmpTabSelez;
        public Bitmap btnBmpCDASelez;
        public Bitmap btnBmpCheck;

        // Gestione delle Multivotazioni
        public int MaxMultiCandSelezionabili = 0;
        public int MinMultiCandSelezionabili = 0;

        // Gestione delle Pagine
        public int CurrPag = 1;

        // i ritardi/watchdog del touch
        private Timer timTouch;
        private Timer timTouchWatchDog;
        private bool TouchEnabled;
        private int TouchWatch;
        //private TTotemConfig TotCfg;

        public CVotoTouchScreen() //ref TTotemConfig ATotCfg)
		{
            // inizializzo
            FFormRect = new Rectangle();       
    
            //Tz = new ArrayList();
            //TotCfg = ATotCfg;

            PaintTouchOnScreen = false;

            // ora mi creo il bottone in bmp
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.bottonetrasp_ok.png");
            btnBmpCandSing = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.btn_alternativo.png");
            btnBmpCand = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);
            //btnBmpCand = new Bitmap(myStream);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.btn_alternativo_cds.png");
            btnBmpCandCda = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);
            //btnBmpCandCda = new Bitmap(myStream);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.btn_alternativo_selez.png");
            btnBmpCandSelez = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);
            //btnBmpCandSelez = new Bitmap(myStream);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.btn_alternativo_selezCDS.png");
            btnBmpCandSelezCda = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);
            //btnBmpCandSelezCda = new Bitmap(myStream);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.tab.png");
            btnBmpTab = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);
            //btnBmpTab = new Bitmap(myStream);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.tab_rev.png");
            btnBmpTabSelez = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);
            //btnBmpTabSelez = new Bitmap(myStream);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.bottone_selezCDS.png");
            btnBmpCDASelez = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);
            //btnBmpCDASelez = new Bitmap(myStream);

            myStream = myAssembly.GetManifestResourceStream("VotoTouch.Resources.check.png");
            btnBmpCheck = myStream != null ? new Bitmap(myStream) : new Bitmap(1, 1);

            //ClasseTipoVotoStartNorm = new CTipoVoto_AStart(FFormRect);
            //ClasseTipoVotoStartDiff = new CTipoVoto_AStart(FFormRect);
            //ClasseTipoVotoConferma = new CTipoVoto_AConferma(FFormRect);

            // gestione delle pagine
            //CurrPag = 1;

            // la variabile che indica quanti candidati si possono toccare
            //MaxMultiCandSelezionabili = 0;

            // ritardo del touch
            TouchEnabled = true;
            TouchWatch = 0;
            // ora i timer
            timTouch = new Timer {Enabled = false, Interval = TIMER_TOUCH_INTERVAL};
            timTouch.Tick += timTouch_Tick;

            timTouchWatchDog = new Timer {Enabled = false, Interval = TIMER_TOUCHWATCH_INTERVAL};
            timTouchWatchDog.Tick += timTouchWatchDog_Tick;
		}

        // --------------------------------------------------------------
        //  CALCOLO DEL RESIZE
        // --------------------------------------------------------------

        /*
        public void CalcolaVotoTouch(Rectangle AFormRect)
        {
            // viene richiamata ad ogni resize della finestra
            FFormRect = AFormRect;

            if (ClasseTipoVotoStartNorm != null)
            {
                ClasseTipoVotoStartNorm.FFormRect = AFormRect;
                ClasseTipoVotoStartNorm.GetTouchSpecialZone(TAppStato.ssvVotoStart, false, false);
            }

            if (ClasseTipoVotoStartDiff != null)
            {
                ClasseTipoVotoStartDiff.FFormRect = AFormRect;
                ClasseTipoVotoStartDiff.GetTouchSpecialZone(TAppStato.ssvVotoStart, true, false);
            }

            if (ClasseTipoVotoConferma != null)
            {
                ClasseTipoVotoConferma.FFormRect = AFormRect;
                ClasseTipoVotoConferma.GetTouchSpecialZone(TAppStato.ssvVotoConferma, false, VTConfig.AbilitaBottoneUscita);
                //ClasseTipoVotoConferma.GetTouchSpecialZone(Stato, Differ);
            }
        }

        public int CalcolaTouchSpecial(TNewVotazione FVotaz, TAppStato Stato, bool AIsVotazioneDifferenziata)
        {
            Tz = null;
            // switcho in funzione dello stato
            switch (Stato)
            {
                case TAppStato.ssvVotoStart:
                    // chiamo la classe apposita
                    if (AIsVotazioneDifferenziata)
                        Tz = FVotaz.ClasseTipoVotoStartDiff.TouchZone;
                    else
                        Tz = FVotaz.ClasseTipoVotoStartNorm.TouchZone;
                    break;

                // conferma del voto
                case TAppStato.ssvVotoConferma:
                    // chiamo la classe apposita
                    Tz = FVotaz.ClasseTipoVotoConferma.TouchZone;
                    break;

                // salvataggio/fine del voto
                case TAppStato.ssvVotoFinito:
                case TAppStato.ssvSalvaVoto:
                    // non fare nulla
                    break;

                default:
                    // non fare nulla
                    break;
            }

            return 0;
        }

*/
        public int CalcolaTouchSpecial(CBaseTipoVoto ASpecial)
        {
            Tz = null;
            if (ASpecial != null && ASpecial.TouchZone != null)
                Tz = ASpecial.TouchZone;
            return 0;
        }

        public int CalcolaTouchVote(TNewVotazione FVotaz)
        {
            Tz = null;
            if (FVotaz != null && FVotaz.TouchZoneVoto != null && FVotaz.TouchZoneVoto.TouchZone != null)
            {
                //foreach (TTZone item in FVotaz.TouchZoneVoto.TouchZone)
                //{
                //    item.Multi = 0;
                //}
                Tz = FVotaz.TouchZoneVoto.TouchZone;
                MaxMultiCandSelezionabili = FVotaz.DammiMaxMultiCandSelezionabili();
                MinMultiCandSelezionabili = FVotaz.DammiMinMultiCandSelezionabili();
            }
            return 0;
        }

        /*
        public int old_CalcolaTouch(object sender, TAppStato Stato, TNewVotazione FVotaz, bool Differ)
        {
            // DR12 OK
            // in funzione del tipo di stato della macchina a stati, del tipo di votazione
            // creo le aree sensibili dello schermo
            
            //TTZone a;  // da togliere, non servirà
            CBaseTipoVoto ClasseTipoVoto = null;

            // cancello comunque la collection
            //Tz.Clear();
            // setto la variabile multi
            NumMulti = 0;

            // switcho in funzione dello stato
            switch (Stato)
            {
                case TAppStato.ssvBadge:
                    // non fare nulla
                    break;

                case TAppStato.ssvVotoStart:
                    // chiamo la classe apposita
                    ClasseTipoVoto = new CTipoVoto_AStart(FFormRect);
                    ClasseTipoVoto.GetTouchVoteZone(Stato, ref FVotaz, Differ, ref Tz);
                    break;

                case TAppStato.ssvVoto:
                    // In funzione del tipo di votazione metto i componenti
                    // VOTAZIONE DI LISTA
                    if (FVotaz.TipoVoto == VSDecl.VOTO_LISTA)
                    {
                        // chiamo la classe del voto apposito
                        ClasseTipoVoto = new CTipoVoto_Lista(FFormRect);
                        ClasseTipoVoto.GetTouchVoteZone(Stato, ref FVotaz, Differ, ref Tz);
                    }   // VOTO_LISTA

                    // VOTAZIONE DI CANDIDATO SINGOLO  ** SINGOLA PAGINA ** 
                    if (FVotaz.TipoVoto == VSDecl.VOTO_CANDIDATO)
                    {
                        // chiamo la classe del voto apposito, se sono meno di 6 candidati
                        // uso una classe più definita graficamente
                        if (FVotaz.NListe <= 6)
                            ClasseTipoVoto = new CTipoVoto_CandidatoSmall(FFormRect);
                        else
                            ClasseTipoVoto = new CTipoVoto_CandidatoOriginal(FFormRect);
                        ClasseTipoVoto.GetTouchVoteZone(Stato, ref FVotaz, Differ, ref Tz);
                    }

                    #region VOTAZIONE DI CANDIDATO SINGOLO ** MULTI PAGINA ** (era VECCHIO, OBSOLETO)
                    // VOTAZIONE DI CANDIDATO SINGOLO ** MULTI PAGINA ** (era VECCHIO, OBSOLETO)
                    if (FVotaz.TipoVoto == VSDecl.VOTO_CANDIDATO_SING)
                    {
                        // chiamo la classe del voto apposito
                        ClasseTipoVoto = new CTipoVoto_CandidatoOriginal(FFormRect);
                        ClasseTipoVoto.GetTouchVoteZone(Stato, ref FVotaz, Differ, ref Tz);
                    }
                    #endregion      

                    // VOTAZIONE DI MULTICANDIDATO (1 O MULTI PAGINA)
                    if (FVotaz.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                    {
                        // setto la variabile multi
                        NumMulti = FVotaz.MaxScelte;

                        // chiamo la classe del voto apposito
                        if (FVotaz.TipoSubVoto == 1)
                            ClasseTipoVoto = new CTipoVoto_MultiCandidatoOriginal(FFormRect);
                        else
                            ClasseTipoVoto = new CTipoVoto_MultiCandidatoNew(FFormRect);
                        ClasseTipoVoto.GetTouchVoteZone(Stato, ref FVotaz, Differ, ref Tz);
                    }

                    break;

                // conferma del voto
                case TAppStato.ssvVotoConferma:
                    // chiamo la classe apposita
                    ClasseTipoVoto = new CTipoVoto_AConferma(FFormRect);
                    ClasseTipoVoto.GetTouchVoteZone(Stato, ref FVotaz, Differ, ref Tz);
                    break;
                
                // salvataggio/fine del voto
                case TAppStato.ssvVotoFinito:
                case TAppStato.ssvSalvaVoto:
                     // non fare nulla
                     break;

                default:
                    // non fare nulla
                    break;
            }

            return 0;
        }
        */
        // --------------------------------------------------------------
        //  Touch
        // --------------------------------------------------------------

        #region Touch Eventi

        // metodo chiamato al tocco dello schermo
        public int TastoPremuto(object sender, MouseEventArgs e, TAppStato Stato)
        {
            // DR12 OK
            // prima di tutto testo se TouchEnabled è false, se lo è, vuol dire che non è ancora
            // passato l'intervallo di sicurezza per evitare doppi click
            timTouchWatchDog.Enabled = false;
            timTouchWatchDog.Enabled = true;
            if (!TouchEnabled)
            {
                // lancia evento watchdog
                if (TouchWatchDog != null) { TouchWatchDog(this, 0); }
                // poi ritorna
                return 0;
            }
            // ****************************************************************************
            // Alla fine di tutto per evitarmi fastidiosi farfallii, ho deciso di fare delle
            // cosiddette zone sensibili a seconda dello stato, queste sono aree della form
            // che in quella particolare videata "corrispondono" ad un tasto
            // 
            // Questa è la classe che li tratta
            // ****************************************************************************
            TTZone a;
            int Trovato = -1;

            if (Tz == null || Tz.Count == 0) return 0;

            // TODO: CVotoToucscreen|TastoPremuto - da rivedere il ciclo è arzigogolato
            // dunque, ciclo lungo la collection attiva per vedere se le coordinate corrispondono
            for (int i = 0; i < Tz.Count; i++)
            {
                a = (TTZone)Tz[i];

                if ((e.X >= a.x) && (e.X <= a.r) && (e.Y >= a.y) && (e.Y <= a.b))
                {
                    // serve per le multivotazioni
                    if (a.pag == CurrPag || a.pag == 0)
                    {
                        Trovato = i;
                        break;
                    }
                }

            }
            // ok, lancio l'evento
            if (Trovato >= 0)
            {
                a = (TTZone)Tz[Trovato];

                switch (a.ev)
                {
                    case TTEvento.steVotaNormale:
                        // manda l'evento
                        if (PremutoVotaNormale != null) { PremutoVotaNormale(this, 0); }
                        break;

                    case TTEvento.steVotaDiffer:
                        // manda l'evento
                        if (PremutoVotaDifferenziato != null) { PremutoVotaDifferenziato(this, 0); }
                        break;

                    case TTEvento.steConferma:
                        // Manda l'evento
                        if (PremutoConferma != null) { PremutoConferma(this, 0); }
                        break;

                    case TTEvento.steAnnulla:
                        // Manda l'evento di annulla
                        if (PremutoAnnulla != null) { PremutoAnnulla(this, 0); }
                        break;

                    case TTEvento.steVotoValido:
                        // manda l'evento di voto valido
                        if (PremutoVotoValido != null) { PremutoVotoValido(this, a.expr, false ); }
                        break;

                    case TTEvento.steMultiValido:
                        // manda l'evento in locale per settare il flag del multicandidato premuto sulla collection
                        ElementoMulti(Trovato);
                        // manda l'evento di Paint alla finestra principale
                        if (PremutoMulti != null) { PremutoMulti(this, a.expr); }
                        break;

                    case TTEvento.steMultiSelezTuttiCDA:
                        // in questo evento si selezionano tutti i cda, deselezionando il resto
                        SelezionaTuttiCDA();
                        // manda l'evento di Paint alla finestra principale
                        if (PremutoMulti != null) { PremutoMulti(this, a.expr); }
                        break;


                    case TTEvento.steMultiAvanti:
                        // ricostruisco chi è stato votato, per trasmetterlo alla routine sopra
                        List<int> votis = new List<int>();
                        int nvt = 0;
                        foreach (TTZone b in Tz)
                        {
                            if (b.ev == TTEvento.steMultiValido && b.Multi > 0)
                            {
                                votis.Add(b.expr);
                                nvt++;
                            }
                        }
                        // verifico che stia nel range min/max espressi
                        if (nvt < MinMultiCandSelezionabili)
                        {
                            if (ShowPopup != null) { ShowPopup(this, "Devi selezionare un minimo di " + 
                                MinMultiCandSelezionabili.ToString() + " candidati per continuare."); }
                            SystemSounds.Beep.Play();
                        }
                        else
                        {
                            // attenzione, se non ho voti, cioè nvt = 0 devo considerare sk bianca
                            if (nvt > 0)
                            {
                                // manda l'evento di Avanti nel caso di Multivotazioni
                                if (PremutoMultiAvanti != null) { PremutoMultiAvanti(this, a.expr, ref votis); }
                            }
                            else
                            {
                                // manda l'evento di SkBianca
                                if (PremutoSchedaBianca != null) { PremutoSchedaBianca(this, VSDecl.VOTO_SCHEDABIANCA); }
                            }
                            votis.Clear();
                            votis = null;
                        }
                        break;

                    //case TTEvento.steSelezTuttiCDAAvanti:
                    //    // in questo evento votano autpmaticamente tutti i cda
                    //    List<int> votiCDA = new List<int>();
                    //    TTZone bz;
                    //    int nvtz = 0;
                    //    for (int i = 0; i < Tz.Count; i++)
                    //    {
                    //        bz = (TTZone)Tz[i];
                    //        if (bz.cda)
                    //        {
                    //            votiCDA.Add(bz.expr);
                    //            nvtz++;
                    //        }
                    //    }
                    //    // attenzione, se non ho voti, cioè nvt = 0 devo considerare sk bianca
                    //    if (nvtz > 0)
                    //    {
                    //        // manda l'evento di Avanti nel caso di Multivotazioni
                    //        if (PremutoMultiAvanti != null) { PremutoMultiAvanti(this, a.expr, ref votiCDA); }
                    //    }
                    //    else
                    //    {
                    //        // manda l'evento di SkBianca
                    //        if (PremutoSchedaBianca != null) { PremutoSchedaBianca(this, VSDecl.VOTO_SCHEDABIANCA); }
                    //    }
                    //    votiCDA.Clear();
                    //    votiCDA = null;
                    //    break;

                    case TTEvento.steSkBianca:
                        // manda l'evento di scheda bianca
                        if (PremutoSchedaBianca != null) { PremutoSchedaBianca(this, a.expr); }
                        break;

                    case TTEvento.steSkNonVoto:
                        // manda l'evento di non voto
                        if (PremutoNonVoto != null) { PremutoNonVoto(this, a.expr); }
                        break;

                    case TTEvento.steSkContrarioTutti:
                        // manda l'evento di contrario a tutti
                        if (PremutoContrarioTutti != null) { PremutoContrarioTutti(this, a.expr); }
                        break;

                    case TTEvento.steSkAstenutoTutti:
                        // manda l'evento di astenuto a tutti
                        if (PremutoAstenutoTutti != null) { PremutoAstenutoTutti(this, a.expr); }
                        break;

                    case TTEvento.steBottoneUscita:
                        // manda l'evento di bottone uscita
                        if (PremutoBottoneUscita != null) { PremutoBottoneUscita(this, a.expr); }
                        break;

                    case TTEvento.steTabs:
                        // manda l'evento di tab premuto per cambiare pagina
                        CurrPag = a.expr; 
                        
                        if (PremutoTab != null) { PremutoTab(this, a.expr); }
                        break;

                    default:
                        if (PremutoInvalido != null) { PremutoInvalido(this, 0); }
                        break;

                }
                // qua parte il ritardo del timer
                TouchEnabled = false;
                timTouch.Enabled = true;
            }
            else
            {
                if (PremutoInvalido != null) { PremutoInvalido(this, 0); }
            }


            return 0;
        }

        private void ElementoMulti(int Trovato)
        {
            // questa routine serve per settare/resettare l'elemento selezionato nelle multivotazioni.
            //TTZone b;
            // se TTZone.Multi <> 0, lo mette a 0
            // se TTZone.Multi == 0 conta le Multi di ogni botttone attivo e verifica se sono già arrivate al massimo
            // nel cui caso non fa nulla, altrimenti mette le  TTZone.Multi = 1 o nella versione successiva, la subvotazione
            // vedi caso BPM con più subvoti nella stessa pagina
            // controllo
            if (Tz == null) return;
            if (Trovato >= Tz.Count) return;

            TTZone a = (TTZone)Tz[Trovato];
            // se è maggiore di 0 faccio che metterlo a 0
            if (a.Multi > 0)
            {
                a.Multi = 0;
            }
            else
            {
                // devo contare quante sono selezionate
                int fount = 0;
                //for (int i = 0; i < Tz.Count; i++)
                //{
                //    b = (TTZone)Tz[i];
                //    if (b.ev == TTEvento.steMultiValido)
                //        if (b.Multi > 0) fount++;
                //}
                foreach (TTZone b in Tz)
                {
                    if (b.ev == TTEvento.steMultiValido)
                        if (b.Multi > 0) fount++;
                }
                // ok, ora se è minore di NumMulti metto il campo a 1
                if (fount < MaxMultiCandSelezionabili)
                    a.Multi = 1;
                else
                {
                    if (ShowPopup != null) { ShowPopup(this, "Hai espresso il numero massimo di scelte, per modificare devi deselezionarne una"); }
                    SystemSounds.Beep.Play();
                }
            }
            // ok lo rimetto a posto
            Tz[Trovato] = a;
        }

        private void SelezionaTuttiCDA()
        {
            // questa routine serve per settare/resettare l'elemento selezionato nelle multivotazioni.
            TTZone b;

            for (int i = 0; i < Tz.Count; i++)
            {
                b = (TTZone)Tz[i];
                if (b.ev == TTEvento.steMultiValido)
                {
                    if (b.Multi > 0 && !b.cda)
                        b.Multi = 0;
                    if (b.cda) b.Multi = 1;
                    Tz[i] = b;
                }
            }
        }

        // --------------------------------------------------------------
        //  timer del touch
        // --------------------------------------------------------------

        private void timTouch_Tick(object sender, EventArgs e)
        {
            TouchEnabled = true;
            timTouch.Enabled = false;
        }

        private void timTouchWatchDog_Tick(object sender, EventArgs e)
        {
            if (!TouchEnabled)
            {
                TouchWatch++;
            }
            if (TouchWatch >= 2)
            {
                TouchEnabled = true;
                TouchWatch = 0;
                // lancia evento watchdog
                if (TouchWatchDog != null) { TouchWatchDog(this, 1); }
            }
        }
        
        #endregion

        // --------------------------------------------------------------
        //  PAINTING DELLE ZONE TOUCH
        // --------------------------------------------------------------

        #region  PAINTING DELLE ZONE TOUCH

        public void PaintTouch(object sender, PaintEventArgs e)
        {
            // ok questo metodo viene chiamato da paint della finestra principale 
            // per evidenziare le zone sensibili del tocco
            if (Tz == null) return;

            if (PaintTouchOnScreen)
            {
                //TTZone a;
                Graphics g = e.Graphics;
                Pen p = new Pen(Color.DarkGray) {DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot};

                //for (int i = 0; i < Tz.Count; i++)
                foreach (TTZone a in Tz)
                {
                    //a = (TTZone)t;
                    if (a.pag == 0 || a.pag == CurrPag)
                        g.DrawRectangle(p, a.x, a.y, (a.r - a.x), (a.b - a.y));
                }

            }
        }

        public void PaintButtonCandidatoPagina(object sender, PaintEventArgs e, bool Multi, int ABaseFontCandidato,
                                                                bool ABaseFontCandidatoBold, Color BaseColorCandidato)
        {
            // ok questo metodo viene chiamato da paint della finestra principale 
            // per disegnare i bottoni dei candidati
            //TTZone a;
            Graphics g = e.Graphics;
            string ss;

            Pen blackPen = new Pen(Color.Black, 1);
            Pen graypen = new Pen(Color.Gray, 2);

            Font myFont22 = new System.Drawing.Font("Arial", ABaseFontCandidato, ABaseFontCandidatoBold ? FontStyle.Bold : FontStyle.Regular);
            Font myFont23 = new System.Drawing.Font("Arial", ABaseFontCandidato + 2, ABaseFontCandidatoBold ? FontStyle.Bold : FontStyle.Regular);  
            //Font myFont24 = new System.Drawing.Font("Arial", 24, FontStyle.Regular);

            Font myFont = null; // = new System.Drawing.Font("Arial", 24, FontStyle.Regular);
            Font myFont2 = new System.Drawing.Font("Arial", 20, FontStyle.Regular);
            Font myFont3 = new System.Drawing.Font("Arial", 10, FontStyle.Italic | FontStyle.Bold);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            //stringFormat.Trimming = StringTrimming.EllipsisCharacter;
            stringFormat.Trimming = StringTrimming.None;
            stringFormat.FormatFlags = StringFormatFlags.FitBlackBox;

            SolidBrush whiteBrush = new SolidBrush(Color.White);

            //for (int i = 0; i < Tz.Count; i++)
            foreach (TTZone a in Tz)
            {
                //a = (TTZone)Tz[i];
                // se è maggiore di 0 vuol dire che è un oggetto da stampare
                if (a.expr >= 0 && !a.MultiNoPrint )
                {
                    Rectangle r = new Rectangle(a.x, a.y, (a.r - a.x), (a.b - a.y));
                    r = Rectangle.Inflate(r, 5, 5);
                    
                    // in funzione dell'evento faccio ( ci possono essere tasti speciali )
                    switch (a.ev)
                    {
                        // controllo se sono tabs
                        case TTEvento.steTabs:
                            // se è selezionata allora la metto al contrario
                            if (a.expr == CurrPag)
                            {
                                e.Graphics.DrawImage(btnBmpTabSelez, r);
                                e.Graphics.DrawString(a.Text, myFont2, Brushes.White,
                                        new RectangleF(a.x, a.y, (a.r - a.x) - 1, (a.b - a.y) - 1), stringFormat);
                            }
                            else
                            {
                                e.Graphics.DrawImage(btnBmpTab, r);
                                e.Graphics.DrawString(a.Text, myFont2, Brushes.DarkSlateGray,
                                        new RectangleF(a.x, a.y, (a.r - a.x) - 1, (a.b - a.y) - 1), stringFormat);
                            }
                            break;

                        // Tasto Seleziona tutti
                        case TTEvento.steMultiSelezTuttiCDA:
                            e.Graphics.DrawImage(btnBmpCDASelez, r);
                            break;

                        // tutto il resto
                        default:
                            // stampa solo quelli in pagina 0 o nella pagina corrente
                            if (a.pag == 0 || a.pag == CurrPag)
                            {
                                // disegnomil bottone facendo attenzione al multicandidato
                                if (!Multi) // no multicandidato
                                {
                                    e.Graphics.DrawImage(a.cda ? btnBmpCandCda : btnBmpCand, r);
                                }
                                else
                                {
                                    // multicandidato
                                    switch (a.PaintMode)
                                    {
                                        case VSDecl.PM_NORMAL:
                                            if (a.Multi == 0)
                                                e.Graphics.DrawImage(a.cda ? btnBmpCandCda : btnBmpCand, r);
                                            else
                                                e.Graphics.DrawImage(a.cda ? btnBmpCandSelezCda : btnBmpCandSelez, r);
                                            break;

                                        case VSDecl.PM_NONE:
                                            // Paint None
                                            break;

                                        case VSDecl.PM_ONLYCHECK:
                                            if (a.Multi > 0)
                                                e.Graphics.DrawImage(btnBmpCheck, a.CKRect);
                                            break;

                                        default:
                                            if (a.Multi == 0)
                                                e.Graphics.DrawImage(a.cda ? btnBmpCandCda : btnBmpCand, r);
                                            else
                                                e.Graphics.DrawImage(a.cda ? btnBmpCandSelezCda : btnBmpCandSelez, r);
                                            break;
                                    }                                 
                                }

                                if (a.PaintMode == VSDecl.PM_NORMAL)
                                {
                                    // ok, prima di disegnare il nome devo controllare la mpresenza della data
                                    // di nascita < >
                                    if (a.Text.Contains("(") && a.Text.Contains(")"))
                                    {
                                        // devo dividere la stringa, per convenzione la divido a partire da
                                        // la posizione le carattere "<"
                                        stringFormat.Alignment = StringAlignment.Far;
                                        ss = a.Text.Substring(a.Text.IndexOf('('),
                                                              a.Text.Length - a.Text.IndexOf('('));
                                        a.Text = a.Text.Substring(0, a.Text.IndexOf('(') - 1);
                                        e.Graphics.DrawString(ss, myFont3, new SolidBrush(BaseColorCandidato),
                                                              //Brushes.DarkSlateGray,
                                                              new RectangleF(a.x, a.b - 20, (a.r - a.x) - 20, 20),
                                                              stringFormat);
                                        stringFormat.Alignment = StringAlignment.Center;
                                    }
                                    // disegno il nome
                                    int ls = a.Text.Length;
                                    myFont = ls <= 16 ? myFont23 : myFont22;
                                    //if (ls <= 16)
                                    //    myFont = myFont23;
                                    //else
                                    //    myFont = myFont22;
                                    e.Graphics.DrawString(a.Text, myFont, new SolidBrush(BaseColorCandidato),
                                                          //Brushes.DarkSlateGray,
                                                          new RectangleF(a.x, a.y, (a.r - a.x) - 1, (a.b - a.y) - 4),
                                                          stringFormat);
                                }
                            }  // if (a.pag == CurrPag || a.pag == 0)
                            break;
                    }
                
                }  //  if (a.expr >= 0)
            }  // for (int i = 0; i < Tz.Count; i++)
        }

        // obsoleto
        public void PaintButtonCandidatoSingola(object sender, PaintEventArgs e)
        {
            // OBSOLETO
            // ok questo metodo viene chiamato da paint della finestra principale 
            // per disegnare i bottoni dei candidati
            TTZone a;
            Graphics g = e.Graphics;
            int ls;

            Pen blackPen = new Pen(Color.Black, 1);
            Pen graypen = new Pen(Color.Gray, 2);

            Font myFont24 = new System.Drawing.Font("Arial", 22, FontStyle.Regular);
            Font myFont26 = new System.Drawing.Font("Arial", 24, FontStyle.Regular);
            Font myFont28 = new System.Drawing.Font("Arial", 26, FontStyle.Regular);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            SolidBrush whiteBrush = new SolidBrush(Color.White);
 
            for (int i = 0; i < Tz.Count; i++)
            {
                a = (TTZone)Tz[i];
                if (a.expr >= 0)
                {
                    Rectangle r = new Rectangle(a.x, a.y, (a.r - a.x), (a.b - a.y));
                    r = Rectangle.Inflate(r, 5, 5);

                    //// non serve più etruria
                    //if (a.expr == VSDecl.VOTO_ETRURIA)
                    //{
                    //    //e.Graphics.DrawImage(btnBmpCandArancio, r);
                    //    //e.Graphics.DrawString("Seleziona Proposte C.d.A.", myFont28, Brushes.DarkSlateGray,
                    //                          new RectangleF(a.x, a.y, (a.r - a.x) - 1, (a.b - a.y) - 1), stringFormat);
                    //}
                    //else
                    //{
                        e.Graphics.DrawImage(btnBmpCand, r);                                                
                        //e.Graphics.DrawImage(btnBmpCandSing, r);

                        // now check the length of the string
                        ls = a.Text.Length;

                        if (ls <= 16)
                            e.Graphics.DrawString(a.Text, myFont28, Brushes.DarkSlateGray,
                                              new RectangleF(a.x, a.y, (a.r - a.x) - 1, (a.b - a.y) - 1), stringFormat);
                        else
                        {
                            if (ls <= 20)
                                e.Graphics.DrawString(a.Text, myFont26, Brushes.DarkSlateGray,
                                                  new RectangleF(a.x, a.y, (a.r - a.x) - 1, (a.b - a.y) - 1), stringFormat);
                            else
                                e.Graphics.DrawString(a.Text, myFont24, Brushes.DarkSlateGray,
                                                  new RectangleF(a.x, a.y, (a.r - a.x) - 1, (a.b - a.y) - 1), stringFormat);
                        }
                    //}
                }

            }  // for (int i = 0; i < Tz.Count; i++)
        }

        #endregion  
        


	}
}
