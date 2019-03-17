using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace VotoTouch
{

    public class CVotoFileDati : CVotoBaseDati
    {

        public CVotoFileDati(ConfigDbData AFDBConfig, Boolean AADataLocal, string AAData_path) : 
            base(AFDBConfig, AADataLocal, AAData_path)
        {
            //
        }

        // --------------------------------------------------------------------------
        //  METODI DATABASE
        // --------------------------------------------------------------------------

        public override object DBConnect()
        {
            return this;
        }

        public override object DBDisconnect()
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
            VTConfig.ValAssemblea = "O";
            VTConfig.AbilitaBottoneUscita = true;
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
            VTConfig.MaxDeleghe = 1000;

            VTConfig.ContrarioATutti = "Contrario";
            VTConfig.AstenutoATutti = "Astenuto";

            //TotCfg.UsaController = false;
            //TotCfg.IPController = "127.0.0.1";
            return 0; 
            //VTConfig.Postazione = VTConfig.NomeTotem;
            //// faccio un  ulteriore controllo
            //VTConfig.IDSeggio = 99;
            //FIDSeggio = 99;
            //VTConfig.Attivo = true;
            //VTConfig.VotoAperto = true;
            //VTConfig.ControllaPresenze = 1;
            //VTConfig.UsaSemaforo = false;
            //VTConfig.IP_Com_Semaforo = "127.0.0.1";
            //VTConfig.TipoSemaforo = 1;
            //VTConfig.SalvaLinkVoto = true;
            //VTConfig.SalvaVotoNonConfermato = true;
            //VTConfig.IDSchedaUscitaForzata = VSDecl.VOTO_SCHEDABIANCA;            
            ////TotCfg.UsaSemaforo = true;
            ////TotCfg.IP_Com_Semaforo = "10.178.6.16";
            ////TotCfg.IP_Com_Semaforo = "192.168.0.32";
            ////TotCfg.UsaSemaforo = true;
            ////TotCfg.IP_Com_Semaforo = "COM3";           
            ////TotCfg.TipoSemaforo = 2;
            //VTConfig.UsaLettore = false;
            //VTConfig.PortaLettore = 0;
            //VTConfig.CodiceUscita = "999999";
            ////TotCfg.UsaController = false;
            ////TotCfg.IPController = "127.0.0.1";
            //return 0;
        }

        override public int DammiConfigDatabase() //ref TTotemConfig TotCfg)
        {
            DataTable dt = new DataTable();

            dt.ReadXml(AData_path + "CONFIG_CfgVotoSegreto.xml");

            VTConfig.CodImpianto = "78";

            foreach (DataRow a in dt.Rows)
            {
                VTConfig.ModoAssemblea = Convert.ToInt32(a["ModoAssemblea"]);
                // il link del voto
                VTConfig.SalvaLinkVoto = Convert.ToBoolean(a["SalvaLinkVoto"]);
                // il salvataggio del voto anche se non ha confermato
                VTConfig.SalvaVotoNonConfermato = Convert.ToBoolean(a["SalvaVotoNonConfermato"]);
                // l'id della scheda che deve essere salvata in caso di 999999
                VTConfig.IDSchedaUscitaForzata = Convert.ToInt32(a["IDSchedaUscitaForzata"]);
                // ModoPosizioneAreeTouch
                VTConfig.ModoPosizioneAreeTouch = Convert.ToInt32(a["ModoPosizioneAreeTouch"]); ;
                // controllo delle presenze
                VTConfig.ControllaPresenze = Convert.ToInt32(a["ControllaPresenze"]); 
                // AbilitaBottoneUscita
                VTConfig.AbilitaBottoneUscita = Convert.ToBoolean(a["AbilitaBottoneUscita"]);
                // AttivaAutoRitornoVoto
                VTConfig.AttivaAutoRitornoVoto = Convert.ToBoolean(a["AttivaAutoRitornoVoto"]);
                // TimeAutoRitornoVoto
                VTConfig.TimeAutoRitornoVoto = Convert.ToInt32(a["TimeAutoRitornoVoto"]); ; ;
                // AbilitaDirittiNonVoglioVotare
                VTConfig.AbilitaDirittiNonVoglioVotare = Convert.ToBoolean(a["AbilitaDirittiNonVoglioVotare"]); ;
            }
            return 0;
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
            //int z;
            DataTable dt = new DataTable();

            dt.ReadXml(AData_path + "VS_MatchVot_Totem.xml");

            foreach (DataRow a in dt.Rows)
            {
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
                    //PreIntermezzo = false,
                    MaxScelte = Convert.ToInt32(a["MaxScelte"]),
                    AbilitaBottoneUscita = Convert.ToBoolean(a["AbilitaBottoneUscita"])
                };
                int idgr = votaz.IDGruppoVoto;
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
            dt.Dispose();

            return true;
        }

        public override bool CaricaListeDaDatabase(ref List<TMainVotazione> AVotazioni)
        {
            DataTable dt = new DataTable();

            dt.ReadXml(AData_path + "VS_Liste_Totem.xml");
            string ASort = "idlista asc";
            // cicla lungo le votazioni e carica le liste
            foreach (TMainVotazione mvotaz in AVotazioni)
            {
                foreach (TVotazione votazione in mvotaz.lvot)
                {
                    // faccio un sorting delle liste
                    switch (votazione.TipoVoto)
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

                    foreach (DataRow riga in dt.Select("NumVotaz = " +
                                                       votazione.IDVoto.ToString(), ASort))
                    {
                        TNewLista Lista = new TNewLista
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
                        votazione.Liste.Add(Lista);
                    }
                }
            }

            dt.Dispose();

            return true;
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

        #region CaricaDirittidiVotoDaDatabase

        public override bool CaricaDirittidiVotoDaDatabase(int AIDBadge, ref List<TAzionista> AAzionisti,
                                                  ref TAzionista ATitolare_badge, ref TListaVotazioni AVotazioni)
        {
            AAzionisti.Clear();
            TAzionista a;

            foreach (TMainVotazione mvotaz in AVotazioni.Votazioni)
            {
                foreach (TVotazione votazione in mvotaz.lvot)
                {
                    int IDVotazione = votazione.IDVoto;
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
                            NVoti = VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP ? 1 : 10000,
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
                            NVoti = VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP ? 1 : 5000,
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
                            NVoti = VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP ? 1 : 300,
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
                            NVoti = VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP ? 1 : 1500,
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

    }

}
