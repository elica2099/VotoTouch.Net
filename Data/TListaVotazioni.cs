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
    // struttura per le votazioni
    public class TNewVotazione
    {
        public int IDVoto { get; set; }
        public int IDGruppoVoto { get; set; }
        public string Descrizione { get; set; }
        public int TipoVoto { get; set; }                //1.norm, 2.Lista, 3.Multi
        public int TipoSubVoto { get; set; }             // a seconda del tipo principale 
        public bool SkBianca { get; set; }               // ha scheda bianca
        public bool SkNonVoto { get; set; }              // ha il non voto
        public int MaxScelte { get; set; }               // n scelte max nel caso di multi
        public bool NeedConferma { get; set; }           // indica che dopo questa votazione necessita la conferma
        public bool PreIntermezzo { get; set; }          // videata intermezzo
        public bool PreIntermezzoFatto { get; set; }     // videata intermezzo

        public bool SelezionaTuttiCDA;

        public int NListe { get { return (Liste == null) ? 0 : Liste.Count; } }
        public int NPresentatoCDA { get { return (Liste == null) ? 0 : Liste.Count(a => a.PresentatodaCDA == true); } }               

        public CBaseTipoVoto TouchZoneVoto;
        public TAreaVotazione AreaVoto;

        public List<TNewLista> Liste;     // collection di strutture Tliste
        public ArrayList Pagine;    // collection delle pagine (per le votazioni candidato)

        public TNewVotazione()
        {
            Liste = new List<TNewLista>();
            Pagine = new ArrayList();
            TouchZoneVoto = null;
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

        public TListaVotazioni(CVotoBaseDati ADBDati, bool ADemoMode)
        {
            // costruttore
            DBDati = ADBDati;
            _Votazioni = new List<TNewVotazione>();
            DemoMode = ADemoMode;

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
            if (_Votazioni.Count > 0)
            {
                //TNewVotazione voto = _Votazioni.First(v => v.IDVoto == AIDVoto);
                idxVotoCorrente = _Votazioni.IndexOf(_Votazioni.First(v => v.IDVoto == AIDVoto));
            }
            return _Votazioni.Count != 0;
        }


        // --------------------------------------------------------------------------
        //  Caricamento dati
        // --------------------------------------------------------------------------

        public bool CaricaListeVotazioni(string AData_path)
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

            if (DemoMode)
            {
                CaricaDatiDemo(AData_path);
                result = true;
            }
            else
            {
                // carica le votazioni dal database
                if (CaricaVotazioniDaDatabase())
                {
                    // carica i dettagli delle votazioni
                    if (CaricaListeDaDatabase())
                    {
                        result = true;
                    }
                }
            }
            // Calcolo l'area di voto per Candidati e multicandidati
            CalcolaAreaDiVotoCandidatiMultiCandidato();
            // ok, ora ordino le liste nel caso in cui siano di candidato
            OrdinaListeInPagineCandidatiMultiCandidato();

            // NOTA: Nelle liste il nome può contenere anche la data di nascita, inserita
            // come token tra ( e ). Serve nel caso di omonimia. La routine di disegno riconoscerà
            // questo e lo tratterà come scritta piccola a lato

            return result;
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
                    voto.TouchZoneVoto.FFormRect = AFormRect;
                else
                {
                    switch (voto.TipoVoto)
                    {
                        case VSDecl.VOTO_LISTA:
                            // TODO: Non è bello la nidificazione di voto
                            voto.TouchZoneVoto = new CTipoVoto_Lista(AFormRect);
                            break;

                        case VSDecl.VOTO_CANDIDATO:
                            if (voto.NListe <= 6)
                                voto.TouchZoneVoto = new CTipoVoto_CandidatoSmall(AFormRect);
                            else
                                voto.TouchZoneVoto = new CTipoVoto_CandidatoOriginal(AFormRect);
                            break;

                        case VSDecl.VOTO_MULTICANDIDATO:
                            // chiamo la classe del voto apposito
                            if (voto.TipoSubVoto == 1)
                                voto.TouchZoneVoto = new CTipoVoto_MultiCandidatoOriginal(AFormRect);
                            else
                                voto.TouchZoneVoto = new CTipoVoto_MultiCandidatoNew(AFormRect);
                            break;

                        #region VOTAZIONE DI CANDIDATO SINGOLO ** MULTI PAGINA ** (era VECCHIO, OBSOLETO)
                        case VSDecl.VOTO_CANDIDATO_SING:
                                // chiamo la classe del voto apposito
                            voto.TouchZoneVoto = new CTipoVoto_CandidatoOriginal(AFormRect);
                            break;
                        #endregion

                        default:
                            voto.TouchZoneVoto = new CTipoVoto_Lista(AFormRect);
                            break;
                    }
                    // calcolo le zone
                    voto.TouchZoneVoto.GetTouchVoteZone(voto);
                }
            }
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
                if (votazione.TipoVoto == VSDecl.VOTO_CANDIDATO ||
                    votazione.TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                    votazione.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
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

            int i, z, pg, pgind;
            string sp;
            TNewLista li;
            TIndiceListe idx; //, idx1;

            // innanzitutto ciclo sulle votazioni
            foreach (TNewVotazione votazione in _Votazioni)
            {
                // solo se il voto è di candidato continuo
                if (votazione.TipoVoto == VSDecl.VOTO_CANDIDATO ||
                    votazione.TipoVoto == VSDecl.VOTO_CANDIDATO_SING ||
                    votazione.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
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

        // --------------------------------------------------------------------------
        //  Caricamento dati da database
        // --------------------------------------------------------------------------

        private bool CaricaVotazioniDaDatabase()
        {
            SqlConnection STDBConn = null;
            SqlDataReader a = null;
            SqlCommand qryStd = null;
            TNewVotazione v;
            bool result = false; //, naz;

            // testo la connessione
            STDBConn = (SqlConnection)DBDati.DBConnect();
            if (STDBConn == null) return false;

            _Votazioni.Clear();

            qryStd = new SqlCommand {Connection = STDBConn};
            try
            {
                // ok ora carico le votazioni
                qryStd.Parameters.Clear();
                qryStd.CommandText = "SELECT * from VS_MatchVot_Totem with (NOLOCK)  where GruppoVotaz < 999 order by NumVotaz";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    while (a.Read())
                    {
                        v = new TNewVotazione  {
                                IDVoto = Convert.ToInt32(a["NumVotaz"]),
                                IDGruppoVoto = Convert.ToInt32(a["GruppoVotaz"]),
                                TipoVoto = Convert.ToInt32(a["TipoVotaz"]),
                                TipoSubVoto = 0,
                                Descrizione = a["Argomento"].ToString(),
                                SkBianca = Convert.ToBoolean(a["SchedaBianca"]),
                                SkNonVoto = Convert.ToBoolean(a["SchedaNonVoto"]),
                                SelezionaTuttiCDA = Convert.ToBoolean(a["SelezTuttiCDA"]),
                                PreIntermezzo = Convert.ToBoolean(a["PreIntermezzo"]),
                                MaxScelte = a.IsDBNull(a.GetOrdinal("MaxScelte")) ? 1 : Convert.ToInt32(a["MaxScelte"])                              
                            };
                        _Votazioni.Add(v);
                        // nota: esisteva nella vecchia versione voto e subvoto, ora tolti, il codice era
                        //  // se è maggiore di 9 e minore di 99 contiene voto e subvoto
                        //  fVoto[nv].TipoVoto = (Int32)Math.Floor((Decimal)TipoVoto / 10);
                        //  fVoto[nv].TipoSubVoto = TipoVoto % 10;                   
                    }
                }
                a.Close();
                result = true;
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("Errore fn CaricaListeVotazioniDaDatabase: err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaListeVotazioniDaDatabase" + "\n\n" +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                DBDati.DBDisconnect();
            }
            return result;
        }

        private bool CaricaListeDaDatabase()
        {
            SqlConnection STDBConn = null;
            SqlDataReader a = null;
            SqlCommand qryStd = null;
            TNewLista l;
            bool result = false; //, naz;

            // testo la connessione
            STDBConn = (SqlConnection)DBDati.DBConnect();
            if (STDBConn == null) return false;

            qryStd = new SqlCommand {Connection = STDBConn};
            try
            {
                // TODO: CaricaListeDaDatabase da vedere in futuro di fare un solo ciclo di caricamento senza ordine
                // ciclo sulle votazioni e carico le liste
                foreach (TNewVotazione votaz in _Votazioni)
                {
                    // ok ora carico le votazioni
                    qryStd.Parameters.Clear();
                    qryStd.CommandText = "SELECT * from VS_Liste_Totem with (NOLOCK) " +
                                         "where NumVotaz = @IDVoto and Attivo = 1 ";
                    // ecco, in funzione del tipo di voto
                    // TODO: CaricaListeDaDatabase TOGLIERE ORDINAMENTO!!!!
                    switch (votaz.TipoVoto)
                    {
                        // se è lista ordino per l'id
                        case VSDecl.VOTO_LISTA:
                            qryStd.CommandText += " order by idlista";
                            break;
                        // se è candidato ordino in modo alfabetico
                        case VSDecl.VOTO_CANDIDATO:
                        case VSDecl.VOTO_CANDIDATO_SING:
                        case VSDecl.VOTO_MULTICANDIDATO:
                            qryStd.CommandText += " order by PresentatoDaCdA desc, OrdineCarica, DescrLista "; //DescrLista ";
                            break;
                        default:
                            qryStd.CommandText += " order by idlista";
                            break;
                    }
                    qryStd.Parameters.Add("@IDVoto", System.Data.SqlDbType.Int).Value = votaz.IDVoto;
                    a = qryStd.ExecuteReader();
                    if (a.HasRows)
                    {
                        while (a.Read())
                        {
                            l = new TNewLista
                                {
                                    NumVotaz = Convert.ToInt32(a["NumVotaz"]),
                                    IDLista = Convert.ToInt32(a["idLista"]),
                                    IDScheda = Convert.ToInt32(a["idScheda"]),
                                    DescrLista = a.IsDBNull(a.GetOrdinal("DescrLista")) ? "DESCRIZIONE" : a["DescrLista"].ToString(),
                                    TipoCarica = Convert.ToInt32(a["TipoCarica"]),
                                    PresentatodaCDA = Convert.ToBoolean(a["PresentatodaCDA"]),
                                    Presentatore = a.IsDBNull(a.GetOrdinal("Presentatore")) ? "" : a["Presentatore"].ToString(),
                                    Capolista = a.IsDBNull(a.GetOrdinal("Capolista")) ? "" : a["Capolista"].ToString(),
                                    ListaElenco = a.IsDBNull(a.GetOrdinal("ListaElenco")) ? "DESCRIZIONE" : a["ListaElenco"].ToString()
                                };
                            votaz.Liste.Add(l);
                        }
                    }
                    a.Close();
                }
                result = true;
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("Errore fn CaricaListeDaDatabase: err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaListeDaDatabase" + "\n\n" +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                DBDati.DBDisconnect();
            }
            return result;
        }

        // --------------------------------------------------------------------------
        //  Area di votazione
        // --------------------------------------------------------------------------

        private void CaricaDatiDemo(string AData_path)
        {
            Demo_CaricaDatiVotazioni(AData_path);
            Demo_CaricaDatiListe(AData_path);
        }

        private int Demo_CaricaDatiVotazioni(string AData_path)
        {
            int z;
            DataTable dt = new DataTable();
            TNewVotazione v;

            dt.ReadXml(AData_path + "VS_MatchVot_Totem.xml");

            foreach (DataRow a in dt.Rows)
            {
                v = new TNewVotazione
                {
                    IDVoto = Convert.ToInt32(a["NumVotaz"]),
                    IDGruppoVoto = Convert.ToInt32(a["GruppoVotaz"]),
                    TipoVoto = Convert.ToInt32(a["TipoVotaz"]),
                    TipoSubVoto = 0,
                    Descrizione = a["Argomento"].ToString(),
                    SkBianca = Convert.ToBoolean(a["SchedaBianca"]),
                    SkNonVoto = Convert.ToBoolean(a["SchedaNonVoto"]),
                    SelezionaTuttiCDA = Convert.ToBoolean(a["SelezTuttiCDA"]),
                    PreIntermezzo = false,
                    MaxScelte = Convert.ToInt32(a["MaxScelte"])
                };
                _Votazioni.Add(v);
            }

            dt.Dispose();

            return 0;
        }

        private int Demo_CaricaDatiListe(string AData_path)
        {
            DataTable dt = new DataTable();
            TNewLista Lista;
            int presCDA;
            string ASort;

            dt.ReadXml(AData_path + "VS_Liste_Totem.xml");
            ASort = "idlista desc";
            // cicla lungo le votazioni e carica le liste
            foreach (TNewVotazione votaz in _Votazioni)
            {
                // faccio un sorting delle liste
                switch (votaz.TipoVoto)
                {
                    // se è lista ordino per l'id
                    case VSDecl.VOTO_LISTA:
                        ASort = "idlista asc";
                        break;
                    // se è candidato ordino in modo alfabetico
                    case VSDecl.VOTO_CANDIDATO:
                    case VSDecl.VOTO_CANDIDATO_SING:
                    case VSDecl.VOTO_MULTICANDIDATO:
                        ASort = "PresentatoDaCdA desc, OrdineCarica, DescrLista asc";
                        break;
                }

                presCDA = 0;
                foreach (DataRow riga in dt.Select("NumVotaz = " +
                    votaz.IDVoto.ToString(), ASort))
                {
                    Lista = new TNewLista
                        {
                            NumVotaz = Convert.ToInt32(riga["NumVotaz"]),
                            IDLista = Convert.ToInt32(riga["idLista"]),
                            IDScheda = Convert.ToInt32(riga["idScheda"]),
                            DescrLista = riga["DescrLista"].ToString(),
                            TipoCarica = Convert.ToInt32(riga["TipoCarica"]),
                            PresentatodaCDA = Convert.ToBoolean(riga["PresentatodaCDA"]),
                            Presentatore = riga["Presentatore"].ToString(),
                            Capolista = riga["Capolista"].ToString(),
                            ListaElenco = riga["ListaElenco"].ToString()
                        };
                    // aggiungo
                    votaz.Liste.Add(Lista);
                }
            }

            dt.Dispose();

            return 0;
        }



    }
}
