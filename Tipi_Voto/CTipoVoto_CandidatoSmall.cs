using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Windows.Forms;


namespace VotoTouch
{
    public class CTipoVoto_CandidatoSmall: CBaseTipoVoto
    {

        // CLASSE DELLA votazione di candidato
		// Versione ORIGINALE da VotoSegreto
        
        public CTipoVoto_CandidatoSmall(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
        }

        //override public void GetTouchVoteZone(TAppStato AStato, TNewVotazione AVotazione, 
        //                                                bool ADiffer, ref ArrayList Tz )
        override public void GetTouchVoteZone(TNewVotazione AVotazione)
        {
            TTZone a;
            TNewLista li;

            Tz.Clear();

            // funziona almeno se in tutto ci sono <= 6 candidati e ci siano <= 4
            // presentati da cda, altrimenti è di pagina
            if (AVotazione.NListe <= 6)
            {
                switch (AVotazione.NListe)
                {                    
                    case 1:
                        #region 1 candidato
                        // 1 candidato presentato da cda / normale, è lo stesso, 99% che sarà sempre questo
                        li = AVotazione.Liste[0];
                        a = new TTZone();
                        GetZone(ref a, 160, 310, 850, 500);
                        a.expr = 0; a.ev = TTEvento.steVotoValido; a.Text = li.DescrLista;
                        a.Multi = 0; a.pag = 0; a.cda = li.PresentatodaCDA; // (AVotazione.NPresentatoCDA == 1); 
                        Tz.Add(a);
                        #endregion
                        break;

                    case 2:
                        #region 2 candidati
                        // 2 candidati presentato da cda o altro non importa li metto sempre in verticale
                        if (AVotazione.NListe == 2) // && AVotazione.NPresentatoCDA == 3)
                        {
                            int str = 290; // partenza
                            int ha = 120; // altezza dei bottoni
                            int sp = 80; // spazio tra i bottoni
                            // ciclo
                            for (int z = 0; z < AVotazione.NListe; z++)
                            {
                                li = AVotazione.Liste[z];
                                a = new TTZone();
                                GetZone(ref a, 180, str, 830, str + ha);
                                a.expr = z; a.ev = TTEvento.steVotoValido; a.Text = li.DescrLista;
                                a.Multi = 0; a.pag = 0; a.cda = li.PresentatodaCDA;
                                Tz.Add(a);
                                str = str + sp + ha;
                            }
                        }
                        #endregion
                        break;

                    case 3:
                        #region 3 candidati
                        // 3 candidati presentato da cda o altro non importa li metto sempre in verticale
                        if (AVotazione.NListe == 3) // && AVotazione.NPresentatoCDA == 3)
                        {
                            int str = 270; // partenza
                            int ha = 100; // altezza dei bottoni
                            int sp = 50;  // spazio tra i bottoni
                            // ciclo
                            for (int z = 0; z < AVotazione.NListe; z++)
                            {
                                li = AVotazione.Liste[z];
                                a = new TTZone();
                                GetZone(ref a, 180, str, 830, str + ha);
                                a.expr = z; a.ev = TTEvento.steVotoValido; a.Text = li.DescrLista;
                                a.Multi = 0; a.pag = 0; a.cda = li.PresentatodaCDA;
                                Tz.Add(a);
                                str = str + sp + ha;
                            }
                        }
                        #endregion
                        break;

                    case 4:
                        #region 4 candidati
                        // sono sempre in due linee, da capire come sono messi
                        // possono essere 1 - 3,  2 - 2,  3 - 1  
                        if (AVotazione.NListe == 4)
                        {
                            int ha = 13; // altezza dei bottoni
                            // schema 2 + 2
                            int[] bx = new int[] { 900, 550, 90, 550 };
                            int[] by = new int[] { 280, 280, 480, 480 };
                            int[] bw = new int[] { 370, 370, 370, 370 };

                            // possono esserci delle differenze se sono 1 - 3 o 3 - 1
                            if (AVotazione.NPresentatoCDA == 1)   // 1 - 3
                            {
                                bx = new int[] { 350, 30, 350, 670 };
                                by = new int[] { 280, 480, 480, 480 };
                                bw = new int[] { 300, 300, 300, 300 };
                            }
                            if (AVotazione.NPresentatoCDA == 3)   // 3 - 1
                            {
                                bx = new int[] { 30, 350, 670, 350 };
                                by = new int[] { 280, 280, 280, 480 };
                                bw = new int[] { 300, 300, 300, 300 };
                            }
                
                            // ciclo, tanto sono sempre ordinati prima cda e poi norm
                            for (int z = 0; z < AVotazione.NListe; z++)
                            {
                                li = AVotazione.Liste[z];
                                a = new TTZone();                        
                                // ok ora mi calcolo
                                GetZone(ref a, bx[z], by[z], bx[z] + bw[z], by[z] + ha);
                                a.expr = z; a.ev = TTEvento.steVotoValido; a.Text = li.DescrLista;
                                a.Multi = 0; a.pag = 0; a.cda = li.PresentatodaCDA;
                                Tz.Add(a);
                            }
                        }
                        #endregion
                        break;

                    case 5:
                        #region 5 candidati
                        // ***** 5 Candidati *****
                        // sono sempre in tre linee, da capire come sono messi
                        // 2 2 1
                        // 1 2 2
                        // 2 1 2
                        if (AVotazione.NListe == 5)
                        {
                            int ha = 100; // altezza dei bottoni
                            int bw = 370;
                            // schema 2 + 2
                            int[] bx = new int[] { 90, 550, 90, 550, 90 };
                            int[] by = new int[] { 270, 270, 420, 420, 570 };

                            // possono esserci delle differenze se sono 1 - 3 o 3 - 1
                            if (AVotazione.NPresentatoCDA == 1)   // 1 - 2 - 2
                            {
                                bx = new int[] { 310, 90, 550, 90, 550 };
                                by = new int[] { 270, 420, 420, 570, 570 };
                            }
                            if (AVotazione.NPresentatoCDA == 3)   // 2 - 1 - 2
                            {
                                bx = new int[] { 90, 550, 90, 90, 550 };
                                by = new int[] { 270, 270, 420, 570, 570 };
                            }

                            // ciclo, tanto sono sempre ordinati prima cda e poi norm
                            for (int z = 0; z < AVotazione.NListe; z++)
                            {
                                li = AVotazione.Liste[z];
                                a = new TTZone();
                                // ok ora mi calcolo
                                GetZone(ref a, bx[z], by[z], bx[z] + bw, by[z] + ha);
                                a.expr = z; a.ev = TTEvento.steVotoValido; a.Text = li.DescrLista;
                                a.Multi = 0; a.pag = 0; a.cda = li.PresentatodaCDA;
                                Tz.Add(a);
                            }
                        }
                        #endregion
                        break;
                
                    case 6:
                        #region 6 candidati
                        // ***** 6 Candidati *****
                        // sono sempre in tre linee, da capire come sono messi
                        // 2, 4 - 2 2 2
                        // 1 - 1 3 2
                        // 3 - 3 3 
                        // 5 - 3 2 1
                        if (AVotazione.NListe == 6)
                        {
                            int ha = 100; // altezza dei bottoni
                            int bw = 370;
                            // schema 2 + 2
                            int[] bx = new int[] { 90, 550, 90, 550, 90, 550 };
                            int[] by = new int[] { 270, 270, 420, 420, 570, 570 };

                            // possono esserci delle differenze
                            if (AVotazione.NPresentatoCDA == 1)   // 1 3 2
                            {
                                bx = new int[] { 350, 30, 350, 670, 30, 350 };
                                by = new int[] { 260, 430, 430, 430, 580, 580 };
                                bw = 30;
                            }
                            if (AVotazione.NPresentatoCDA == 3)   // 3 - 3 
                            {
                                bx = new int[] { 30, 350, 670, 30, 350, 670 };
                                by = new int[] { 280, 280, 280, 500, 500, 500 };
                                bw = 30;
                                ha = 13;
                            }
                            if (AVotazione.NPresentatoCDA == 5)   // 3 - 2 - 1
                            {
                                bx = new int[] { 30, 350, 670, 30, 350, 350 };
                                by = new int[] { 260, 260, 260, 410, 410, 580 };
                                bw = 30;
                            }

                            // ciclo, tanto sono sempre ordinati prima cda e poi norm
                            for (int z = 0; z < AVotazione.NListe; z++)
                            {
                                li = AVotazione.Liste[z];
                                a = new TTZone();
                                // ok ora mi calcolo
                                GetZone(ref a, bx[z], by[z], bx[z] + bw, by[z] + ha);
                                a.expr = z; a.ev = TTEvento.steVotoValido; a.Text = li.DescrLista;
                                a.Multi = 0; a.pag = 0; a.cda = li.PresentatodaCDA;
                                Tz.Add(a);
                            }
                        }
                        #endregion
                        break;
                }
            }

            // Le schede Speciali
            MettiSchedeSpeciali(AVotazione);

            // nella classe base c'è qualcosa
            base.GetTouchVoteZone(AVotazione);
        }

        //private void MettiSkBianca(TNewVotazione AVotazione, ref ArrayList Tz)
        //{
        //    TTZone a;

        //    // Ok, ora la scheda bianca e il non voto
        //    if (AVotazione.SkBianca && !AVotazione.SkNonVoto)
        //    {
        //        // la scheda bianca ( che è sempre l'ultima, quindi ntasti)
        //        a = new TTZone();
        //        GetZone(ref a, 23, 75, 78, 90);
        //        a.expr = VSDecl.VOTO_SCHEDABIANCA;
        //        a.pag = 0;
        //        a.cda = false;
        //        a.Multi = 0;
        //        a.Text = "";
        //        a.ev = TTEvento.steSkBianca;
        //        Tz.Add(a);
        //    }
        //    else
        //    {
        //        // Ok, ora la scheda bianca
        //        if (AVotazione.SkBianca)
        //        {
        //            a = new TTZone();
        //            // se c'è anche non voto devo spostarla
        //            //if (!AVotazione.SkNonVoto)
        //            //    GetZone(ref a, 32, 76, 67, 90); // non la sposto sta in centro
        //            //else
        //            GetZone(ref a, 10, 75, 45, 90); //la sposto a sinistra
        //            a.expr = VSDecl.VOTO_SCHEDABIANCA;
        //            a.Text = "";
        //            a.ev = TTEvento.steSkBianca;
        //            a.pag = 0;
        //            a.Multi = 0;
        //            Tz.Add(a);
        //        }
        //        // il non voto, se presente (caso BPM)
        //        if (AVotazione.SkNonVoto)
        //        {
        //            a = new TTZone();
        //            // se c'è anche SkBianca devo spostarla
        //            //if (!AVotazione.SkBianca)
        //            //    GetZone(ref a, 32, 75, 67, 90); // non la sposto, sta in centro
        //            //else
        //            //    GetZone(ref a, 55, 75, 90, 90); //la sposto a destra
        //            GetZone(ref a, 75, 88, 97, 100); // in bass a sx
        //            a.expr = VSDecl.VOTO_NONVOTO;
        //            a.Text = "";
        //            a.ev = TTEvento.steSkNonVoto;
        //            a.pag = 0;
        //            a.Multi = 0;
        //            Tz.Add(a);
        //        }
        //    }

        //}
        

    }
}
