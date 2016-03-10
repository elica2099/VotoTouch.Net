using System;
using System.Resources;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Threading;
using System.Media;
using System.Reflection;
namespace VotoTouch
{

    public partial class frmMain : Form
    {

        //******************************************************************************
        // ----------------------------------------------------------------
        //	EVENTI DI PRESSIONE SCHERMO DA CTOUCHSCREEN
        // ----------------------------------------------------------------
        //******************************************************************************

        #region Eventi pressione schermo da CTouchscreen

        // ATTENZIONE, mettere una modifica di un qualsiasi controllo in queste routine, 
        // può causare refresh e paint indesiderati (caso a.Text) fare attenzione

        public void onPremutoTab(object source, int VParam)
        {
            // arriva l'evento cambio pagina (ora lo faccio all'interno di Ctouchscreen)
            //oVotoTouch.CurrPag = VParam;
            // ricalcolo, non serve più con la nuova gestione pagine
            //oVotoTouch.CalcolaTouch(this, Stato, ref FParVoto[CurrVoteIDX], DatiUsr.utente_voti > 1);
            // devo ridisegnare tutto, qui lo voglio
            this.Invalidate();
        }

        public void onPremutoVotaNormale(object source, int VParam)
        {
            // ok, questo evento arriva all'inizio votazione quando è stato premuto l'avvio del voto normale
            // o nel caso di scelta differenziato/normale, evidenzia il voto in un unica soluzione
            IsVotazioneDifferenziata = false;
            Logging.WriteToLog("Voto normale");
            Stato = TAppStato.ssvVoto;
            CambiaStato();
        }

        public void onPremutoVotaDifferenziato(object source, int VParam)
        {
            // ok, questo evento arriva all'inizio votazione 
            // nel caso di scelta differenziato/normale, evidenzia il voto in soluzioni separate
            IsVotazioneDifferenziata = true;
            Logging.WriteToLog("Voto differenziato");
            Stato = TAppStato.ssvVoto;
            CambiaStato();
        }

        public void onPremutoVotoValido(object source, int VParam, bool ZParam)
        {
            // TODO: Usare IdScheda invece di indice in VParam
            // TODO: Serve che TVotoEspresso abbia Str_ListaElenco o StrUp_DescrLista ? solo per i multi, risparmiamo spazio

            // ok, questo evento arriva quando, nella selezione del voto, è stata
            // premuna una zona valida
            // devo veder in funzione della lista selezionata
            TNewLista a;
            TVotoEspresso VExp;
            // questo controllo dell'indice è inutile, però è meglio farlo,
            // in caso di problemi, indici scassati, mette una scheda bianca
            int ct = Votazioni.VotoCorrente.Liste.Count;
            if (VParam >= 0 && VParam < ct)
            {
                a = (TNewLista)Votazioni.VotoCorrente.Liste[VParam];
                //VotoEspressoCarica = a.TipoCarica;
                VotoEspresso = a.IDScheda;
                VotoEspressoStr = a.ListaElenco;
                VotoEspressoStrUp = a.DescrLista;
                // da aggiungere successivamente:
                VExp = new TVotoEspresso
                    {
                        NumVotaz = a.NumVotaz,
                        VotoExp_IDScheda = a.IDScheda,
                        TipoCarica = a.TipoCarica,
                        Str_ListaElenco = a.ListaElenco,
                        StrUp_DescrLista = a.DescrLista
                    };
                FVotiExpr.Add(VExp);
            }
            else
            {
                // se succede qualcosa di strano mette sk bianca
                Logging.WriteToLog("<error> onPremutoVotoValido Indice voto non valido");
                VotoEspresso = VSDecl.VOTO_SCHEDABIANCA;
                //VotoEspressoCarica = 0;
                VotoEspressoStr = "";
                VotoEspressoStrUp = rm.GetString("SAPP_SKBIANCA");      // "Scheda Bianca";
                VExp = new TVotoEspresso
                {
                    NumVotaz = Votazioni.VotoCorrente.IDVoto,
                    VotoExp_IDScheda = VSDecl.VOTO_SCHEDABIANCA,
                    TipoCarica = 0,
                    Str_ListaElenco = "",
                    StrUp_DescrLista = rm.GetString("SAPP_SKBIANCA")
                };
                FVotiExpr.Add(VExp);
            }
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        // ORA E' MULTICANDIDATO, MA DIVENTERA' STANDARD
        public void onPremutoVotoValidoMulti(object source, int VParam, ref List<int> voti)
        {
            // in realtà corrisponde all'AVANTI
            if (voti == null) return;  // in teoria non serve
            TNewLista a;
            TVotoEspresso vt;
            int ct = Votazioni.VotoCorrente.Liste.Count;
            // ok, ora riempio la collection di voti
            for (int i = 0; i < voti.Count; i++)
            {
                if (voti[i] >= 0 && voti[i] < ct)
                {
                    a = (TNewLista)Votazioni.VotoCorrente.Liste[voti[i]];
                    vt = new TVotoEspresso
                        {
                            NumVotaz = a.NumVotaz,
                            TipoCarica = a.TipoCarica,
                            VotoExp_IDScheda = a.IDScheda,
                            Str_ListaElenco = a.ListaElenco,
                            StrUp_DescrLista = a.DescrLista
                        };
                    FVotiExpr.Add(vt);
                }
            }
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        public void onPremutoVotoMulti(object source, int VParam)
        {
            // mi serve per il repaint per settare o meno i tasti verdi
            this.Invalidate();
        }

        public void onPremutoSchedaBianca(object source, int VParam)
        {
            // scheda bianca
            VotoEspresso = VSDecl.VOTO_SCHEDABIANCA;
            //VotoEspressoCarica = 0;
            VotoEspressoStr = "";
            VotoEspressoStrUp = rm.GetString("SAPP_SKBIANCA");      // "Scheda Bianca";
            // nuova versione array
            TVotoEspresso VExp = new TVotoEspresso
                {
                    NumVotaz = Votazioni.VotoCorrente.IDVoto,
                    VotoExp_IDScheda = VSDecl.VOTO_SCHEDABIANCA,
                    TipoCarica = 0,
                    Str_ListaElenco = "",
                    StrUp_DescrLista = rm.GetString("SAPP_SKBIANCA")
                };
            FVotiExpr.Add(VExp);
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        public void onPremutoNonVoto(object source, int VParam)
        {
            // NonVotante (caso BPM)
            VotoEspresso = VSDecl.VOTO_NONVOTO;
            //VotoEspressoCarica = 0;
            VotoEspressoStr = "";
            VotoEspressoStrUp = rm.GetString("SAPP_NOVOTO");      // "Non Voglio Votare";
            // nuova versione array
            TVotoEspresso VExp = new TVotoEspresso
                {
                    NumVotaz = Votazioni.VotoCorrente.IDVoto,
                    VotoExp_IDScheda = VSDecl.VOTO_NONVOTO,
                    TipoCarica = 0,
                    Str_ListaElenco = "",
                    StrUp_DescrLista = rm.GetString("SAPP_NOVOTO")
                };
            FVotiExpr.Add(VExp);
            // a questo punto vado in conferma con la stessa CurrVote
            Stato = TAppStato.ssvVotoConferma;
            CambiaStato();
        }

        public void onPremutoInvalido(object source, int VParam)
        {
            // ok, questo evento arriva quando, nella selezione del voto, è stata
            // premuna una zona invalida, quindi nulla, potrebbe essere un beep
            SystemSounds.Beep.Play();
        }

        // ----------------------------------------------------------------
        //	 CONFERMA - ANNULLA VOTI
        //      versione con SalvaVotoNonConfermato
        // ----------------------------------------------------------------

        public void onPremutoConferma(object source, int VParam)
        {
            // ok, questo evento arriva quando, nella conferma del voto, è stata scelta l'opzione
            //  conferma, cioè il salvataggio del voto

            // CHiamo la funzione di Conferma Voti di Azionisti
            Azionisti.ConfermaVoti_VotoCorrente(ref FVotiExpr);

            // cambio stato
            Stato = Azionisti.IsVotazioneFinita() ? TAppStato.ssvSalvaVoto : TAppStato.ssvVoto;
            // cambio
            CambiaStato();


            /*
            // testo la votazione
            if (!IsVotazioneDifferenziata)     // VOTAZIONE NORMALE
            {
                // passo alla votazione successiva
                CurrVoteIDX++;

                // testo se ho finito
                if (CurrVoteIDX == NVoti)
                {
                    // per precauzione metto CurrVoteIDX all'ultima votazione 
                    CurrVoteIDX = NVoti - 1;
                    Stato = TAppStato.ssvSalvaVoto;         // finito, salvo i voti e poi chiudo
                }
                else
                    Stato = TAppStato.ssvVoto;              // passo al voto successivo
                CambiaStato();
            }
            else
            {
                // punto all'indice della delega corrente
                CurrIdAzionDelega++;
                // controllo se ho finito la singola multi-votazione
                if (utente_voti_bak == 0)
                {
                    // poi passo alla votazione successiva
                    CurrVoteIDX++;
                    // resetto le deleghe
                    CurrIdAzionDelega = 0;
                    // rimetto a posto i valori
                    utente_voti_bak = DatiUsr.utente_voti;
                    utente_voti_diff = DatiUsr.utente_voti;
                }
                // testo se ho finito tutte le votazioni
                if (CurrVoteIDX == NVoti)
                {
                    // per precauzione metto CurrVoteIDX all'ultima votazione 
                    CurrVoteIDX = NVoti - 1;
                    Stato = TAppStato.ssvSalvaVoto;             // finito, salvo i voti e poi chiudo
                }
                else
                    // passo al voto della votazione successiva (o successiva delega)
                    Stato = TAppStato.ssvVoto;
                // cambio
                CambiaStato();
            }
             */

        }

        /*
        public void ConfermaVotiEspressi()
        {
            int i, k;
            TVotiDaSalvare v;
            TVotoEspresso vt;
            TAzionista c;

            // non è il massimo, ma setta a 1 la pagina del touch quando preme conferma
            oVotoTouch.CurrPag = 1;
            // ok, questo evento arriva quando, nella conferma del voto, è stata scelta l'opzione
            //  conferma, cioè il salvataggio del voto
            if (!IsVotazioneDifferenziata)     // VOTAZIONE NORMALE
            {
                // Votazione Normale, In VParam c'è l'idx del voto
                // Ok, ora salvo i voti espressi:
                // 1. Nell'arrayList
                for (i = 0; i < FNAzionisti; i++)
                {
                    // prendo l'azionista
                    c = (TAzionista)FAzionisti[i];
                    // ok qui distinguo i multicandidati
                    // NOTA: unificare con il metodo collection
                    if (Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                    {
                        // ok, ora riempio la collection di voti
                        for (k = 0; k < FVotiExpr.Count; k++)
                        {
                            vt = (TVotoEspresso)FVotiExpr[k];
                            v = new TVotiDaSalvare();
                            // parte da 1
                            v.NumVotaz_1 = Votazioni.VotoCorrente.IDVoto;
                            v.AScheda_2 = vt.VotoExp_IDScheda;
                            v.NVoti_3 = 1;
                            v.AIDBadge_4 = Badge_Letto;
                            v.ProgDelega_5 = c.ProgDeleg;
                            v.IdCarica_6 = vt.TipoCarica;
                            v.IDazion = c.IDAzion;
                            // carico
                            FVotiDaSalvare.Add(v);
                        }
                    }
                    else
                    {

                        //if (VotoEspresso == VSDecl.VOTO_ETRURIA)
                        //{
                        //    v = new TVotiDaSalvare();
                        //    // parte da 1
                        //    //c = (clsAzionisti)FAzionisti[i];
                        //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                        //    v.AScheda_2 = 201;
                        //    v.NVoti_3 = 1;
                        //    v.AIDBadge_4 = Badge_Letto;
                        //    v.ProgDelega_5 = c.ProgDeleg;
                        //    v.IdCarica_6 = VotoEspressoCarica;
                        //    v.IDazion = c.IDAzion;
                        //    // carico
                        //    FVotiDaSalvare.Add(v);
                        //    v = new TVotiDaSalvare();
                        //    // parte da 1
                        //    //c = (clsAzionisti)FAzionisti[i];
                        //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                        //    v.AScheda_2 = 202;
                        //    v.NVoti_3 = 1;
                        //    v.AIDBadge_4 = Badge_Letto;
                        //    v.ProgDelega_5 = c.ProgDeleg;
                        //    v.IdCarica_6 = VotoEspressoCarica;
                        //    v.IDazion = c.IDAzion;
                        //    // carico
                        //    FVotiDaSalvare.Add(v);
                        //}
                        //else
                        //{
                            // -------------------------------------------
                            // in obsolescenza, si dovrà usare la collection
                            //
                            v = new TVotiDaSalvare();
                            // parte da 1
                            //c = (clsAzionisti)FAzionisti[i];
                            v.NumVotaz_1 = Votazioni.VotoCorrente.IDVoto;
                            v.AScheda_2 = VotoEspresso;
                            v.NVoti_3 = 1;
                            v.AIDBadge_4 = Badge_Letto;
                            v.ProgDelega_5 = c.ProgDeleg;
                            v.IdCarica_6 = VotoEspressoCarica;
                            v.IDazion = c.IDAzion;
                            // carico
                            FVotiDaSalvare.Add(v);
                            // -------------------------------------------
                        //}

                    }
                }
                // alla fine saranno da unificare i metodi
            }
            else
            {
                // Votazione differenziata
                utente_voti_bak --;
                utente_voti_diff --;
                // trovo l'azionista
                c = (TAzionista)FAzionisti[CurrIdAzionDelega];
                // Ok, ora salvo i voti espressi Nell'arrayList
                // ok qui distinguo i multicandidati
                // NOTA: unificare con il metodo collection
                if (Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                {
                    // ok, ora riempio la collection di voti
                    for (k = 0; k < FVotiExpr.Count; k++)
                    {
                        vt = (TVotoEspresso)FVotiExpr[k];
                        v = new TVotiDaSalvare();
                        // parte da 1
                        v.NumVotaz_1 = Votazioni.VotoCorrente.IDVoto;
                        v.AScheda_2 = vt.VotoExp_IDScheda;
                        v.NVoti_3 = 1;
                        v.AIDBadge_4 = Badge_Letto;
                        v.ProgDelega_5 = c.ProgDeleg;
                        v.IdCarica_6 = vt.TipoCarica;
                        v.IDazion = c.IDAzion;
                        // carico
                        FVotiDaSalvare.Add(v);
                    }
                }
                else
                {
                    //if (VotoEspresso == VSDecl.VOTO_ETRURIA)
                    //{
                    //    v = new TVotiDaSalvare();
                    //    // parte da 1
                    //    //c = (clsAzionisti)FAzionisti[i];
                    //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                    //    v.AScheda_2 = 201;
                    //    v.NVoti_3 = 1;
                    //    v.AIDBadge_4 = Badge_Letto;
                    //    v.ProgDelega_5 = c.ProgDeleg;
                    //    v.IdCarica_6 = VotoEspressoCarica;
                    //    v.IDazion = c.IDAzion;
                    //    // carico
                    //    FVotiDaSalvare.Add(v);
                    //    v = new TVotiDaSalvare();
                    //    // parte da 1
                    //    //c = (clsAzionisti)FAzionisti[i];
                    //    v.NumVotaz_1 = FParVoto[CurrVoteIDX].IDVoto;
                    //    v.AScheda_2 = 202;
                    //    v.NVoti_3 = 1;
                    //    v.AIDBadge_4 = Badge_Letto;
                    //    v.ProgDelega_5 = c.ProgDeleg;
                    //    v.IdCarica_6 = VotoEspressoCarica;
                    //    v.IDazion = c.IDAzion;
                    //    // carico
                    //    FVotiDaSalvare.Add(v);
                    //}
                    //else
                    //{
                        // -------------------------------------------
                        // in obsolescenza, si dovrà usare la collection
                        //
                        //c = (clsAzionisti)FAzionisti[CurrIdAzionDelega];
                        v = new TVotiDaSalvare();
                        // parte da 1
                        v.NumVotaz_1 = Votazioni.VotoCorrente.IDVoto;
                        v.AScheda_2 = VotoEspresso;
                        v.NVoti_3 = 1;
                        v.AIDBadge_4 = Badge_Letto;
                        v.ProgDelega_5 = c.ProgDeleg;
                        v.IdCarica_6 = VotoEspressoCarica;
                        v.IDazion = c.IDAzion;
                        // carico
                        FVotiDaSalvare.Add(v);
                        // -------------------------------------------
                    //}
                }
            }
             * 
             
        }

    */
        public void onPremutoAnnulla(object source, int VParam)
        {
            // ok, questo evento arriva quando, nella conferma del voto, è stata scelta l'opzione
            //  annulla, cioè il ritorno all'espressione del voto
            CancellaTempVotiCorrenti();
            // Annulla del voto (torno dove ero prima)
            Stato = TAppStato.ssvVoto;
            CambiaStato();
        }

        public void CancellaTempVotiCorrenti()
        {
            // cancella i voti correnti
            FVotiExpr.Clear();
            VotoEspresso = -1;
            VotoEspressoStr = "";
            VotoEspressoStrUp = "";
            //VotoEspressoCarica = 0;
        }

        #endregion



    }
}
