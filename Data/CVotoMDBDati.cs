using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;


namespace VotoTouch
{

    public class CVotoMDBDati : CVotoBaseDati
    {

        public CVotoMDBDati(ConfigDbData AFDBConfig, Boolean AADataLocal, string AAData_path) :
            base(AFDBConfig, AADataLocal, AAData_path)
        {
            //
        }

        // --------------------------------------------------------------------------
        //  METODI DATABASE
        // --------------------------------------------------------------------------

        override public object DBConnect()
        {
            return this;
        }

        override public object DBDisconnect()
        {
            return this;
        }

        // --------------------------------------------------------------------------
        //  LETTURA CONFIGURAZIONE NEL DATABASE
        // --------------------------------------------------------------------------

        #region Lettura/Scrittura Configurazione

        override public int CaricaConfigDB(ref int ABadgeLen, ref string ACodImpianto)
        {
            ABadgeLen = 8;
            ACodImpianto = "00";
            return 0;
        }

        override public int DammiConfigTotem() //, ref TTotemConfig TotCfg)
        {
            VTConfig.Postazione = VTConfig.NomeTotem;
            // faccio un  ulteriore controllo
            VTConfig.IDSeggio = 99;
            FIDSeggio = 99;
            VTConfig.Attivo = true;
            VTConfig.VotoAperto = true;
            //VTConfig.ControllaPresenze = 1;
            VTConfig.UsaSemaforo = false;
            VTConfig.IP_Com_Semaforo = "127.0.0.1";
            VTConfig.TipoSemaforo = 1;
            //VTConfig.SalvaLinkVoto = true;
            //VTConfig.SalvaVotoNonConfermato = true;
            //VTConfig.IDSchedaUscitaForzata = VSDecl.VOTO_SCHEDABIANCA;
            //TotCfg.UsaSemaforo = true;
            //TotCfg.IP_Com_Semaforo = "10.178.6.16";
            //TotCfg.IP_Com_Semaforo = "192.168.0.32";
            //TotCfg.UsaSemaforo = true;
            //TotCfg.IP_Com_Semaforo = "COM3";           
            //TotCfg.TipoSemaforo = 2;
            VTConfig.UsaLettore = false;
            VTConfig.PortaLettore = 0;
            VTConfig.CodiceUscita = "999999";
            //TotCfg.UsaController = false;
            //TotCfg.IPController = "127.0.0.1";
            return 0;
        }

        override public int DammiConfigDatabase() //ref TTotemConfig TotCfg)
        {
            OleDbConnection conn = null;
            OleDbCommand qryStd = null;
            OleDbDataReader a = null;
            int result = 0;

            string source = AData_path + "DemoVotoTouchData.mdb";
            // create the connection 
            conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Mode=Read;Data Source=" + source);

            // create the command
            qryStd = new OleDbCommand
                {
                    Connection = conn,
                    CommandText = "select * from CONFIG_CfgVotoSegreto where attivo = true"
                };
            try
            {
                // open the connection
                conn.Open();
                // open the query
                a = qryStd.ExecuteReader();
                if (a != null && a.HasRows)
                {
                    while (a.Read())
                    {
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
                    }
                    a.Close();
                }
                result = 0;
            }
            catch (Exception objExc)
            {
                result = 1;
                Logging.WriteToLog("<dberror> Errore nella funzione DammiConfigDatabase: " + objExc.Message);
#if DEBUG
                MessageBox.Show("Errore nella funzione DammiConfigDatabase" + "\n" + "Eccezione : \n" + objExc.Message, "Error");
#endif
            }
            finally
            {
                qryStd.Dispose();
                conn.Close();
            }

            return result;
        }

        override public int SalvaConfigurazione() //, ref TTotemConfig ATotCfg)
        {
            return 0;
        }

        override public int SalvaConfigurazionePistolaBarcode() //, ref TTotemConfig ATotCfg)
        {
            return 0;
        }

        #endregion

        // --------------------------------------------------------------------------
        //  CARICAMENTO DATI VOTAZIONI
        // --------------------------------------------------------------------------

        #region CARICAMENTO DATI VOTAZIONI

        public override bool CaricaVotazioniDaDatabase(ref List<TMainVotazione> AVotazioni)
        {
            OleDbConnection conn = null;
            OleDbCommand qryStd = null;
            OleDbDataReader a = null;
            bool result = false;

            string source = AData_path + "DemoVotoTouchData.mdb";
            // create the connection 
            conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Mode=Read;Data Source=" + source);

            // create the command
            qryStd = new OleDbCommand
                {
                    Connection = conn,
                    CommandText = "select * from VS_MatchVot_Totem where GruppoVotaz < 999 order by NumVotaz"
                };
            try
            {
                // open the connection
                conn.Open();
                // open the query
                a = qryStd.ExecuteReader();
                if (a != null && a.HasRows)
                {
                    while (a.Read())
                    {
                        // verifica se la votazione appartiene a un gruppo
                        int idgr = Convert.ToInt32(a["GruppoVotaz"]);

                        // leggo i campi in TVotazione comunque, per non farlo due volte
                        TVotazione votaz = new TVotazione
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
                            MaxScelte = a.IsDBNull(a.GetOrdinal("MaxScelte")) ? 1 : Convert.ToInt32(a["MaxScelte"]),
                            AbilitaBottoneUscita = VTConfig.AbilitaBottoneUscita
                        };
                        // testo se il gruppo è 0, cioè un voto normale
                        if (idgr == 0)
                        {
                            // se il gruppo è 0 mi comporto normalmente
                            TMainVotazione v = new TMainVotazione
                            {
                                ModoVoto = VSDecl.MODO_VOTO_NORMALE,
                                IDGruppoVoto = 0,
                            };
                            v.lvot.Add(votaz);
                            AVotazioni.Add(v);
                        }
                        else
                        {
                            // se il gruppo è > 0 vuol dire che la votazione fa parte di un gruppo
                            // per prima cosa, testo nella lista se il gruppo esiste, se si aggiungo la votazione a questo gruppo
                            // se no, devo crearlo all'interno di TMainVotazione e aggiungerlo alla lista
                            TMainVotazione gvot = AVotazioni.First(v => v.IDGruppoVoto == idgr);
                            if (gvot != null)
                            {
                                // devo aggiungerlo
                                gvot.lvot.Add(votaz);
                            }
                            else
                            {
                                // devo creare 
                                TMainVotazione vg = new TMainVotazione();
                                vg.ModoVoto = VSDecl.MODO_VOTO_GRUPPO;
                                vg.IDGruppoVoto = idgr;
                                vg.lvot.Add(votaz);
                                AVotazioni.Add(vg);
                            }
                        }
                    }
                    a.Close();
                }
                result = true;
            }
            catch (Exception objExc)
            {
                result = false;
                Logging.WriteToLog("<dberror> Errore nella funzione DammiConfigDatabaseMDB: " + objExc.Message);
#if DEBUG
                MessageBox.Show("Errore nella funzione DammiConfigDatabaseMDB" + "\n" + "Eccezione : \n" + objExc.Message, "Error");
#endif
            }
            finally
            {
                qryStd.Dispose();
                conn.Close();
            }

            return result;
        }

        public override bool CaricaListeDaDatabase(ref List<TMainVotazione> AVotazioni)
        {
            OleDbConnection conn = null;
            OleDbCommand qryStd = null;
            OleDbDataReader a = null;
            TNewLista l;
            bool result = false; //, naz;

            string source = AData_path + "DemoVotoTouchData.mdb";
            // create the connection 
            conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Mode=Read;Data Source=" + source);

            qryStd = new OleDbCommand {Connection = conn};
            try
            {
                // open the connection
                conn.Open();
                // ciclo sulle votazioni e carico le liste
                foreach (TMainVotazione mvotaz in AVotazioni)
                {
                    foreach (TVotazione votazione in mvotaz.lvot)
                    {
                        // ok ora carico le votazioni
                        qryStd.Parameters.Clear();
                        qryStd.CommandText = "SELECT * from VS_Liste_Totem  " +
                                             "where NumVotaz = @IDVoto and Attivo = true ";

                        // todo: occhio all'ordine dell'idlista che luca lo usa per i suoi calcoli, non è meglio idscheda?
                        // ecco, in funzione del tipo di voto
                        switch (votazione.TipoVoto)
                        {
                            // se è lista ordino per l'id
                            case VSDecl.VOTO_LISTA:
                                qryStd.CommandText += " order by idlista";
                                break;
                            // se è candidato ordino in modo alfabetico
                            case VSDecl.VOTO_CANDIDATO:
                            case VSDecl.VOTO_CANDIDATO_SING:
                            case VSDecl.VOTO_MULTICANDIDATO:
                                qryStd.CommandText +=
                                    " order by PresentatoDaCdA desc, OrdineCarica, DescrLista "; //DescrLista ";
                                break;
                            default:
                                qryStd.CommandText += " order by idlista";
                                break;
                        }
                        qryStd.Parameters.AddWithValue("@IDVoto",
                            votazione.IDVoto); // System.Data.SqlDbType.Int).Value = votaz.IDVoto;
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
                                    DescrLista = a.IsDBNull(a.GetOrdinal("DescrLista"))
                                        ? "DESCRIZIONE"
                                        : a["DescrLista"].ToString(),
                                    TipoCarica = Convert.ToInt32(a["TipoCarica"]),
                                    PresentatodaCDA = Convert.ToBoolean(a["PresentatodaCDA"]),
                                    Presentatore = a.IsDBNull(a.GetOrdinal("Presentatore"))
                                        ? ""
                                        : a["Presentatore"].ToString(),
                                    Capolista = a.IsDBNull(a.GetOrdinal("Capolista")) ? "" : a["Capolista"].ToString(),
                                    ListaElenco = a.IsDBNull(a.GetOrdinal("ListaElenco"))
                                        ? "DESCRIZIONE"
                                        : a["ListaElenco"].ToString()
                                };
                                votazione.Liste.Add(l);
                            }
                        }
                        a.Close();
                    }
                }
                result = true;
            }
            catch (Exception objExc)
            {
                Logging.WriteToLog("Errore fn CaricaListeDaDatabaseMDB: err: " + objExc.Message);
                MessageBox.Show("Errore nella funzione CaricaListeDaDatabaseMDB" + "\n\n" +
                    "Chiamare operatore esterno.\n\n " +
                    "Eccezione : \n" + objExc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                qryStd.Dispose();
                conn.Close();
            }
            return result;
           
        }

        #endregion

        // --------------------------------------------------------------------------
        //  METODI SUI BADGE
        // --------------------------------------------------------------------------

        #region METODI SUI BADGE
        //        override public bool ControllaBadge(int AIDBadge, TTotemConfig TotCfg, ref int AReturnFlags)
        override public bool ControllaBadge(int AIDBadge, ref int AReturnFlags)
        {
            AReturnFlags = 0;
            return true;
        }

        override public bool BadgeAnnullato(int AIDBadge)
        {
            return false;
        }

        override public bool BadgePresente(int AIDBadge, bool ForzaTimbr)
        {
            return true;
        }

        override public bool BadgeHaGiaVotato(int AIDBadge)
        {
            return false;
        }

        override public bool HaVotato(int ANVotaz, int AIDBadge, int ProgDelega)
        {
            return false;
        }

        #endregion

        // --------------------------------------------------------------------------
        //  LETTURA DATI AZIONISTA
        // --------------------------------------------------------------------------

        #region METODI SUI BADGE

        public override bool CaricaDirittidiVotoDaDatabase(int AIDBadge, ref List<TAzionista> AAzionisti,
                                                  ref TAzionista ATitolare_badge, ref TListaVotazioni AVotazioni)
        {
            AAzionisti.Clear();

            foreach (TMainVotazione mvotaz in AVotazioni.Votazioni)
            {
                foreach (TVotazione votazione in mvotaz.lvot)
                {
                    int IDVotazione = votazione.IDVoto;
                    // un voto
                    if (AIDBadge == 1000)
                    {
                        TAzionista a = new TAzionista
                        {
                            CoAz = "10000",
                            IDAzion = 10000,
                            IDBadge = 1000,
                            ProgDeleg = 0,
                            RaSo = "Mario Rossi",
                            Sesso = "M",
                            NVoti = 1,
                            IDVotaz = IDVotazione,
                            HaVotato = TListaAzionisti.VOTATO_NO
                        };
                        AAzionisti.Add(a);
                        // poi lo salvo come titolare
                        ATitolare_badge.CopyFrom(ref a);
                    }
                    // tre voti
                    if (AIDBadge == 1001)
                    {
                        TAzionista a = new TAzionista
                        {
                            CoAz = "10001",
                            IDAzion = 10001,
                            IDBadge = 1001,
                            ProgDeleg = 0,
                            RaSo = "Mario Rossi",
                            Sesso = "M",
                            NVoti = 1,
                            IDVotaz = IDVotazione,
                            HaVotato = TListaAzionisti.VOTATO_NO
                        };
                        AAzionisti.Add(a);
                        // poi lo salvo come titolare
                        ATitolare_badge.CopyFrom(ref a);

                        a = new TAzionista
                        {
                            CoAz = "10002",
                            IDAzion = 10002,
                            IDBadge = 1001,
                            ProgDeleg = 1,
                            Sesso = "M",
                            RaSo = "Mario Rossi - Delega 1",
                            NVoti = 1,
                            IDVotaz = IDVotazione,
                            HaVotato = TListaAzionisti.VOTATO_NO
                        };
                        AAzionisti.Add(a);

                        a = new TAzionista
                        {
                            CoAz = "10003",
                            IDAzion = 10003,
                            IDBadge = 1003,
                            ProgDeleg = 0,
                            NVoti = 1,
                            Sesso = "M",
                            RaSo = "Mario Rossi - Delega 2",
                            IDVotaz = IDVotazione,
                            HaVotato = TListaAzionisti.VOTATO_NO
                        };
                        AAzionisti.Add(a);
                    }
                }
            }

            return true;
        }

        public override int SalvaTutto(int AIDBadge, ref TListaAzionisti FAzionisti)
        {
            return 0;
        }

        public override int SalvaTuttoInGeas(int AIDBadge, ref TListaAzionisti AAzionisti)
        {
            return 0;
        }

        public override int NumAzTitolare(int AIDBadge)
        {
            return 0;
        }

        public override int CheckStatoVoto(string ANomeTotem)
        {
            return 1;
        }

        public override bool CancellaBadgeVotazioni(int AIDBadge)
        {
            return true;
        }

        public override Boolean CancellaTuttiVoti()
        {
            return true;
        }

        #endregion

        // --------------------------------------------------------------------------
        //  REGISTRAZIONE NEL DATABASE
        // --------------------------------------------------------------------------

        public override int RegistraTotem(string ANomeTotem)
        {
            return 0;
        }

        public override int UnregistraTotem(string ANomeTotem)
        {
            return 0;
        }

        // --------------------------------------------------------------
        //  METODI PRIVATI
        // --------------------------------------------------------------

        public override string DammiStringaConnessione()
        {
            return "";
        }

        // --------------------------------------------------------------
        //  METODI DI CONFIGURAZIONE
        // --------------------------------------------------------------

        // carica la configurazione 
        public override Boolean CaricaConfig()
        {
            return true;
        }

        // --------------------------------------------------------------------------
        //  METODI Di TEST
        // --------------------------------------------------------------------------

        public override bool DammiTuttiIBadgeValidi(ref ArrayList badgelist)
        {

            return true;
        }

    }

}
