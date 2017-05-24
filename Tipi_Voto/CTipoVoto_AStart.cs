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
        override public void GetTouchSpecialZone(TAppStato AStato, bool ADiffer, bool ABtnUscita)
        {
            // DR12 OK
            TTZone a;
            Tz.Clear();

            if (ADiffer)
			{
			        // differenziato tasto grande
			        a = new TTZone();
                    switch (VTConfig.ModoPosizioneAreeTouch)
			        {
                        case VSDecl.MODO_POS_TOUCH_NORMALE:
                            GetZone(ref a, 90, 450, 570, 900);
			                break;
                        case VSDecl.MODO_POS_TOUCH_MODERN:
                            GetZone(ref a, 120, 450, 640, 900);      
			                break;
                        case VSDecl.MODO_POS_TOUCH_BIG_BTN:
                            GetZone(ref a, 120, 450, 735, 960);      
                            break;
			        }
                    a.expr = 0; a.pag = 0; a.Multi = 0;			        
                    a.Text = "";
			        a.ev = TTEvento.steVotaNormale;
			        Tz.Add(a);
			        // differenziato tasto piccolo
			        a = new TTZone();
                    switch (VTConfig.ModoPosizioneAreeTouch)
                    {
                        case VSDecl.MODO_POS_TOUCH_NORMALE:
                            GetZone(ref a, 620, 520, 930, 900);
                            break;
                        case VSDecl.MODO_POS_TOUCH_MODERN:
                            GetZone(ref a, 690, 520, 960, 900);
                            break;
                        case VSDecl.MODO_POS_TOUCH_BIG_BTN:
                            GetZone(ref a, 760, 520, 990, 960);
                            break;
                    }
                    a.expr = 1;
			        a.pag = 0;
			        a.Multi = 0;
			        a.Text = "";
			        a.ev = TTEvento.steVotaDiffer;
			        Tz.Add(a);
			}
			else
			{
				// normale, tutto lo schermo
				a = new TTZone();
				GetZone(ref a, 20, 20, 980, 980); a.expr = 0; a.pag = 0; a.Multi = 0;
				a.Text = ""; a.ev = TTEvento.steVotaNormale;
				Tz.Add(a);
			}

            base.GetTouchSpecialZone(AStato, ADiffer, ABtnUscita);
        }

    }
}
