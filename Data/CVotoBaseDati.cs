using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.IO;

namespace VotoTouch
{

    //public delegate void ehProgressoSalvaTutto(object source, int ATot, int AProg);
    
    public class CVotoBaseDati
    {
        //public event ehProgressoSalvaTutto ProgressoSalvaTutto;

        public ConfigDbData FDBConfig;
        public Boolean FConnesso;
        public int FIDSeggio;
        //public string LogNomeFile;
        public string NomeTotem;

        public string AData_path;
        public Boolean ADataLocal;

        public CVotoBaseDati()		
        {
            // i file devono essere in locale nella cartella Data
            AData_path = "c:" + VSDecl.DATA_PATH_ABS; // "c:\\data\\";
            ADataLocal = false;
        }

        // --------------------------------------------------------------------------
        //  EVENTI
        // --------------------------------------------------------------------------

        //protected void OnProgressoSalvaTutto(object source, int ATot, int AProg)
        //{
        //    if (ProgressoSalvaTutto != null) { ProgressoSalvaTutto(this, ATot, AProg); }
        //}

        // --------------------------------------------------------------------------
        //  LETTURA CONFIGURAZIONE NEL DATABASE
        // --------------------------------------------------------------------------

        virtual public int CaricaConfigDB(ref int BadgeLen, ref string CodImpianto)
        {
            return 0;
        }

        virtual public int DammiConfigTotem(string NomeTotem, ref TTotemConfig TotCfg)
        {
            return 0;
        }

        virtual public int DammiConfigDatabase(ref TTotemConfig TotCfg)
        {
            return 0;
        }
        
        virtual public int SalvaConfigurazione(string ANomeTotem, ref TTotemConfig ATotCfg)
        {
            return 0;
        }

        // --------------------------------------------------------------------------
        //  CARICAMENTO DATI VOTAZIONI
        // --------------------------------------------------------------------------
/*
        virtual public int CaricaDatiVotazioni(ref int NVoti, ref TVotazione[] fVoto)
        {
            return 0;
        }

        virtual public int CaricaDatiListe(ref int NVoti, ref TVotazione[] fVoto)
        {
            return 0;
        }
*/
        // --------------------------------------------------------------------------
        //  METODI SUI BADGE
        // --------------------------------------------------------------------------

        virtual public bool ControllaBadge(int AIDBadge, TTotemConfig TotCfg, ref int AReturnFlags)
        {
            return true;
        }

        virtual public bool BadgeAnnullato(int AIDBadge)
        {
            return false;
        }

        virtual public bool BadgePresente(int AIDBadge, bool ForzaTimbr)
        {
            return false;
        }

        virtual public bool BadgeHaGiaVotato(int AIDBadge)
        {
            return false;
        }

        virtual public bool HaVotato(int ANVotaz, int AIDBadge, int ProgDelega)
        {
            return false;
        }

		// --------------------------------------------------------------------------
        //  LETTURA DATI AZIONISTA 
		// --------------------------------------------------------------------------

        virtual public int DammiDatiAzionistaOneShot(int AIDBadge, ref int ANAzionisti, ref ArrayList FAzionisti)
        {
            return 0;
        }

        virtual public string DammiNomeAzionista(int AIDBadge)
        {
            return "";
        }

        // --------------------------------------------------------------------------
        //  CONTROLLO DELLA VOTAZIONE
        // --------------------------------------------------------------------------

        virtual public int SalvaTutto(int AIDBadge, TTotemConfig ATotCfg, ref TListaAzionisti FAzionisti)
        {
            return 0;
        }

        virtual public int NumAzTitolare(int AIDBadge)
        {
            return 0;
        }

        virtual public int CheckStatoVoto(string NomeTotem)
        {
            return 1;
        }

        virtual public bool CancellaBadgeVotazioni(int AIDBadge)
        {
            return true;
        }

        virtual public Boolean CancellaTuttiVoti()
        {
            return true;
        }

        // --------------------------------------------------------------------------
        //  METODI DATABASE
        // --------------------------------------------------------------------------

        virtual public object DBConnect()
        {
            // ritorna l'oggetto connessione
            return null;
        }

        virtual public object DBDisconnect()
        {
            // ritorna l'oggetto connessione
            return null;
        }

        // --------------------------------------------------------------------------
        //  REGISTRAZIONE NEL DATABASE
        // --------------------------------------------------------------------------

        virtual public int RegistraTotem(string NomeTotem)
        {
            return 0;
        }

        virtual public int UnregistraTotem(string NomeTotem)
        {
            return 0;
        }

        // --------------------------------------------------------------
        //  METODI PRIVATI
        // --------------------------------------------------------------

        virtual public string DammiStringaConnessione()
        {
            return "";
        }

        // --------------------------------------------------------------
        //  METODI DI CONFIGURAZIONE
        // --------------------------------------------------------------

        // carica la configurazione 
        virtual public Boolean CaricaConfig()
        {
                return true;
        }

        // --------------------------------------------------------------------------
        //  FUNZIONE DI RETRIEVE DI UNA STRINGA SQL DALLE RISORSE
        // --------------------------------------------------------------------------

        public string getModelsQueryProcedure(string ANameSqlFile)
        {
            string ret;
            // load from resources the query strings
            Stream stream;
            StreamReader reader;
            // -> detailsByIDShareholder
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VotoTouch.Data.Query." + ANameSqlFile);
            if (stream == null)
                ret = "";
            else
            {
                reader = new StreamReader(stream);
                ret = reader.ReadToEnd();
                reader = null;
            }
            stream = null;
            // replacing the newline with spaces for query syntax
            ret = ret.Replace("\r", " ");
            ret = ret.Replace("\n", " ");
            ret = ret.Replace("\t", " ");

            return ret;
        }

     }
}
