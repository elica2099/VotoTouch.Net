using System;
using System.Drawing;
using System.Windows.Forms;

namespace VotoTouch
{

    public partial class frmMain : Form
    {
        // DR16 - Classe intera

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
            //TAzionista c;
            // gestione degli stati della votazione
            switch (Stato)
            {
                case TAppStato.ssvBadge:
                    timAutoRitorno.Enabled = false;
                    oVotoTouch.CalcolaTouchSpecial(null);
                    //oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    SettaComponenti(false);
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
                    if (AperturaVotoEsterno != VTConfig.VotoAperto)
                    {
                        if (AperturaVotoEsterno)
                        {
                            // TODO: Possibili bachi: Ricaricamento Liste ad apertura votazione
                            Logging.WriteToLog("Evento Apertura votazione");
                            Rectangle FFormRect = new Rectangle(0, 0, this.Width, this.Height);
                            Votazioni.CaricaListeVotazioni(Data_Path, FFormRect, false);
                        }
                        else
                            Logging.WriteToLog("Evento Chiusura votazione");
                        // ok, ora setto la variabile locale di configurazione
                        VTConfig.VotoAperto = AperturaVotoEsterno;
                        // se la votazione è aperta il timer di controllo voto batte di meno
                        timVotoApero.Interval = VTConfig.VotoAperto ? VSDecl.TIM_CKVOTO_MAX : VSDecl.TIM_CKVOTO_MIN;
                    }

                    // a seconda dello stato, mostro il semaforo e metto l'immagine corretta
                    if (VTConfig.VotoAperto)
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
                    oVotoTouch.CalcolaTouchSpecial(Azionisti.HaDirittiDiVotoMultipli()
                                                       ? Votazioni.ClasseTipoVotoStartDiff
                                                       : Votazioni.ClasseTipoVotoStartNorm);
                    //oVotoTouch.CalcolaTouchSpecial(Stato, Azionisti.HaDirittiDiVotoMultipli());
                    oSemaforo.SemaforoOccupato();
                    // quà metto il voto differenziato
                    MettiComponentiStartVoto();
                    break;

                case TAppStato.ssvVoto:
                    // in generale non so quale è il voto corrente, perchè non c'è più una sequenza
                    // ma dipende dai diritti di voto dell'azionista, espressi o no, potrei avere espresso
                    // tutti i voti sulla prima e nessuno sulla seconda votazione.
                    // lo vedo caricando di volta in volta l'azionista che non ha diritti di voto espressi (havotato = false)

                    // ok, ora estraggo l'azionista o il gruppo di azionisti (se non è differenziato) che devono votare
                    // in Azionisti.AzionistiInVotoCorrente ho l'elenco dei diritti
                    // setto il voto corrente sul primo item dell'oggetto
                    if (Azionisti.EstraiAzionisti_VotoCorrente(IsVotazioneDifferenziata) &&
                        Votazioni.SetVotoCorrente(Azionisti.DammiIDVotazione_VotoCorrente()))
                    {
                        // calibro il touch sul voto
                        oVotoTouch.CalcolaTouchVote(Votazioni.VotoCorrente);
                        // ora devo capire che votazione è e mettere i componenti, attenzione che posso tornare da un'annulla
                        SettaComponenti(false);
                        // cancello i voti temporanei correnti 
                        CancellaTempVotiCorrenti();
                        // ora metto in quadro l'immagine, che deve essere presa da un file composto da
                        oVotoImg.LoadImages(VSDecl.IMG_voto + Votazioni.VotoCorrente.IDVoto.ToString());
                        // mostro comunque i diritti di voto in lbDirittiDiVoto e il nome di quello corrente
                        lbNomeDisgiunto.Text = rm.GetString("SAPP_VOTE_D_RASO") + "\n" +
                                               Azionisti.DammiNomeAzionistaInVoto_VotoCorrente(IsVotazioneDifferenziata);
                        lbNomeDisgiunto.Visible = true;
                        //lbNomeDisgiunto.Visible = (IsVotazioneDifferenziata || Azionisti.DammiCountDirittiDiVoto_VotoCorrente() ==1);
                        int dir_riman = IsVotazioneDifferenziata
                                            ? Azionisti.DammiTotaleDirittiRimanenti_VotoCorrente()
                                            : Azionisti.DammiCountDirittiDiVoto_VotoCorrente();
                        if (!IsVotazioneDifferenziata && dir_riman > 1)
                            lbNomeDisgiunto.Text += " e altre " + (dir_riman - 1).ToString() + " deleghe";
                        lbDirittiDiVoto.Text = dir_riman.ToString() + rm.GetString("SAPP_VOTE_D_DIRITTI");
                        lbDirittiDiVoto.Visible = true;
                    }
                    else
                    {
                        // si sono verificati dei problemi, lo segnalo
                        Logging.WriteToLog("Errore fn Azionisti.EstraiAzionisti_VotoCorrente(IsVotazioneDifferenziata), zero ");
                        MessageBox.Show("Si è verificato un errore (Azionisti.EstraiAzionisti_VotoCorrente(IsVotazioneDifferenziata))" + "\n\n" +
                            "Chiamare operatore esterno.\n\n ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Stato = TAppStato.ssvBadge;
                        CambiaStato();
                    }
                    break;

                case TAppStato.ssvVotoConferma:
                    oVotoTouch.CalcolaTouchSpecial(Votazioni.ClasseTipoVotoConferma);
                    //oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    // conferma
                    MettiComponentiConferma();
                    // ora metto in quadro l'immagine, che deve essere presa da un file composto da
                    oVotoImg.LoadImages(VSDecl.IMG_voto + Votazioni.VotoCorrente.IDVoto.ToString() + VSDecl.IMG_voto_c);
                    lbNomeDisgiunto.Visible = true; // (IsVotazioneDifferenziata || Azionisti.DammiCountDirittiDiVoto_VotoCorrente() == 1);
                    break;

                case TAppStato.ssvVotoContinua:
                    oVotoTouch.CalcolaTouchSpecial(null);
                    //oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    break;

                case TAppStato.ssvVotoFinito:
                    oVotoTouch.CalcolaTouchSpecial(null);
                    //oVotoTouch.CalcolaTouchSpecial(Stato, false);
                    lbDirittiDiVoto.Visible = false;
                    SettaComponenti(false);
                    // labels
                    lbDirittiDiVoto.Text = "";
                    // messaggio di arrivederci
                    if (VTConfig.UsaLettore)
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
                    {
                        oVotoImg.LoadImages(VSDecl.IMG_fine);
                        // ora devo vediricare se è attivo AttivaAutoRitornoVoto
                        if (VTConfig.AttivaAutoRitornoVoto)
                        {
                            timAutoRitorno.Enabled = true;
                        }
                    }

                    break;

                case TAppStato.ssvSalvaVoto:
                    if (Azionisti.DammiQuantiDirittiSonoStatiVotati() > VSDecl.MINVOTI_PROGRESSIVO)
                    {
                        // metti lo spinning wheel
                    }
                    // salvo i dati sul database
                    //oDBDati.SalvaTutto(Badge_Letto, TotCfg, ref Azionisti);
                    oDBDati.SalvaTutto(Badge_Letto, ref Azionisti);
                     
                    // togli lo spinning wheel
                    pbSalvaDati.Visible = false;
                    
                    oSemaforo.SemaforoFineOccupato();
                    Stato = TAppStato.ssvVotoFinito;
                    CambiaStato();
                    break;
            }
        }

        private void timVotoApero_Tick(object sender, EventArgs e)
        {
            // dr11 ok
            bool vtaperto;
            // devo verificare sul database se il voto per questa postazione è aperto
            int getvtaperto = oDBDati.CheckStatoVoto(VTConfig.NomeTotem);
            // se sono in una condizione di errore (es db non risponde) lascio il valore precedente
            if (getvtaperto != -1)
            {
                vtaperto = getvtaperto == 1;
                // se sono diversi e sono all'inizio allora cambio lo stato
                if (VTConfig.VotoAperto != vtaperto)
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

        private void timAutoRitorno_Tick(object sender, EventArgs e)
        {
            timAutoRitorno.Enabled = false;

            // esco
            TornaInizio();
        }

        #endregion

    
    }
}
