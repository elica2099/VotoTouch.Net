// --------------------------------------------------------------
//  FILE DELLE DICHIARAZIONI DI VOTO SEGRATO
// --------------------------------------------------------------

// --------------------------------------------------------------
//  CRONOLOGIA DELLE VERSIONI
//
//  V.3.0 10/04/2011                ASSEMBLEA BPER
//  V.3.1 10/10/2011                ASSEMBLEA BPM
//                                  Aggiunto tasto Non Votante, aggiunto tema esterno label
//                                  migliorata randomizzazione id,
//  V.3.2 15/03/2012                Multivotazioni, Rivisto TouchScreen



using System;
using System.Collections;

namespace VotoTouch
{
	
	public enum TTipoVoto:  int  {stvNormale, stvLista, stvMulti};

	public enum TAppStato: int {ssvBadge, ssvVotoStart, ssvVoto, ssvVotoConferma,ssvVotoContinua, ssvSalvaVoto, 
                                ssvVotoFinito, ssvPreIntermezzo, ssvConfermaNonVoto};

	public enum TStatoSemaforo: int {stsNulla, stsLibero, stsOccupato, stsErrore, stsFineoccupato, stsChiusoVoto};


    // configurazione del programma
    public struct TTotemConfig
    {
        // variabili di configurazione Singola Postazione
        public string Postazione;
        public string Descrizione;
        public int IDSeggio;

        // variabili di configurazione Generali
        public int ControllaPresenze;
        public bool SalvaLinkVoto;
        public bool SalvaVotoNonConfermato;
        public int IDSchedaUscitaForzata;
        public bool TastoRicominciaDaCapo;
        public bool AbilitaLogV;
        public bool AbilitaDirittiNonVoglioVotare;

        // Comportamento
        public bool Attivo;
        public bool VotoAperto;

        // Semaforo
        public bool UsaSemaforo;
        public string IP_Com_Semaforo;
        public int TipoSemaforo;

        // Variabili di configurazione Lettore
        public bool UsaLettore;
        public int PortaLettore;
        public string CodiceUscita;

        // codici impianto
        public int BadgeLen;
        public string CodImpianto;

        // controller centrale
        public bool UsaController;
        public string IPController;

        // IdSala
        public int Sala;
    }

    // struttura di configurazione del database
    public struct ConfigDbData
    {
        public Boolean DB_ConfigOK;
        public string DB_Type;
        public string DB_Dsn;
        public string DB_Name;
        public string DB_Uid;
        public string DB_Pwd;
        public string DB_Server;
    }

    // ------------------------------------------------------------------
    // STRUTTURE PER LE VOTAZIONI
    // ------------------------------------------------------------------
    
    public struct TAreaVotazione
    {
        // Area di Voto
        public int XVt;
        public int YVt;
        public int WVt;
        public int HVt;

        // N.candidati per pagina in caso di multi o candidato
        public int CandidatiPerPagina;
        // Uso o meno delle linguette (in caso di pochi candidati sono inutili)
        public bool NeedTabs;

        // AreaCandidatiCDA
        public int XCda;
        public int YCda;
        public int WCda;
        public int HCda;
        public int RCda() { return XCda + WCda; }
        public int BCda() { return YCda + HCda; }
        // AreaCandidatiAlt
        public int XAlt;
        public int YAlt;
        public int WAlt;
        public int HAlt;
        public int RAlt() { return XAlt + WAlt; }
        public int BAlt() { return YAlt + HAlt; }
    }

    // struttura per le votazioni
    // *************** OBSOLETO CON I NUOVI OGGETTI *************************
    //public struct TVotazione
    //{
    //    public int IDVoto;
    //    public int IDGruppoVoto;
    //    public string Descrizione;
    //    public int TipoVoto;                //1.norm, 2.Lista, 3.Multi
    //    public int TipoSubVoto;             // a seconda del tipo principale 
    //    public int NListe;                  // >nota era nscelte
    //    public bool SkBianca;               // ha scheda bianca
    //    public bool SkNonVoto;              // ha il non voto
    //    public int MaxScelte;               // n scelte max nel caso di multi
    //    public bool NeedConferma;           // indica che dopo questa votazione necessita la conferma
    //    public bool PreIntermezzo;          // videata intermezzo
    //    public bool PreIntermezzoFatto;     // videata intermezzo

    //    public TAreaVotazione AreaVoto;

    //    // quanti sono stati preszentati da cda (serve per candidati e multi)
    //    public int NPresentatoCDA;
    //    public bool SelezionaTuttiCDA;

    //    // classe CVotazione
    //    public ArrayList Liste;     // collection di strutture Tliste
    //    public ArrayList Pagine;    // collection delle pagine (per le votazioni candidato)
    //}

    // *************** OBSOLETO CON I NUOVI OGGETTI *************************
    //public struct TLista
    //{
    //    public int NumVotaz;
    //    public int IDLista;
    //    public int IDScheda;
    //    public string DescrLista;
    //    public int TipoCarica;
    //    public bool PresentatodaCDA;
    //    public string Presentatore;
    //    public string Capolista;
    //    public string ListaElenco;
    //    public int Pag;
    //    public string PagInd;
    //}

    public struct TIndiceListe
    {
        public int pag;
        public string sp;
        public string ep;
        public string indice;
        public int idx_start;
        public int idx_end;
    }

    public struct TVotoEspresso
    {
        public int NumVotaz;
        public int TipoCarica;
        public int VotoExp_IDScheda;
        public string Str_ListaElenco;
        public string StrUp_DescrLista;
    }

    //public struct TVotiDaSalvare     // non serve, si inegra con cls Azionisti
    //{
    //    public int NumVotaz_1;
    //    public int AScheda_2;
    //    public int NVoti_3;
    //    public int AIDBadge_4;
    //    public int ProgDelega_5;
    //    public int IdCarica_6;
    //    public int IDazion;
    //}

    //public struct TAzionista
    //{
    //    // dati dell'utente
    //    public int IDBadge;
    //    public string CoAz;
    //    public int IDAzion;
    //    public int ProgDeleg;
    //    public string RaSo;
    //    public int NAzioni;
    //    public string Sesso;
    //    // dati del voto
    //    public int IDVotaz;
    //    public int IDScheda;
    //    public int NVoti;
    //    public int IDCarica;
    //}

    // dati dell'utente
    //public struct DatiUtente
    //{
    //    public int utente_badge;
    //    public string utente_nome;
    //    public int utente_voti;
    //    public int utente_voti_bak;
    //    public int utente_id;
    //    public string utente_sesso;
    //}

    public class VSDecl
    {
        // Classe che mantiene tutte le costanti
        public const string VTS_VERSION = "3.7 01/03/2016";

        public const string RIPETIZ_VOTO = "88889999";
        public const string CONFIGURA = "88889990";
        public const string PANNELLO_STATO = "88889991";
        public const string PANNELLO_AZION = "88889992";

        public const string MSG_RIPETIZ_VOTO = "ATTENZIONE! Questa operazione annullerà la votazione corrente " +
                                               "\nI voti espressi fino ad ora NON saranno salvati." +
                                               "\n NON sarà salvata la consegna scheda, a tutti gli effetti " +
                                               "il badge non avrà votato." +
                                               "\nSarà quindi possibile in seguito ripetere la votazione. NON ci saranno voti doppi." +
                                               "\nConfermi l'annullamento? (Si = annulla la votazione, No = continua a votare)" +
                                               "\n ----> Badge corrente : ";
        public const string MSG_RIPETIZ_VOTO_C = "Sei proprio sicuro di Annullare la votazione ? \n Badge: ";
        public const string MSG_CANC_VOTO = "ATTENZIONE! Questa operazione cancellerà i voti di questo badge sul database." +
                                               "\nI voti salvati fino ad ora saranno CANCELLATI." +
                                               "\n Sarà CANCELLATA la consegna scheda, quindi a tutti gli effetti " +
                                               "il badge non avrà votato." +
                                               "\nSarà quindi possibile in seguito ripetere la votazione. NON ci saranno voti doppi." +
                                               "\nConfermi la cancellazione? (Si = cancella la votazione, No = non fare nulla)" +
                                               "\n ----> Badge da cancellare : ";
        public const string MSG_CANC_VOTO_C = "Sei proprio sicuro di Cancellare i voti dal DB ? \n Badge: ";


        // path Immagini assoluti
        public const string DATA_PATH_ABS = "\\Data\\";
        public const string IMG_PATH_ABS = "\\Data\\VtsNETImg\\";
        // path immagine da server
        public const string SOURCE_IMG_PATH = "\\Data\\VtsNETImg\\";
        // path immagine locale
        public const string IMG_PATH_LOC = "\\VtsNETImgLocali\\";
        public const string IMG_type = ".png";

        public const string IMG_Badge = "badge";
        public const string IMG_VotostartD = "votostart_D";
        public const string IMG_Votostart1 = "votostart_1";
        public const string IMG_fine = "fine";
        public const string IMG_intermezzo = "intermezzo";
        public const string IMG_Votochiuso = "votochiuso";
        public const string IMG_Salva = "salvataggio";

        public const string IMG_voto = "voto_";
        public const string IMG_voto_c = "_conf";
        public const string IMG_voto_pre = "_pre";

        // ----------------------------
        // COSTANTI VOTAZIONE
        // ----------------------------
        // tipi di Votazione
        public const int VOTO_LISTA = 1;            // voto di lista
        public const int VOTO_CANDIDATO = 2;        // voto per candidato a pagine
        public const int VOTO_CANDIDATO_SING = 3;   // voto per candidato singola pagina (da cancellare)
        public const int VOTO_MULTICANDIDATO = 4;   // voto multicandidato
        //
        // Voti
        public const int LISTA_1 = 0;
        public const int LISTA_2 = 1;
        public const int LISTA_3 = 2;
        public const int LISTA_4 = 3;
        public const int LISTA_5 = 4;
        public const int LISTA_6 = 5;
        public const int VOTO_SCHEDABIANCA = -1;
        public const int VOTO_NONVOTO = -2;
        public const int VOTO_MULTIAVANTI = -10;
        public const int VOTO_TUTTI_ABS = 226;
        public const int VOTO_TUTTI_CON = 227;
        //
        // ----------------------------

        //public const int VOTO_MULTICAND_ETRURIA = 5;   // voto multicandidato

        // n. di selezioni per pagina in caso di VOTO_CANDIDATO
        public const int CANDIDATI_PER_PAGINA = 10;
        public const int CANDXPAG_10 = 10;
        public const int CANDXPAG_8 = 8;
        public const int CANDXPAG_6 = 6;

        public static readonly string[] abt ={ 
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", 
                "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

        public const int PRES_NON_CONTROLLARE = 0;
        public const int PRES_CONTROLLA = 1;
        public const int PRES_FORZA_INGRESSO = 2;

        // timer del chck voto
        public const int TIM_CKVOTO_MIN = 15000;   // 15 secondi
        public const int TIM_CKVOTO_MAX = 40000;   // 50 secondi

        // costante del randomize id (link scollegato)
        public const int MAX_ID_RANDOM = 9999999;


        //public const int VOTO_ETRURIA = 999999;

        public const int SEMAFORO_IP = 1;
        public const int SEMAFORO_COM = 2;

        // salvataggio su log
        public const bool SALVAVOTISULOG = true;

        // progressivo voti
        public const int MINVOTI_PROGRESSIVO = 30;

        // costanti per la finestra
        public const int BTN_FONT_SIZE = 14;
        public const string BTN_FONT_NAME = "Arial";
        //
    
    }
}