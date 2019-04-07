using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    public class CTipoVoto_AK_NoAbilitato: CBaseTipoVoto
    {

        // CLASSE DELLA PAGINA DI voto AK, quando il voto non è abilitato
        
        public CTipoVoto_AK_NoAbilitato(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
        }

        //override public void GetTouchVoteZone(TAppStato AStato, TNewVotazione AFVotaz, 
        //                                                bool ADiffer, ref ArrayList Tz )
        public override void GetTouchSpecialZone(TAppStato AStato, TStartVoteMode AMode, bool ABtnUscita)
        {
            // DR12 OK
            TTZone a;
            Tz.Clear();

            // normale, tutto lo schermo, ma disabilita il voto
            a = new TTZone();
            GetZone(ref a, 20, 130, 980, 980);
            a.expr = VSDecl.VOTO_AK;
            a.pag = 0; a.Multi = 0;
            a.Text = ""; a.ev = TTEvento.ste_AK_Avanti;
            Tz.Add(a);

            base.GetTouchSpecialZone(AStato, AMode, ABtnUscita);
        }

    }
}
