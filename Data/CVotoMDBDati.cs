using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
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

        override public bool CaricaVotazioniDaDatabase(ref List<TNewVotazione> AVotazioni)
        {
            OleDbConnection conn = null;
            OleDbCommand qryStd = null;
            OleDbDataReader a = null;
            TNewVotazione v;
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
                            AbilitaBottoneUscita = VTConfig.AbilitaBottoneUscita
                        };
                        AVotazioni.Add(v);
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

        override public bool CaricaListeDaDatabase(ref List<TNewVotazione> AVotazioni)
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
                foreach (TNewVotazione votaz in AVotazioni)
                {
                    // ok ora carico le votazioni
                    qryStd.Parameters.Clear();
                    qryStd.CommandText = "SELECT * from VS_Liste_Totem  " +
                                         "where NumVotaz = @IDVoto and Attivo = true ";

                    // todo: occhio all'ordine dell'idlista che luca lo usa per i suoi calcoli, non è meglio idscheda?
                    // ecco, in funzione del tipo di voto
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
                    qryStd.Parameters.AddWithValue("@IDVoto", votaz.IDVoto); // System.Data.SqlDbType.Int).Value = votaz.IDVoto;
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
           
            
            
            
            /*é
            DataTable dt = new DataTable();
            TNewLista Lista;
            int presCDA;
            string ASort;

            dt.ReadXml(AData_path + "VS_Liste_Totem.xml");
            ASort = "idlista desc";
            // cicla lungo le votazioni e carica le liste
            foreach (TNewVotazione votaz in AVotazioni)
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

            return true;
             */
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

        override public bool CaricaDirittidiVotoDaDatabase(int AIDBadge, ref List<TAzionista> AAzionisti,
                                                  ref TAzionista ATitolare_badge, ref TListaVotazioni AVotazioni)
        {
            int IDVotazione = -1;
            AAzionisti.Clear();
            TAzionista a;

            foreach (TNewVotazione voto in AVotazioni.Votazioni)
            {
                IDVotazione = voto.IDVoto;
                // un voto
                if (AIDBadge == 1000)
                {
                    a = new TAzionista
                        {
                            CoAz = "10000",
                            IDAzion = 10000,
                            IDBadge = 1000,
                            ProgDeleg = 0,
                            RaSo = "Mario Rossi",
                            Sesso = "M",
                            NAzioni = 1,
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
                    a = new TAzionista
                        {
                            CoAz = "10001",
                            IDAzion = 10001,
                            IDBadge = 1001,
                            ProgDeleg = 0,
                            RaSo = "Mario Rossi",
                            Sesso = "M",
                            NAzioni = 1,
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
                            NAzioni = 1,
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
                            NAzioni = 1,
                            Sesso = "M",
                            RaSo = "Mario Rossi - Delega 2",
                            IDVotaz = IDVotazione,
                            HaVotato = TListaAzionisti.VOTATO_NO
                        };
                    AAzionisti.Add(a);
                }
            }

            return true;
        }

        override public int SalvaTutto(int AIDBadge, ref TListaAzionisti FAzionisti)
        {
            return 0;
        }

        override public int NumAzTitolare(int AIDBadge)
        {
            return 0;
        }

        override public int CheckStatoVoto(string ANomeTotem)
        {
            return 1;
        }

        override public bool CancellaBadgeVotazioni(int AIDBadge)
        {
            return true;
        }

        override public Boolean CancellaTuttiVoti()
        {
            return true;
        }

        #endregion

        // --------------------------------------------------------------------------
        //  REGISTRAZIONE NEL DATABASE
        // --------------------------------------------------------------------------

        override public int RegistraTotem(string ANomeTotem)
        {
            return 0;
        }

        override public int UnregistraTotem(string ANomeTotem)
        {
            return 0;
        }

        // --------------------------------------------------------------
        //  METODI PRIVATI
        // --------------------------------------------------------------

        override public string DammiStringaConnessione()
        {
            return "";
        }

        // --------------------------------------------------------------
        //  METODI DI CONFIGURAZIONE
        // --------------------------------------------------------------

        // carica la configurazione 
        override public Boolean CaricaConfig()
        {
            return true;
        }

        // --------------------------------------------------------------------------
        //  METODI Di TEST
        // --------------------------------------------------------------------------

        override public bool DammiTuttiIBadgeValidi(ref ArrayList badgelist)
        {

            return true;
        }

    }

}
