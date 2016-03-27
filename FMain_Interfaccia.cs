using System;
using System.Globalization;
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
        //	GESTIONE DELL'AREA SENSIBILE
        // ----------------------------------------------------------------
        //******************************************************************************

        private void frmMain_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // chiamo il metodo in CTouchscreen che mi ritornerà eventi diversi a seconda del caso
            oVotoTouch.TastoPremuto(sender, e, Stato);
        }

        //******************************************************************************
        // ----------------------------------------------------------------
        //	 STATO DI START VOTE
        // ----------------------------------------------------------------
        //******************************************************************************

        private void MettiComponentiStartVoto()
        {
            string PrefNomeAz = "";
            // DR->OK11	
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
                string ss = string.Format("{0:N0}", Azionisti.DammiMaxNumeroAzioniTotali());
                lbDirittiDiVoto.Text = ss;
                lbDirittiStart.Text = ss;
            }
            // in funzione del n. di deleghe metto
            if (Azionisti.HaDirittiDiVotoMultipli())
            {
                oVotoImg.LoadImages(VSDecl.IMG_VotostartD);
                // sono le label del differenziato
                lbDirittiStart.Visible = true;
            }
            else
            {
                // immagine di 1 voto
                oVotoImg.LoadImages(VSDecl.IMG_Votostart1);
                if (VTConfig.ModoAssemblea == VSDecl.MODO_AGM_SPA)
                    lbDirittiStart.Visible = true;
            }
        }


        // ************************************************************************
        // ----------------------------------------------------------------
        //    CONFERMA/SALVATAGGIO DEL VOTO
        // ----------------------------------------------------------------
        // ************************************************************************

        private void MettiComponentiConferma()
        {
            bool NODirittiLabel = false;

            // crea la pagina di conferma
            SettaComponenti(false);
            lbDirittiDiVoto.Visible = true;
            // Sistemo la label dei diritti di voto
            int NDirittiAzioniConferma = Azionisti.DammiDirittiAzioniDiVotoConferma(IsVotazioneDifferenziata);
            lbConfermaNVoti.Text = string.Format("{0:N0}", NDirittiAzioniConferma); // +rm.GetString("SAPP_VOTE_DIRITTIPER");

            //if (VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP)
            //{
            //if (IsVotazioneDifferenziata)
            //    lbConfermaNVoti.Text = rm.GetString("SAPP_VOTE_1DIRITTOPER"); //"1 diritto di voto per";
            //else
            //{
            //    if (!Azionisti.HaDirittiDiVotoMultipli())
            //        lbConfermaNVoti.Text = rm.GetString("SAPP_VOTE_1DIRITTOPER"); //"1 diritto di voto per";
            //    else
            //        lbConfermaNVoti.Text = Azionisti.DammiCountDirittiDiVoto_VotoCorrente() +
            //                               rm.GetString("SAPP_VOTE_DIRITTIPER"); //" diritti di voto per";
            //}
            //}
            //else
            //{
                
            //}

            // ok, per ora distinguiamo tra i due metodi di voto, quello normale e quello multicandidato
            // che ha i voti salvati in una collection
            // in un secondo tempo dovrà essere unificato
            if (Votazioni.VotoCorrente.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
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
                oVotoTheme.SetTheme_lbConfermaUp(ref lbConfermaUp);
            }

            // attenzione, se ho una sk bianca o non voto non metto i diritii
            if (NODirittiLabel)
            {
                lbConfermaNVoti.Text = "";
            }

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
            Font MyFont = new Font(VSDecl.BTN_FONT_NAME, VSDecl.BTN_FONT_SIZE, FontStyle.Bold);

            lbDirittiStart.BackColor = PaintTouch ? Color.Tan : Color.Transparent;
            lbDirittiDiVoto.BackColor = PaintTouch ? Color.Coral : Color.Transparent;
            // il pannello della conferma
            lbConferma.BackColor = PaintTouch ? Color.Red : Color.Transparent;
            lbConfermaUp.BackColor = PaintTouch ? Color.Turquoise : Color.Transparent;
            lbConfermaNVoti.BackColor = PaintTouch ? Color.GreenYellow : Color.Transparent;

            if (DebugMode) pnBadge.Visible = true;
        }

        // ----------------------------------------------------------------
        //  SETTAGGIO DEI COMPONENTI A INIZIO CAMBIO STATO
        // ----------------------------------------------------------------

        private void SettaComponenti(bool AVisibile)
        {
            // DR->OK				
            lbConferma.Visible = AVisibile;
            lbConfermaUp.Visible = AVisibile;
            lbConfermaNVoti.Visible = AVisibile;
            // label del differenziato
            lbDirittiStart.Visible = AVisibile;

            //lbNome.Visible = AVisibile;
            lbNomeDisgiunto.Visible = AVisibile;
            lbDisgiuntoRimangono.Visible = AVisibile;
            lbNomeAzStart.Visible = AVisibile;

            if (DemoVersion)
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
