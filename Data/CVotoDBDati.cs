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
                        " IPCOMSemaforo, TipoSemaforo, UsaLettore, PortaLettore, CodiceUscita, " +
                        " UsaController, IPController, Sala) " +
                        " VALUES ('" + NomeTotem + "', 'Desc_" + NomeTotem + "', 999, 1, 0, 0, " +
                        "'127.0.0.1', 2, 0, 1, '999999', 0, '127.0.0.1', 1)";
                    
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
            qryStd.CommandText = "select * from CONFIG_CfgVotoSegreto with (nolock) where attivo = 1";

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
                    TotCfg.IDSchedaUscitaForzata = Convert.ToInt32(a["IDSchedaUscitaForzata"]);
                    // ModoPosizioneAreeTouch
                    TotCfg.ModoPosizioneAreeTouch = Convert.ToInt32(a["ModoPosizioneAreeTouch"]);
                    // controllo delle presenze
                    TotCfg.ControllaPresenze = Convert.ToInt32(a["ControllaPresenze"]);
                    // AbilitaBottoneUscita
                    TotCfg.AbilitaBottoneUscita = Convert.ToBoolean(a["AttivaAutoRitornoVoto"]);
                    // AttivaAutoRitornoVoto
                    TotCfg.AttivaAutoRitornoVoto = Convert.ToBoolean(a["AttivaAutoRitornoVoto"]);
                    // TimeAutoRitornoVoto
                    TotCfg.TimeAutoRitornoVoto = Convert.ToInt32(a["TimeAutoRitornoVoto"]);
                    // AbilitaDirittiNonVoglioVotare
                    TotCfg.AbilitaDirittiNonVoglioVotare = Convert.ToBoolean(a["AbilitaDirittiNonVoglioVotare"]);


                    // qua dovrei in teoria controllare che vada bene
                    // prima faccio un piccolo controllo, se è un valore a c..., metto scheda bianca che c'è sempre
                    //if (IDSchedaForz == VSDecl.VOTO_SCHEDABIANCA || IDSchedaForz == VSDecl.VOTO_NONVOTO)
                    //    TotCfg.IDSchedaUscitaForzata = IDSchedaForz;
                    //else
                    //    TotCfg.IDSchedaUscitaForzata = VSDecl.VOTO_SCHEDABIANCA;
                    // ok, ora il tasto ricomincia da capo
                    //TotCfg.TastoRicominciaDaCapo = Convert.ToBoolean(a["TastoRicominciaDaCapo"]);
                }
                else
                {
                    TotCfg.SalvaLinkVoto = true;
                    TotCfg.SalvaVotoNonConfermato = false;
                    TotCfg.IDSchedaUscitaForzata = VSDecl.VOTO_NONVOTO;
                    TotCfg.ModoPosizioneAreeTouch = VSDecl.MODO_POS_TOUCH_NORMALE;
                    TotCfg.ControllaPresenze = VSDecl.PRES_CONTROLLA;
                    TotCfg.AbilitaBottoneUscita = false;
                    TotCfg.AttivaAutoRitornoVoto = false;
                    TotCfg.TimeAutoRitornoVoto = VSDecl.TIME_AUTOCLOSEVOTO;
                    TotCfg.AbilitaDirittiNonVoglioVotare = false;
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
		//  METODI SUI BADGE
		// --------------------------------------------------------------------------

        #region Metodi sui Badge (Presenza, ha già votato...)

        override public bool ControllaBadge(int AIDBadge, TTotemConfig ATotCfg, ref int AReturnFlags)
        {
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

        #region Caricamento dati Azionista **** OBSOLETO CON I NUOVI OGGETTI *****

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
