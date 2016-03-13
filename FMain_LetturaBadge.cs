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

        #region StatoBadge E Lettura Dati Utente

        private void btmBadge_Click(object sender, System.EventArgs e)
        {
            BadgeLetto(edtBadge.Text);
        }

        private void ObjDataReceived(object sender, string data)
        {
            this.BeginInvoke(evtDataReceived, new object[] { this, data });
        }

        public void onDataReceived(object source, string dato)
        {
            Badge_Seriale = dato;
            timLetturaBadge.Enabled = true;
        }

        private void timLetturaBadge_Tick(object sender, EventArgs e)
        {
            timLetturaBadge.Enabled = false;
            // ora chiamo l'evento
            Serial_NewRead(Badge_Seriale);
        }

        private void Serial_NewRead(string dato)
        {
            // arriva dal timer che "disaccoppia" la funzione
            // può esser di due tipi
            // - codimpianto + badge
            // - com. particolari "999999"
            // faccio un unico test per vedere se è valido, non può superare la lunghezza totale 
            if (dato.Length <= (TotCfg.BadgeLen + TotCfg.CodImpianto.Length))
            {
                if (fConfig != null)
                    fConfig.BadgeLetto(dato);
                else
                    BadgeLetto(dato);
            }
        }

        // ----------------------------------------------------------------
        //	  VERIFICA DATI UTENTE, EVENTUALE SCRITTURA DEL VOTANTE TOTEM E
        //    DELLA CONSEGNA SCHEDE
        // ----------------------------------------------------------------

        private void BadgeLetto(string AText)
        {
            //DR12 OK, aggiunto solo controllo badge->999999
            int Badge_Lettura;
            string codimp, bbadge;
            int ErrorFlag = 0;

            // allora prima di tutto controllo se c'è stato un comando di Reset Votazione
            // cioè 88889999
            if (AText == VSDecl.RIPETIZ_VOTO && TotCfg.VotoAperto)
            {
                CodiceRipetizioneVoto(AText);
                return;
            } // if (AText == VSDecl.RIPETIZ_VOTO && TotCfg.VotoAperto)

            // COMANDI SPECIALI
            // poi verifico se è stato premuto
            if (AText == VSDecl.CONFIGURA)
            {
                timConfigura.Enabled = true;
                return;
            }
            // pannello stato
            if (AText == VSDecl.PANNELLO_STATO)
            {
                MostraPannelloStato();
                return;
            }
            // pannello stato azionista
            if (AText == VSDecl.PANNELLO_AZION)
            {
                MostaPannelloStatoAzionista();
                return;
            }

            // CONTINUO
            // ok, per prima cosa, se il voto è chiuso o la postazione non è attiva
            // esco direttamente
            if (!TotCfg.VotoAperto && Stato == TAppStato.ssvBadge)
            {
                FVSTest test = new FVSTest(AText);
                test.ShowDialog();
                return;
            }

            // Devo considerare il caso in cui le finestre messaggio sono visibili ed uscire
            if ((frmVSMessage != null) && frmVSMessage.Visible) return;

            // se ho il badge il 999999 non esce fuori (doppia lettura)
            if (Stato == TAppStato.ssvBadge && AText == TotCfg.CodiceUscita) return;

            // stato iniziale, ho già filtrato le finestre
            if (Stato == TAppStato.ssvBadge)
            {
                // metto una variabile globale non si sa mai
                Badge_Lettura = -1;
                codimp = "00";  // codice impianto universale
                // ok, qua devo fare dei controlli sul codice impianto e sul badge
                // se la lunghezza è giusta allora estraggo le due parti e controllo
                if (AText.Length >= (TotCfg.BadgeLen + TotCfg.CodImpianto.Length))
                {
                    // estraggo il badge, parto sempre da sinistra
                    bbadge = AText.Substring(AText.Length - TotCfg.BadgeLen, TotCfg.BadgeLen);

                    // estraggo il cod impianto
                    codimp = AText.Substring(AText.Length - TotCfg.BadgeLen -
                        TotCfg.CodImpianto.Length, TotCfg.CodImpianto.Length);
                }
                else
                    bbadge = AText.Trim();

                // NOTA: in questa routine uso codici di errore bitwise, in pratica nella variabile 
                // ErrorFlag ci sono tutti gli errori che la procedura ha trovato e così posso
                // elaborarli alla fine
                // lo converto in intero
                try
                {
                    Badge_Lettura = Convert.ToInt32(bbadge);
                }
                catch
                {
                    ErrorFlag = ErrorFlag | 0x20;  // setto l'errore                }
                }

                // controllo il codice impianto, uso 00 come codice normale
                if ((codimp != "00") && (codimp != TotCfg.CodImpianto))
                    ErrorFlag = ErrorFlag | 0x10;

                // se non ho trovato errori continuo, testo anche il codice uscita così mi
                // evito un inutile lettura del db
                // nota: quando avrò più codici mi bastera fare una funzione
                if ((ErrorFlag == 0) && (AText != TotCfg.CodiceUscita))
                {
                    // variabile
                    Badge_Letto = Badge_Lettura;
                    // ok ora iniziano i test
                    bool Controllato = oDBDati.ControllaBadge(Badge_Lettura, TotCfg, ref ErrorFlag);

                    // NOTA
                    // per creval c'è il problema del non voto, quindi devo caricare tutti i diritti
                    // non espressi, usiamo il trucco:
                    // se non ha votato va bene come prima
                    // se ha votato, dopo bisogna controllare se ha espresso almeno un non voglio votare -2
                    //  se è così si deve:
                    //     cancellare la riga su votanti totem,
                    //     cancellare conschede e intonse che hanno voto -2
                    //     andare avanti, dovrebbe entrare nel giro badge banana

                    // mettere flag su db che caricato dalla configurazione

                    // if ((ErrorFlag & 0x04) == 0x04 && AbilitaDirittiNonVoglioVotare == true)
                    //{

                    //}


                    // separo così mi evito un controllo in più
                    if (Controllato)
                    {
                        if (Azionisti.CaricaDirittidiVotoDaDatabase(Badge_Letto, ref Votazioni) && !Azionisti.TuttiIDirittiSonoStatiEspressi())
                        {
                            // resetto alcune variabili
                            //VotoCorrente = Votazioni.DammiPrimaVotazione();
                            //CurrVoteIDX = 0;                    // resetto alla 1° votazione
                            //FVotiDaSalvare.Clear();             // cancello i voti
                            IsVotazioneDifferenziata = false;                   // dico che è un voto normale
                            CancellaTempVotiCorrenti();         // cancello i voti temporanei
                            //CurrIdAzionDelega = 0;              // la prima delega
                            // cambio lo stato
                            Logging.WriteToLog("** Inizio Voto : " + Badge_Letto.ToString() +
                                " Diritti di Voto Max: " + Azionisti.DammiMaxNumeroDirittiDiVotoTotali().ToString());

                            Stato = TAppStato.ssvVotoStart;
                            CambiaStato();
                        }
                        else  // if (DammiUtente())
                            ErrorFlag = ErrorFlag | 0x08;  // setto l'errore
                    }  // if (Controllato)
                } // if (ErrorFlag == 0)

                // ok, se ora qualcosa è andato storto ResultFlag è > 0
                if (ErrorFlag > 0 || AText == TotCfg.CodiceUscita)
                {
                    string messaggio = rm.GetString("SAPP_ERR_BDG") + Badge_Lettura.ToString();     //"Errore sul badge : "
                    // compongo il messaggio della finestra di errore
                    // 0x01 : Badge Annullato o mai esistito
                    if ((ErrorFlag & 0x01) == 0x01) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGANN");   // "\n - Badge Annullato";
                    // 0x40 : Il Badge non esiste
                    if ((ErrorFlag & 0x40) == 0x40) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGEST");   // "\n - Il Badge non esiste";
                    // 0x02 : Badge non presente (controllo disabilitato)
                    if (TotCfg.ControllaPresenze == VSDecl.PRES_CONTROLLA && (ErrorFlag & 0x01) != 0x01)
                    {
                        if ((ErrorFlag & 0x02) == 0x02) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGPRES");   // "\n - Badge non presente.";
                    }
                    // 0x04 : Badge ha già votato
                    if ((ErrorFlag & 0x04) == 0x04) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGVOT");   // "\n - Il Badge ha già votato";
                    // 0x08 : Tutti i soci hanno già votato
                    if ((ErrorFlag & 0x08) == 0x08) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGZERO");   // "\n - Socio con azioni zero\tutti i soci hanno già votato";
                    // 0x10 : Codice Impianto diverso
                    if ((ErrorFlag & 0x10) == 0x10) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGIMP");   // "\n - Codice Impianto diverso";
                    // 0x20 : Errore nella conversione
                    if ((ErrorFlag & 0x20) == 0x20) messaggio += "\n" + rm.GetString("SAPP_ERR_BDGCONV");   // "\n - Errore nella conversione Badge";

                    // se il badge è 999999, metto un codice a parte
                    if (AText == TotCfg.CodiceUscita)
                    {
                        messaggio = rm.GetString("SAPP_ERR_BDGUSCITA") + TotCfg.CodiceUscita + ")";
                    }

                    // evidenzio
                    oSemaforo.SemaforoErrore();
                    Logging.WriteToLog(messaggio);

                    // non so se è cancellata o no, x sicurezza la ricreo
                    if (frmVSMessage == null)
                    {
                        frmVSMessage = new FVSMessage();
                        this.AddOwnedForm(frmVSMessage);
                    }
                    frmVSMessage.Show(messaggio);

                    this.Focus();
                    oSemaforo.SemaforoLibero();
                    return;
                }

                edtBadge.Text = "";
                return;
            }

            // la conferma di uscita
            if (Stato == TAppStato.ssvVotoFinito)
            {
                if (AText == TotCfg.CodiceUscita) //"999999")
                {
                    Logging.WriteToLog("--> Voto " + Badge_Letto.ToString() + " terminato.");
                    TornaInizio();
                }
                edtBadge.Text = "";
                return;
            }

            // ora il codice di uscita in "mezzo" al voto
            if (AText == TotCfg.CodiceUscita &&
                Stato != TAppStato.ssvVotoStart &&
                Stato != TAppStato.ssvBadge)
            {
                CodiceUscitaInVotazione();
            }  // if (AText == TotCfg.CodiceUscita && Stato
        }

        private void CodiceRipetizioneVoto(string AText)
        {
            // qua è un casino, perché ho due casi:
            // 1. è stato digitato un badge, quindi c'è il messaggio "ha già votato"
            //    devo cancellare i voti
            if (Stato == TAppStato.ssvBadge)
            {
                if (MessageBox.Show(VSDecl.MSG_CANC_VOTO + Badge_Letto, "Cancellazione Voto", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    // ulteriore conferma
                    if (MessageBox.Show(VSDecl.MSG_CANC_VOTO_C + Badge_Letto, "Conferma Cancellazione Voto", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Logging.WriteToLog("--> Voto " + Badge_Letto.ToString() + " Cancellati voti (88889999).");
                        oDBDati.CancellaBadgeVotazioni(Badge_Letto);
                    }
                }
                return;
            }
            // 2. sono durante la votazione , esco senza salvare
            if (Stato != TAppStato.ssvVotoFinito)
            {
                if (MessageBox.Show(VSDecl.MSG_RIPETIZ_VOTO + Badge_Letto.ToString(), "Annullamento Voto", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    // ritorna all'inizio
                    // ulteriore conferma
                    if (MessageBox.Show(VSDecl.MSG_RIPETIZ_VOTO_C + Badge_Letto, "Conferma Annullamento Voto", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Logging.WriteToLog("--> Voto " + Badge_Letto.ToString() + " Annullato (88889999).");
                        TornaInizio();
                        edtBadge.Text = "";
                    }
                }
                return;
            }
        }

        public void CodiceUscitaInVotazione()
        {
            // ok, è proprio l'uscita dalla votazione
            // il problema è che qua devo far votare scheda bianca/nulla, ma non so a che punto sono arrivato
            // qundi devo fare un po di eculubrazioni
            int NSKSalvate = MettiSchedeDaInterruzione();
            // loggo
            Logging.WriteToLog("--> USCITA IN VOTO (999999) id:" + Badge_Letto.ToString() +
                                              " (" + NSKSalvate.ToString() + ")");
            // resetto il tutto
            lbDirittiDiVoto.Visible = false;
            SettaComponenti(false);
            // labels
            lbDirittiDiVoto.Text = "";
            // messaggio di arrivederci
            //VotoCorrente = Votazioni.DammiPrimaVotazione();
            //CurrVoteIDX = 0;
            Stato = TAppStato.ssvSalvaVoto;
            UscitaInVotazione = true;
            CambiaStato();
            edtBadge.Text = "";
        }

        // ----------------------------------------------------------------
        //   RITROVO DATI UTENTE DA DB
        // ----------------------------------------------------------------

        //private bool DammiUtente()
        //{
        //    //DR12 OK, non è stato cambiato
        //    //bool result, funz;
        //    //TAzionista c;

        //    // ok, ora dovrei capire le cose leggendole dal database
        //    //funz = (oDBDati.DammiDatiAzionistaOneShot(Badge_Letto, ref FNAzionisti, ref FAzionisti) == 1);

        //    return Azionisti.CaricaDirittidiVotoDaDatabase(Badge_Letto, ref Votazioni);

        //    /*
        //    if (funz)
        //    {
        //        // dati
        //        utente_voti_bak = FNAzionisti;
        //        utente_voti_diff = FNAzionisti;
        //         metto i dati dell'utente principale, il primo
        //        DatiUsr.utente_badge = Badge_Letto;
        //        if (FAzionisti.Count > 0)
        //        {
        //            c = (TAzionista)FAzionisti[0];
        //            DatiUsr.utente_id = c.IDAzion;
        //            DatiUsr.utente_nome = c.RaSo;
        //            DatiUsr.utente_sesso = c.Sesso;
        //        }
        //        else
        //        {
        //            DatiUsr.utente_id = 0;
        //            DatiUsr.utente_nome = "NONE";
        //            DatiUsr.utente_sesso = "N";
        //        }
        //        DatiUsr.utente_voti = FNAzionisti;
        //        DatiUsr.utente_voti_bak = FNAzionisti;
        //        result = true;
        //    }
        //    else
        //        result = false;

        //    return result;
        //     */
        //}

        #endregion


    }
}
