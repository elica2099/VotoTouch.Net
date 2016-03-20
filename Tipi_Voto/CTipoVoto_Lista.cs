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
                GetZone(ref a, 16, 24, 85, 68); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 2 Liste
            if (AVotazione.NListe == 2)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 9, 24, 43, 68); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 56, 24, 90, 68); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 3 Liste
            if (AVotazione.NListe == 3)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 3, 24, 31, 68); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 36, 24, 63, 68); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 68, 24, 96, 68); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 4 Liste
            if (AVotazione.NListe == 4)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 5, 22, 45, 44); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 54, 22, 94, 44); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 5, 49, 45, 71); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 4° Lista
                a = new TTZone();
                GetZone(ref a, 54, 49, 94, 71); a.expr = 3; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 5 Liste
            if (AVotazione.NListe == 5)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 2, 23, 30, 43); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 34, 23, 65, 43); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 69, 23, 98, 43); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // seconda riga
                // 4° Lista
                a = new TTZone();
                GetZone(ref a, 17, 48, 45, 68); a.expr = 3; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 5° Lista
                a = new TTZone();
                GetZone(ref a, 54, 48, 82, 68); a.expr = 4; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
            }
            // 6 Liste
            if (AVotazione.NListe == 6)
            {
                // 1° Lista
                a = new TTZone();
                GetZone(ref a, 2, 23, 30, 43); a.expr = 0; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 2° Lista
                a = new TTZone();
                GetZone(ref a, 34, 23, 65, 43); a.expr = 1; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 3° Lista
                a = new TTZone();
                GetZone(ref a, 69, 23, 98, 43); a.expr = 2; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // seconda riga
                // 4° Lista
                a = new TTZone();
                GetZone(ref a, 2, 48, 30, 68); a.expr = 3; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 5° Lista
                a = new TTZone();
                GetZone(ref a, 34, 48, 65, 68); a.expr = 4; a.pag = 0; a.Multi = 0;
                a.Text = ""; a.ev = TTEvento.steVotoValido;
                Tz.Add(a);
                // 6° Lista
                a = new TTZone();
                GetZone(ref a, 69, 48, 98, 68); a.expr = 5; a.pag = 0; a.Multi = 0;
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
