using System;
using System.Collections.Generic;
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
    // Classe di test

    public partial class frmMain : Form
    {
        private ArrayList badgelst = null;
        private System.Windows.Forms.Timer timTest;
        private bool TestOK = true;
        private int b_pos = 0;

        // questa è la routine di test
        // ci sarà un timer che batte ogni secondo e vede lo stato della finestra
        // in funzione di questo farà delle attività

        public void StopTest()
        {
            TestOK = false;
            b_pos = 0;
            timTest.Enabled = false;
            MessageBox.Show("Stop test by User");
        }

        public void StartTest()
        {
            timTest = new System.Windows.Forms.Timer();
            timTest.Enabled = false;
            timTest.Interval = 1000;
            timTest.Tick += timTest_Tick;

            badgelst = new ArrayList();

            // per prima cosa carica i badge validi
            if (oDBDati.DammiTuttiIBadgeValidi(ref badgelst))
            {
                TestOK = true;
                b_pos = 0;
                timTest.Enabled = true;

            }

        }

        private void timTest_Tick(object sender, EventArgs e)
        {
            if (!TestOK)
                timTest.Enabled = false;

            Test_VediDoveSei();
        }


        private void Test_VediDoveSei()
        {
            // ok, in che stato è la macchina a stati??

            switch (Stato)
            {

                // sono in attesa del badge, quindi prendo il primo della lista e simulo la lettura
                case TAppStato.ssvBadge:
                    if (b_pos < badgelst.Count)
                    {
                        string bdg = (string) badgelst[b_pos];
                        bdg = bdg.TrimEnd();
                        // devo aggiungere il codice impianto e la lunghezza giusta
                        string bdg2 = VTConfig.CodImpianto + PrependSpaces2(bdg, VTConfig.BadgeLen);

                        onDataReceived(null, bdg2 );
                        b_pos++;
                    }
                    else
                    {
                        TestOK = false;
                        MessageBox.Show("Test Finito!");
                    }
                    break;


                // sono nella videata di start, seleziono sempre il voto totale
                case TAppStato.ssvVotoStart:
                    // verifico se è differenziata o no
                    if (Azionisti.HaDirittiDiVotoMultipli())
                    {
                        Random random = new Random();
                        int quale = random.Next(1, 100);
                        if (quale <= 70)
                            onPremutoVotaNormale(null, 0);
                        else
                            onPremutoVotaDifferenziato(null, 0);
                    }
                    else
                        onPremutoVotaNormale(null, 0);
                    break;

                // sono nella videata di voto, seleziono il non voglio votare
                case TAppStato.ssvVoto:
                    // qua devo capire in che voto sono e quali liste ci sono
                    int liselez = 0;
                    int nliste = Votazioni.VotoCorrente.Liste.Count;
                    if (nliste > 1)
                    {
                        Random rListe = new Random();
                        liselez = rListe.Next(1, nliste) - 1;
                    }

                    onPremutoVotoValido(null, liselez, false);
                    // devo vedere
                   


                    //onPremutoNonVoto(null, 0);
                    break;

                // sono nella videata di conferma, confermo comunque
                case TAppStato.ssvVotoConferma:
                    Random random2 = new Random();
                    int quale2 = random2.Next(1, 100);
                    if (quale2 <= 70)
                        onPremutoConferma(null, 0);
                    else
                        onPremutoAnnulla(null, 0);
                    break;

                // voto finito non faccio nulla, aspetto che torni al badge
                case TAppStato.ssvVotoFinito:

                    break;

            }

        }

        public string PrependSpaces2(string str, int nu)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < (nu - str.Length); i++)
            {
                sb.Append("0");
            }
            sb.Append(str);
            return sb.ToString();
        }

    }

}
