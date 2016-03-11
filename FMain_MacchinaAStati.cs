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
        //		ROUTINE DI GESTIONE DEGLI STATI
        // ----------------------------------------------------------------
        //******************************************************************************

        #region Macchina A Stati

        private void CambiaStato()
        {
            // disaccoppia la funzione attraverso un timer che chiama un evento
            timCambiaStato.Enabled = true;
        }

        private void timCambiaStato_Tick(object sender, EventArgs e)
        {
            timCambiaStato.Enabled = false;
            CambiaStatoDaTimer();
        }

        private void CambiaStatoDaTimer()
        {
            TAzionista c;
            // gestione degli stati della votazione
            // Touchscreen
            //oVotoTouch.CalcolaTouch(this, Stato, Votazioni.VotoCorrente, DatiUsr.utente_voti > 1);
            //
            switch (Stato)
            {
                case TAppStato.ssvBadge:
                    oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    SettaComponenti(false);
                    // resetto la votazione, lo faccio sempre
                    //CurrVoteIDX = 0;
                    UscitaInVotazione = false;
                    // labels
                    lbDirittiDiVoto.Text = "";
                    lbNomeAzStart.Text = "";
                    // ok ora testo eventuali eventi di apertura o chisura votazione dall'esterno
                    // questo mi viene dalla variabile AperturaVotoEsterno, che viene settata nell'evento
                    // di controllo del timer che testa la variabile votoaperto relativo alla postazione
                    // sul db. 
                    // All'avvio dell'applicazione AperturaVotoEsterno viene settato come la lettura
                    // sul db, quindi quando qui cambia vuol dire (in funzione del valore)
                    // che un evento di apertura/chiusura votazione è avvenuto
                    if (AperturaVotoEsterno != TotCfg.VotoAperto)
                    {
                        if (AperturaVotoEsterno)
                        {
                            Logging.WriteToLog("Evento Apertura votazione");
                            Votazioni.CaricaListeVotazioni(Data_Path);
                        }
                        else
                            Logging.WriteToLog("Evento Chiusura votazione");
                        // ok, ora setto la variabile locale di configurazione
                        TotCfg.VotoAperto = AperturaVotoEsterno;
                        // se la votazione è aperta il timer di controllo voto batte di meno
                        if (TotCfg.VotoAperto)
                            timVotoApero.Interval = VSDecl.TIM_CKVOTO_MAX;
                        else
                            timVotoApero.Interval = VSDecl.TIM_CKVOTO_MIN;
                    }

                    // a seconda dello stato, mostro il semaforo e metto l'immagine corretta
                    if (TotCfg.VotoAperto)
                    {
                        oSemaforo.SemaforoLibero();
                        oVotoImg.LoadImages(VSDecl.IMG_Badge);
                    }
                    else
                    {
                        oSemaforo.SemaforoChiusoVoto();
                        oVotoImg.LoadImages(VSDecl.IMG_Votochiuso);
                    }
                    break;

                case TAppStato.ssvVotoStart:
                    //IsStartingVoto = true;
                    oVotoTouch.CalcolaTouchSpecial(Stato, Azionisti.HaDirittiDiVotoMultipli());
                    oSemaforo.SemaforoOccupato();
                    // quà metto il voto differenziato
                    MettiComponentiStartVoto();
                    // resetto comunque la delega che è sempre la prima anche nel caso di un voto solo
                    // nota: l'array degli azionisti parte da 1
                    //CurrIdAzionDelega = 0;

                    break;

                case TAppStato.ssvVoto:
                    // in generale non so quale è il voto corrente, perchè non c'è più una sequenza
                    // ma dipende dai diritti di voto dell'azionista, espressi o no, potrei avere espresso
                    // tutti i voti sulla prima e nessuno sulla seconda votazione.
                    // lo vedo caricando di volta in volta l'azionista che non ha diritti di voto espressi (havotato = false)

                    //// segnalo all'oggetto Azionista che è partito il voto
                    //if (IsStartingVoto)
                    //{
                    //    IsStartingVoto = false;
                    //    Azionisti.InizioProceduraVotazione(IsVotazioneDifferenziata);                        
                    //}
                    // ok, ora estraggo l'azionista o il gruppo di azionisti (se non è differenziato) che devono votare
                    // in Azionisti.AzionistiInVotoCorrente ho l'elenco dei diritti
                    Azionisti.EstraiAzionisti_VotoCorrente(IsVotazioneDifferenziata);
                    // setto il voto corrente sul primo item dell'oggetto
                    Votazioni.SetVotoCorrente(Azionisti.DammiIDVotazione_VotoCorrente());
                    // calibro il touch sul voto
                    oVotoTouch.CalcolaTouchVote(Votazioni.VotoCorrente);
                    // ora devo capire che votazione è e mettere i componenti, attenzione che posso tornare
                    // da un'annulla
                    SettaComponenti(false);
                    // cancello i voti temporanei correnti 
                    CancellaTempVotiCorrenti();
                    // ora metto in quadro l'immagine, che deve essere presa da un file composto da
                    oVotoImg.LoadImages(VSDecl.IMG_voto + Votazioni.VotoCorrente.IDVoto.ToString());
                    // mostro comunque i diritti di voto in lbDirittiDiVoto e il nome di quello corrente
                    lbNomeDisgiunto.Text = rm.GetString("SAPP_VOTE_D_RASO") + "\n" + Azionisti.DammiNomeAzionistaInVoto_VotoCorrente(IsVotazioneDifferenziata);
                    lbNomeDisgiunto.Visible = (IsVotazioneDifferenziata || Azionisti.DammiCountDirittiDiVoto_VotoCorrente() ==1);
                    int dir_riman = IsVotazioneDifferenziata
                                        ? Azionisti.DammiTotaleDirittiRimanenti_VotoCorrente()
                                        : Azionisti.DammiCountDirittiDiVoto_VotoCorrente();
                    lbDirittiDiVoto.Text = dir_riman.ToString() + rm.GetString("SAPP_VOTE_D_DIRITTI");
                    lbDirittiDiVoto.Visible = true;
                    break;

                case TAppStato.ssvVotoConferma:
                    oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    // conferma
                    MettiComponentiConferma();
                    // ora metto in quadro l'immagine, che deve essere presa da un file composto da
                    oVotoImg.LoadImages(VSDecl.IMG_voto + Votazioni.VotoCorrente.IDVoto.ToString() + VSDecl.IMG_voto_c);
                    lbNomeDisgiunto.Visible = (IsVotazioneDifferenziata || Azionisti.DammiCountDirittiDiVoto_VotoCorrente() == 1);

                // Differenziato
                    //if (IsVotazioneDifferenziata || DatiUsr.utente_voti == 1)
                    //{
                    //    if (TotCfg.SalvaLinkVoto) lbNomeDisgiunto.Visible = true;
                    //    c = (TAzionista)FAzionisti[CurrIdAzionDelega];
                    //    lbNomeDisgiunto.Text = rm.GetString("SAPP_VOTE_D_RASO") + "\n" + c.RaSo; // "Si sta votando per:\n"
                    //    if (IsVotazioneDifferenziata)
                    //        lbDisgiuntoRimangono.Visible = true;
                    //}
                    break;

                case TAppStato.ssvVotoContinua:
                    oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    break;

                case TAppStato.ssvVotoFinito:
                    oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    //if (prbSalvaTutto.Visible)
                    //    prbSalvaTutto.Visible = false;
                    lbDirittiDiVoto.Visible = false;
                    SettaComponenti(false);
                    // labels
                    lbDirittiDiVoto.Text = "";
                    // messaggio di arrivederci
                    if (TotCfg.UsaLettore)
                    {
                        NewReader.Flush();
                    }
                    // se è uscito in votazione con il 999999
                    if (UscitaInVotazione)
                    {
                        UscitaInVotazione = false;
                        TornaInizio();
                    }
                    else
                        oVotoImg.LoadImages(VSDecl.IMG_fine);
                    break;

                case TAppStato.ssvSalvaVoto:

                    /*
                    // salva i voti e le presenze in un one shot sul database
                    if (FVotiDaSalvare.Count > VSDecl.MINVOTI_PROGRESSIVO)
                    {
                        // devo mettere la videata di salvataggio
                        oVotoImg.LoadImages(VSDecl.IMG_Salva);
                        // devo settare le dimensioni della barra
                        prbSalvaTutto.Left = 200;
                        prbSalvaTutto.Width = this.Width - 400;
                        prbSalvaTutto.Top = this.Height / 2;
                        prbSalvaTutto.Visible = true;
                        lbDirittiDiVoto.Visible = false;
                        SettaComponenti(false);
                        this.Refresh();
                    }

                    // controllo i voti, alla ricerca di problemi, ma ora non posso più farlo
                    // perché ci sono i multicandidati
                    //ControllaVoti();

                    // salvataggio dati su log
                    if (TotCfg.AbilitaLogV) SalvaTuttoSuLog();

                    // ok, ora prima di salvare controllo il parametro SalvaLinkVoto
                    // se è true non faccio nulla, altrimenti distruggo il link voto-badge
                    ControllaSalvaLinkVoto();
                    */

                    if (Azionisti.DammiQuantiDirittiSonoStatiVotati() > VSDecl.MINVOTI_PROGRESSIVO)
                    {
                        // metti lo spinning wheel
                    }
                    // salvo i dati sul database
                    oDBDati.SalvaTutto(Badge_Letto, TotCfg, ref Azionisti);
                     
                    // togli lo spinning wheel
                    pbSalvaDati.Visible = false;
                    
                    oSemaforo.SemaforoFineOccupato();
                    //CurrVoteIDX = 0;
                    Stato = TAppStato.ssvVotoFinito;
                    CambiaStato();

                    break;
            }
        }

        private void timVotoApero_Tick(object sender, EventArgs e)
        {
            // dr11 ok
            bool vtaperto;
            int getvtaperto;
            // devo verificare sul database se il voto per questa postazione è aperto
            getvtaperto = oDBDati.CheckStatoVoto(NomeTotem);
            // se sono in una condizione di errore (es db non risponde) lascio il valore precedente
            if (getvtaperto != -1)
            {
                if (getvtaperto == 1) vtaperto = true; else vtaperto = false;
                // se sono diversi e sono all'inizio allora cambio lo stato
                if (TotCfg.VotoAperto != vtaperto)
                {
                    // segnalo che c'è stato un evento e setto una variabile che sarà controllata
                    // appena lo stato sarà su Badge, quindi sarà finita l'eventuale votazione 
                    // in corso
                    AperturaVotoEsterno = vtaperto;
                    // ma se per caso sono in badge devo forzare
                    if (Stato == TAppStato.ssvBadge)
                        CambiaStato();
                }
            }
            // chiaramente se non sono diversi non faccio nulla
        }

        #endregion

    
    }
}
