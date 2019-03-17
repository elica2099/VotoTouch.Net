using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Media;

namespace VotoTouch
{
    public class CBaseTipoVoto
    {

        public Rectangle FFormRect;
        protected ArrayList Tz;

        public ArrayList TouchZone { get { return Tz ?? null; } }

        public const float Nqx = VSDecl.VOTESCREEN_DIVIDE_WIDTH;
        public const float Nqy = VSDecl.VOTESCREEN_DIVIDE_HEIGHT;

        public const float HRETT_CANDIDATO = 6F; //67px;

        protected bool CustomPaint = false;

        public CBaseTipoVoto(Rectangle AFormRect)		
        {
            // costruttore

            // inizializzo
            FFormRect = new Rectangle();
            FFormRect = AFormRect;

            Tz = new ArrayList();
        }


        // --------------------------------------------------------------------------
        //  FUNZIONI VIRTUALI
        // --------------------------------------------------------------------------

        public virtual void GetTouchVoteZone(TVotazione AVotazione) //ref ArrayList Tz)
        {
            // l'implementazione è nelle varie classi

            //c 'è una parte comune
            // il Bottone Uscita
            if (!CustomPaint && AVotazione.AbilitaBottoneUscita)
            {
                TTZone a = new TTZone();
                GetZone(ref a, 760, 0, 980, 120); // in alto a dx
                a.expr = VSDecl.VOTO_BTN_USCITA;
                a.Text = ""; a.ev = TTEvento.steBottoneUscita; a.pag = 0; a.Multi = 0; a.MultiNoPrint = true;
                Tz.Add(a);
            }
        }

        public virtual void GetTouchSpecialZone(TAppStato AStato, bool ADiffer, bool ABtnUscita) //, ref ArrayList Tz
        {
            // l'implementazione è nelle varie classi

            //c 'è una parte comune
            // il Bottone Uscita
            if (ABtnUscita)
            {
                TTZone a = new TTZone();
                GetZone(ref a, 760, 0, 980, 120); // in alto a dx
                a.expr = VSDecl.VOTO_BTN_USCITA;
                a.Text = ""; a.ev = TTEvento.steBottoneUscita; a.pag = 0; a.Multi = 0; a.MultiNoPrint = true;
                Tz.Add(a);
            }
        }


        public virtual void CallbackPaintTouch(object sender, PaintEventArgs e)
        {
            // ok questo metodo viene chiamato da paint della finestra principale 
            // nel caso in cui debba fare dei disegni speciali
        }

        // --------------------------------------------------------------
        //  SCHEDE SPECIALI
        // --------------------------------------------------------------

        protected void MettiSchedeSpeciali(TVotazione AVotazione)
        {
            TTZone a;
            
            // le schede speciali possono essere 4
            // - Bianca - Contrario Tutti - Astenuto tutti - Cotinua (Multivoto)
            // solo  bianca : in centro
            // solo contrario e astenuto vanno in fila a sx
            // tutti e 3 : in teoria non dovrebbero andare

            // per evitarmi 1000 casi metto i casi + usati

            // solo sk Bianca
            if (AVotazione.SkBianca && !AVotazione.SkContrarioTutte && !AVotazione.SkAstenutoTutte)
            {
                a = new TTZone();
                // se c'è anche non voto devo spostarla
                if (VTConfig.ModoPosizioneAreeTouch == VSDecl.MODO_POS_TOUCH_NORMALE)
                    GetZone(ref a, 280, 720, 720, 930); // non la sposto sta in centro
                else
                    GetZone(ref a, 350, 760, 640, 930); // non la sposto sta in centro
                //if (!AVotazione.SkNonVoto)
                //    GetZone(ref a, 28, 74, 73, 90); // non la sposto sta in centro
                //else
                //    GetZone(ref a, 10, 72, 44, 90); //la sposto a sinistra
                a.expr = VSDecl.VOTO_SCHEDABIANCA;
                a.Text = ""; a.ev = TTEvento.steSkBianca; a.pag = 0; a.Multi = 0; a.MultiNoPrint = true;
                Tz.Add(a);
            }

            // ora solo Contrario + Astenuto
            if (!AVotazione.SkBianca && AVotazione.SkContrarioTutte && AVotazione.SkAstenutoTutte)
            {
                // Contrario A Tutti
                a = new TTZone();
                GetZone(ref a, 60, 760, 290, 930); // non la sposto sta in centro
                a.expr = VSDecl.VOTO_CONTRARIO_TUTTI;
                a.Text = ""; a.ev = TTEvento.steSkContrarioTutti; a.pag = 0; a.Multi = 0; a.MultiNoPrint = true;
                Tz.Add(a);
                // Astenuti A Tutti
                a = new TTZone();
                if (AVotazione.SkContrarioTutte)
                    GetZone(ref a, 340, 760, 570, 930); 
                else
                    GetZone(ref a, 60, 760, 290, 930); 
                a.expr = VSDecl.VOTO_ASTENUTO_TUTTI;
                a.Text = ""; a.ev = TTEvento.steSkAstenutoTutti; a.pag = 0; a.Multi = 0; a.MultiNoPrint = true;
                Tz.Add(a);
            }

            // Ok, ora la scheda bianca
            //if (AVotazione.SkBianca)
            //{
            //    a = new TTZone();
            //    // se c'è anche non voto devo spostarla
            //    GetZone(ref a, 35, 75, 64, 92); // non la sposto sta in centro
            //    //if (!AVotazione.SkNonVoto)
            //    //    GetZone(ref a, 28, 74, 73, 90); // non la sposto sta in centro
            //    //else
            //    //    GetZone(ref a, 10, 72, 44, 90); //la sposto a sinistra
            //    a.expr = VSDecl.VOTO_SCHEDABIANCA;
            //    a.Text = ""; a.ev = TTEvento.steSkBianca; a.pag = 0; a.Multi = 0;
            //    Tz.Add(a);
            //}
            // il non voto, se presente (caso BPM)
            if (AVotazione.SkNonVoto)
            {
                a = new TTZone();
                // nella nuova versione è in basso a dx
                //if (!AVotazione.SkBianca)
                //    GetZone(ref a, 32, 72, 67, 90); // non la sposto, sta in centro
                //else
                //    GetZone(ref a, 55, 72, 89, 90); //la sposto a destra
                GetZone(ref a, 760, 870, 980, 1000); // in bass a sx
                a.expr = VSDecl.VOTO_NONVOTO;
                a.Text = ""; a.ev = TTEvento.steSkNonVoto; a.pag = 0; a.Multi = 0; a.MultiNoPrint = true;
                Tz.Add(a);
            }
        }


        // --------------------------------------------------------------
        //  UTILITA DI RICALCOLO SCHERMO
        // --------------------------------------------------------------

        #region UTILITA DI RICALCOLO SCHERMO

        protected int GetX(int n)
        {
            return (int)(FFormRect.Width / Nqx) * n;
        }

        protected int GetY(int n)
        {
            return (int)(FFormRect.Height / Nqy) * n;
        }

        protected void GetZone(ref TTZone a, int qx, int qy, int qr, int qb)
        {
            float x, y, r, b;
            // prendo le unità di misura
            x = (FFormRect.Width / Nqx) * qx;
            y = (FFormRect.Height / Nqy) * qy;
            r = (FFormRect.Width / Nqx) * qr;
            b = (FFormRect.Height / Nqy) * qb;
            a.x = (int)x;
            a.y = (int)y;
            a.r = (int)r;
            a.b = (int)b;
        }

        protected void GetZoneFloat(ref TTZone a, float qx, float qy, float qr, float qb)
        {
            float x, y, r, b;
            // prendo le unità di misura
            x = (FFormRect.Width / Nqx) * qx;
            y = (FFormRect.Height / Nqy) * qy;
            r = (FFormRect.Width / Nqx) * qr;
            b = (FFormRect.Height / Nqy) * qb;
            a.x = (int)x;
            a.y = (int)y;
            a.r = (int)r;
            a.b = (int)b;
        }

        #endregion


    }
}
