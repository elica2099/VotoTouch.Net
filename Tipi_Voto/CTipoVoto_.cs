using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoSegreto
{
    public class CTipoVoto_CandidatoOriginal: CBaseTipoVoto
    {

        // CLASSE DELLA votazione di candidato
		// Versione ORIGINALE da VotoSegreto
        
        public CTipoVoto_CandidatoOriginal(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
        }

        override public void GetTouchVoteZone(TAppStato AStato, ref TParVotazione AFVotaz, 
                                                        bool ADiffer, ref ArrayList Tz )
        {
            // DR12 OK
            TTZone a;

        }


    }
}
