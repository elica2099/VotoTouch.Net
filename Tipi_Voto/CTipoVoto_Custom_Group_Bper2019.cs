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
            // ok, ora divido per subvotazione

            // **** SUBVOTO 1 - PRESIDENTE DEL CS
            calcVoto1(AVotazione);

            // **** SUBVOTO 2 - EFFETTIVI
            calcVoto2(AVotazione);

            // devo aggiungere il tasto avanti con evento           
            TTZone a = new TTZone();
            GetZone(ref a, 735, 875, 990, 1000);
            a.expr = VSDecl.VOTO_MULTIAVANTI;
            a.Text = ""; a.ev = TTEvento.steGruppoAvanti; a.pag = 0; a.cda = false; a.Multi = 0;
            Tz.Add(a);

            // e il bottone di uscita
            if (AVotazione.AbilitaBottoneUscita)
            {
                a = new TTZone();
                GetZone(ref a, 8, 885, 240, 1000); // in alto a dx
                a.expr = VSDecl.VOTO_BTN_USCITA;
                a.Text = ""; a.ev = TTEvento.steBottoneUscita; a.pag = 0; a.Multi = 0; a.MultiNoPrint = true;
                Tz.Add(a);
            }

        }

        private void calcVoto1(TNewVotazione AVotazione)
        {
            // **** SUBVOTO 1 - PRESIDENTE DEL CS
            // liste pure
            var voto1 = AVotazione.Liste.Where(o => o.NumSubVotaz == 1 && o.IDScheda != 100);
            var newListas = voto1 as IList<TNewLista> ?? voto1.ToList();
            int NListe1 = newListas.Count();
            // in base a quello setto le liste
            const int c_w = 475;
            const int c_w6 = 325;
            const int c_y = 140;
            switch (NListe1)
            {
                case 1:     // in centro
                    TNewLista firstOrDefault = newListas.FirstOrDefault();
                    if (firstOrDefault != null)
                        Tz.Add(calczone(firstOrDefault.IDScheda, 1, firstOrDefault.DescrLista, 310, 235, 725, 335));
                    break;
                case 2:     // in centro due colonne
                    int x2 = 53;
                    int r2 = 470;
                    foreach (TNewLista newLista in newListas)
                    {
                        Tz.Add(calczone(newLista.IDScheda, 1, newLista.DescrLista, x2, 235, r2, 335));
                        x2 += c_w;
                        r2 += c_w;
                    }
                    break;

                #region Case 3 - 4
                case 3:     // 
                case 4:     // due file due colonne
                    int x4 = 53;
                    int y4 = 190;
                    int r4 = 470;
                    int b4 = 290;
                    int cc4 = 1;
                    foreach (TNewLista newLista in newListas)
                    {
                        Tz.Add(calczone(newLista.IDScheda, 1, newLista.DescrLista, x4, y4, r4, b4));
                        cc4++;
                        switch (cc4)
                        {
                            case 2:     // sposto a dx
                                x4 += c_w;
                                r4 += c_w;
                                break;
                            case 3:
                                x4 = 53;
                                y4 += c_y;
                                r4 = 470;
                                b4 += c_y;
                                break;
                            case 4:
                                x4 += c_w;
                                r4 += c_w;
                                break;
                        }
                    }
                    break;
                #endregion

                case 5:     // tre file 
                case 6:     //
                    int x6 = 25;
                    int y6 = 190;
                    int r6 = 320;
                    int b6 = 290;
                    int cc6 = 1;
                    foreach (TNewLista newLista in newListas)
                    {
                        Tz.Add(calczone(newLista.IDScheda, 1, newLista.DescrLista, x6, y6, r6, b6));
                        cc6++;
                        switch (cc6)
                        {
                            case 2:     // sposto a dx
                            case 3:
                                x6 += c_w6;
                                r6 += c_w6;
                                break;
                            case 4:
                                x6 = 25;
                                y6 += c_y;
                                r6 = 320;
                                b6 += c_y;
                                break;
                            case 5:
                            case 6:
                                x6 += c_w6;
                                r6 += c_w6;
                                break;
                        }
                    }
                    break;

                default:

                    break;
            }
            // trovo il non voglio votare, prima
            var nv1 = AVotazione.Liste.FirstOrDefault(o => o.IDScheda == 100);
            if (nv1 != null)
                Tz.Add(calczone(nv1.IDScheda, 1, nv1.DescrLista, 683, 85, 990, 174, true));

        }

        private void calcVoto2(TNewVotazione AVotazione)
        {
            // **** SUBVOTO 2 - EFFETTIVI
            // trovo il non voglio votare, prima
            var nv2 = AVotazione.Liste.FirstOrDefault(o => o.IDScheda == 200);
            if (nv2 != null)
                Tz.Add(calczone(nv2.IDScheda, 2, nv2.DescrLista, 683, 470, 990, 558, true));
            // liste pure
            var voto2 = AVotazione.Liste.Where(o => o.NumSubVotaz == 2 && o.IDScheda != 200);
            var newListas = voto2 as IList<TNewLista> ?? voto2.ToList();
            int NListe1 = newListas.Count();
            // in base a quello setto le liste
            const int dy = 396;
            const int c_w = 475;
            const int c_w6 = 325;
            const int c_y = 140;
            switch (NListe1)
            {
                case 1:     // in centro
                    TNewLista firstOrDefault = newListas.FirstOrDefault();
                    if (firstOrDefault != null)
                        Tz.Add(calczone(firstOrDefault.IDScheda, 2, firstOrDefault.DescrLista, 310 + dy, 235, 725, 335));
                    break;
                case 2:     // in centro due colonne
                    int x2 = 53;
                    int r2 = 470;
                    foreach (TNewLista newLista in newListas)
                    {
                        Tz.Add(calczone(newLista.IDScheda, 2, newLista.DescrLista, x2, 235 + dy, r2, 335 + dy));
                        x2 += c_w;
                        r2 += c_w;
                    }
                    break;

                #region Case 3 - 4
                case 3:     // 
                case 4:     // due file due colonne
                    int x4 = 53;
                    int y4 = 190;
                    int r4 = 470;
                    int b4 = 290;
                    int cc4 = 1;
                    foreach (TNewLista newLista in newListas)
                    {
                        Tz.Add(calczone(newLista.IDScheda, 2, newLista.DescrLista, x4, y4 + dy, r4, b4 + dy));
                        cc4++;
                        switch (cc4)
                        {
                            case 2:     // sposto a dx
                                x4 += c_w;
                                r4 += c_w;
                                break;
                            case 3:
                                x4 = 53;
                                y4 += c_y;
                                r4 = 470;
                                b4 += c_y;
                                break;
                            case 4:
                                x4 += c_w;
                                r4 += c_w;
                                break;
                        }
                    }
                    break;
                #endregion

                case 5:     // tre file 
                case 6:     //
                    int x6 = 25;
                    int y6 = 190;
                    int r6 = 320;
                    int b6 = 290;
                    int cc6 = 1;
                    foreach (TNewLista newLista in newListas)
                    {
                        Tz.Add(calczone(newLista.IDScheda, 2, newLista.DescrLista, x6, y6 + dy, r6, b6 + dy));
                        cc6++;
                        switch (cc6)
                        {
                            case 2:     // sposto a dx
                            case 3:
                                x6 += c_w6;
                                r6 += c_w6;
                                break;
                            case 4:
                                x6 = 25;
                                y6 += c_y;
                                r6 = 320;
                                b6 += c_y;
                                break;
                            case 5:
                            case 6:
                                x6 += c_w6;
                                r6 += c_w6;
                                break;
                        }
                    }
                    break;

                default:

                    break;
            }
        }

        private TTZone calczone(int expr, int Group, string cand, int qx, int qy, int qr, int qb, bool OnlyCheck = false)
        {
            TTZone a = new TTZone();
            GetZone(ref a, qx, qy, qr, qb);
            a.expr = expr; a.pag = 0; a.cda = false; a.Multi = 0; a.Group = Group;
            a.PaintMode = OnlyCheck ? VSDecl.PM_ONLYCHECK : VSDecl.PM_NORMAL;
            a.Text = cand; a.ev = TTEvento.steGruppoValido;
            a.CKRect = new Rectangle(a.x + 313, a.y + 4, 76, 68);

            return a;
        }

    }
}
