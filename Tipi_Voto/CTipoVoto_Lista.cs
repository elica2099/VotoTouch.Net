using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    public class CTipoVoto_Lista: CBaseTipoVoto
    {

        // CLASSE DELLA votazione di lista
        
        public CTipoVoto_Lista(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
        }

        //override public void GetTouchVoteZone(TAppStato AStato, TNewVotazione AVotazione, 
        //                                                bool ADiffer, ref ArrayList Tz )
        override public void GetTouchVoteZone(TNewVotazione AVotazione)
        {
            // DR12 OK
            TTZone a;

            Tz.Clear();

            // ok, in funzione dell liste e della votazione faccio
            if (AVotazione.NListe == 1)
            {
                a = new TTZone();
                GetZone(ref a, 160, 240, 850, 680); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 2 Liste
            if (AVotazione.NListe == 2)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 80, 240, 430, 680); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 560, 240, 920, 680); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 3 Liste
            if (AVotazione.NListe == 3)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 30, 240, 310, 680); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 360, 240, 630, 680); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 680, 240, 960, 680); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 4 Liste
            if (AVotazione.NListe == 4)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 50, 220, 450, 440); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 540, 220, 940, 440); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 50, 490, 450, 710); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 4° Lista
                a = new TTZone();
                GetZone(ref a, 540, 490, 940, 710); a.expr = 3; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 5 Liste
            if (AVotazione.NListe == 5)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 20, 230, 300, 430); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 340, 230, 650, 430); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 690, 230, 980, 430); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // seconda riga
                // 4° Lista
                a = new TTZone();
                GetZone(ref a, 170, 480, 450, 680); a.expr = 3; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 5° Lista
                a = new TTZone();
                GetZone(ref a, 540, 480, 820, 680); a.expr = 4; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 6 Liste
            if (AVotazione.NListe == 6)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 20, 230, 300, 430); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 340, 230, 650, 430); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 690, 230, 980, 430); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // seconda riga
                // 4° Lista
                a = new TTZone();
                GetZone(ref a, 20, 480, 300, 680); a.expr = 3; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 5° Lista
                a = new TTZone();
                GetZone(ref a, 340, 480, 650, 680); a.expr = 4; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 6° Lista
                a = new TTZone();
                GetZone(ref a, 690, 480, 980, 680); a.expr = 5; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }

            // Le schede Speciali
            MettiSchedeSpeciali(AVotazione);

            // nella classe base c'è qualcosa
            base.GetTouchVoteZone(AVotazione);
        }


    }
}
