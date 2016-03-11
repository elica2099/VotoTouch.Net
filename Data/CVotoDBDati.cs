using System;
using System.IO;
using System.Data;
using System.Collections; 
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Reflection;
using System.Windows; //.Forms.Design;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;


// -----------------------------------------------------------------------
//			VOTO TOUCH - TSTSQLCONN ClassE
//  Classe di gestione del database e task relativi
// -----------------------------------------------------------------------
//		AUTH	: M.Binello
//		VER		: 1.1 
//		DATE	: Feb 2013
// -----------------------------------------------------------------------
//	History
//  Aggiunto controllo versione db
// -----------------------------------------------------------------------

namespace VotoTouch
{
	/// <summary>
	/// Summary description for CVotoDBDati.
	/// </summary>
    public class CVotoDBDati : CVotoBaseDati
	{

		private string DriveM = @"M:\";
		// connessione database
		private SqlConnection STDBConn;

        // stringhe sql
	    private string qry_DammiDatiAzionista_Deleganti = "";
        private string qry_DammiDatiAzionista_Titolare = "";
		
		public CVotoDBDati()
		{
			//
			STDBConn = new SqlConnection();
			FConnesso = false;
			// setto i parametri di default di DBConfig
			FDBConfig.DB_ConfigOK = false;
			FDBConfig.DB_Type = "ODBC";
			FDBConfig.DB_Dsn = "GEAS";
			FDBConfig.DB_Name = "GEAS_BPER";
			FDBConfig.DB_Uid = "geas";
			FDBConfig.DB_Pwd = "geas";
			FDBConfig.DB_Server = @"TOGTA-SRVSQL2k\SQL2kGTA";
			//
			FIDSeggio = 2;

            // load the query
            qry_DammiDatiAzionista_Deleganti = getModelsQueryProcedure("DammiDatiAzionista_Deleganti.sql");
            qry_DammiDatiAzionista_Titolare = getModelsQueryProcedure("DammiDatiAzionista_Titolare.sql");
		}

        ~CVotoDBDati()
        {
            // Destructor
        }

        // --------------------------------------------------------------------------
        //  METODI DATABASE
        // --------------------------------------------------------------------------

        #region Metodi Database

        override public object DBConnect()
        {
            // connessione al DB in funzione dei parametri che ci sono in TSTConfig
            STDBConn.ConnectionString = DammiStringaConnessione();
            try
            {
                STDBConn.Open();
                FConnesso = true;
                return STDBConn;
            }
            catch
            {
                FConnesso = false;
                return null;
            }
        }

        override public object DBDisconnect()
        {
            // disconnessione al DB
            STDBConn.Close();
            FConnesso = false;
            return STDBConn;
        }

        override public string DammiStringaConnessione()
        {
            // devo aggiungere dei controlli
            // compone la stringa di connessione in funzione di TSTConfig
            string ssconn = "server=" + FDBConfig.DB_Server + ";database=" + FDBConfig.DB_Name +
                ";uid=" + FDBConfig.DB_Uid + ";pwd=" + FDBConfig.DB_Pwd;
            return ssconn;
        }

        public bool OpenConnection(string NomeFunzione)
        {
            // apro la connessione se è chiusa
            if (STDBConn.State == ConnectionState.Open) return true;
            try
            {
                STDBConn.Open();
                return true;
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("<dberror> Errore fn " + NomeFunzione + " - OpenConnection: " + objExc.Message);
                MessageBox.Show("Errore fn " + NomeFunzione + " - OpenConnection: " + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool CloseConnection(string NomeFunzione)
        {
            if (STDBConn.State == ConnectionState.Closed) return true;
            try
            {
                STDBConn.Close();
                return true;
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("<dberror> Errore fn " + NomeFunzione + " - CloseConnection: " + objExc.Message);
                MessageBox.Show("Errore fn " + NomeFunzione + " - CloseConnection: " + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion
        
        // --------------------------------------------------------------------------
        //  LETTURA CONFIGURAZIONE NEL DATABASE
        // --------------------------------------------------------------------------

        #region Lettura/Scrittura Configurazione

        override public int CaricaConfigDB(ref int BadgeLen, ref string CodImpianto)
        {
            // DROK 13
            // mi dice la lunghezza del badge e il codice impianto per il lettore
            SqlDataReader a;
            SqlCommand qryStd;
            int Tok;

            // testo la connessione
            if (!OpenConnection("CaricaConfigDB")) return 0;

            BadgeLen = 8;
            CodImpianto = "00";
            Tok = 0;
            qryStd = new SqlCommand();
            try
            {
                qryStd.Connection = STDBConn;
                // Leggo ora da GEAS_Titolari	
                qryStd.CommandText = "select * from CONFIG_cfgParametri with (nolock)";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    // devo verificare 
                    a.Read();

                    BadgeLen = a.IsDBNull(a.GetOrdinal("LenNumBadge")) ? 8 : Convert.ToInt32(a["LenNumBadge"]);
                    CodImpianto = a.IsDBNull(a.GetOrdinal("CodImpRea")) ? "00" : (a["CodImpRea"]).ToString();
                }
                else
                {
                    BadgeLen = 8;
                    CodImpianto = "00";
                }
                a.Close();
            }
            catch (Exception objExc)
            {
                Tok = 1;
                Logging.WriteToLog("<dberror> Errore nella funzione CaricaConfigDB: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaConfigDB" + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }
            return Tok;
        }

        override public int DammiConfigTotem(string NomeTotem, ref TTotemConfig TotCfg)
        {
            // DR13 OK
            SqlDataReader a;
            SqlCommand qryStd;
            SqlTransaction traStd;
            int result;
            bool inserisci;

            // testo la connessione
            if (!OpenConnection("DammiConfigTotem")) return 0;
            
            result = 0;
            // preparo gli oggetti
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            // registra il totem aggiungendo il record in CONFIG_POSTAZIONI, e chiaramente verifica che ci sia già
            qryStd.CommandText = "select * from CONFIG_POSTAZIONI_TOTEM with (nolock) where Postazione = '" + NomeTotem + "'";
            
            traStd = STDBConn.BeginTransaction();
            qryStd.Transaction = traStd;
            inserisci = false;

            try
            {
                a = qryStd.ExecuteReader();
                // se c'è il record
                if (a.HasRows)
                {
                    // devo verificare 
                    a.Read();
                    // carico
                    TotCfg.Postazione = a["Postazione"].ToString();
                    // faccio un  ulteriore controllo
                    if (NomeTotem != TotCfg.Postazione) TotCfg.Postazione = NomeTotem;

                    TotCfg.Descrizione = a.IsDBNull(a.GetOrdinal("Descrizione")) ? NomeTotem : a["Descrizione"].ToString();
                    TotCfg.IDSeggio = Convert.ToInt32(a["IdSeggio"]);
                    FIDSeggio = Convert.ToInt32(a["IdSeggio"]);

                    TotCfg.Attivo = Convert.ToBoolean(a["Attivo"]);
                    TotCfg.VotoAperto = Convert.ToBoolean(a["VotoAperto"]);

                    TotCfg.UsaSemaforo = Convert.ToBoolean(a["UsaSemaforo"]);
                    TotCfg.IP_Com_Semaforo = a["IPCOMSemaforo"].ToString();
                    TotCfg.TipoSemaforo = Convert.ToInt32(a["TipoSemaforo"]);

                    TotCfg.UsaLettore = Convert.ToBoolean(a["UsaLettore"]);
                    TotCfg.PortaLettore = Convert.ToInt32(a["PortaLettore"]);
                    TotCfg.CodiceUscita = a["CodiceUscita"].ToString();

                    TotCfg.ControllaPresenze = Convert.ToInt32(a["ControllaPresenze"]);

                    TotCfg.UsaController = Convert.ToBoolean(a["UsaController"]);
                    TotCfg.IPController = a["IPController"].ToString();

                    TotCfg.Sala = a.IsDBNull(a.GetOrdinal("Sala")) ? 1 : Convert.ToInt32(a["Sala"]);

                }
                else
                    inserisci = true;
                // chiudo
                a.Close();

                // ok, se inserisci è true, vuol dire che non ha trovato record e devo inserirlo
                if (inserisci)
                {
                    // non c'è configurazione, devo inserirla
                    qryStd.CommandText = "INSERT into CONFIG_POSTAZIONI_TOTEM " +
                        "(Postazione, Descrizione, IdSeggio, Attivo, VotoAperto, UsaSemaforo, "+
                        " IPCOMSemaforo, TipoSemaforo, UsaLettore, PortaLettore, CodiceUscita, ControllaPresenze, " +
                        " UsaController, IPController, Comando, RicaricaConfig, Configurazione, Sala) " +
                        " VALUES ('" + NomeTotem + "', 'Desc_" + NomeTotem + "', 999, 1, 0, 0, " +
                        "'127.0.0.1', 2, 0, 1, '999999', " + VSDecl.PRES_CONTROLLA.ToString() + 
                        ", 0, '127.0.0.1', 0, 0, 'n', 1)";
                    
                    // metto in quadro i valori
                    TotCfg.Postazione = NomeTotem;
                    TotCfg.Descrizione = NomeTotem; 
                    TotCfg.IDSeggio = 999;
                    FIDSeggio = 999;
                    TotCfg.Attivo = true;
                    TotCfg.VotoAperto = false;
                    TotCfg.UsaSemaforo = false;
                    TotCfg.IP_Com_Semaforo = "127.0.0.1";
                    TotCfg.UsaLettore = false;
                    TotCfg.PortaLettore = 1;
                    TotCfg.CodiceUscita = "999999";
                    TotCfg.ControllaPresenze = VSDecl.PRES_CONTROLLA;
                    TotCfg.UsaController = false;
                    TotCfg.IPController = "127.0.0.1";
                    TotCfg.Sala = 1;
                    // parte come semaforo com per facilitare gli esterni,
                    // poi bisognerà fare un wizard di configurazione
                    TotCfg.TipoSemaforo = VSDecl.SEMAFORO_COM;
                    // ora scrivo
                    int NumberofRows = qryStd.ExecuteNonQuery();
                }

                // chiudo la transazione
                traStd.Commit();
                result = 0;
            }
            catch (Exception objExc)
            {
                result = 1;
                traStd.Rollback();
                Logging.WriteToLog("<dberror> Errore nella funzione DammiConfigTotem: " + objExc.Message);
                MessageBox.Show("Errore nella funzione DammiConfigTotem" + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                traStd.Dispose();
                CloseConnection("");
            }

            return result;
        }

        override public int DammiConfigDatabase(ref TTotemConfig TotCfg)
        {
            // DR13 OK
            SqlDataReader a;
            SqlCommand qryStd;
            int result;

            // testo la connessione
            if (!OpenConnection("DammiConfigDatabase")) return 0;

            result = 0;
            // preparo gli oggetti
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            // la configurazione ci deve essere, non è necessario inserirla
            qryStd.CommandText = "select * from CONFIG_CfgVotoSegreto with (nolock)";

            try
            {
                a = qryStd.ExecuteReader();
                // se c'è il record
                if (a.HasRows)
                {
                    // devo verificare 
                    a.Read();
                    // carico
                    // il link del voto
                    TotCfg.SalvaLinkVoto = Convert.ToBoolean(a["SalvaLinkVoto"]);
                    // il salvataggio del voto anche se non ha confermato
                    TotCfg.SalvaVotoNonConfermato = Convert.ToBoolean(a["SalvaVotoNonConfermato"]);
                    // l'id della scheda che deve essere salvata in caso di 999999
                    int IDSchedaForz = Convert.ToInt32(a["IDSchedaUscitaForzata"]);
                    // qua dovrei in teoria controllare che vada bene
                    // prima faccio un piccolo controllo, se è un valore a c..., metto scheda bianca che c'è sempre
                    if (IDSchedaForz == VSDecl.VOTO_SCHEDABIANCA || IDSchedaForz == VSDecl.VOTO_NONVOTO)
                        TotCfg.IDSchedaUscitaForzata = IDSchedaForz;
                    else
                        TotCfg.IDSchedaUscitaForzata = VSDecl.VOTO_SCHEDABIANCA;
                    // ok, ora il tasto ricomincia da capo
                    TotCfg.TastoRicominciaDaCapo = Convert.ToBoolean(a["TastoRicominciaDaCapo"]);
                    // ok, ora il LOG del voto
                    TotCfg.AbilitaLogV = Convert.ToBoolean(a["AbilitaLogV"]);
                    // AbilitaDirittiNonVoglioVotare
                    TotCfg.AbilitaDirittiNonVoglioVotare = Convert.ToBoolean(a["AbilitaDirittiNonVoglioVotare"]);

                }
                else
                {
                    TotCfg.SalvaLinkVoto = true;
                    TotCfg.SalvaVotoNonConfermato = false;
                    TotCfg.IDSchedaUscitaForzata = VSDecl.VOTO_NONVOTO;
                    TotCfg.TastoRicominciaDaCapo = false;
                    TotCfg.AbilitaLogV = true;
                }
                // chiudo
                a.Close();

                // chiudo la transazione
                result = 0;
            }
            catch (Exception objExc)
            {
                result = 1;
                Logging.WriteToLog("<dberror> Errore nella funzione DammiConfigDatabase: " + objExc.Message);
                MessageBox.Show("Errore nella funzione DammiConfigDatabase" + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }

            return result;
        }
        
        override public int SalvaConfigurazione(string ANomeTotem, ref TTotemConfig ATotCfg)
        {
            // DROK 13
            SqlCommand qryStd;
            SqlTransaction traStd;
            int NumberofRows, result;
            short usal, usas;

            // testo la connessione
            if (!OpenConnection("SalvaConfigurazione")) return 0;

            result = 0;
            // preparo gli oggetti
            if (ATotCfg.UsaLettore) usal = 1; else usal = 0;
            if (ATotCfg.UsaSemaforo) usas = 1; else usas = 0;
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
             // devo inserirlo
            traStd = STDBConn.BeginTransaction();
            try
            {
                qryStd.Transaction = traStd;
                qryStd.CommandText = "update CONFIG_POSTAZIONI_TOTEM with (rowlock) set " +
                        "  UsaLettore = " + usal.ToString() + 
                        ", PortaLettore = " + ATotCfg.PortaLettore.ToString() +
                        ", UsaSemaforo = " + usas.ToString() +
                        ", IPCOMSemaforo = '" + ATotCfg.IP_Com_Semaforo + "'" +
                        " where Postazione = '" + ANomeTotem + "'";
                NumberofRows = qryStd.ExecuteNonQuery();
                traStd.Commit();
                result = 1;
            }
            catch (Exception objExc)
            {
                traStd.Rollback();
                result = 0;
                Logging.WriteToLog("<dberror> Errore nella funzione SalvaConfigurazioneLettore: " + objExc.Message);
                MessageBox.Show("Errore nella funzione SalvaConfigurazioneLettore" + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                traStd.Dispose();
                CloseConnection("");
            }

            return result;
        }

        #endregion       
        
        // --------------------------------------------------------------------------
        //  CARICAMENTO DATI VOTAZIONI
        // --------------------------------------------------------------------------

        #region Caricamento Votazioni e Liste *************** OBSOLETO CON I NUOVI OGGETTI *************************
/*
        // *************** OBSOLETO CON I NUOVI OGGETTI *************************
        override public int CaricaDatiVotazioni(ref int NVoti, ref TVotazione[] fVoto)
        {
            // DR11 OK
            SqlDataReader a;
            SqlCommand qryStd;
            int result, nvotaz, nv, grp;

            // testo la connessione
            if (!OpenConnection("CaricaDatiVotazioni")) return 0;
            
            result = 0;
            NVoti = 0;          // setto comunque un default se va male
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            try
            {
                // prima di tutto conto quante votazioni ci sono e creo l'array
                qryStd.CommandText = "SELECT  count(*) as NVotazioni from VS_MatchVot_Totem with (NOLOCK) where GruppoVotaz < 999";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    a.Read();
                    nvotaz = Convert.ToInt32(a["NVotazioni"]);
                }
                else
                    return 1;
                a.Close();

                // ok, ora se è tutto andato bene creo l'array
                // metto un massimo di 20 votazioni
                if (nvotaz > 0 && nvotaz <= 20)
                {
                    fVoto = new TVotazione[nvotaz];
                    NVoti = nvotaz;
                }
                else
                    return 1;

                // ok, ora leggo le votazioni
                nv = 0;
                qryStd.CommandText = "SELECT  * from VS_MatchVot_Totem with (NOLOCK)  where GruppoVotaz < 999 order by NumVotaz";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    int TipoVoto = 0;

                    while (a.Read())
                    {
                        // leggo i dati delle votazioni
                        fVoto[nv].IDVoto = Convert.ToInt32(a["NumVotaz"]);
                        fVoto[nv].IDGruppoVoto = Convert.ToInt32(a["GruppoVotaz"]);
                        fVoto[nv].Descrizione = a["Argomento"].ToString();
                        TipoVoto = Convert.ToInt32(a["TipoVotaz"]);
                        //FParVoto[nv].TipoVoto  = Convert.ToInt32(a["TipoVotaz"]);
                        fVoto[nv].NListe = Convert.ToInt32(a["NListe"]);
                        fVoto[nv].SkBianca = Convert.ToBoolean(a["SchedaBianca"]);
                        fVoto[nv].SkNonVoto = Convert.ToBoolean(a["SchedaNonVoto"]);
                        fVoto[nv].SelezionaTuttiCDA = Convert.ToBoolean(a["SelezTuttiCDA"]);
                        fVoto[nv].PreIntermezzo = Convert.ToBoolean(a["PreIntermezzo"]);
                        // non so se può essere nullo ( lo è )
                        fVoto[nv].MaxScelte = a.IsDBNull(a.GetOrdinal("MaxScelte")) ? 1 : Convert.ToInt32(a["MaxScelte"]);

                        // ora spacchetto il TipoVoto che contiene il TipoVoto e il TipoSubvoto
                        // se è minore di 10 vuol dire che il tipovoto è quello e il subvoto = 0
                        if (TipoVoto < 10)
                        {
                            fVoto[nv].TipoVoto = TipoVoto;
                            fVoto[nv].TipoSubVoto = 0;
                        }
                        else
                        {
                            // se è maggiore di 9 e minore di 99 contiene voto e subvoto
                            fVoto[nv].TipoVoto = (Int32)Math.Floor((Decimal)TipoVoto / 10);
                            fVoto[nv].TipoSubVoto = TipoVoto % 10;
                        }

                        nv++;
                    }
                }

                // in ogni caso, per pulizia qui faccio una cosa, cioè creare la collection
                // che sta in ogni elemento dell'array FParVoto e che conterrà le liste
                for (int i = 0; i < NVoti; i++)
                {
                    fVoto[i].Liste = new ArrayList();
                    fVoto[i].Pagine = new ArrayList();
                }

                // qui bisognerebbe settare il flag NeedConferma che indica la fine della serie di votazioni
                // date da IDGruppoVoto e il bisogno della videata di conferma
                grp = -1;
                for (int i = 0; i < NVoti; i++)
                {
                    // se il gruppo è maggiore di zero c'è un gruppo, sennò è una votazione singola
                    if (fVoto[i].IDGruppoVoto > 0)
                    {
                        grp = fVoto[i].IDGruppoVoto;

                        // ci sono alcuni casi
                        // OK, ora devo controllare se è l'ultimo, in tal caso


                        // ok, vedo il successivo
//                        if (i == (NVoti
                        

                    }
                    else
                        // è una votazione singola e richiede una conferma
                        fVoto[i].NeedConferma = true;
                
                }


                a.Close();
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("<dberror> Errore fn CaricaDatiVotazioni: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaDatiVotazioni\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                NVoti = 0;
                result = 1;
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }

            // carico i dati
            return result;
        }

        // --------------------------------------------------------------------------
        //  CARICAMENTO DATI VOTAZIONI
        // --------------------------------------------------------------------------

        // *************** OBSOLETO CON I NUOVI OGGETTI *************************
        override public int CaricaDatiListe(ref int NVoti, ref TVotazione[] fVoto)
        {
            // DR11 OK
            SqlDataReader a;
            SqlCommand qryStd;
            int result, presCDA;
            TLista Lista;

            // testo la connessione
            if (!OpenConnection("CaricaDatiListe")) return 0;

            result = 0;
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            try
            {
                // cicla lungo le votazioni e carica le liste
                for (int i = 0; i < NVoti; i++)
                {
                    presCDA = 0;
                    // preparo la query e la apro, però devo fare delle distinzioni in funzione 
                    // del tipo di votazione, più che altro dell'ordine
                    qryStd.CommandText = "SELECT  * from VS_Liste_Totem with (NOLOCK) where NumVotaz = " +
                        fVoto[i].IDVoto + " and Attivo = 1";
                    // ecco, in funzione del tipo di voto
                    switch (fVoto[i].TipoVoto)
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
                    }
                    // apro
                    a = qryStd.ExecuteReader();
                    if (a.HasRows)
                    {
                        // ok, vuol dire che esistono delle liste
                        while (a.Read())
                        {
                            // creo l'oggetto
                            Lista = new TLista();

                            Lista.NumVotaz = Convert.ToInt32(a["NumVotaz"]);
                            Lista.IDLista = Convert.ToInt32(a["idLista"]);
                            Lista.IDScheda = Convert.ToInt32(a["idScheda"]);
                            // non so se può essere nullo
                            if (a.IsDBNull(a.GetOrdinal("DescrLista"))) Lista.DescrLista = "DESCRIZIONE";
                            else Lista.DescrLista = a.IsDBNull(a.GetOrdinal("DescrLista")) ? "" : a["DescrLista"].ToString();
                            Lista.TipoCarica = Convert.ToInt32(a["TipoCarica"]);
                            
                            // devo verificare se è presentato da cda, e aggiornare la variabile
                            Lista.PresentatodaCDA = Convert.ToBoolean(a["PresentatodaCDA"]);
                            if (Lista.PresentatodaCDA) presCDA++;

                            Lista.Presentatore = a.IsDBNull(a.GetOrdinal("Presentatore")) ? "" : a["Presentatore"].ToString();
                            Lista.Capolista = a.IsDBNull(a.GetOrdinal("Capolista")) ? "" : a["Capolista"].ToString();
                            // non so se è nullo
                            if (a.IsDBNull(a.GetOrdinal("ListaElenco"))) Lista.ListaElenco = "DESCRIZIONE";
                            else Lista.ListaElenco = a.IsDBNull(a.GetOrdinal("ListaElenco")) ? "" : a["ListaElenco"].ToString();
 
                            // aggiungo l'oggetto
                            fVoto[i].Liste.Add(Lista);
                        }

                    }
                    a.Close();
                    fVoto[i].NPresentatoCDA = presCDA;
                    // aggiunge il count alla fine delle liste caricate, conta più questa tabella
                    fVoto[i].NListe = fVoto[i].Liste.Count;
                } // for (int i = 0; i < NVoti; i++)

                result = 0;
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("<dberror> Errore fn CaricaDatiListe: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaDatiListe\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                result = 1;
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }

            // carico i dati
            return result;
        }
*/
        #endregion

		// --------------------------------------------------------------------------
		//  METODI SUI BADGE
		// --------------------------------------------------------------------------

        #region Metodi sui Badge (Presenza, ha già votato...)

        override public bool ControllaBadge(int AIDBadge, TTotemConfig ATotCfg, ref int AReturnFlags)
        {
            // DR15 OK
            // questa procedura effettua in un colpo solo tutti i controlli relativi al badge
            // 1 - Se il badge è annullato
            // 2 - Controlla se è presente e in caso di forzatura mette il movimento
            // 3 - Controlla se ha già votato
            // Il tutto in un unica transazione
            // naturalmente true indica che il controllo è andato a buon fine e può continuare

            SqlDataReader a;
            SqlCommand qryStd;
            SqlTransaction traStd = null;
            bool result, Presente, resCons, BAnnull, BNonEsiste;
            int NumberofRows;

            // testo la connessione
            if (!OpenConnection("ControllaBadge")) return false;

            result = true;
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            // apro una transazione atomica
            // metto sotto try
            try
            {
                traStd = STDBConn.BeginTransaction();
                qryStd.Transaction = traStd;
                
                // -------------------------------------------------
                // ok, ora testo se è annullato
                BAnnull = false;
                qryStd.CommandText = "SELECT Annullato FROM GEAS_Titolari with (NOLOCK) WHERE Badge ='" + AIDBadge.ToString() + "'";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    // annullato non può essere null 
                    a.Read();
                    int ii = Convert.ToInt32(a["Annullato"]);
                    BAnnull = (ii > 0);
                    BNonEsiste = false;
                }
                else
                {
                    BAnnull = true;     // se non ha record non esiste
                    BNonEsiste = true;
                }
                a.Close();

                // -------------------------------------------------
                //  ok ora testo se è presente
                Presente = false;
                qryStd.CommandText = "SELECT TipoMov FROM GEAS_TimbInOut with (NOLOCK) " +
                    "WHERE Badge='" + AIDBadge.ToString() + "' AND GEAS_TimbInOut.Reale=1 " +
                    "AND DataOra=(SELECT MAX(DataOra) FROM GEAS_TimbInOut with (NOLOCK) " +
                    "WHERE Badge='" + AIDBadge.ToString() + "' AND GEAS_TimbInOut.Reale=1)";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    // devo verificare, il campo non può essere null 
                    a.Read();
                    string mv = a["TipoMov"].ToString();
                    Presente = (mv == "E");
                }
                a.Close();
                // se non è annullato e non è presente e il flag è VSDecl.PRES_FORZA_INGRESSO
                // forzo un movimento di ingresso
                if (!BAnnull && !Presente && (ATotCfg.ControllaPresenze == VSDecl.PRES_FORZA_INGRESSO))
                {
                    // forzo il movimento
                    qryStd.CommandText = "insert into Geas_TimbinOut with (ROWLOCK) (" +
                        " DataOra, Badge, TipoMov, Reale, Classe, Terminale, DataIns " +
                        ") values ({ fn NOW() } , '" +
                        AIDBadge.ToString() + "', 'E', 1, 3, " +
                        ATotCfg.Sala.ToString() + ", { fn NOW() })";
                    // eseguo
                    NumberofRows = qryStd.ExecuteNonQuery();
                    Presente = true;
                }
                // qua faccio un elaborazione successsiva in funzione del flag ControllaPresenze
                // per avere un valore assoluto nel confronto finale
                // perché se Presente = true va tutto bene, ma se Presente è a false
                // bisogna testare il flag ControllaPresenze perché nel caso "PRES_NON_CONTROLLARE"
                // è ok lo stesso e bisogna mettere Presente a true x il confronto finale
                if (!Presente && ATotCfg.ControllaPresenze == VSDecl.PRES_NON_CONTROLLARE)
                    Presente = true;

                // -------------------------------------------------
                // ok, ora testo se ha votato

                // modifiche AbilitaDirittiNonVoglioVotare.
                if (ATotCfg.AbilitaDirittiNonVoglioVotare)
                {
                    // se è abilitato il controllo su non voglio votare vuol dire che non salvo in 
                    // vs_votanti_totem, ma controllo i residui diritti di voto
                    // e quindi forza il controllo a false
                    resCons = false;
                }
                else
                {
                    // si comporta normalmente
                    qryStd.CommandText = "SELECT * from VS_ConSchede with (NOLOCK) where Badge = '" + AIDBadge.ToString() + "'";
                    a = qryStd.ExecuteReader();
                    resCons = a.HasRows;
                    a.Close();
                    // se non ha consegnato schede allora verifico se ha già votato
                    if (!resCons)
                    {
                        qryStd.CommandText = "SELECT * from VS_Votanti_Totem with (NOLOCK) where Badge = '" + AIDBadge.ToString() + "'";
                        a = qryStd.ExecuteReader();
                        resCons = a.HasRows;
                        a.Close();
                    }
                }               
                traStd.Commit();

                // ok, ora devo elaborare il risultato che deve essere
                //  BAnnull = false, Presente = true (ma solo se il controllo è attivato), resCons = false
                // naturalmente true indica che il controllo è andato a buon fine e può continuare
                // è un and quindi tutti i valori devono essere a true
                result = (!BAnnull &&  // se non è annullato è a true
                          Presente &&  // è presente
                          !resCons);   // se non ha schede consegnate è a true

                // ok ora compongo il flag degli eventuali errori
                AReturnFlags = 0;
                if (BAnnull)
                {
                    if (BNonEsiste)
                        AReturnFlags = AReturnFlags | 0x40;
                    else
                        AReturnFlags = AReturnFlags | 0x01;
                }
                if (!Presente) AReturnFlags = AReturnFlags | 0x02;
                if (resCons) AReturnFlags = AReturnFlags | 0x04;

            }
            catch (Exception objExc)
            {
                if (traStd != null) traStd.Rollback();
                Logging.WriteToLog("<dberror> Errore fn ControllaBadge : " + AIDBadge.ToString() + " err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione ControllaBadge" + "\n\n" +
                    "Il controllo del Badge non è andato a buon fine.\n\n " +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                result = false;
            }
            finally
            {
                qryStd.Dispose();
                traStd.Dispose();
                CloseConnection("");
            }

            return result;
        }

        override public string DammiNomeAzionista(int AIDBadge)
        {
            // DROK 13
            // mi dice la lunghezza del badge e il codice impianto per il lettore
            SqlDataReader a;
            SqlCommand qryStd;
            string NomeAz = "", Sesso = "";

            // testo la connessione
            if (!OpenConnection("DammiNomeAzionista")) return "";

            qryStd = new SqlCommand();
            try
            {
                qryStd.Connection = STDBConn;
                // Leggo ora da GEAS_Titolari	
                qryStd.CommandText = "select T.badge, T.idazion, A.Sesso, " + 
                                     " CASE WHEN A.FisGiu ='F' THEN A.Cognome+ ' ' + A.Nome ELSE A.Raso END as Raso1 " +
                                     " from geas_titolari T " + 
                                     " INNER JOIN GEAS_Anagrafe As A  with (NOLOCK) ON T.IdAzion = A.IdAzion " + 
                                     " WHERE T.Badge = @Badge AND T.Reale=1";
                qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    // devo verificare 
                    a.Read();
                    NomeAz = a.IsDBNull(a.GetOrdinal("Raso1")) ? "" : (a["Raso1"]).ToString();
                    Sesso = a.IsDBNull(a.GetOrdinal("Sesso")) ? "" : (a["Sesso"]).ToString();
                }
                a.Close();

                if (Sesso == "M")
                    NomeAz = "Sig. " + NomeAz;
                if (Sesso == "F")
                    NomeAz = "Sig.ra " + NomeAz;

            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("<dberror> Errore nella funzione DammiNomeAzionista: " + objExc.Message);
                MessageBox.Show("Errore nella funzione DammiNomeAzionista" + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }

            return NomeAz;
        }


        #endregion

		// --------------------------------------------------------------------------
		//  LETTURA DATI AZIONISTA
		// --------------------------------------------------------------------------

        #region Caricamento dati Azionista

        // *************** OBSOLETO CON I NUOVI OGGETTI *************************
        override public int DammiDatiAzionistaOneShot(int AIDBadge, ref int ANAzionisti, ref ArrayList FAzionisti)
        {
            // DR12 OK
            // ok, questa procedura mi carica tutti i dati
            SqlDataReader a;
            SqlCommand qryStd;
            int result, nazioni, AzO, AzS; //, naz;
            TAzionista c;

            // testo la connessione
            if (!OpenConnection("DammiDatiAzionistaOneShot")) return 0;
                
            result = 0;
            //naz = 0;
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
 			try
			{
                // AGGIUNGO IL TITOLARE
                nazioni = NumAzTitolare(AIDBadge);
                // se ha azioni inserisco i dati sennò è un rapperesentante
                if (nazioni > 0)
                {
                    qryStd.CommandText = qry_DammiDatiAzionista_Titolare;
                    // parameters
                    qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                    // execute
                    a = qryStd.ExecuteReader();
                    if (a.HasRows)   // in teoria non può non avere righe
                    {
                        // devo verificare 
                        a.Read();

                        if (Convert.ToInt32(a["TitIdAzion"]) == -1) // va bene, indica se ha già votato??
                        {
                            c = new TAzionista();
                            if (a.IsDBNull(a.GetOrdinal("CoAz"))) c.CoAz = "0000000";
                            else c.CoAz = a["CoAz"].ToString();
                            c.IDAzion = Convert.ToInt32(a["IdAzion"]);
                            c.IDBadge = AIDBadge;
                            c.ProgDeleg = 0;
                            c.RaSo = a["Raso1"].ToString();
                            c.NAzioni = nazioni;
                            c.Sesso = a.IsDBNull(a.GetOrdinal("Sesso")) ? "N" : a["Sesso"].ToString();
                            FAzionisti.Add(c);
                            //naz = 1;
                        }
                    } // if (a.HasRows)
                    a.Close();
                }
                // ORA I DELEGANTI
                qryStd.Parameters.Clear();
			    qryStd.CommandText = qry_DammiDatiAzionista_Deleganti;
                // parameters
                qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                // execute
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    while (a.Read())
                    {
                        if (Convert.ToInt32(a["ConIDAzion"]) == -1) // va bene, la carico
                        {
                            // carico ma prima testo i badge banana, cioè quelli che hanno azioni 0
                            // in mezzo ai deleganti, possono essere rapp legali di minori
                            if (a.IsDBNull(a.GetOrdinal("AzOrd"))) AzO = 0;
                            else AzO = Convert.ToInt32(a["AzOrd"]);

                            if (a.IsDBNull(a.GetOrdinal("AzStr"))) AzS = 0;
                            else AzS = Convert.ToInt32(a["AzStr"]);
                            // se hanno azioni possso caricarli
                            if ((AzO + AzS) > 0)
                            {
                                // li carico
                                c = new TAzionista();
                                if (a.IsDBNull(a.GetOrdinal("CoAz"))) c.CoAz = "0000000";
                                else c.CoAz = a["CoAz"].ToString();
                                c.IDAzion = Convert.ToInt32(a["IdAzion"]);
                                c.IDBadge = AIDBadge;
                                c.ProgDeleg = Convert.ToInt32(a["ProgDeleg"]);  //ndeleg;
                                c.RaSo = a["Raso1"].ToString();
                                c.NAzioni = AzO;
                                c.Sesso = "N";
                                FAzionisti.Add(c);
                                //naz++;
                            } // if ((AzO + AzS) > 0)
                        }  // if (Convert.ToInt32(a["ConIDAzion"]) == -1)
                    }  // while (a.Read())
                }  // if (a.HasRows)
                a.Close();
                ANAzionisti = FAzionisti.Count;// naz;
                // se arriva alla fine tutto è aposto, altrimenti il risultato è 0
                result = 1;

            }
            catch (Exception objExc)
            {
                result = 0;
                Logging.WriteToLog("Errore fn DammiDatiAzionistaOneShot: " + AIDBadge.ToString() + " err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione DammiDatiAzionista" + "\n\n" +
                    "Il caricamento dei dati azionista non è andato a buon fine.\n\n " +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }

            if (FAzionisti.Count == 0)
                return 0;
            else
                return result;
        }

	    #endregion

        // --------------------------------------------------------------------------
        //  CONTROLLO DELLA VOTAZIONE
        // --------------------------------------------------------------------------

        #region Salvataggio Voti

        override public int SalvaTutto(int AIDBadge, TTotemConfig ATotCfg, ref TListaAzionisti AAzionisti)
        {
            // questa funzione viene chhiamata alla fine della votazione ed effettua le operazioni 
            // IN UN UNICA TRANSAZIONE:
            //
            //  1. un record in VS_Votanti_Totem che indica che il badge ha votato, per il controlo iniziale
            //  2. tanti record quanti sono gli azionisti con azioni > 0 in VS_ConSchede
            //  3. l'arraylist FVotiDaSalvare in VS_Intonse_Totem, i voti veri e propri

            SqlCommand qryStd = null, qryVoti = null;
            SqlTransaction traStd = null;
            int result = 0, NumberofRows;
            int TopRand = VSDecl.MAX_ID_RANDOM;
            Random random;

            // testo la connessione
            if (!OpenConnection("SalvaTutto")) return 0;

            qryStd = new SqlCommand {Connection = STDBConn};
            qryVoti = new SqlCommand { Connection = STDBConn };
            try
            {
                // abilito la transazione
                traStd = STDBConn.BeginTransaction();
                qryStd.Transaction = traStd;
                qryVoti.Transaction = traStd;

                // 1. scrivo che ha votato in VS_Votanti_Totem
                // se non è abilitato il non voto si comporta normalmente, quindi salva in vs_votanti_totem
                if (!ATotCfg.AbilitaDirittiNonVoglioVotare)
                {
                    qryStd.Parameters.Clear();
                    qryStd.CommandText = "insert into VS_Votanti_Totem with (ROWLOCK) " +
                                         " (Badge, idSeggio, DataOraVotaz, NomeComputer) " +
                                         " VALUES " +
                                         " (@Badge, @idSeggio, { fn NOW() }, @NomeComputer)";
                    qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                    qryStd.Parameters.Add("@idSeggio", System.Data.SqlDbType.Int).Value = FIDSeggio;
                    qryStd.Parameters.Add("@NomeComputer", System.Data.SqlDbType.VarChar).Value = NomeTotem;
                                         //") values ('" +
                                         //AIDBadge.ToString() + "'," + FIDSeggio.ToString() + ",{ fn NOW() } , '" + NomeTotem + "')";
                    NumberofRows = qryStd.ExecuteNonQuery();
                }

                // 2. ora scrivo vs_conschede e vs_intonse_totem insieme
                random = new Random();
                foreach (TAzionista az in AAzionisti.Azionisti)
                {
                    // salva solo se ha votato
                    if (az.HaVotato == TListaAzionisti.VOTATO_SESSIONE && !az.HaNonVotato)
                    {
                        // conschede
                        qryStd.Parameters.Clear();
                        qryStd.CommandText = "INSERT INTO VS_ConSchede with (ROWLOCK) " +
                                             " (Badge, NumVotaz, IdAzion, ProgDeleg, IdSeggio, DataOraVotaz, NomeComputer) " +
                                             " VALUES " +
                                             " (@Badge, @NumVotaz, @IdAzion, @ProgDeleg, @IdSeggio, { fn NOW() }, @NomeComputer) ";
                        qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                        qryStd.Parameters.Add("@NumVotaz", System.Data.SqlDbType.Int).Value = az.IDVotaz;
                        qryStd.Parameters.Add("@IdAzion", System.Data.SqlDbType.Int).Value = az.IDAzion;
                        qryStd.Parameters.Add("@ProgDeleg", System.Data.SqlDbType.Int).Value = az.ProgDeleg;
                        qryStd.Parameters.Add("@idSeggio", System.Data.SqlDbType.Int).Value = FIDSeggio;
                        qryStd.Parameters.Add("@NomeComputer", System.Data.SqlDbType.VarChar).Value = NomeTotem;
                        //"VALUES ('" + AIDBadge.ToString() + "'," +
                        //            NVotaz.ToString() + "," + c.IDAzion + "," + c.ProgDeleg +
                        //            "," + FIDSeggio.ToString() + ",{ fn NOW() } , '" + NomeTotem + "')";
                        NumberofRows = qryStd.ExecuteNonQuery();

                        // 
                        foreach (TVotoEspresso vt in az.VotiEspressi)
                        {
                            // intonse_totem, salvo il voto, ma prima devo fare qualche elaborazione
                            // 1. testo se devo togliere il link voto-azionista
                            int AIDBadge_OK = AIDBadge;
                            if (!ATotCfg.SalvaLinkVoto)
                                AIDBadge_OK = random.Next(1, TopRand);

                            // salvo nel db
                            qryVoti.Parameters.Clear();
                            qryVoti.CommandText = "insert into VS_Intonse_Totem  with (rowlock) " +
                                                  " (NumVotaz, idTipoScheda, idSeggio, voti, Badge, ProgDeleg, IdCarica) " +
                                                  " VALUES " +
                                                  " (@NumVotaz, @idTipoScheda, @idSeggio, @voti, @Badge, @ProgDeleg, @IdCarica) ";
                            qryVoti.Parameters.Add("@NumVotaz", System.Data.SqlDbType.Int).Value = az.IDVotaz;
                            qryVoti.Parameters.Add("@idTipoScheda", System.Data.SqlDbType.Int).Value = vt.VotoExp_IDScheda;
                            qryVoti.Parameters.Add("@idSeggio", System.Data.SqlDbType.Int).Value = FIDSeggio;
                            qryVoti.Parameters.Add("@voti", System.Data.SqlDbType.Int).Value = 1;
                            qryVoti.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge_OK.ToString();
                            qryVoti.Parameters.Add("@ProgDeleg", System.Data.SqlDbType.Int).Value = az.ProgDeleg;
                            qryVoti.Parameters.Add("@IdCarica", System.Data.SqlDbType.Int).Value = vt.TipoCarica;
                            NumberofRows = qryVoti.ExecuteNonQuery();
                        }
                    }
                }

                // chiudo la transazione
                traStd.Commit();
                result = 1;
            }
            catch (Exception objExc)
            {
                if (traStd != null) traStd.Rollback();
                result = 0;
                Logging.WriteToLog("<dberror> fn SalvaTutto: " + AIDBadge.ToString() + " err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione SalvaTutto " + "\n\n" +
                    "Impossibile salvare i voti.\n\n " +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                qryVoti.Dispose();
                traStd.Dispose();
                CloseConnection("");
            }
            return result;

            /*
             
            SqlCommand qryStd;
            SqlTransaction traStd = null;
            TVotiDaSalvare v;
            TAzionista c;
            int NumberofRows, i, z, result, NVotaz; // NAzTitolare;
            bool HaNonVoto;


            // testo la connessione
            if (!OpenConnection("SalvaTutto")) return 0;

            result = 0;

            // mi calcolo il totale dei passi
            int ProgTotale = (NVoti * ANAzionisti) + FVotiDaSalvare.Count + 2;
            int ProgPos = 0;
            int TotVoti = FVotiDaSalvare.Count;

            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            try
            {
                // devo cancellarlo
                traStd = STDBConn.BeginTransaction();
                qryStd.Transaction = traStd;

                // se non è abilitato il non voto si comporta normalmente, quindi salva in vs_votanti_totem
                if (!ATotCfg.AbilitaDirittiNonVoglioVotare)
                {
                    // 1. scrivo che ha votato in VS_Votanti_Totem
                    qryStd.CommandType = CommandType.Text;
                    qryStd.CommandText = "insert into VS_Votanti_Totem with (ROWLOCK) (" +
                                         " Badge, idSeggio, DataOraVotaz, NomeComputer" +
                                         ") values ('" +
                                         AIDBadge.ToString() + "'," + FIDSeggio.ToString() + ",{ fn NOW() } , '" + NomeTotem + "')";
                    NumberofRows = qryStd.ExecuteNonQuery();
                    // *****************************************************************
                }
                ProgPos++;

                // 2. ora scrivo in VS_ConSchede
                for (z = 0; z < NVoti; z++)
                {
                    NVotaz = fVot[z].IDVoto;

                    for (i = 0; i < ANAzionisti; i++)
                    {
                        c = (TAzionista)FAzionisti[i];

                        // testo il parametro sul salvataggio dei diritti di voto
                        if (ATotCfg.AbilitaDirittiNonVoglioVotare)
                        {
                            // salva solo se non c'è il non voto
                            HaNonVoto = false;
                            // controlla se nei voti non c'è il non voto

                            // ****************************

                            for (int j = 0; j < FVotiDaSalvare.Count; j++)
                            {
                                v = (TVotiDaSalvare)FVotiDaSalvare[i];
                                if (v.IDazion == c.IDAzion && v.AScheda_2 == VSDecl.VOTO_NONVOTO)
                                    HaNonVoto = true;
                            }
                            if (!HaNonVoto)
                            {
                                // le schede
                                qryStd.CommandText = "INSERT INTO VS_ConSchede with (ROWLOCK) VALUES ('" + AIDBadge.ToString() + "'," +
                                                     NVotaz.ToString() + "," + c.IDAzion + "," + c.ProgDeleg +
                                                     "," + FIDSeggio.ToString() + ",{ fn NOW() } , '" + NomeTotem + "')";
                                NumberofRows = qryStd.ExecuteNonQuery();
                            }                            
                        }
                        else
                        {
                            // si comporta normalmente
                            // le schede
                            qryStd.CommandText = "INSERT INTO VS_ConSchede with (ROWLOCK) VALUES ('" + AIDBadge.ToString() + "'," +
                                                 NVotaz.ToString() + "," + c.IDAzion + "," + c.ProgDeleg +
                                                 "," + FIDSeggio.ToString() + ",{ fn NOW() } , '" + NomeTotem + "')";
                            NumberofRows = qryStd.ExecuteNonQuery();
                        }
                        ProgPos++;
                        // mando l'evento progressivo
                        MandaEventoProgVoto(ProgTotale, ProgPos, TotVoti);
                    }
                }

                // ok, ora i voti
                for (i = 0; i < FVotiDaSalvare.Count; i++)
                {
                    // trovo
                    v = (TVotiDaSalvare)FVotiDaSalvare[i];

                    // *****************************************************************
                    if (ATotCfg.AbilitaDirittiNonVoglioVotare && v.AScheda_2 == VSDecl.VOTO_NONVOTO)
                    {
                        // non salvare
                    }
                    // *****************************************************************
                    else
                    {
                        qryStd.CommandText = "insert into VS_Intonse_Totem  with (rowlock) (" +
                                             " NumVotaz, idTipoScheda, idSeggio, voti, Badge, ProgDeleg, IdCarica" +
                                             ") values (" + " " +
                                             v.NumVotaz_1.ToString() + ", " +
                                             v.AScheda_2.ToString() + ", " +
                                             FIDSeggio.ToString() + ", 1, " +
                                             v.AIDBadge_4.ToString() + ", " +
                                             v.ProgDelega_5.ToString() + ", " +
                                             v.IdCarica_6.ToString() + ")";
                        NumberofRows = qryStd.ExecuteNonQuery();
                    }
                    ProgPos++;
                    // mando l'evento progressivo
                    MandaEventoProgVoto(ProgTotale, ProgPos, TotVoti);
                }

                // chiudo la transazione
                traStd.Commit();
                ProgPos = ProgTotale;
                // mando l'evento progressivo
                MandaEventoProgVoto(ProgTotale, ProgPos, TotVoti);
                result = 1;
            }
            catch (Exception objExc)
            {
                if (traStd != null) traStd.Rollback();
                result = 0;
                MandaEventoProgVoto(ProgTotale, ProgTotale, TotVoti);
                Logging.WriteToLog("<dberror> fn SalvaTutto: " + AIDBadge.ToString() + " err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione SalvaTutto " + "\n\n" +
                    "Impossibile salvare i voti.\n\n " +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                traStd.Dispose();
                CloseConnection("");
            }
            return result;
             */

            return 0;
        }

        //private void MandaEventoProgVoto(int ProgTotale, int ProgVoto, int TotVoti)
        //{
        //    // DR12 OK
        //    try
        //    {
        //        // questa funzione si occupa di mandare un evento alla finestra principale 
        //        // sull'andamento del salvataggio del voto
        //        if (TotVoti > VSDecl.MINVOTI_PROGRESSIVO)
        //        {
        //            float cc = ((float)ProgVoto / (float)10);
        //            // mando l'evento perchè ho diviso per dieci
        //            if ((cc - Math.Truncate(cc)) == 0)
        //                OnProgressoSalvaTutto(this, ProgTotale, ProgVoto);
        //            // mando evento finale
        //            if (ProgVoto == ProgTotale)
        //                OnProgressoSalvaTutto(this, ProgTotale, ProgVoto);
        //        }
        //    }
        //    catch (Exception objExc)
        //    {
        //        System.Diagnostics.Debug.WriteLine(objExc.Message);
        //        // non faccio nulla, stanto non serve, è solo per precauzione
        //    }
        //}

        #endregion

        // --------------------------------------------------------------------------
        //  ALTRE FUNZIONI DELLA VOTAZIONE
        // --------------------------------------------------------------------------

        #region Altre funzioni votazione

        override public int NumAzTitolare(int AIDBadge)
        {
            //  mi da quante azioni ha un titolare
            SqlDataReader ab;
            SqlCommand qryStd1;
            int AzO, AzS, result;

            // testo la connessione
            if (!OpenConnection("NumAzTitolare")) return 0;

            result = 0;
            qryStd1 = new SqlCommand();
            qryStd1.Connection = STDBConn;
            // apro la query
            qryStd1.CommandText = " SELECT A.TipoAssemblea,COALESCE(Azioni1Ord,0)+COALESCE(Azioni2Ord,0) AS AzOrd,COALESCE(Azioni1Str,0)+COALESCE(Azioni2Str,0) AS AzStr " +
                                 " FROM GEAS_Titolari AS T with (nolock), CONFIG_AppoggioR AS A with (nolock) WHERE (T.ValAssem Like '%'+A.TipoAssemblea + '%') " +
                                 " AND T.Badge='" + AIDBadge.ToString() + "' AND T.Reale=1";
            ab = qryStd1.ExecuteReader();
            if (ab.HasRows)
            {
                ab.Read();
                // possono essere nulli
                if (ab.IsDBNull(ab.GetOrdinal("AzOrd"))) AzO = 0;
                else AzO = Convert.ToInt32(ab["AzOrd"]);

                if (ab.IsDBNull(ab.GetOrdinal("AzStr"))) AzS = 0;
                else AzS = Convert.ToInt32(ab["AzStr"]);

                result = AzO + AzS; //Convert.ToInt32( ab["AzOrd"] ) + Convert.ToInt32( ab["AzStr"] );
                ab.Close();
            }
            ab.Close();
            qryStd1.Dispose();

            return result;
        }

        override public int CheckStatoVoto(string NomeTotem)
        {
            //  mi da quante azioni ha un titolare
            SqlDataReader ab;
            SqlCommand qryStd1;
            int result;

            result = 1;
            // 0: chiuso
            // 1: aperto
            // -1 errore

            // testo la connessione
            if (STDBConn.State != ConnectionState.Open)
            {
                try
                {
                    STDBConn.Open();
                }
                catch (Exception objExc)
                {
                    Logging.WriteToLog("<dberror> Errore fn Open CheckStatoVoto err: " + objExc.Message);
                    //MessageBox.Show("Errore nella funzione CheckStatoVoto" + "\n\n" +
                    //    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    result = -1;
                    return -1;
                }
            }
            
            qryStd1 = new SqlCommand();
            qryStd1.Connection = STDBConn;
            try
            {
                // apro la query
                qryStd1.CommandText = "select * from CONFIG_POSTAZIONI_TOTEM with (nolock) where Postazione = '" + NomeTotem + "'";
                ab = qryStd1.ExecuteReader();
                if (ab.HasRows)
                {
                    ab.Read();
                    // possono essere nulli
                    if (ab.IsDBNull(ab.GetOrdinal("VotoAperto")))
                        result = 0;
                    else
                    {
                        bool pippo = Convert.ToBoolean(ab["VotoAperto"]);
                        if (pippo) result = 1; else  result = 0;
                    }
                }
                ab.Close();
            }
            catch (Exception objExc)
            {
                result = -1;
                Logging.WriteToLog("<dberror> fn CheckStatoVoto: " + NomeTotem + " err: " + objExc.Message);
            }
            finally
            {
                qryStd1.Dispose();
#if _DBClose
                if (STDBConn.State == ConnectionState.Open)
                    STDBConn.Close();
#endif
            }

            return result;
        }


        override public bool CancellaBadgeVotazioni(int AIDBadge)
        {
            // questa routine cancella i dati di un badge

            SqlCommand qryStd;
            SqlTransaction traStd;
            int NumberofRows;
            bool result;

            // questa procedura cancella i dati del badge dalle tre tabelle

            // testo la connessione
            if (!OpenConnection("CancellaBadgeVotazioni")) return false;

            result = false;
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            // devo cancellarlo
            traStd = STDBConn.BeginTransaction();
            try
            {
                qryStd.Transaction = traStd;
                qryStd.CommandText = "delete from vs_votanti_totem with (ROWLOCK) where badge = " + AIDBadge.ToString();
                NumberofRows = qryStd.ExecuteNonQuery();

                qryStd.CommandText = "delete from vs_intonse_totem with (ROWLOCK) where badge = " + AIDBadge.ToString();
                NumberofRows = qryStd.ExecuteNonQuery();

                qryStd.CommandText = "delete from vs_ConSchede with (ROWLOCK) where badge = " + AIDBadge.ToString();
                NumberofRows = qryStd.ExecuteNonQuery();
                traStd.Commit();
                result = true;
                //
                MessageBox.Show("I Voti sono stati cancellati", "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception objExc)
            {
                traStd.Rollback();
                result = false;
                MessageBox.Show("Errore nella funzione CancellaBadgeVotazioni, badge: " + AIDBadge.ToString() + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                traStd.Dispose();
#if _DBClose
                STDBConn.Close();
#endif
            }
            return result;
        }

        override public Boolean CancellaTuttiVoti()
        {
            // questa routine cancella tutti i voti

            SqlCommand qryStd;
            SqlTransaction traStd;
            int NumberofRows;
            bool result;

            // testo la connessione
            if (STDBConn.State != ConnectionState.Open) STDBConn.Open();

            result = false;
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            // devo cancellarlo
            traStd = STDBConn.BeginTransaction();
            try
            {
                qryStd.Transaction = traStd;
                qryStd.CommandText = "delete from vs_votanti_totem with (ROWLOCK)";
                NumberofRows = qryStd.ExecuteNonQuery();

                qryStd.CommandText = "delete from vs_intonse_totem with (ROWLOCK) ";
                NumberofRows = qryStd.ExecuteNonQuery();

                qryStd.CommandText = "delete from vs_ConSchede with (ROWLOCK) ";
                NumberofRows = qryStd.ExecuteNonQuery();
                traStd.Commit();
                result = true;
                //
                MessageBox.Show("TUTTI I Voti sono stati cancellati", "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception objExc)
            {
                traStd.Rollback();
                result = false;
                MessageBox.Show("Errore nella funzione CancellaTuttiVoti" + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            qryStd.Dispose();
            traStd.Dispose();
#if _DBClose
            STDBConn.Close();
#endif
            return result;
        }

        #endregion

        // --------------------------------------------------------------
		//  METODI DI CONFIGURAZIONE
		// --------------------------------------------------------------
		
		// carica la configurazione 
        override public Boolean CaricaConfig()
		{
            //DR11 OK
			string ss = "", GeasFileName = "";

            // verifica se è locale oppure no
            if (ADataLocal)
            {
                if (File.Exists(AData_path + "geas.sql"))
                    GeasFileName = AData_path + "geas.sql";
                else
                    return false;
            }
            else
            {
                if (Directory.Exists(DriveM) && File.Exists(DriveM + "geas.sql"))
                    GeasFileName = DriveM + "geas.sql";
                else
                    return false;
            }

            // leggo cosa c'è dentro
            try
            {               
                StreamReader file1 = File.OpenText(GeasFileName);
                ss = file1.ReadLine();
                // testo se il file è giusto
                if (ss.IndexOf("GEAS") >= 0)
                //if (ss == "GEAS 2000 -- Stringa Connesione a SQL")
                {
                    // tutto ok leggo
                    FDBConfig.DB_Type = file1.ReadLine();
                    FDBConfig.DB_Dsn = file1.ReadLine();
                    FDBConfig.DB_Name = file1.ReadLine();
                    FDBConfig.DB_Uid = file1.ReadLine();
                    FDBConfig.DB_Pwd = file1.ReadLine();
                    FDBConfig.DB_Server = file1.ReadLine();
                    FDBConfig.DB_ConfigOK = true;
                    file1.Close();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                Logging.WriteToLog("<dberror> fn CaricaConfig: " + NomeTotem + " err: " + e.Message);
                return false;
            }		

        }


	}
}
