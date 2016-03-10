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

        public const float Nqx = 100;
        public const float Nqy = 100;

        public const float HRETT_CANDIDATO = 6F; //67px;

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

        virtual public void GetTouchVoteZone(TNewVotazione AVotazione) //ref ArrayList Tz)
        {
            // l'implementazione è nelle varie classi
        }

        virtual public void GetTouchSpecialZone(TAppStato AStato, bool ADiffer) //, ref ArrayList Tz
        {
            // l'implementazione è nelle varie classi
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
