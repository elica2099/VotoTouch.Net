using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    public class CTipoVoto_AStart: CBaseTipoVoto
    {

        // CLASSE DELLA PAGINA DI START
        
        public CTipoVoto_AStart(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
        }

        //override public void GetTouchVoteZone(TAppStato AStato, TNewVotazione AFVotaz, 
        //                                                bool ADiffer, ref ArrayList Tz )
        override public void GetTouchSpecialZone(TAppStato AStato, bool ADiffer)
        {
            // DR12 OK
            TTZone a;
            Tz.Clear();

            if (ADiffer)
			{
				// differenziato tasto grande
				a = new TTZone();
				GetZone(ref a, 9, 45, 57, 90); a.expr = 0; a.pag = 0; a.Multi = 0;
				a.Text = ""; a.ev = TTEvento.steVotaNormale;
				Tz.Add(a);
				// differenziato tasto piccolo
				a = new TTZone();
				GetZone(ref a, 62, 52, 93, 90); a.expr = 1; a.pag = 0; a.Multi = 0;
				a.Text = ""; a.ev = TTEvento.steVotaDiffer;
				Tz.Add(a);
			}
			else
			{
				// normale, tutto lo schermo
				a = new TTZone();
				GetZone(ref a, 2, 2, 98, 98); a.expr = 0; a.pag = 0; a.Multi = 0;
				a.Text = ""; a.ev = TTEvento.steVotaNormale;
				Tz.Add(a);
			}

		}

    }
}
