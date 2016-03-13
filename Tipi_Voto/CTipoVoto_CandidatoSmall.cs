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
                        GetZone(ref a, 16, 31, 85, 50);
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
                            int str = 29; // partenza
                            int ha = 12; // altezza dei bottoni
                            int sp = 8; // spazio tra i bottoni
                            // ciclo
                            for (int z = 0; z < AVotazione.NListe; z++)
                            {
                                li = AVotazione.Liste[z];
                                a = new TTZone();
                                GetZone(ref a, 18, str, 83, str + ha);
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
                            int str = 27; // partenza
                            int ha = 10; // altezza dei bottoni
                            int sp = 5;  // spazio tra i bottoni
                            // ciclo
                            for (int z = 0; z < AVotazione.NListe; z++)
                            {
                                li = AVotazione.Liste[z];
                                a = new TTZone();
                                GetZone(ref a, 18, str, 83, str + ha);
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
                            int[] bx = new int[] { 9, 55, 9, 55 };
                            int[] by = new int[] { 28, 28, 48, 48 };
                            int[] bw = new int[] { 37, 37, 37, 37 };

                            // possono esserci delle differenze se sono 1 - 3 o 3 - 1
                            if (AVotazione.NPresentatoCDA == 1)   // 1 - 3
                            {
                                bx = new int[] { 35, 3, 35, 67 };
                                by = new int[] { 28, 48, 48, 48 };
                                bw = new int[] { 30, 30, 30, 30 };
                            }
                            if (AVotazione.NPresentatoCDA == 3)   // 3 - 1
                            {
                                bx = new int[] { 3, 35, 67, 35 };
                                by = new int[] { 28, 28, 28, 48 };
                                bw = new int[] { 30, 30, 30, 30 };
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
                            int ha = 10; // altezza dei bottoni
                            int bw = 37;
                            // schema 2 + 2
                            int[] bx = new int[] { 9, 55, 9, 55, 9 };
                            int[] by = new int[] { 27, 27, 42, 42, 57 };

                            // possono esserci delle differenze se sono 1 - 3 o 3 - 1
                            if (AVotazione.NPresentatoCDA == 1)   // 1 - 2 - 2
                            {
                                bx = new int[] { 31, 9, 55, 9, 55 };
                                by = new int[] { 27, 42, 42, 57, 57 };
                            }
                            if (AVotazione.NPresentatoCDA == 3)   // 2 - 1 - 2
                            {
                                bx = new int[] { 9, 55, 9, 9, 55 };
                                by = new int[] { 27, 27, 42, 57, 57 };
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
                            int ha = 10; // altezza dei bottoni
                            int bw = 37;
                            // schema 2 + 2
                            int[] bx = new int[] { 9, 55, 9, 55, 9, 55 };
                            int[] by = new int[] { 27, 27, 42, 42, 57, 57 };

                            // possono esserci delle differenze
                            if (AVotazione.NPresentatoCDA == 1)   // 1 3 2
                            {
                                bx = new int[] { 35, 3, 35, 67, 3, 35 };
                                by = new int[] { 26, 43, 43, 43, 58, 58 };
                                bw = 30;
                            }
                            if (AVotazione.NPresentatoCDA == 3)   // 3 - 3 
                            {
                                bx = new int[] { 3, 35, 67, 3, 35, 67 };
                                by = new int[] { 28, 28, 28, 50, 50, 50 };
                                bw = 30;
                                ha = 13;
                            }
                            if (AVotazione.NPresentatoCDA == 5)   // 3 - 2 - 1
                            {
                                bx = new int[] { 3, 35, 67, 3, 35, 35 };
                                by = new int[] { 26, 26, 26, 41, 41, 58 };
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
