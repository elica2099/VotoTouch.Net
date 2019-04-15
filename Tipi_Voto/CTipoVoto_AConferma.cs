using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    public class CTipoVoto_AConferma: CBaseTipoVoto
    {

        // CLASSE DELLA votazione di candidato
		// Versione ORIGINALE da VotoSegreto
        
        public CTipoVoto_AConferma(Rectangle AFormRect) : base(AFormRect)
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

			 // Bottone Annulla
			 a = new TTZone();
             if (VTConfig.ModoPosizioneAreeTouch == VSDecl.MODO_POS_TOUCH_NORMALE)
                 GetZone(ref a, 80, 600, 450, 900); 
             else
                 GetZone(ref a, 140, 660, 450, 900);  
             a.expr = 0; a.pag = 0; a.Multi = 0; 
			 a.Text = ""; a.ev = TTEvento.steAnnulla;
			 Tz.Add(a);
			 // Bottone Conferma
			 a = new TTZone();
             if (VTConfig.ModoPosizioneAreeTouch == VSDecl.MODO_POS_TOUCH_NORMALE)
			    GetZone(ref a, 550, 600, 920, 900);  
             else
			    GetZone(ref a, 550, 660, 860, 900);   
			 a.expr = 1; a.pag = 0; a.Multi = 0;
             a.Text = ""; a.ev = TTEvento.steConferma;
			 Tz.Add(a);

			 // da vedere: conferma anche se schiaccia il candidato
             //a = new TTZone();
             //GetZone(ref a, 12, 16, 88, 52); a.expr = 1; a.pag = 0; a.Multi = 0;  
             //a.Text = ""; a.ev = TTEvento.steConferma;
             //Tz.Add(a);

             base.GetTouchSpecialZone(AStato, ADiffer, ABtnUscita);
        }


    }
}
