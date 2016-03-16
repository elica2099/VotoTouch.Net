using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace VotoTouch
{

    public class CVotoFileDati : CVotoBaseDati
    {

        public CVotoFileDati()		
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

        override public int DammiConfigTotem(string ANomeTotem) //, ref TTotemConfig TotCfg)
        {
            VTConfig.Postazione = NomeTotem;
            // faccio un  ulteriore controllo
            VTConfig.IDSeggio = 99;
            FIDSeggio = 99;
            VTConfig.Attivo = true;
            VTConfig.VotoAperto = true;
            VTConfig.ControllaPresenze = 1;
            VTConfig.UsaSemaforo = false;
            VTConfig.IP_Com_Semaforo = "127.0.0.1";
            VTConfig.TipoSemaforo = 1;
            VTConfig.SalvaLinkVoto = true;
            VTConfig.SalvaVotoNonConfermato = true;
            VTConfig.IDSchedaUscitaForzata = VSDecl.VOTO_SCHEDABIANCA;            
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
            VTConfig.SalvaLinkVoto = true;
            VTConfig.SalvaVotoNonConfermato = true;
            VTConfig.IDSchedaUscitaForzata = VSDecl.VOTO_SCHEDABIANCA;
            //TotCfg.TastoRicominciaDaCapo = false;
            //TotCfg.AbilitaLogV = true;
            return 0;
        }

        override public int SalvaConfigurazione(string ANomeTotem) //, ref TTotemConfig ATotCfg)
        {
            return 0;
        }

        #endregion

        // --------------------------------------------------------------------------
        //  CARICAMENTO DATI VOTAZIONI
        // --------------------------------------------------------------------------
/*
        override public int CaricaDatiVotazioni(ref int NVoti, ref TVotazione[] fVoto)
        {

            int z;
            DataTable dt = new DataTable();

            dt.ReadXml(AData_path + "VS_MatchVot_Totem.xml");

            NVoti = dt.Rows.Count;
            fVoto = new TVotazione[NVoti];

            z = 0;
            foreach (DataRow riga in dt.Rows)
            {
                fVoto[z].IDVoto = Convert.ToInt32(riga["NumVotaz"]);
                fVoto[z].IDGruppoVoto = Convert.ToInt32(riga["GruppoVotaz"]);
                fVoto[z].Descrizione = riga["Argomento"].ToString();
                fVoto[z].TipoVoto = Convert.ToInt32(riga["TipoVotaz"]);
                fVoto[z].TipoSubVoto = 0;
                fVoto[z].NListe = Convert.ToInt32(riga["NListe"]);
                fVoto[z].SkBianca = Convert.ToBoolean(riga["SchedaBianca"]);
                fVoto[z].SkNonVoto = Convert.ToBoolean(riga["SchedaNonVoto"]);
                fVoto[z].MaxScelte = Convert.ToInt32(riga["MaxScelte"]);
                fVoto[z].SelezionaTuttiCDA = Convert.ToBoolean(riga["SelezTuttiCDA"]);
                fVoto[z].PreIntermezzo = false;
                fVoto[z].Liste = new ArrayList();
                fVoto[z].Pagine = new ArrayList();
                z++;

            }

            dt.Dispose();

            return 0;
        }

        override public int CaricaDatiListe(ref int NVoti, ref TVotazione[] fVoto)
        {
            DataTable dt = new DataTable();
            TLista Lista;
            int presCDA;
            string ASort;

            dt.ReadXml(AData_path + "VS_Liste_Totem.xml");
            ASort = "idlista desc";
            // cicla lungo le votazioni e carica le liste
            for (int i = 0; i < NVoti; i++)
            {
                // faccio un sorting delle liste
                switch (fVoto[i].TipoVoto)
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
                    fVoto[i].IDVoto.ToString(), ASort ))
                {
                    Lista = new TLista();
                    Lista.NumVotaz = Convert.ToInt32(riga["NumVotaz"]);
                    Lista.IDLista = Convert.ToInt32(riga["idLista"]);
                    Lista.IDScheda = Convert.ToInt32(riga["idScheda"]);
                    Lista.DescrLista = riga["DescrLista"].ToString();
                    Lista.TipoCarica = Convert.ToInt32(riga["TipoCarica"]);
                    Lista.PresentatodaCDA = Convert.ToBoolean(riga["PresentatodaCDA"]);
                    if (Lista.PresentatodaCDA) presCDA++;
                    Lista.Presentatore = riga["Presentatore"].ToString();
                    Lista.Capolista = riga["Capolista"].ToString();
                    Lista.ListaElenco = riga["ListaElenco"].ToString();

                    // aggiungo
                    fVoto[i].Liste.Add(Lista);
                }
                fVoto[i].NPresentatoCDA = presCDA;
                fVoto[i].NListe = fVoto[i].Liste.Count;

            }

            dt.Dispose();

            return 0;
        }

 * */
        // --------------------------------------------------------------------------
        //  METODI SUI BADGE
        // --------------------------------------------------------------------------

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

		// --------------------------------------------------------------------------
		//  LETTURA DATI AZIONISTA
		// --------------------------------------------------------------------------

        override public int DammiDatiAzionistaOneShot(int AIDBadge, ref int ANAzionisti, ref ArrayList FAzionisti)
        {
            TAzionista a;

            // un voto
            if (AIDBadge == 1000)
            {
                ANAzionisti = 1;
                a = new TAzionista();
                a.CoAz = "10000";
                a.IDAzion = 10000;
                a.IDBadge = 1000;
                a.ProgDeleg = 0;
                a.RaSo = "Mario Rossi";
                a.Sesso = "M";
                FAzionisti.Add(a);
            }
            // tre voti
            if (AIDBadge == 1001)
            {
                ANAzionisti = 3;

                a = new TAzionista();
                a.CoAz = "10001";
                a.IDAzion = 10001;
                a.IDBadge = 1001;
                a.ProgDeleg = 0;
                a.RaSo = "Mario Rossi";
                a.Sesso = "M";
                FAzionisti.Add(a);

                a = new TAzionista();
                a.CoAz = "10002";
                a.IDAzion = 10002;
                a.IDBadge = 1001;
                a.ProgDeleg = 1;
                a.RaSo = "Mario Rossi - Delega 1";
                FAzionisti.Add(a);
 
                a = new TAzionista();
                a.CoAz = "10003";
                a.IDAzion = 10003;
                a.IDBadge = 1003;
                a.ProgDeleg = 0;
                a.RaSo = "Mario Rossi - Delega 2";
                FAzionisti.Add(a);

            }

            return 1;
        }

        override public string DammiNomeAzionista(int AIDBadge)
        {
            return "Mario Rossi";
        }

        // --------------------------------------------------------------------------
        //  CONTROLLO DELLA VOTAZIONE
        // --------------------------------------------------------------------------

//        override public int SalvaTutto(int AIDBadge, TTotemConfig ATotCfg, ref TListaAzionisti FAzionisti)
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
