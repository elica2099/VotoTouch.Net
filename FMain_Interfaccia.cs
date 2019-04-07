using System.Drawing;
using System.Windows.Forms;

namespace VotoTouch
{
    public partial class frmMain : Form
    {
        // DR16 - Classe intera

        //******************************************************************************
        // ----------------------------------------------------------------
        //	GESTIONE DELL'AREA SENSIBILE
        // ----------------------------------------------------------------
        //******************************************************************************

        private void frmMain_MouseUp(object sender, MouseEventArgs e)
        {
            // chiamo il metodo in CTouchscreen che mi ritornerà eventi diversi a seconda del caso
            oVotoTouch.TastoPremuto(sender, e, Stato);
        }

        //******************************************************************************
        // ----------------------------------------------------------------
        //	 STATO DI START VOTE
        // ----------------------------------------------------------------
        //******************************************************************************

        private void MettiComponentiStartVoto(TStartVoteMode AModoStart)
        {
            string PrefNomeAz = "";
            // start del voto
            SettaComponenti(false);
            edtBadge.Text = "";
            // le labels
            // nome azionista
            PrefNomeAz = Azionisti.Titolare_Badge.RaSo_Sesso;

            PrefNomeAz = UppercaseWords(PrefNomeAz.ToLower());
            lbNomeAzStart.Text = PrefNomeAz;
            lbNomeAzStart.Visible = true;
            // diritti di voto  
            if (VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP)
            {
                string ss = string.Format("{0:N0}", Azionisti.DammiMaxNumeroDirittiDiVotoTotali());
                lbDirittiDiVoto.Text = ss + rm.GetString("SAPP_VOTE_D_DIRITTI"); // " Diritti di voto";
                lbDirittiStart.Text = ss;
            }
            else
            {
                string ss = string.Format("{0:N0}", Azionisti.DammiMaxNumeroVotiTotali());
                lbDirittiDiVoto.Text = ss;
                lbDirittiStart.Text = ss;
            }
            // diritti di voto
            lbDirittiStart.Visible = true;

            switch (AModoStart)
            {
                case TStartVoteMode.vszNormal:
                    // immagine di 1 voto
                    oVotoImg.LoadImages(VSDecl.IMG_Votostart1);
                    break;
                case TStartVoteMode.vszMixedDiffer:
                    // verifico se ho AbilitaDifferenziatoSuRichiesta Attivato
                    if (VTConfig.AbilitaDifferenziatoSuRichiesta)
                    {
                        oVotoImg.LoadImages(LocalAbilitaVotazDifferenziataSuRichiesta
                            ? VSDecl.IMG_VotostartD
                            : VSDecl.IMG_Votostart1);
                        return;
                    }
                    // si comporta normalmente
                    oVotoImg.LoadImages(VSDecl.IMG_VotostartD);
                    break;
                case TStartVoteMode.vszOnlyDiffer:
                    // solo il differenziato
                    oVotoImg.LoadImages(VSDecl.IMG_VotostartD_AK);
                    break;
            }

            //// in funzione del n. di deleghe metto
            //if (Azionisti.HaDirittiDiVotoMultipli())
            //{
            //    // verifico se ho AbilitaDifferenziatoSuRichiesta Attivato
            //    if (VTConfig.AbilitaDifferenziatoSuRichiesta)
            //    {
            //        oVotoImg.LoadImages(LocalAbilitaVotazDifferenziataSuRichiesta
            //                                ? VSDecl.IMG_VotostartD
            //                                : VSDecl.IMG_Votostart1);
            //        return;
            //    }
            //    // verifico se è abilitato AK
            //    if (VTConfig.AKCheckVote && 
            //        (Azionisti.checkAKIsPresentDelegheDifformi(Votazioni.getAKSchedeDisabilitateInAllVote()) == VSDecl.AK_SKDIFF_PRESENT_MIXED ))
            //    {
            //        oVotoImg.LoadImages(VSDecl.IMG_VotostartD_AK);
            //        return;
            //    }
            //    // si comporta normalmente
            //    oVotoImg.LoadImages(VSDecl.IMG_VotostartD);
            //}
            //else
            //{
            //    // immagine di 1 voto
            //    oVotoImg.LoadImages(VSDecl.IMG_Votostart1);
            //}
        }


        // ************************************************************************
        // ----------------------------------------------------------------
        //    CONFERMA/SALVATAGGIO DEL VOTO
        // ----------------------------------------------------------------
        // ************************************************************************

        private void MettiComponentiConferma()
        {
            //bool NODirittiLabel = false;

            // crea la pagina di conferma
            //SettaComponenti(false);
            lbDirittiDiVoto.Visible = true;
            // Sistemo la label dei diritti di voto
            int NDirittiAzioniConferma = Azionisti.DammiDirittiAzioniDiVotoConferma(IsVotazioneDifferenziata);
            lbConfermaNVoti.Text = string.Format("{0:N0}", NDirittiAzioniConferma) + rm.GetString("SAPP_VOTE_VOTIPER");

            if (VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP)
            {
                if (IsVotazioneDifferenziata)
                    lbConfermaNVoti.Text = rm.GetString("SAPP_VOTE_1DIRITTOPER"); //"1 diritto di voto per";
                else
                {
                    if (!Azionisti.HaDirittiDiVotoMultipli())
                        lbConfermaNVoti.Text = rm.GetString("SAPP_VOTE_1DIRITTOPER"); //"1 diritto di voto per";
                    else
                        lbConfermaNVoti.Text = Azionisti.DammiCountDirittiDiVoto_VotoCorrente() +
                                               rm.GetString("SAPP_VOTE_DIRITTIPER"); //" diritti di voto per";
                }
            }
            else
            {
                // TODO: lbConfermaNVoti METTERE AZIONI????
            }

            // ok, per ora distinguiamo tra i due metodi di voto, quello normale e quello multicandidato
            // che ha i voti salvati in una collection
            // in un secondo tempo dovrà essere unificato
            if (Votazioni.VotoCorrente.TipoVoto == TTipoVoto.stvMultiCandidato) // VSDecl.VOTO_MULTICANDIDATO)
            {
                /*
                // ciclo e metto i candidati
                TVotoEspresso vt;
                bool acapo = false;
                lbConfermaUp.Text = "";
                lbConferma.Text = "";
                int cnt = FVotiExpr.Count;
                // ok, ora riempio la collection di voti
                for (int i = 0; i < cnt; i++)
                {
                    vt = (TVotoEspresso)FVotiExpr[i];

                    // se è sk bianca o non voto non metto i diritti
                    NODirittiLabel = (vt.VotoExp_IDScheda == VSDecl.VOTO_SCHEDABIANCA || vt.VotoExp_IDScheda == VSDecl.VOTO_NONVOTO);

                    lbConfermaUp.Text += vt.StrUp_DescrLista;
                    if (acapo)
                        lbConfermaUp.Text += "\n";
                    else
                    {
                        if (i < (cnt - 1))   // per evitarmi l'ultimo " - "
                            lbConfermaUp.Text += "  -  ";
                    }
                    acapo = !acapo;
                }
                 * */
                lbConferma.Text = VotoEspressoStrUp;
                lbConferma.TextNote = VotoEspressoStrNote;
                lbConferma.Visible = true;
                //oVotoTheme.SetTheme_lbConfermaUp_Cand(ref lbConfermaUp);
            }
            else
            {
                // se è sk bianca o non voto non metto i diritti
                //NODirittiLabel = (VotoEspresso == VSDecl.VOTO_SCHEDABIANCA || VotoEspresso == VSDecl.VOTO_NONVOTO);
                if (VotoEspresso == VSDecl.VOTO_SCHEDABIANCA || VotoEspresso == VSDecl.VOTO_NONVOTO)
                    lbConfermaNVoti.Text = "-";
                // voto di lista/candidato              
                lbConfermaUp.Text = VotoEspressoStrUp;
                lbConferma.Text = VotoEspressoStr;
                lbConferma.TextNote = VotoEspressoStrNote;
                oVotoTheme.SetTheme_lbConfermaUp(ref lbConfermaUp);
            }

            // attenzione, se ho una sk bianca o non voto non metto i diritii
            //if (NODirittiLabel)
            //{
            //    lbConfermaNVoti.Text = "";
            //}

            // ok, ora le mostro
            lbConferma.Visible = true;
            lbConfermaNVoti.Visible = true;
            lbConfermaUp.Visible = true;
        }

        // *************************************************************************
        // ----------------------------------------------------------------
        //   INTERFACCIA GRAFICA
        // ----------------------------------------------------------------
        // *************************************************************************

        // ----------------------------------------------------------------
        //   Caricamento Tema da VotoTheme
        // ----------------------------------------------------------------

        private void CaricaTemaInControlli()
        {
            // carico il tema da vototouch, semplicemente richiamando le singole label
            oVotoTheme.SetTheme_lbNomeDisgiunto(ref lbNomeDisgiunto);
            oVotoTheme.SetTheme_lbDisgiuntoRimangono(ref lbDisgiuntoRimangono);
            oVotoTheme.SetTheme_lbDirittiStart(ref lbDirittiStart);
            oVotoTheme.SetTheme_lbDirittiDiVoto(ref lbDirittiDiVoto);
            oVotoTheme.SetTheme_lbConferma(ref lbConferma);
            oVotoTheme.SetTheme_lbConfermaUp(ref lbConfermaUp);
            oVotoTheme.SetTheme_lbConfermaNVoti(ref lbConfermaNVoti);
            oVotoTheme.SetTheme_lbNomeAzStart(ref lbNomeAzStart);
        }

        // ----------------------------------------------------------------
        // Inizializzazione dei controlli
        // ----------------------------------------------------------------

        private void InizializzaControlli()
        {
            //Font MyFont = new Font(VSDecl.BTN_FONT_NAME, VSDecl.BTN_FONT_SIZE, FontStyle.Bold);

            lbDirittiStart.BackColor = VTConfig.IsPaintTouch ? Color.Tan : Color.Transparent;
            lbDirittiDiVoto.BackColor = VTConfig.IsPaintTouch ? Color.Coral : Color.Transparent;
            // il pannello della conferma
            lbConferma.BackColor = VTConfig.IsPaintTouch ? Color.Red : Color.Transparent;
            lbConfermaUp.BackColor = VTConfig.IsPaintTouch ? Color.Turquoise : Color.Transparent;
            lbConfermaNVoti.BackColor = VTConfig.IsPaintTouch ? Color.GreenYellow : Color.Transparent;

            if (VTConfig.IsDebugMode) pnBadge.Visible = true;
        }

        // ----------------------------------------------------------------
        //  SETTAGGIO DEI COMPONENTI A INIZIO CAMBIO STATO
        // ----------------------------------------------------------------

        private void SettaComponenti(bool AVisibile)
        {
            lbConferma.Visible = AVisibile;
            lbConfermaUp.Visible = AVisibile;
            lbConfermaNVoti.Visible = AVisibile;
            // label del differenziato
            lbDirittiStart.Visible = AVisibile;

            //lbNome.Visible = AVisibile;
            lbNomeDisgiunto.Visible = AVisibile;
            lbDisgiuntoRimangono.Visible = AVisibile;
            lbNomeAzStart.Visible = AVisibile;

            if (VTConfig.IsDemoMode)
            {
                if (btnBadgeUnVoto != null)
                    btnBadgeUnVoto.Visible = (Stato == TAppStato.ssvBadge);
                if (btnBadgePiuVoti != null)
                    btnBadgePiuVoti.Visible = (Stato == TAppStato.ssvBadge);
                if (btnFineVotoDemo != null)
                    btnFineVotoDemo.Visible = (Stato == TAppStato.ssvVotoFinito);
            }
        }

        private void TornaInizio()
        {
            // dall'inizio
            timAutoRitorno.Enabled = false;
            Stato = TAppStato.ssvBadge;
            CambiaStato();
        }


    }
}
