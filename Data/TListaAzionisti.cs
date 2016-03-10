using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows.Forms;

namespace VotoTouch
{
    
    public class TAzionista
	{
        // dati dell'utente
        public int IDBadge { get; set; }
        public string CoAz { get; set; }
        public int IDAzion { get; set; }
        public int ProgDeleg { get; set; }
        public string RaSo { get; set; }
        public int NAzioni { get; set; }
        public string Sesso { get; set; }
        public int HaVotato { get; set; }
        
        // TODO: METTERE I VOTI DENTRO TListAzionista e togliere le strutture voti
        // dati del voto 
        public int IDVotaz { get; set; }
        //public int IDScheda;
        //public int NVoti;
        //public int IDCarica;

        public TAzionista()
        {
            HaVotato = TListaAzionisti.VOTATO_NO;
        }

        public void CopyFrom(ref TAzionista cp)
        {
            IDBadge = cp.IDBadge; CoAz = cp.CoAz; IDAzion = cp.IDAzion; ProgDeleg = cp.ProgDeleg;
            RaSo = cp.RaSo; NAzioni = cp.NAzioni; Sesso = cp.Sesso; HaVotato = cp.HaVotato;
            IDVotaz = cp.IDVotaz;
        }

	}

    public class TListaAzionisti
    {

        public const int VOTATO_NO = 0;
        public const int VOTATO_SESSIONE = 1;
        public const int VOTATO_DBASE = 2;

        // oggetto lista azionisti
        protected List<TAzionista> _Azionisti;
        public List<TAzionista> Azionisti
        {
            get { return _Azionisti; }
            set
            {
                _Azionisti = value;
            }
        }

        // oggetto voto corrente
        protected List<TAzionista> ListaDiritti_VotoCorrente;
        // Oggetto titolare che contene il titolare del badge
        public TAzionista Titolare_Badge { get; set; }

        // stringhe sql
        private string qry_DammiDirittiDiVoto_Deleganti = "";
        private string qry_DammiDirittiDiVoto_Titolare = "";

        private CVotoBaseDati DBDati;

        public TListaAzionisti(CVotoBaseDati ADBDati)
        {
            // costruttore
            DBDati = ADBDati;
            _Azionisti = new List<TAzionista>();
            Titolare_Badge = new TAzionista();

            ListaDiritti_VotoCorrente = new List<TAzionista>();

            // load the query
            qry_DammiDirittiDiVoto_Deleganti = DBDati.getModelsQueryProcedure("DammiDirittiDiVoto_Deleganti.sql");
            qry_DammiDirittiDiVoto_Titolare = DBDati.getModelsQueryProcedure("DammiDirittiDiVoto_Titolare.sql");
        }

        ~TListaAzionisti()
        {
            // Distruttore
        }

        // --------------------------------------------------------------------------
        //  Ritorno dati Normali
        // --------------------------------------------------------------------------

        #region Ritorno dati Normali 

        public List<TAzionista> DammiDirittiDiVotoPerIDVotazione(int AIDVotazione, bool ASoloDaVotare)
        {
            // mi da la collection di diritti di voto per singola votazione
            // in più se ASoloDaVotare = true mi da solo quello che devono esprimere il voto
            // sennò mi da tutti quanti

            //List<TAzionista> newList =  
            if (ASoloDaVotare)
                return _Azionisti.Where(a => a.IDVotaz == AIDVotazione && a.HaVotato == VOTATO_NO).ToList();
            else
                return  _Azionisti.Where(a => a.IDVotaz == AIDVotazione).ToList();
        }

        public int DammiTotaleDirittiRimanentiPerIDVotazione(int AIDVotazione)
        {
            return _Azionisti.Count(a => a.IDVotaz == AIDVotazione && a.HaVotato == VOTATO_NO);
        }

        public int DammiTotaleDirittiRimanenti()
        {
            return _Azionisti.Count(a => a.HaVotato == VOTATO_NO);
        }

        public bool HaDirittiDiVotoMultipli()
        {
            return (DammiMaxNumeroDirittiDiVotoTotali() > 0);
        }

        public int DammiMaxNumeroDirittiDiVotoTotali()
        {
            // questa funzione non è banale, perchè deve estrarre il numero massimo di ritti di voto, 
            // che nel caso normale è semplice, ma in caso di votazioni differenziate già espresse
            // può essere difficoltoso. In pratica seleziono il conteggio dei diritti per IDVotazioni
            // e prendo il numero maggiore
            var maxDiritti = Azionisti
               .Where(n => n.HaVotato == VOTATO_NO)
               .GroupBy(n => n.IDVotaz)
               .Select(group =>
                       new
                       {
                           IDVotaz = group.Key,
                           //Diritti = group.ToList(),
                           Count = group.Count()
                       })
               .Max(n => n.Count);
            return (int) maxDiritti;
        }

        #endregion

        // --------------------------------------------------------------------------
        //  Gestione della procedura di votazione
        // --------------------------------------------------------------------------

        #region Gestione della procedura di votazione

        public void InizioProceduraVotazione(bool ADifferenziato)
        {
            // segnala che è l'inizio della procedura di votazione, vediamo a cosa serve

            // resetta i diritti di voto
            ListaDiritti_VotoCorrente.Clear();
        }

        public bool EstraiAzionisti_VotoCorrente(bool ADifferenziato)
        {
            // estrae l'azionista  (se diff o se ha 1 solo diritto) o l'elenco di azionisti che sono in voto
            // tipicamente prende i primi della lista che non hanno votato:
            // - Se normale prende il gruppo che non ha votato di una singola votazione
            // - Se differenziato prende il primo record che non ha votato indipendentemente dalla votazione
           
            // ritorna true se ce ne sono, false se è finito il voto
            if (_Azionisti != null && _Azionisti.Count > 0)
            {
                // resetta la lista dei diritti di voto correnti
                ListaDiritti_VotoCorrente.Clear();
                if (ADifferenziato)
                {
                    // LINQ Prende il primo e lo trasferisce nella lista correnti
                    var listatemp = _Azionisti.Where(a => a.HaVotato == VOTATO_NO).Take(1);
                    //var azionistas = listatemp as IList<TAzionista> ?? listatemp.ToList();
                    //ListaDiritti_VotoCorrente = azionistas.ToList();
                    foreach (TAzionista c in listatemp)
                        ListaDiritti_VotoCorrente.Add(c);
                }
                else
                {
                    // LINQ Prende i primi n con idvotazioni contigui partendo dal primo
                    var listatemp = _Azionisti.Where(a => a.HaVotato == VOTATO_NO).Take(1);
                    //var azionistas = listatemp as IList<TAzionista> ?? listatemp.ToList();
                    //if (azionistas.Any())
                    if (listatemp.Count() > 0)
                    {
                        // estrae la votazione del primo
                        //TAzionista v = azionistas.ElementAt(0);
                        TAzionista v = listatemp.ElementAt(0);
                        listatemp = _Azionisti.Where(a => a.HaVotato == VOTATO_NO && a.IDVotaz == v.IDVotaz);
                        foreach (TAzionista c in listatemp)
                            ListaDiritti_VotoCorrente.Add(c);
                    }
                }

                // ritorno se 
                return (ListaDiritti_VotoCorrente.Count > 0);
            }
            else
                return false;

            // NOTA: POTREBBERO ESSERE SOLO GLI ID DEGLI AZIONISTI O I PUNTATORI O LINQ
        }

        public int DammiCountDirittiDiVoto_VotoCorrente()
        {
            // conta i diritti di voto relativi agli azionisti in votazione (lista sopra) 
            if (ListaDiritti_VotoCorrente != null)
                return ListaDiritti_VotoCorrente.Count(a => a.HaVotato == VOTATO_NO);
            else
                return 0;
        }

        public int DammiIDVotazione_VotoCorrente()
        {
            // ritorna l'IDVotazione relativo alla lista o al singolo, in pratica del primo item 
            // NOTA: se è normale prende il primo della lista che sono tutti uguali, se diff prende comunque il primo
            if (ListaDiritti_VotoCorrente != null && ListaDiritti_VotoCorrente.Count > 0)
            {
                TAzionista c = ListaDiritti_VotoCorrente.First();
                return  c != null ? c.IDVotaz : 0;
            }
            else
                return 0;
        }

        public string DammiNomeAzionistaInVoto_VotoCorrente(bool AIsVotazioneDifferenziata)
        {
            // se votazione normale prende il titolare, se differenziata il primo della lista dei voti correnti (che sarà sempre 1)
            if (!AIsVotazioneDifferenziata)
                return Titolare_Badge.RaSo;
            else
            {
                if (ListaDiritti_VotoCorrente != null && ListaDiritti_VotoCorrente.Count > 0)
                {
                    TAzionista c = ListaDiritti_VotoCorrente.First();
                    return c != null ? c.RaSo : "";
                }
                else
                    return "";
            }         
        }

        #endregion

        // --------------------------------------------------------------------------
        //  Gestione dei voti, sono demandati a azionisti (bisognerà cambiarlo in diritti di voto)
        // --------------------------------------------------------------------------

        public bool SetVotoNormale_VotoCorrente(int AVoto)
        {

            return true;
        }

        public bool SetVotoMultiCandidato_VotoCorrente(ref ArrayList AVotiDaSalvare)
        {

            return true;
        }

        public bool ConfermaVoti_VotoCorrente()
        {

            return true;
        }

        public bool AnnullaVoti_VotoCorrente()
        {

            return true;
        }

        // --------------------------------------------------------------------------
        //  Caricamento dati da database
        // --------------------------------------------------------------------------

        public bool CaricaDirittidiVotoDaDatabase(int AIDBadge, ref TListaVotazioni AVotazioni)
        {
            // ok, questa funziomne carica i diritti di voto in funzione
            // del idbadge, in pratica alla fine avrò una lista di diritti *per ogni votazione*
            // con l'indicazione se sono stati già espressi o no

            // ok, questa procedura mi carica tutti i dati
		    SqlConnection STDBConn = null;
            SqlDataReader a = null;
            SqlCommand qryStd = null;
            TAzionista c;
            int IDVotazione = -1;
            bool result = false; //, naz;

            // testo la connessione
            STDBConn = (SqlConnection)DBDati.DBConnect();
            if (STDBConn == null) return false;

            _Azionisti.Clear();

            qryStd = new SqlCommand {Connection = STDBConn};
            try
            {
                // ciclo sul voto per crearmi l'array dei diritti di voto per ogni singola votazione
                //for (int i = 0  ; i < NVoti; i++)
                foreach (TNewVotazione voto in AVotazioni.Votazioni)
                {
                    IDVotazione = voto.IDVoto;

                    // resetto la query
                    qryStd.Parameters.Clear();

                    // ok ora carico il titolare
                    qryStd.CommandText = qry_DammiDirittiDiVoto_Titolare;
                    qryStd.Parameters.Add("@IDVotaz", System.Data.SqlDbType.Int).Value = IDVotazione;
                    qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                    a = qryStd.ExecuteReader();
                    // in teoria non può non avere righe, testa anche se ha azioni, se no è un rappr
                    if (a.HasRows && a.Read()) 
                    {
                        c = new TAzionista {
                                CoAz = a.IsDBNull(a.GetOrdinal("CoAz")) ? "0000000" : a["CoAz"].ToString(),
                                IDAzion = Convert.ToInt32(a["IdAzion"]),
                                IDBadge = AIDBadge,
                                ProgDeleg = 0,
                                RaSo = a["Raso1"].ToString(),
                                NAzioni = Convert.ToInt32(a["AzOrd"]),
                                Sesso = a.IsDBNull(a.GetOrdinal("Sesso")) ? "N" : a["Sesso"].ToString(),
                                HaVotato = Convert.ToInt32(a["TitIDVotaz"]) >= 0 ? VOTATO_DBASE : VOTATO_NO,
                                IDVotaz = IDVotazione
                            };

                        // ok, ora se è titolare e ha azioni l'aggiungo alla lista
                        if ((Convert.ToInt32(a["AzOrd"]) + Convert.ToInt32(a["AzStr"])) > 0)
                            _Azionisti.Add(c);

                        // poi lo salvo come titolare
                        Titolare_Badge.CopyFrom(ref c);
                    }
                    a.Close();

                    // resetto la query
                    qryStd.Parameters.Clear();

                    // ora carico i deleganti
                    qryStd.CommandText = qry_DammiDirittiDiVoto_Deleganti;
                    qryStd.Parameters.Add("@IDVotaz", System.Data.SqlDbType.Int).Value = IDVotazione;
                    qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                    a = qryStd.ExecuteReader();
                    if (a.HasRows)  
                    {
                        while (a.Read())        // qua posso avere più righe
                        {
                            // anche qua devo testare se ha azioni 0, potrebbe essere un badge banana
                            if ((Convert.ToInt32(a["AzOrd"]) + Convert.ToInt32(a["AzStr"])) > 0)
                            {
                                c = new TAzionista {
                                        CoAz = a.IsDBNull(a.GetOrdinal("CoAz")) ? "0000000" : a["CoAz"].ToString(),
                                        IDAzion = Convert.ToInt32(a["IdAzion"]),
                                        IDBadge = AIDBadge,
                                        ProgDeleg = Convert.ToInt32(a["ProgDeleg"]),
                                        RaSo = a["Raso1"].ToString(),
                                        NAzioni = Convert.ToInt32(a["AzOrd"]),
                                        Sesso = "N",
                                        HaVotato = Convert.ToInt32(a["ConIDVotaz"]) >= 0 ? VOTATO_DBASE : VOTATO_NO,
                                        IDVotaz = IDVotazione
                                    };
                                _Azionisti.Add(c);
                            }
                        }   //while (a.Read()) 
                    }   //if (a.HasRows)
                    a.Close();

                }   //for (int i = 0...
                result = true;

            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("Errore fn DammiDatiAzionistaOneShot: " + AIDBadge.ToString() + " err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaDirittidiVotoDaDatabase" + "\n\n" +
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



    }
}
