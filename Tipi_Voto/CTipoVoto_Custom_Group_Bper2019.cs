using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    class CTipoVoto_Custom_Group_Bper2019 : CBaseTipoVoto
    {

        // CLASSE CUSTOM PER BPER 2019
        public CTipoVoto_Custom_Group_Bper2019(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
            CustomPaint = true;
        }

        public override void GetTouchVoteZone(TNewVotazione AVotazione)
        {
            // DR12 OK
            Tz.Clear();

            CalcolaTouch_Bper2019(AVotazione);

            // nella classe base c'è qualcosa
            // base.GetTouchVoteZone(AVotazione);
        }

        // calcolo del multitouch bper 2019 ------------------------------------------------------------------------

        public void CalcolaTouch_Bper2019(TNewVotazione AVotazione)
        {
            
        }



    }
}
