using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    public class CTipoVoto_Custom_Multi_Manutencoop: CBaseTipoVoto
    {

        // CLASSE CUSTOM PER MANUTENCOOP 2017
        
        public CTipoVoto_Custom_Multi_Manutencoop(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
            CustomPaint = true;
        }

       override public void GetTouchVoteZone(TNewVotazione AVotazione)
        {
            // DR12 OK
            Tz.Clear();

            CalcolaTouch_Manutencoop(AVotazione);
            
            // nella classe base c'è qualcosa
            base.GetTouchVoteZone(AVotazione);
        }

        // --------------------------------------------------------------
        //  CALCOLO DEL TOUCH MULTICANDIDATO PER MANUTENCOOP 2017
        // --------------------------------------------------------------

        #region calcolo candidato/multicandidato touch nuovo

        private const int basey = 119;
        private const int deltay = 74;      
        static private int y = basey;
        static private int b = y + deltay;
        
        private const int basex = 18;
        private const int deltax = 320;
        static private int x = basex;
        static private int r = x + deltax;

        public void CalcolaTouch_Manutencoop(TNewVotazione AVotazione)
        {
            TTZone a;

            // prima colonna
            // 1 - LEVORATO CLAUDIO
            Tz.Add(calczone(0, "LEVORATO CLAUDIO"));
            y += deltay; b += deltay;

            // 2 - ENZO GRENZI
            Tz.Add(calczone(1, "ENZO GRENZI"));
            y += deltay; b += deltay;

            // 3 - CHIARA FILIPPI
            Tz.Add(calczone(2, "CHIARA FILIPPI"));
            y += deltay; b += deltay;

            // 4 - LUCA BUGLIONE
            Tz.Add(calczone(3, "LUCA BUGLIONE"));
            y += deltay; b += deltay;

            // 5 - STEFANIA LORI
            Tz.Add(calczone(4, "STEFANIA LORI"));
            y += deltay; b += deltay;

            // 6 - CARMELA ARMENTO
            Tz.Add(calczone(5, "CARMELA ARMENTO"));
            y += deltay; b += deltay;

            // 7 - ALESSANDRO BENSI
            Tz.Add(calczone(6, "ALESSANDRO BENSI"));
            y += deltay; b += deltay;

            // 8 - SERGIO CAPPE’
            Tz.Add(calczone(7, "SERGIO CAPPE’"));
            y += deltay; b += deltay;

            // 9 - FABRIZIO LAGHI
            Tz.Add(calczone(8, "FABRIZIO LAGHI"));
            y += deltay; b += deltay;

            // 10 - GINO SERGIO BENASSI
            Tz.Add(calczone(9, "GINO SERGIO BENASSI"));
            y += deltay; b += deltay;

            // seconda colonna
            x += deltax;
            r += deltax;
            y = basey;
            b = y + deltay;

            // 11 - RAFFAELE POTRINO
            Tz.Add(calczone(10, "RAFFAELE POTRINO"));
            y += deltay; b += deltay;

            // 12 - LUIGI FACCHINI
            Tz.Add(calczone(11, "LUIGI FACCHINI"));
            y += deltay; b += deltay;

            // 13 - EMMA RAPONE
            Tz.Add(calczone(12, "EMMA RAPONE"));
            y += deltay; b += deltay;

            // 14 - MARIANGELA FONTANA
            Tz.Add(calczone(13, "MARIANGELA FONTANA"));
            y += deltay; b += deltay;

            // 15 - GIULIANO DI BERNARDO
            Tz.Add(calczone(14, "GIULIANO DI BERNARDO"));
            y += deltay; b += deltay;

            // 16 - CLAUDIO BONAFE’
            Tz.Add(calczone(15, "CLAUDIO BONAFE’"));
            y += deltay; b += deltay;

            // 17 - SABRINA ANNOVI
            Tz.Add(calczone(16, "SABRINA ANNOVI"));
            y += deltay; b += deltay;

            // 18 - MARIA D’AMELIO
            Tz.Add(calczone(17, "MARIA D’AMELIO"));
            y += deltay; b += deltay;

            // 19 - ALESSANDRO DALLA TORRE
            Tz.Add(calczone(18, "ALESSANDRO DALLA TORRE"));
            y += deltay; b += deltay;

            // terza colonna
            x += deltax;
            r += deltax;
            y = basey;
            b = y + deltay;

            // 20 - LICIA AVRAAM
            Tz.Add(calczone(19, "LICIA AVRAAM"));
            y += deltay; b += deltay;

            // 21 - PAOLO ZANIBONI
            Tz.Add(calczone(20, "PAOLO ZANIBONI"));
            y += deltay; b += deltay;

            // 22 - FRANCO PALAGANO
            Tz.Add(calczone(21, "FRANCO PALAGANO"));
            y += deltay; b += deltay;

            // 23 - ENRICO INVERNO
            Tz.Add(calczone(22, "ENRICO INVERNO"));
            y += deltay; b += deltay;

            // 24 - CINZIA CATERI
            Tz.Add(calczone(23, "CINZIA CATERI"));
            y += deltay; b += deltay;

            // 25 - ALFREDO DELLISANTI
            Tz.Add(calczone(24, "ALFREDO DELLISANTI"));
            y += deltay; b += deltay;

            // 26 - LAURA DUO’
            Tz.Add(calczone(25, "LAURA DUO’"));
            y += deltay; b += deltay;

            // 27 - GABRIELE STANZANI
            Tz.Add(calczone(26, "GABRIELE STANZANI"));
            y += deltay; b += deltay;

            // 28 - CRISTINA CAVICCHIOLI
            Tz.Add(calczone(27, "CRISTINA CAVICCHIOLI"));
            y += deltay; b += deltay;

            // devo aggiungere il tasto avanti con evento           
            a = new TTZone();
            GetZone(ref a, 670, 820, 1000, 1000);
            a.expr = VSDecl.VOTO_MULTIAVANTI;
            a.Text = ""; a.ev = TTEvento.steMultiAvanti; a.pag = 0; a.cda = false; a.Multi = 0;
            Tz.Add(a);

            // devo aggiungere il tasto uscita           
            a = new TTZone();
            GetZone(ref a, 24, 895, 240, 1000);
            a.expr = VSDecl.VOTO_BTN_USCITA;
            a.Text = ""; a.ev = TTEvento.steBottoneUscita; a.pag = 0; a.Multi = 0; 
            Tz.Add(a);

        }

        private TTZone calczone(int expr, string cand)
        {
            TTZone a = new TTZone();
            GetZone(ref a, x, y, r, b);
            a.expr = expr; a.pag = 0; a.cda = false; a.Multi = 0;
            a.PaintMode = VSDecl.PM_ONLYCHECK;
            a.Text = cand; a.ev = TTEvento.steMultiValido;
            a.CKRect = new Rectangle(a.x + 23, a.y + 5, 56, 60);

            return a;
        }

        /*       
        LEVORATO CLAUDIO        ENZO GRENZI             CHIARA FILIPPI        LUCA BUGLIONE        STEFANIA LORI        
        CARMELA ARMENTO        ALESSANDRO BENSI        SERGIO CAPPE’        FABRIZIO LAGHI        GINO SERGIO BENASSI
        
        RAFFAELE POTRINO        LUIGI FACCHINI        EMMA RAPONE        MARIANGELA FONTANA        GIULIANO DI BERNARDO
        CLAUDIO BONAFE’        SABRINA ANNOVI        MARIA D’AMELIO       ALESSANDRO DALLA TORRE        
        
        LICIA AVRAAM            PAOLO ZANIBONI        FRANCO PALAGANO        ENRICO INVERNO        CINZIA CATERI        
        ALFREDO DELLISANTI        LAURA DUO’        GABRIELE STANZANI        CRISTINA CAVICCHIOLI
         
        
        */

        #endregion

    }
}
