using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

// -----------------------------------------------------------------------
//			VOTO SEGRETO - 
//  Classe di gestione del tema grafico dell'applicazione
// -----------------------------------------------------------------------
//		AUTH	: M.Binello
//		VER		: 1.0 
//		DATE	: Feb 2010
// -----------------------------------------------------------------------
//	History  3.1 : Aggiunto supporto a tema esterno
// -----------------------------------------------------------------------

namespace VotoTouch
{
    // struttura del tema
    public struct TTheme
    {
        public string Oggetto;
		public string Descrizione;
        public short Tipo;
        public bool Visible;
        public Rectangle a;
        public Color AColor;
        public bool Shadow;
        public Font AFont;
        public short Align;
    }


    public class CVotoTheme
    {

        public const float Nqx = 100;
        public const float Nqy = 100;
        public string AData_path;

        // font
        public int BaseFontCandidato;
        public bool BaseFontCandidatoBool;

        DataTable dtTema = new DataTable();
        public Boolean IsThemed;

        public Rectangle FFormRect;

        public CVotoTheme()
		{
            // inizializzo
            FFormRect = new Rectangle();

            BaseFontCandidato = 22;
		}

        //-----------------------------------------------------------------------------
        //  CARICAMENTO DEI TEMI
        //-----------------------------------------------------------------------------

        public void CaricaTemaDaXML(string Img_path)
        {
            IsThemed = false;
            // il file xm del tema deve essere in locale nella cartella Data
            // e chiamarsi "VS_TemaGrafico.xml
            //AData_path = "c:" + VSDecl.DATA_PATH_ABS; // "c:\\data\\";
            // ok, ora devo caricare da un file esterno XML residente nella stessa cartella locale
            // tutti i parametri delle label, carico il file nella Datatable
            if (System.IO.File.Exists(Img_path + "VS_TemaGrafico.xml"))
            {
                // uso un try così se c'è qualche problema deseleziono il tema e uso l'embedded
                try
                {
                    dtTema.ReadXml(Img_path + "VS_TemaGrafico.xml");
                    IsThemed = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    IsThemed = false;
                }
            }
            else
                IsThemed = false;
            // ok, ora nella datatable ho le varie proprietà delle label che userò ricercando la riga della label
            // e settando font, dimensione, bold, italic, colore

            // ho anche delle proprietà particolari
            try
            {
                if (IsThemed && dtTema.Rows.Count > 0)  // se ha il tema la cerco nella datatable
                {
                    foreach (DataRow r in dtTema.Select("Oggetto = 'BaseFontCandidato'"))
                    {
                        BaseFontCandidato = Convert.ToInt32(r["Point"]);
                        BaseFontCandidatoBool = Convert.ToBoolean(r["Bold"]);
                    }
                }
            }
            catch (Exception ex)
            {
                // non fare nulla, rimane come prima
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

        }

        //-----------------------------------------------------------------------------
        //  GESTIONE DEI TEMI
        //-----------------------------------------------------------------------------

        public bool ThemeToLabel(string ObjName, ref Label c, ref Rectangle a)
        {
            bool ret = false;

            try
            {
                if (IsThemed && dtTema.Rows.Count > 0)  // se ha il tema la cerco nella datatable
                {
                    foreach (DataRow r in dtTema.Select("Oggetto = '" + ObjName + "'"))
                    {
                        GetZone(ref a, Convert.ToInt32(r["ULeft"]), Convert.ToInt32(r["UTop"]),
                                       Convert.ToInt32(r["URight"]), Convert.ToInt32(r["UBottom"]));
                        c.ForeColor = System.Drawing.ColorTranslator.FromHtml(r["Color"].ToString());
                        //c.Visible = Convert.ToBoolean(r["Visible"]);
                        c.TextAlign = GetTextAlignment(Convert.ToInt32(r["Align"]));
                        // font
                        FontStyle fs = FontStyle.Regular;
                        if (Convert.ToBoolean(r["Bold"])) fs = FontStyle.Bold;
                        c.Font = new Font(r["Font"].ToString(), Convert.ToSingle(r["Point"]), fs);

                        ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // non fare nulla, rimane come prima
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        public bool ThemeToPaint(string ObjName, ref TTheme th)
        {
            bool ret = false;

            try
            {
                if (IsThemed && dtTema.Rows.Count > 0)  // se ha il tema la cerco nella datatable
                {
                    foreach (DataRow r in dtTema.Select("Oggetto = '" + ObjName + "'"))
                    {
                        th.Oggetto = r["Oggetto"].ToString();
                        th.Descrizione = r["Descrizione"].ToString();
                        th.Tipo = Convert.ToInt16(r["Tipo"]);
                        th.Visible = Convert.ToBoolean(r["Visible"]); 
                        GetZone(ref th.a, Convert.ToInt32(r["ULeft"]), Convert.ToInt32(r["UTop"]),
                                        Convert.ToInt32(r["URight"]), Convert.ToInt32(r["UBottom"]));
                        th.Shadow = Convert.ToBoolean(r["Shadow"]);
                        th.AColor = System.Drawing.ColorTranslator.FromHtml(r["Color"].ToString()); //r["Color"].ToString();
                        // ora il font
                        FontStyle fs = FontStyle.Regular;
                        if (Convert.ToBoolean(r["Bold"])) fs = FontStyle.Bold;
                        th.AFont = new Font(r["Font"].ToString(), Convert.ToSingle(r["Point"]), fs);
                        ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                // non fare nulla, rimane come prima
            }
            // controllo se è andato male qualcosa metto i default
            if (!ret)
            {
                th.Tipo = 0;
                th.AFont = new System.Drawing.Font("Arial", 28, FontStyle.Bold);
                th.AColor = Color.Black;
            }
            return ret;
        }

        private ContentAlignment GetTextAlignment(int AAlign)
        {
            switch (AAlign)
            {
                case 0:
                    return ContentAlignment.MiddleCenter;
                    break;
                case 1:
                    return ContentAlignment.MiddleLeft;
                    break;
                case 2:
                    return ContentAlignment.MiddleRight;
                    break;
                default:
                    return ContentAlignment.MiddleCenter;
                    break;
            }
        }

        //-----------------------------------------------------------------------------
        //  PAINT DELLE LABEL
        //-----------------------------------------------------------------------------

        // tengo queste due funzioni come esempio
        //public void PaintDirittiDiVotoPiuOld(object sender, PaintEventArgs e, int Diritti)
        //{
        //    // DR11 Ok
        //    // ok questo metodo viene chiamato da paint della finestra principale 
        //    // per stampare i diritti di voto
        //    Rectangle a = new Rectangle(); //TTZone a = new TTZone();
        //    StringFormat stringFormat = new StringFormat();
        //    stringFormat.Alignment = StringAlignment.Center;
        //    stringFormat.LineAlignment = StringAlignment.Center;

        //    if (IsThemed && dtTema.Rows.Count > 0)  // se ha il tema la cerco nella datatable
        //    {
        //        // metto un try non si sa mai
        //        try
        //        {
        //            foreach (DataRow r in dtTema.Select("Oggetto = 'lbDirittiStartMin'"))
        //            {
        //                GetZone(ref a, Convert.ToInt32(r["ULeft"]), Convert.ToInt32(r["UTop"]),
        //                               Convert.ToInt32(r["URight"]), Convert.ToInt32(r["UBottom"]));
        //                Brush myBrush = new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(r["Color"].ToString()));
        //                // font
        //                FontStyle fs = FontStyle.Regular;
        //                if (Convert.ToBoolean(r["Bold"])) fs = FontStyle.Bold;
        //                Font myFont1 = new Font(r["Font"].ToString(), Convert.ToSingle(r["Point"]), fs);
        //                e.Graphics.DrawString(Diritti.ToString(), myFont1, myBrush, a, stringFormat);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            GetZone(ref a, 16, 60, 34, 77);
        //            Font myFont = new System.Drawing.Font("Tahoma", 66, FontStyle.Bold);
        //            e.Graphics.DrawString(Diritti.ToString(), myFont, Brushes.Firebrick, a, stringFormat);
        //        }

        //    }
        //    else
        //    {
        //        GetZone(ref a, 16, 60, 34, 77);
        //        Font myFont = new System.Drawing.Font("Tahoma", 66, FontStyle.Bold);
        //        e.Graphics.DrawString(Diritti.ToString(), myFont, Brushes.Firebrick, a, stringFormat);
        //    }
        //}

        //public void PaintDirittiDiVotoOld(object sender, PaintEventArgs e, int Diritti)
        //{
        //    Rectangle a = new Rectangle(); //TTZone a = new TTZone();
        //    StringFormat stringFormat = new StringFormat();
        //    stringFormat.Alignment = StringAlignment.Center;
        //    stringFormat.LineAlignment = StringAlignment.Center;
        //    // sono costretto a crearmi una finta label
        //    Label c = new Label();
        //    GetZone(ref a, 16, 60, 34, 77);
        //    c.ForeColor = Color.Firebrick;
        //    c.Font = new System.Drawing.Font("Tahoma", 66, FontStyle.Bold);
        //    // cerco il tema
        //    if (IsThemed) { if (!ThemeToLabel("lbDirittiStartMin", ref c, ref a)) GetZone(ref a, 16, 60, 34, 77); }
        //    Brush myBrush = new System.Drawing.SolidBrush(c.ForeColor);

        //    e.Graphics.DrawString(Diritti.ToString(), c.Font, myBrush, a, stringFormat);
        //    // cancello la label
        //    c.Dispose();
        //}

        // in questo caso uso il paint invce della label per un problema grafico
        public void PaintDirittiDiVoto(object sender, PaintEventArgs e, int Diritti)
        {
            TTheme th =  new TTheme();
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            GetZone(ref th.a, 16, 60, 34, 77);
            // cerco il tema
            if (!ThemeToPaint("lbDirittiStartMin", ref th))
            {
                th.AFont = new System.Drawing.Font("Tahoma", 66, FontStyle.Bold);
                th.AColor = Color.Firebrick;
            }
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            SizeF sf = e.Graphics.MeasureString(Diritti.ToString(), th.AFont);
            float fx = th.a.Left + ((th.a.Width - sf.Width) / 2);
            float fy = th.a.Top + ((th.a.Height - sf.Height) / 2);
            if (th.Shadow)
            {
                Brush myBrush1 = new System.Drawing.SolidBrush(Color.Gray); //E3E3E3
                e.Graphics.DrawString(Diritti.ToString(), th.AFont, myBrush1, fx + 1, fy + 1); //rr, stringFormat);
            }
            Brush myBrush = new System.Drawing.SolidBrush(th.AColor);
            e.Graphics.DrawString(Diritti.ToString(), th.AFont, myBrush, fx, fy); //th.a, stringFormat);
            th.AFont.Dispose();
        }

        //-----------------------------------------------------------------------------
        //  PAINT DELLE LABEL PROPOSTE CDA o PROPOSTE ALTERNATIVE
        //-----------------------------------------------------------------------------

        public void PaintlabelProposteCdaAlt(object sender, PaintEventArgs e, TNewVotazione vt, bool Candidato)
        {
            // DR12 OK
            // devo stampare le label/immagini delle proposte alternative o del cda
            // devo chiaramente capire se ci sono e lo so attraverso TParVotazione
            TTheme th = new TTheme();
            Brush myBrush1 = new System.Drawing.SolidBrush(Color.WhiteSmoke);  //E3E3E3
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
 
            // Proposta CDA
            if (vt.NPresentatoCDA > 0)
            {
                // poi verifico nel tema
                if (!ThemeToPaint("lbCandidati_PresCDA", ref th))
                    th.Descrizione = "Proposto (Cooptato) C.d.A.";
                // se Candidato è true vul dire che è un amministratore cooptato
                //if (Candidato) th.Descrizione = "Proposto (Cooptato) C.d.A.";
                // mi calcolo la zona
                GetZone(ref th.a, vt.AreaVoto.XCda, vt.AreaVoto.YCda - 6, vt.AreaVoto.RCda(), vt.AreaVoto.YCda);
                // ok, ora secondo il tipo faccio
                SizeF sf = e.Graphics.MeasureString(th.Descrizione, th.AFont);
                float fx = th.a.Left + ((th.a.Width / 2) - (sf.Width / 2));
                float fy = (th.a.Bottom - sf.Height - 14);
                e.Graphics.DrawString(th.Descrizione, th.AFont, myBrush1, fx + 1, fy + 1);//rr, stringFormat);
                Brush myBrush = new System.Drawing.SolidBrush(th.AColor);
                e.Graphics.DrawString(th.Descrizione, th.AFont, myBrush, fx, fy);//, th.a, stringFormat);
            }
        
            // Proposte alternative
            if ((vt.NListe - vt.NPresentatoCDA) > 0)
            {
                // poi verifico nel tema
                if (!ThemeToPaint("lbCandidati_Altern", ref th))
                {
                    th.Descrizione = "Proposte Alternative";
                    // se Candidato è true vul dire che è un amministratore cooptato
                    if (Candidato) th.Descrizione = "Candidati Alternativi";
                }
                // mi calcolo la zona
                GetZone(ref th.a, vt.AreaVoto.XAlt, vt.AreaVoto.YAlt - 6, vt.AreaVoto.RAlt(), vt.AreaVoto.YAlt);
                // ok, ora secondo il tipo faccio
                SizeF sf = e.Graphics.MeasureString(th.Descrizione, th.AFont);
                float fx = th.a.Left + ((th.a.Width - sf.Width) / 2);
                float fy = (th.a.Bottom - sf.Height - 14);
                e.Graphics.DrawString(th.Descrizione, th.AFont, myBrush1, fx+1, fy+1);//rr, stringFormat);
                Brush myBrush = new System.Drawing.SolidBrush(th.AColor);
                e.Graphics.DrawString(th.Descrizione, th.AFont, myBrush, fx, fy);//, th.a, stringFormat);
            }

            if (th.AFont != null)
                th.AFont.Dispose();
        }

        //-----------------------------------------------------------------------------
        //  SETTING DELLE LABEL
        //-----------------------------------------------------------------------------
        
        public void SetTheme_lbDirittiStart(ref Label c)
        {
            // La label dell'apertura del voto grande con i diritti di voto
            Rectangle a = new Rectangle();
            GetZone(ref a, 20, 26, 38, 42);
            if (IsThemed) { if (!ThemeToLabel("lbDirittiStart", ref c, ref a)) GetZone(ref a, 20, 26, 38, 42); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }

        public void SetTheme_lbConfermaUp(ref Label c)
        {
            // questa è la label di conferma dove sono scritte chi ha votato (es. LISTA 1)
            Rectangle a = new Rectangle();
            GetZone(ref a, 15, 37, 85, 41);
            if (IsThemed) { if (!ThemeToLabel("lbConfermaUp", ref c, ref a)) GetZone(ref a, 15, 37, 85, 41); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }

        public void SetTheme_lbConfermaUp_Cand(ref Label c)
        {
            // questa è la label di conferma dove sono scritte chi ha votato (es. LISTA 1)
            Rectangle a = new Rectangle();
            GetZone(ref a, 15, 37, 85, 41);
            if (IsThemed) { if (!ThemeToLabel("lbConfermaUp_Cand", ref c, ref a)) GetZone(ref a, 15, 37, 85, 41); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }


        public void SetTheme_lbConferma(ref Label c)
        {
            // questa è la label di conferma dove sono scritte chi ha votato (es. Pippo Franco, Gianni Rossi)
            Rectangle a = new Rectangle();
            GetZone(ref a, 15, 41, 85, 56);
            if (IsThemed) { if (!ThemeToLabel("lbConferma", ref c, ref a)) GetZone(ref a, 15, 41, 85, 56); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }

        public void SetTheme_lbConfermaNVoti(ref Label c)
        {
            // questa è la label che è dopo Esprima: nella schermata di conferma (es. 1 Diritto di voto)
            Rectangle a = new Rectangle();
            GetZone(ref a, 14, 28, 85, 34);
            if (IsThemed) { if (!ThemeToLabel("lbConfermaNVoti", ref c, ref a))  GetZone(ref a, 14, 28, 85, 34); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }

        public void SetTheme_lbDirittiDiVoto(ref Label c)
        {
            // questa è la label in basso a sx che indica i diritti di voto
            Rectangle a = new Rectangle();
            GetZone(ref a, 0, 95, 25, 99);
            if (IsThemed) { if (!ThemeToLabel("lbDirittiDiVoto", ref c, ref a)) GetZone(ref a, 0, 95, 25, 99); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);

        }
        
        public void SetTheme_lbNomeDisgiunto(ref Label c)
        {
            // mi arriva un controllo qualsiasi
            Rectangle a = new Rectangle();
            GetZone(ref a, 26, 93, 51, 100);
            if (IsThemed) { if (!ThemeToLabel("lbNomeDisgiunto", ref c, ref a)) GetZone(ref a, 26, 93, 51, 100); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }

        public void SetTheme_lbDisgiuntoRimangono(ref Label c)
        {
            // mi arriva un controllo qualsiasi
            Rectangle a = new Rectangle();
            GetZone(ref a, 1, 91, 25, 94);
            if (IsThemed) { if (!ThemeToLabel("lbDisgiuntoRimangono", ref c, ref a)) GetZone(ref a, 1, 91, 25, 94); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }

        public void SetTheme_lbNomeAzStart(ref Label c)
        {
            // mi arriva un controllo qualsiasi
            Rectangle a = new Rectangle();
            GetZone(ref a, 1, 30, 99, 40);
            if (IsThemed) { if (!ThemeToLabel("lbNomeAzStart", ref c, ref a)) GetZone(ref a, 1, 30, 99, 40); }
            c.SetBounds(a.Left, a.Top, a.Width, a.Height);
        }

        // ------------------------------------------------------------------------------------------

        private void GetZone(ref Rectangle a, int qx, int qy, int qr, int qb)
        {
            float x, y, r, b;
            // prendo le unità di misura
            x = (FFormRect.Width / Nqx) * qx;
            y = (FFormRect.Height / Nqy) * qy;
            r = (FFormRect.Width / Nqx) * qr;
            b = (FFormRect.Height / Nqy) * qb;
            a.X = (int)x;
            a.Y = (int)y;
            a.Width = (int)r - (int)x;
            a.Height = (int)b - (int)y;
        }


    }
}
