using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows.Forms;

namespace VotoTouch
{
    // DR16 - Classe intera

    // struttura per le votazioni
    public class TNewVotazione
    {
        public int IDVoto { get; set; }
        public int IDGruppoVoto { get; set; }
        public string Descrizione { get; set; }
        public TTipoVoto TipoVoto { get; set; }                //1.norm, 2.Lista, 3.Multi
        public int TipoSubVoto { get; set; }             // a seconda del tipo principale 
        public bool SkBianca { get; set; }               // ha scheda bianca
        public bool SkNonVoto { get; set; }              // ha il non voto
        public bool SkContrarioTutte { get; set; }       // ha il contraio a tutte       
        public bool SkAstenutoTutte { get; set; }       // ha il astenuto a tutte       
        public int MaxScelte { get; set; }               // n scelte max nel caso di multi
        public int MinScelte { get; set; }               // n scelte min nel caso di multi
        public bool NeedConferma { get; set; }           // indica che dopo questa votazione necessita la conferma
        public bool AbilitaBottoneUscita { get; set; }

        public bool SelezionaTuttiCDA;

        public int NListe { get { return (Liste == null) ? 0 : Liste.Count; } }
        public int NPresentatoCDA { get { return (Liste == null) ? 0 : Liste.Count(a => a.PresentatodaCDA == true); } }               

        public int NMultiSelezioni
        {
            get
            {
                return TouchZoneVoto.TouchZone.Cast<TTZone>().Count(item => item.Multi > 0);
            }
        }

        public CBaseTipoVoto TouchZoneVoto;
        public TAreaVotazione AreaVoto;

        public List<TNewLista> Liste;     // collection di strutture Tliste
        public ArrayList Pagine;    // collection delle pagine (per le votazioni candidato)

        public TNewVotazione()
        {
            Liste = new List<TNewLista>();
            Pagine = new ArrayList();
            TouchZoneVoto = null;
            AbilitaBottoneUscita = false;
        }

        public int DammiMaxMultiCandSelezionabili()
        {
            return TipoVoto == TTipoVoto.stvMultiCandidato ? MaxScelte : 0;  //VSDecl.VOTO_MULTICANDIDATO
        }

        public int DammiMinMultiCandSelezionabili()
        {
            return TipoVoto == TTipoVoto.stvMultiCandidato ? MinScelte : 0;  // VSDecl.VOTO_MULTICANDIDATO
        }

        ~TNewVotazione()
        {
            // Distruttore
            Liste.Clear();
            Pagine.Clear();
        }
    }

    public class TNewLista
    {
        public int NumVotaz;
        public int NumSubVotaz;
        public int IDLista;
        public int IDScheda;
        public string DescrLista;
        public int TipoCarica;
        public bool PresentatodaCDA;
        public string Presentatore;
        public string Capolista;
        public string ListaElenco;
        public int Pag;
        public string PagInd;

        public TNewLista()
        {
        }
    }

    public class TListaVotazioni
    {
        // oggetto lista votazioni
        protected List<TNewVotazione> _Votazioni;
        public List<TNewVotazione> Votazioni
        {
            get { return _Votazioni; }
            set
            {
                _Votazioni = value;
            }
        }

        private int idxVotoCorrente;
        public TNewVotazione VotoCorrente
        {
            get { return _Votazioni.Count == 0 ? null : _Votazioni[idxVotoCorrente]; }
            set
            {
                if (_Votazioni.Count > 0)
                    _Votazioni[idxVotoCorrente] = value;
            }
        }

        private CVotoBaseDati DBDati;
        private bool DemoMode = false;

        // oggetti conferma e inizio voto
        public CBaseTipoVoto ClasseTipoVotoStartNorm = null;
        public CBaseTipoVoto ClasseTipoVotoStartDiff = null;
        public CBaseTipoVoto ClasseTipoVotoConferma = null;


        public TListaVotazioni(CVotoBaseDati ADBDati)
        {
            // costruttore
            DBDati = ADBDati;
            _Votazioni = new List<TNewVotazione>();

            idxVotoCorrente = 0;
        }

        ~TListaVotazioni()
        {
            // Distruttore
        }

        // --------------------------------------------------------------------------
        //  Ritorno dati / Settaggio voto corrente
        // --------------------------------------------------------------------------

        public int NVotazioni()
        {
            return _Votazioni.Count;
        }

        public bool SetVotoCorrente(int AIDVoto)
        {
            if (_Votazioni.Count > 0 && AIDVoto >= 0)
            {
                //TNewVotazione voto = _Votazioni.First(v => v.IDVoto == AIDVoto);
                TNewVotazione vot = _Votazioni.First(v => v.IDVoto == AIDVoto);
                if (vot != null)
                {
                    idxVotoCorrente = _Votazioni.IndexOf(vot);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
            //return _Votazioni.Count != 0;
        }

        // --------------------------------------------------------------------------
        //  Ritorno dati / Settaggio voto corrente
        // --------------------------------------------------------------------------

        //public string DammiListaElencoPerIDVotazione(int AIDVoto, int AIDLista)
        //{
        //    var rit = _Votazioni.FirstOrDefault((a => a.IDVoto == AIDVoto);
        //    return rit != null ? rit.
        //}

        // --------------------------------------------------------------------------
        //  Caricamento dati
        // --------------------------------------------------------------------------

        public bool CaricaListeVotazioni(string AData_path, Rectangle AFormRect, bool AInLoading)
        {
            // questa routine serve a caricara/ricaricare le votazioni / liste
            // dal database ai file
            // è disegnata per essere richiamata in qualsiasi momento durante
            // l'esecuzione senza creare problemi
            // In realtà viene richhiamata in funzione del votoaperto
            // - durante il loading della finestra se il voto è già aperto
            // - all'apertura della votazione
            Logging.WriteToLog("Caricamento Liste/Votazioni");
            bool result = false;

            _Votazioni.Clear();

            //if (DemoMode)
            //{
            //    CaricaDatiDemo(AData_path);
            //    result = true;
            //}
            //else
            //{
                // carica le votazioni dal database
                if (DBDati.CaricaVotazioniDaDatabase(ref _Votazioni))
                {
                    // carica i dettagli delle votazioni
                    if (DBDati.CaricaListeDaDatabase(ref _Votazioni))
                    {
                        result = true;
                    }
                }
            //}
            // Calcolo l'area di voto per Candidati e multicandidati
            CalcolaAreaDiVotoCandidatiMultiCandidato();
            // ok, ora ordino le liste nel caso in cui siano di candidato
            OrdinaListeInPagineCandidatiMultiCandidato();
            // calcolo le zone touch
            if (!AInLoading)
            {
                // votazioni
                CalcolaTouchZoneVotazioni(AFormRect);
                // speciali
                CalcolaTouchZoneSpeciali(AFormRect);
            }

            // NOTA: Nelle liste il nome può contenere anche la data di nascita, inserita
            // come token tra ( e ). Serve nel caso di omonimia. La routine di disegno riconoscerà
            // questo e lo tratterà come scritta piccola a lato

            return result;
        }

        public void ResizeZoneVotazioni(Rectangle AFormRect)
        {
            // votazioni
            CalcolaTouchZoneVotazioni(AFormRect);
            // speciali
            CalcolaTouchZoneSpeciali(AFormRect);
        }

        // --------------------------------------------------------------------------
        //  Calcolo delle TouchZone
        // --------------------------------------------------------------------------

        public void CalcolaTouchZoneVotazioni(Rectangle AFormRect)
        {
            foreach (TNewVotazione voto in _Votazioni)
            {
                // prima cancello eventuali oggetti se ci sono
                if (voto.TouchZoneVoto != null)
                {
                    voto.TouchZoneVoto.FFormRect = AFormRect;
                }
                else
                {
                    switch (voto.TipoVoto)
                    {
                        case TTipoVoto.stvLista:  //VSDecl.VOTO_LISTA:
                            voto.TouchZoneVoto = new CTipoVoto_Lista(AFormRect);
                            break;

                        case TTipoVoto.stvCandidato: // VSDecl.VOTO_CANDIDATO:
                            // chiamo la classe del voto apposito
                            switch (voto.TipoSubVoto)
                            {
                                case VSDecl.SUBVOTO_NORMAL:
                                    if (voto.NListe <= 6)
                                         voto.TouchZoneVoto = new CTipoVoto_CandidatoSmall(AFormRect);
                                    else
                                        voto.TouchZoneVoto = new CTipoVoto_CandidatoOriginal(AFormRect);                                    
                                    break;

                                default:
                                    voto.TouchZoneVoto = new CTipoVoto_CandidatoOriginal(AFormRect);
                                    break;
                            } 
                            break;

                        case TTipoVoto.stvMultiCandidato: //VSDecl.VOTO_MULTICANDIDATO:
                            // chiamo la classe del voto apposito
                            switch (voto.TipoSubVoto)
                            {
                                case VSDecl.SUBVOTO_NORMAL:
                                    voto.TouchZoneVoto = new CTipoVoto_MultiCandidatoOriginal(AFormRect);
                                    break;
                                case VSDecl.SUBVOTO_NEW:
                                    voto.TouchZoneVoto = new CTipoVoto_MultiCandidatoNew(AFormRect);
                                    break;

                                // subvoti speciali
                                case VSDecl.SUBVOTO_CUSTOM_MANUTENCOOP:
                                    voto.TouchZoneVoto = new CTipoVoto_Custom_Multi_Manutencoop(AFormRect);
                                    break;

                                default:
                                    voto.TouchZoneVoto = new CTipoVoto_MultiCandidatoOriginal(AFormRect);
                                    break;
                            }
                            break;

                        case TTipoVoto.stvGruppo:
                            voto.TouchZoneVoto = new CTipoVoto_Custom_Group_Bper2019(AFormRect);
                            break;

                        #region VOTAZIONE DI CANDIDATO SINGOLO ** MULTI PAGINA ** (era VECCHIO, OBSOLETO)

                        case TTipoVoto.stvCandidatoSing:   //VSDecl.VOTO_CANDIDATO_SING:
                            // chiamo la classe del voto apposito
                            voto.TouchZoneVoto = new CTipoVoto_CandidatoOriginal(AFormRect);
                            break;

                            #endregion

                        default:
                            voto.TouchZoneVoto = new CTipoVoto_Lista(AFormRect);
                            break;
                    }
                }
                // calcolo le zone
                voto.TouchZoneVoto.GetTouchVoteZone(voto);
            }
        }

        public void CalcolaTouchZoneSpeciali(Rectangle AFormRect)
        {
            // start normale
            if (ClasseTipoVotoStartNorm != null)
                ClasseTipoVotoStartNorm.FFormRect = AFormRect;
            else
                ClasseTipoVotoStartNorm = new CTipoVoto_AStart(AFormRect);
            ClasseTipoVotoStartNorm.GetTouchSpecialZone(TAppStato.ssvVotoStart, false, false);

            // start diff
            if (ClasseTipoVotoStartDiff != null)
                ClasseTipoVotoStartDiff.FFormRect = AFormRect;
            else
                ClasseTipoVotoStartDiff = new CTipoVoto_AStart(AFormRect);
            ClasseTipoVotoStartDiff.GetTouchSpecialZone(TAppStato.ssvVotoStart, true, false);

            // start conferma
            if (ClasseTipoVotoConferma != null)
                ClasseTipoVotoConferma.FFormRect = AFormRect;
            else
                ClasseTipoVotoConferma = new CTipoVoto_AConferma(AFormRect);
            ClasseTipoVotoConferma.GetTouchSpecialZone(TAppStato.ssvVotoConferma, false, VTConfig.AbilitaBottoneUscita);

        }

        // --------------------------------------------------------------------------
        //  Area di votazione
        // --------------------------------------------------------------------------

        private void CalcolaAreaDiVotoCandidatiMultiCandidato()
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
            int CandAlt;

            // area di voto standard x Candidati è:
            // x: 20 y:180 ax:980 (w:960) ay:810 (h:630)  

            foreach (TNewVotazione votazione in _Votazioni)
            {
                // solo se il voto è di candidato continuo
                //if (votazione.TipoVoto == VSDecl.VOTO_CANDIDATO ||
                //    votazione.TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                //    votazione.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                if (votazione.TipoVoto == TTipoVoto.stvCandidato ||
                    votazione.TipoVoto == TTipoVoto.stvCandidatoSing ||
                    votazione.TipoVoto == TTipoVoto.stvMultiCandidato)
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
                    CandAlt = votazione.NListe - votazione.NPresentatoCDA;

                    switch (votazione.NPresentatoCDA)
                    {
                        case 0:
                            // vedo se mi servono i tabs
                            votazione.AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_10);
                            // ok, ora setto l'area in pixel dei Alt
                            votazione.AreaVoto.XAlt = 3; //40px;
                            votazione.AreaVoto.YAlt = 25; //265px;
                            if (votazione.AreaVoto.NeedTabs)
                                votazione.AreaVoto.WAlt = 72; //930px;
                            else
                                votazione.AreaVoto.WAlt = 94; //1200px;
                            votazione.AreaVoto.HAlt = 52; //535px;
                            votazione.AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_10;
                            if (CandAlt < votazione.AreaVoto.CandidatiPerPagina)
                            {
                                votazione.AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x10 = new int[] { 0, 6, 6, 4, 4, 2, 2, 0, 0, 0, 0 };
                                votazione.AreaVoto.YAlt = votazione.AreaVoto.YAlt + x10[CandAlt];
                                votazione.AreaVoto.HAlt = votazione.AreaVoto.HAlt - (x10[CandAlt] * 2);
                            }
                            break;

                        case 1:
                        case 2:
                        case 3:
                            // vedo se mi servono i tabs
                            votazione.AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_8);
                            // ok, ora setto l'area in pixel
                            votazione.AreaVoto.XCda = 3; //40px;
                            votazione.AreaVoto.YCda = 25; //265px;
                            votazione.AreaVoto.WCda = 94; //1200px;
                            votazione.AreaVoto.HCda = 8; //80px;
                            // ok, ora setto l'area in pixel dei Alt
                            votazione.AreaVoto.XAlt = 3; //40px;
                            votazione.AreaVoto.YAlt = 42; //430px;
                            if (votazione.AreaVoto.NeedTabs)
                                votazione.AreaVoto.WAlt = 72; //930px;
                            else
                                votazione.AreaVoto.WAlt = 94; //1200px;
                            votazione.AreaVoto.HAlt = 36; //370px;
                            votazione.AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_8;
                            if (CandAlt < votazione.AreaVoto.CandidatiPerPagina)
                            {
                                votazione.AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x8 = new int[] { 0, 6, 6, 4, 4, 2, 2, 0, 0, 0, 0 };
                                votazione.AreaVoto.YAlt = votazione.AreaVoto.YAlt + x8[CandAlt];
                                votazione.AreaVoto.HAlt = votazione.AreaVoto.HAlt - (x8[CandAlt] * 2);
                            }
                            break;

                        case 4:
                        case 5:
                        case 6:
                            // vedo se mi servono i tabs
                            votazione.AreaVoto.NeedTabs = (CandAlt > VSDecl.CANDXPAG_6);
                            // ok, ora setto l'area in pixel dei CDA
                            votazione.AreaVoto.XCda = 3; //40px;
                            votazione.AreaVoto.YCda = 25; //265px;
                            votazione.AreaVoto.WCda = 94; //1200px;
                            votazione.AreaVoto.HCda = 17; //178px;
                            // ok, ora setto l'area in pixel dei Alt
                            votazione.AreaVoto.XAlt = 3; //40px;
                            votazione.AreaVoto.YAlt = 51; //520px;
                            if (votazione.AreaVoto.NeedTabs)
                                votazione.AreaVoto.WAlt = 72; //930px;
                            else
                                votazione.AreaVoto.WAlt = 94; //1200px;
                            votazione.AreaVoto.HAlt = 27; //280px;
                            votazione.AreaVoto.CandidatiPerPagina = VSDecl.CANDXPAG_6;
                            if (CandAlt < votazione.AreaVoto.CandidatiPerPagina)
                            {
                                votazione.AreaVoto.CandidatiPerPagina = CandAlt;
                                // correttivo per centrare i bottoni in caso di meno righe
                                int[] x6 = new int[] { 0, 4, 4, 2, 2, 0, 0, 0, 0, 0, 0 };
                                votazione.AreaVoto.YAlt = votazione.AreaVoto.YAlt + x6[CandAlt];
                                votazione.AreaVoto.HAlt = votazione.AreaVoto.HAlt - (x6[CandAlt] * 2);
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

        private void OrdinaListeInPagineCandidatiMultiCandidato()
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

            int z, pg, pgind;
            string sp;
            TNewLista li;
            TIndiceListe idx; //, idx1;

            // innanzitutto ciclo sulle votazioni
            foreach (TNewVotazione votazione in _Votazioni)
            {
                // solo se il voto è di candidato continuo
                //if (votazione.TipoVoto == VSDecl.VOTO_CANDIDATO ||
                //    votazione.TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                //    votazione.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                if (votazione.TipoVoto == TTipoVoto.stvCandidato ||
                    votazione.TipoVoto == TTipoVoto.stvCandidatoSing ||
                    votazione.TipoVoto == TTipoVoto.stvMultiCandidato)
                {
                    // comunque cancello la collection delle pagine
                    votazione.Pagine.Clear();
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
                    votazione.Pagine.Add(idx);
                    // ok, ora ciclo
                    for (z = 0; z < votazione.Liste.Count; z++)
                    {
                        // prelevo la lista che dovrebbe già essere ordinata in modo alfabetico
                        li = (TNewLista)votazione.Liste[z];
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
                            if (pgind > votazione.AreaVoto.CandidatiPerPagina ||
                                z == (votazione.Liste.Count - 1))
                            {
                                // cognome di fine e aggiungo pagina
                                idx = new TIndiceListe();
                                idx.pag = pg;
                                idx.sp = sp + "    ";  // metto gli spazi per il substring dopo
                                idx.ep = li.DescrLista + "    "; // come sopra, brutta ma efficace
                                votazione.Pagine.Add(idx);

                                // setto le variabili per la pagina successiva
                                sp = "";
                                pg++;
                                pgind = 1;
                            }
                        }
                        // aggiorno
                        votazione.Liste[z] = li;
                    } //for (z = 0; z < FParVoto[i].Liste.Count; z++)

                    // ok ora devo creare l'indice nella collection
                    for (z = 1; z < votazione.Pagine.Count; z++)
                    {
                        idx = (TIndiceListe)votazione.Pagine[z];

                        if (z == 1) idx.sp = "A  ";
                        if (z == (votazione.Pagine.Count - 1)) idx.ep = "Z  ";
                        idx.indice = idx.sp.Substring(0, 3).Trim() + "-" +
                                idx.ep.Substring(0, 3).Trim();
                        idx.indice = idx.indice.Trim();
                        votazione.Pagine[z] = idx;
                    }

                    // ok, ora metto le informazioni nelle liste
                    for (z = 0; z < votazione.Liste.Count; z++)
                    {
                        // prelevo la lista che dovrebbe già essere ordinata in modo alfabetico
                        li = (TNewLista)votazione.Liste[z];
                        // controllo per scrupolo l'indice
                        if (li.Pag < votazione.Liste.Count)
                        {
                            idx = (TIndiceListe)votazione.Pagine[li.Pag];
                            li.PagInd = idx.indice.ToLower();
                        }
                        votazione.Liste[z] = li;
                    }

                }  //if (FParVoto[i].TipoVoto == VSDecl.VOTO_CANDIDATO
            }  // for (i = 0; i < NVoti; i++)
        }

    }
}
