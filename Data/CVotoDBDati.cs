using System;
using System.Collections.Generic;
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
//		VER		: 4.0 
//		DATE	: Mar 2016
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
	    private string qry_DammiDirittiDiVoto_Titolare = "";
	    private string qry_DammiDirittiDiVoto_Deleganti = "";
        private string qry_DammiVotazioniTotem = "";

        public CVotoDBDati(ConfigDbData AFDBConfig, string ANomeTotem, Boolean AADataLocal, string AAData_path) : 
            base(AFDBConfig, ANomeTotem, AADataLocal, AAData_path)
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
            qry_DammiDirittiDiVoto_Titolare = getModelsQueryProcedure("DammiDirittiDiVoto_Titolare.sql");
            qry_DammiDirittiDiVoto_Deleganti = getModelsQueryProcedure("DammiDirittiDiVoto_Deleganti.sql");
            qry_DammiVotazioniTotem = getModelsQueryProcedure("DammiVotazioniTotem.sql");
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
            // apro la connessione se � chiusa
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

        override public int CaricaConfigDB(ref int ABadgeLen, ref string ACodImpianto)
        {
            // DROK 13
            // mi dice la lunghezza del badge e il codice impianto per il lettore
            SqlDataReader a;
            SqlCommand qryStd;
            int Tok;

            // testo la connessione
            if (!OpenConnection("CaricaConfigDB")) return 0;

            ABadgeLen = 8;
            ACodImpianto = "00";
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

                    ABadgeLen = a.IsDBNull(a.GetOrdinal("LenNumBadge")) ? 8 : Convert.ToInt32(a["LenNumBadge"]);
                    ACodImpianto = a.IsDBNull(a.GetOrdinal("CodImpRea")) ? "00" : (a["CodImpRea"]).ToString();
                }
                else
                {
                    ABadgeLen = 8;
                    ACodImpianto = "00";
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

        override public int DammiConfigTotem(string ANomeTotem) //, ref TTotemConfig TotCfg)
        {
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
            // registra il totem aggiungendo il record in CONFIG_POSTAZIONI, e chiaramente verifica che ci sia gi�
            qryStd.CommandText = "select * from CONFIG_POSTAZIONI_TOTEM with (nolock) where Postazione = '" + ANomeTotem + "'";
            
            traStd = STDBConn.BeginTransaction();
            qryStd.Transaction = traStd;
            inserisci = false;

            try
            {
                a = qryStd.ExecuteReader();
                // se c'� il record
                if (a.HasRows)
                {
                    // devo verificare 
                    a.Read();
                    // carico
                    VTConfig.Postazione = a["Postazione"].ToString();
                    // faccio un  ulteriore controllo
                    if (ANomeTotem != VTConfig.Postazione) VTConfig.Postazione = ANomeTotem;

                    VTConfig.Descrizione = a.IsDBNull(a.GetOrdinal("Descrizione")) ? ANomeTotem : a["Descrizione"].ToString();
                    VTConfig.IDSeggio = Convert.ToInt32(a["IdSeggio"]);
                    FIDSeggio = Convert.ToInt32(a["IdSeggio"]);

                    VTConfig.Attivo = Convert.ToBoolean(a["Attivo"]);
                    VTConfig.VotoAperto = Convert.ToBoolean(a["VotoAperto"]);

                    VTConfig.UsaSemaforo = Convert.ToBoolean(a["UsaSemaforo"]);
                    VTConfig.IP_Com_Semaforo = a["IPCOMSemaforo"].ToString();
                    VTConfig.TipoSemaforo = Convert.ToInt32(a["TipoSemaforo"]);

                    VTConfig.UsaLettore = Convert.ToBoolean(a["UsaLettore"]);
                    VTConfig.PortaLettore = Convert.ToInt32(a["PortaLettore"]);
                    VTConfig.CodiceUscita = a["CodiceUscita"].ToString();

                    VTConfig.Sala = a.IsDBNull(a.GetOrdinal("Sala")) ? 1 : Convert.ToInt32(a["Sala"]);
                }
                else
                    inserisci = true;
                // chiudo
                a.Close();

                // ok, se inserisci � true, vuol dire che non ha trovato record e devo inserirlo
                if (inserisci)
                {
                    // non c'� configurazione, devo inserirla
                    qryStd.CommandText = "INSERT into CONFIG_POSTAZIONI_TOTEM " +
                        "(Postazione, Descrizione, IdSeggio, Attivo, VotoAperto, UsaSemaforo, "+
                        " IPCOMSemaforo, TipoSemaforo, UsaLettore, PortaLettore, CodiceUscita, " +
                        " UsaController, IPController, Sala) " +
                        " VALUES ('" + ANomeTotem + "', 'Desc_" + ANomeTotem + "', 999, 1, 0, 0, " +
                        "'127.0.0.1', 2, 0, 1, '999999', 0, '127.0.0.1', 1)";
                    
                    // metto in quadro i valori
                    VTConfig.Postazione = ANomeTotem;
                    VTConfig.Descrizione = ANomeTotem;
                    VTConfig.IDSeggio = 999;
                    FIDSeggio = 999;
                    VTConfig.Attivo = true;
                    VTConfig.VotoAperto = false;
                    VTConfig.UsaSemaforo = false;
                    VTConfig.IP_Com_Semaforo = "127.0.0.1";
                    VTConfig.UsaLettore = false;
                    VTConfig.PortaLettore = 1;
                    VTConfig.CodiceUscita = "999999";
                    VTConfig.Sala = 1;
                    // parte come semaforo com per facilitare gli esterni,
                    // poi bisogner� fare un wizard di configurazione
                    VTConfig.TipoSemaforo = VSDecl.SEMAFORO_COM;
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

        override public int DammiConfigDatabase() //ref TTotemConfig TotCfg)
        {
            SqlDataReader a;
            SqlCommand qryStd;
            int result = 0;

            // testo la connessione
            if (!OpenConnection("DammiConfigDatabase")) return 0;

            // preparo gli oggetti
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
            // la configurazione ci deve essere, non � necessario inserirla
            qryStd.CommandText = "select * from CONFIG_CfgVotoSegreto with (nolock) where attivo = 1";

            try
            {
                a = qryStd.ExecuteReader();
                // se c'� il record
                if (a.HasRows)
                {
                    // devo verificare 
                    a.Read();
                    // carico
                    VTConfig.ModoAssemblea = Convert.ToInt32(a["ModoAssemblea"]);
                    // il link del voto
                    VTConfig.SalvaLinkVoto = Convert.ToBoolean(a["SalvaLinkVoto"]);
                    // il salvataggio del voto anche se non ha confermato
                    VTConfig.SalvaVotoNonConfermato = Convert.ToBoolean(a["SalvaVotoNonConfermato"]);
                    // l'id della scheda che deve essere salvata in caso di 999999
                    VTConfig.IDSchedaUscitaForzata = Convert.ToInt32(a["IDSchedaUscitaForzata"]);
                    // ModoPosizioneAreeTouch
                    VTConfig.ModoPosizioneAreeTouch = Convert.ToInt32(a["ModoPosizioneAreeTouch"]);
                    // controllo delle presenze
                    VTConfig.ControllaPresenze = Convert.ToInt32(a["ControllaPresenze"]);
                    // AbilitaBottoneUscita
                    VTConfig.AbilitaBottoneUscita = Convert.ToBoolean(a["AttivaAutoRitornoVoto"]);
                    // AttivaAutoRitornoVoto
                    VTConfig.AttivaAutoRitornoVoto = Convert.ToBoolean(a["AttivaAutoRitornoVoto"]);
                    // TimeAutoRitornoVoto
                    VTConfig.TimeAutoRitornoVoto = Convert.ToInt32(a["TimeAutoRitornoVoto"]);
                    // AbilitaDirittiNonVoglioVotare
                    VTConfig.AbilitaDirittiNonVoglioVotare = Convert.ToBoolean(a["AbilitaDirittiNonVoglioVotare"]);

                    // qua dovrei in teoria controllare che vada bene
                    // prima faccio un piccolo controllo, se � un valore a c..., metto scheda bianca che c'� sempre
                    //if (IDSchedaForz == VSDecl.VOTO_SCHEDABIANCA || IDSchedaForz == VSDecl.VOTO_NONVOTO)
                    //    TotCfg.IDSchedaUscitaForzata = IDSchedaForz;
                    //else
                    //    TotCfg.IDSchedaUscitaForzata = VSDecl.VOTO_SCHEDABIANCA;
                    // ok, ora il tasto ricomincia da capo
                    //TotCfg.TastoRicominciaDaCapo = Convert.ToBoolean(a["TastoRicominciaDaCapo"]);
                }
                //else
                //{
                //    VTConfig.SalvaLinkVoto = true;
                //    VTConfig.SalvaVotoNonConfermato = false;
                //    VTConfig.IDSchedaUscitaForzata = VSDecl.VOTO_NONVOTO;
                //    VTConfig.ModoPosizioneAreeTouch = VSDecl.MODO_POS_TOUCH_NORMALE;
                //    VTConfig.ControllaPresenze = VSDecl.PRES_CONTROLLA;
                //    VTConfig.AbilitaBottoneUscita = false;
                //    VTConfig.AttivaAutoRitornoVoto = false;
                //    VTConfig.TimeAutoRitornoVoto = VSDecl.TIME_AUTOCLOSEVOTO;
                //    VTConfig.AbilitaDirittiNonVoglioVotare = false;
                //}
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
        
        override public int SalvaConfigurazione(string ANomeTotem) //, ref TTotemConfig ATotCfg)
        {
            SqlCommand qryStd;
            SqlTransaction traStd;
            int NumberofRows, result;
            short usal, usas;

            // testo la connessione
            if (!OpenConnection("SalvaConfigurazione")) return 0;

            result = 0;
            // preparo gli oggetti
            if (VTConfig.UsaLettore) usal = 1; else usal = 0;
            if (VTConfig.UsaSemaforo) usas = 1; else usas = 0;
            qryStd = new SqlCommand();
            qryStd.Connection = STDBConn;
             // devo inserirlo
            traStd = STDBConn.BeginTransaction();
            try
            {
                qryStd.Transaction = traStd;
                qryStd.CommandText = "update CONFIG_POSTAZIONI_TOTEM with (rowlock) set " +
                        "  UsaLettore = " + usal.ToString() +
                        ", PortaLettore = " + VTConfig.PortaLettore.ToString() +
                        ", UsaSemaforo = " + usas.ToString() +
                        ", IPCOMSemaforo = '" + VTConfig.IP_Com_Semaforo + "'" +
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

        override public bool CaricaVotazioniDaDatabase(ref List<TNewVotazione> AVotazioni)
        {
            SqlDataReader a = null;
            SqlCommand qryStd = null;
            TNewVotazione v;
            bool result = false; //, naz;

            // testo la connessione
            if (!OpenConnection("CaricaVotazioniDaDatabase")) return false;

            AVotazioni.Clear();

            qryStd = new SqlCommand { Connection = STDBConn };
            try
            {
                // ok ora carico le votazioni
                qryStd.Parameters.Clear();
                qryStd.CommandText = qry_DammiVotazioniTotem;
                //qryStd.CommandText =   "SELECT * from VS_MatchVot_Totem with (NOLOCK)  where GruppoVotaz < 999 order by NumVotaz";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    while (a.Read())
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
                            SkContrarioTutte = Convert.ToBoolean(a["SchedaContrarioTutte"]),
                            SkAstenutoTutte = Convert.ToBoolean(a["SchedaAstenutoTutte"]),
                            SelezionaTuttiCDA = Convert.ToBoolean(a["SelezTuttiCDA"]),
                            //PreIntermezzo = Convert.ToBoolean(a["PreIntermezzo"]),
                            MaxScelte = a.IsDBNull(a.GetOrdinal("MaxScelte")) ? 1 : Convert.ToInt32(a["MaxScelte"]),
                            AbilitaBottoneUscita = Convert.ToBoolean(a["AbilitaBottoneUscita"])
                        };
                        AVotazioni.Add(v);
                        // nota: esisteva nella vecchia versione voto e subvoto, ora tolti, il codice era
                        //  // se � maggiore di 9 e minore di 99 contiene voto e subvoto
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
                CloseConnection("");
            }
            return result;
        }

        override public bool CaricaListeDaDatabase(ref List<TNewVotazione> AVotazioni)
        {
            SqlDataReader a = null;
            SqlCommand qryStd = null;
            TNewLista l;
            bool result = false; //, naz;

            // testo la connessione
            if (!OpenConnection("CaricaVotazioniDaDatabase")) return false;

            qryStd = new SqlCommand { Connection = STDBConn };
            try
            {
                // TODO: CaricaListeDaDatabase da vedere in futuro di fare un solo ciclo di caricamento senza ordine
                // ciclo sulle votazioni e carico le liste
                foreach (TNewVotazione votaz in AVotazioni)
                {
                    // ok ora carico le votazioni
                    qryStd.Parameters.Clear();
                    qryStd.CommandText = "SELECT * from VS_Liste_Totem with (NOLOCK) " +
                                         "where NumVotaz = @IDVoto and Attivo = 1 ";
                    // ecco, in funzione del tipo di voto
                    switch (votaz.TipoVoto)
                    {
                        // se � lista ordino per l'id
                        case VSDecl.VOTO_LISTA:
                            qryStd.CommandText += " order by idlista";
                            break;
                        // se � candidato ordino in modo alfabetico
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
                CloseConnection("");
            }
            return result;
        }
        
        // --------------------------------------------------------------------------
		//  METODI SUI BADGE
		// --------------------------------------------------------------------------

        #region Metodi sui Badge (Presenza, ha gi� votato...)

        //override public bool ControllaBadge(int AIDBadge, TTotemConfig ATotCfg, ref int AReturnFlags)
        override public bool ControllaBadge(int AIDBadge, ref int AReturnFlags)
        {
            // questa procedura effettua in un colpo solo tutti i controlli relativi al badge
            // 1 - Se il badge � annullato
            // 2 - Controlla se � presente e in caso di forzatura mette il movimento
            // 3 - Controlla se ha gi� votato
            // Il tutto in un unica transazione
            // naturalmente true indica che il controllo � andato a buon fine e pu� continuare

            SqlDataReader a;
            SqlCommand qryStd;
            SqlTransaction traStd = null;
            bool result, Presente, resCons, BAnnull, BNonEsiste;

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
                // ok, ora testo se � annullato
                BAnnull = false;
                qryStd.CommandText = "SELECT Annullato FROM GEAS_Titolari with (NOLOCK) WHERE Badge ='" + AIDBadge.ToString() + "'";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    // annullato non pu� essere null 
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
                //  ok ora testo se � presente
                Presente = false;
                qryStd.CommandText = "SELECT TipoMov FROM GEAS_TimbInOut with (NOLOCK) " +
                    "WHERE Badge='" + AIDBadge.ToString() + "' AND GEAS_TimbInOut.Reale=1 " +
                    "AND DataOra=(SELECT MAX(DataOra) FROM GEAS_TimbInOut with (NOLOCK) " +
                    "WHERE Badge='" + AIDBadge.ToString() + "' AND GEAS_TimbInOut.Reale=1)";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    // devo verificare, il campo non pu� essere null 
                    a.Read();
                    string mv = a["TipoMov"].ToString();
                    Presente = (mv == "E");
                }
                a.Close();
                // se non � annullato e non � presente e il flag � VSDecl.PRES_FORZA_INGRESSO
                // forzo un movimento di ingresso
                if (!BAnnull && !Presente && (VTConfig.ControllaPresenze == VSDecl.PRES_FORZA_INGRESSO))
                {
                    // forzo il movimento
                    qryStd.CommandText = "insert into Geas_TimbinOut with (ROWLOCK) (" +
                        " DataOra, Badge, TipoMov, Reale, Classe, Terminale, DataIns " +
                        ") values ({ fn NOW() } , '" +
                        AIDBadge.ToString() + "', 'E', 1, 3, " +
                        VTConfig.Sala.ToString() + ", { fn NOW() })";
                    // eseguo
                    qryStd.ExecuteNonQuery();
                    Presente = true;
                }
                // qua faccio un elaborazione successsiva in funzione del flag ControllaPresenze
                // per avere un valore assoluto nel confronto finale
                // perch� se Presente = true va tutto bene, ma se Presente � a false
                // bisogna testare il flag ControllaPresenze perch� nel caso "PRES_NON_CONTROLLARE"
                // � ok lo stesso e bisogna mettere Presente a true x il confronto finale
                if (!Presente && VTConfig.ControllaPresenze == VSDecl.PRES_NON_CONTROLLARE)
                    Presente = true;

                // -------------------------------------------------
                // ok, ora testo se ha votato

                // modifiche AbilitaDirittiNonVoglioVotare.
                if (VTConfig.AbilitaDirittiNonVoglioVotare)
                {
                    // se � abilitato il controllo su non voglio votare vuol dire che non salvo in 
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
                    // se non ha consegnato schede allora verifico se ha gi� votato
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
                //  BAnnull = false, Presente = true (ma solo se il controllo � attivato), resCons = false
                // naturalmente true indica che il controllo � andato a buon fine e pu� continuare
                // � un and quindi tutti i valori devono essere a true
                result = (!BAnnull &&  // se non � annullato � a true
                          Presente &&  // � presente
                          !resCons);   // se non ha schede consegnate � a true

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
                    "Il controllo del Badge non � andato a buon fine.\n\n " +
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

        //override public string DammiNomeAzionista(int AIDBadge)
        //{
        //    // mi dice la lunghezza del badge e il codice impianto per il lettore
        //    SqlDataReader a;
        //    SqlCommand qryStd;
        //    string NomeAz = "", Sesso = "";

        //    // testo la connessione
        //    if (!OpenConnection("DammiNomeAzionista")) return "";

        //    qryStd = new SqlCommand();
        //    try
        //    {
        //        qryStd.Connection = STDBConn;
        //        // Leggo ora da GEAS_Titolari	
        //        qryStd.CommandText = "select T.badge, T.idazion, A.Sesso, " + 
        //                             " CASE WHEN A.FisGiu ='F' THEN A.Cognome+ ' ' + A.Nome ELSE A.Raso END as Raso1 " +
        //                             " from geas_titolari T " + 
        //                             " INNER JOIN GEAS_Anagrafe As A  with (NOLOCK) ON T.IdAzion = A.IdAzion " + 
        //                             " WHERE T.Badge = @Badge AND T.Reale=1";
        //        qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
        //        a = qryStd.ExecuteReader();
        //        if (a.HasRows)
        //        {
        //            // devo verificare 
        //            a.Read();
        //            NomeAz = a.IsDBNull(a.GetOrdinal("Raso1")) ? "" : (a["Raso1"]).ToString();
        //            Sesso = a.IsDBNull(a.GetOrdinal("Sesso")) ? "" : (a["Sesso"]).ToString();
        //        }
        //        a.Close();

        //        if (Sesso == "M")
        //            NomeAz = "Sig. " + NomeAz;
        //        if (Sesso == "F")
        //            NomeAz = "Sig.ra " + NomeAz;

        //    }
        //    catch (Exception objExc)
        //    {
        //        Logging.WriteToLog("<dberror> Errore nella funzione DammiNomeAzionista: " + objExc.Message);
        //        MessageBox.Show("Errore nella funzione DammiNomeAzionista" + "\n" +
        //            "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        qryStd.Dispose();
        //        CloseConnection("");
        //    }

        //    return NomeAz;
        //}

        #endregion

		// --------------------------------------------------------------------------
		//  LETTURA DATI AZIONISTA
		// --------------------------------------------------------------------------

        #region Caricamento dati Azionista 

        override public bool CaricaDirittidiVotoDaDatabase(int AIDBadge, ref List<TAzionista> AAzionisti,
                                                                ref TAzionista ATitolare_badge, ref TListaVotazioni AVotazioni)
        {
            // ok, questa funziomne carica i diritti di voto in funzione
            // del idbadge, in pratica alla fine avr� una lista di diritti *per ogni votazione*
            // con l'indicazione se sono stati gi� espressi o no

            // ok, questa procedura mi carica tutti i dati
            //SqlConnection STDBConn = null;
            SqlDataReader a = null;
            SqlCommand qryStd = null;
            TAzionista c;
            int IDVotazione = -1;
            bool result = false; //, naz;

            // testo la connessione
            if (!OpenConnection("CaricaDirittidiVotoDaDatabase")) return false;

            AAzionisti.Clear();

            qryStd = new SqlCommand { Connection = STDBConn };
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
                    // in teoria non pu� non avere righe, testa anche se ha azioni, se no � un rappr
                    if (a.HasRows && a.Read())
                    {
                        c = new TAzionista
                        {
                            CoAz = a.IsDBNull(a.GetOrdinal("CoAz")) ? "0000000" : a["CoAz"].ToString(),
                            IDAzion = Convert.ToInt32(a["IdAzion"]),
                            IDBadge = AIDBadge,
                            ProgDeleg = 0,
                            RaSo = a["Raso1"].ToString(),
                            NAzioni = Convert.ToDouble(a["AzOrd"]),
                            Sesso = a.IsDBNull(a.GetOrdinal("Sesso")) ? "N" : a["Sesso"].ToString(),
                            HaVotato = Convert.ToInt32(a["TitIDVotaz"]) >= 0 ? TListaAzionisti.VOTATO_DBASE : TListaAzionisti.VOTATO_NO,
                            IDVotaz = IDVotazione
                        };

                        // ok, ora se � titolare e ha azioni l'aggiungo alla lista
                        if ((Convert.ToInt32(a["AzOrd"]) + Convert.ToInt32(a["AzStr"])) > 0)
                            AAzionisti.Add(c);

                        // poi lo salvo come titolare
                        ATitolare_badge.CopyFrom(ref c);
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
                        while (a.Read())        // qua posso avere pi� righe
                        {
                            // anche qua devo testare se ha azioni 0, potrebbe essere un badge banana
                            if ((Convert.ToInt32(a["AzOrd"]) + Convert.ToInt32(a["AzStr"])) > 0)
                            {
                                c = new TAzionista
                                {
                                    CoAz = a.IsDBNull(a.GetOrdinal("CoAz")) ? "0000000" : a["CoAz"].ToString(),
                                    IDAzion = Convert.ToInt32(a["IdAzion"]),
                                    IDBadge = AIDBadge,
                                    ProgDeleg = Convert.ToInt32(a["ProgDeleg"]),
                                    RaSo = a["Raso1"].ToString(),
                                    NAzioni = Convert.ToInt32(a["AzOrd"]),
                                    Sesso = "N",
                                    HaVotato = Convert.ToInt32(a["ConIDVotaz"]) >= 0 ? TListaAzionisti.VOTATO_DBASE : TListaAzionisti.VOTATO_NO,
                                    IDVotaz = IDVotazione
                                };
                                AAzionisti.Add(c);
                            }
                        }   //while (a.Read()) 
                    }   //if (a.HasRows)
                    a.Close();

                }   //for (int i = 0...
                result = true;

            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("Errore fn CaricaDirittidiVotoDaDatabaseDBDATI: " + AIDBadge.ToString() + " err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaDirittidiVotoDaDatabaseDBDATI" + "\n\n" +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }
            return result;
        }

	    #endregion

        // --------------------------------------------------------------------------
        //  CONTROLLO DELLA VOTAZIONE
        // --------------------------------------------------------------------------

        #region Salvataggio Voti

        //override public int SalvaTutto(int AIDBadge, TTotemConfig ATotCfg, ref TListaAzionisti AAzionisti)
        override public int SalvaTutto(int AIDBadge, ref TListaAzionisti AAzionisti)
        {
            // questa funzione viene chhiamata alla fine della votazione ed effettua le operazioni 
            // IN UN UNICA TRANSAZIONE:
            //
            //  1. un record in VS_Votanti_Totem che indica che il badge ha votato, per il controlo iniziale
            //  2. tanti record quanti sono gli azionisti con azioni > 0 in VS_ConSchede
            //  3. l'arraylist FVotiDaSalvare in VS_Intonse_Totem, i voti veri e propri

            SqlCommand qryStd = null, qryVoti = null;
            SqlTransaction traStd = null;
            int result = 0; int TopRand = VSDecl.MAX_ID_RANDOM;
            double PNAzioni = 0;
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
                // se non � abilitato il non voto si comporta normalmente, quindi salva in vs_votanti_totem
                if (!VTConfig.AbilitaDirittiNonVoglioVotare)
                {
                    qryStd.Parameters.Clear();
                    qryStd.CommandText = "insert into VS_Votanti_Totem with (ROWLOCK) " +
                                         " (Badge, idSeggio, DataOraVotaz, NomeComputer) " +
                                         " VALUES " +
                                         " (@Badge, @idSeggio, { fn NOW() }, @NomeComputer)";
                    qryStd.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge.ToString();
                    qryStd.Parameters.Add("@idSeggio", System.Data.SqlDbType.Int).Value = FIDSeggio;
                    qryStd.Parameters.Add("@NomeComputer", System.Data.SqlDbType.VarChar).Value = NomeTotem;
                    qryStd.ExecuteNonQuery();
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
                        qryStd.ExecuteNonQuery();

                        // 
                        foreach (TVotoEspresso vt in az.VotiEspressi)
                        {
                            // intonse_totem, salvo il voto, ma prima devo fare qualche elaborazione
                            // 1. testo se devo togliere il link voto-azionista
                            int AIDBadge_OK = AIDBadge;
                            if (!VTConfig.SalvaLinkVoto)
                                AIDBadge_OK = random.Next(1, TopRand);

                            if (VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP)
                                PNAzioni = 1;
                            else
                                PNAzioni = az.NAzioni;

                            // salvo nel db
                            qryVoti.Parameters.Clear();
                            qryVoti.CommandText = "insert into VS_Intonse_Totem  with (rowlock) " +
                                                  " (NumVotaz, idTipoScheda, idSeggio, voti, Badge, ProgDeleg, IdCarica) " +
                                                  " VALUES " +
                                                  " (@NumVotaz, @idTipoScheda, @idSeggio, @voti, @Badge, @ProgDeleg, @IdCarica) ";
                            qryVoti.Parameters.Add("@NumVotaz", System.Data.SqlDbType.Int).Value = az.IDVotaz;
                            qryVoti.Parameters.Add("@idTipoScheda", System.Data.SqlDbType.Int).Value = vt.VotoExp_IDScheda;
                            qryVoti.Parameters.Add("@idSeggio", System.Data.SqlDbType.Int).Value = FIDSeggio;
                            qryVoti.Parameters.Add("@voti", System.Data.SqlDbType.Float).Value = PNAzioni;
                            qryVoti.Parameters.Add("@Badge", System.Data.SqlDbType.VarChar).Value = AIDBadge_OK.ToString();
                            qryVoti.Parameters.Add("@ProgDeleg", System.Data.SqlDbType.Int).Value = az.ProgDeleg;
                            qryVoti.Parameters.Add("@IdCarica", System.Data.SqlDbType.Int).Value = vt.TipoCarica;
                            qryVoti.ExecuteNonQuery();
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
        }

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

        override public int CheckStatoVoto(string ANomeTotem)
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
                qryStd1.CommandText = "select * from CONFIG_POSTAZIONI_TOTEM with (nolock) where Postazione = '" + ANomeTotem + "'";
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
                Logging.WriteToLog("<dberror> fn CheckStatoVoto: " + ANomeTotem + " err: " + objExc.Message);
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

            // verifica se � locale oppure no
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

            // leggo cosa c'� dentro
            try
            {               
                StreamReader file1 = File.OpenText(GeasFileName);
                ss = file1.ReadLine();
                // testo se il file � giusto
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

        // --------------------------------------------------------------------------
        //  METODI Di TEST
        // --------------------------------------------------------------------------

        override public bool DammiTuttiIBadgeValidi(ref ArrayList badgelist)
        {
            if (badgelist == null) return false;

            SqlDataReader a;
            SqlCommand qryStd;
            string bdg = "0";
 
            badgelist.Clear();

            // testo la connessione
            if (!OpenConnection("DammiTuttiIBadgeValidi")) return false;

            qryStd = new SqlCommand();
            try
            {
                qryStd.Connection = STDBConn;
                // Leggo ora da GEAS_Titolari	
                qryStd.CommandText = "select T.badge, T.idazion from geas_titolari T  where T.Reale=1 and T.Annullato = 0 order by Badge";
                a = qryStd.ExecuteReader();
                if (a.HasRows)
                {
                    while (a.Read()) // qua posso avere pi� righe
                    {
                        bdg = a.IsDBNull(a.GetOrdinal("Badge")) ? "" : (a["Badge"]).ToString();
                        badgelist.Add(bdg);
                    }
                }
                a.Close();
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("<dberror> Errore nella funzione DammiTuttiIBadgeValidi: " + objExc.Message);
                MessageBox.Show("Errore nella funzione DammiTuttiIBadgeValidi" + "\n" +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                CloseConnection("");
            }

            return true;
        }

	}
}
