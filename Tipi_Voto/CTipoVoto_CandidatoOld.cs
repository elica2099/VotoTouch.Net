using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    public class CTipoVoto_CandidatoOld: CBaseTipoVoto
    {

        // CLASSE DELLA votazione di candidato
		// Versione ORIGINALE da VotoSegreto
        
        public CTipoVoto_CandidatoOld(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
        }

        //override public void GetTouchVoteZone(TAppStato AStato, TNewVotazione AFVotaz, 
        //                                                bool ADiffer, ref ArrayList Tz )
        override public void GetTouchVoteZone(TNewVotazione AVotazione)
        {
            // DR12 OK
            Tz.Clear();
            
        }


        // --------------------------------------------------------------
        //  CALCOLO DEL TOUCH CANDIDATO PAGINA VECCHIO
        // --------------------------------------------------------------

        #region CALCOLO DEL TOUCH CANDIDATO PAGINA VECCHIO

        /*
        public void CalcolaTouchCandidatoPagina(TAppStato AStato, ref TVotazione AFVotaz,
                                                    bool ADiffer, ref ArrayList Tz)
        {
            // DR11 OK
            TTZone a;
            TLista li;
            int z, PosPresCda, PosCandAlt;
            TTEvento evento;

            // in funzione della votazione seleziono l'evento corrispondente al tocco del voto
            // se è Multicandidato, l'evento sarà solo locale e setterà un flag nella collection, 
            // altrimenti richiamerà il voto valido all'esterno
            //if (AFVotaz.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
            //    evento = TTEvento.steMultiValido;
            //else
            evento = TTEvento.steVotoValido;

            PosPresCda = 1;
            PosCandAlt = 1;
            // ok, ciclo lungo i candidati per metterli nell'area giusta
            for (z = 0; z < AFVotaz.NListe; z++)
            {
                li = (TLista)AFVotaz.Liste[z];

                // Devo testare se il candidato è presentato dal cda
                if (li.PresentatodaCDA)
                {
                    // ok, è presentato, va nell'area in alto
                    a = new TTZone();
                    GetCandidatoCdaZone(ref a, PosPresCda, AFVotaz.NPresentatoCDA);
                    a.expr = z; a.ev = evento;
                    a.Text = li.DescrLista;
                    a.Multi = 0; a.pag = 0; 
                    Tz.Add(a);
                    PosPresCda++;
                }
                else
                {
                    // controllo che sia nella pagina corrente
                    //if (li.Pag == CurrPag)
                    //{
                        // non è presentato, va nell'area in basso
                        a = new TTZone();
                        GetCandidatoAltPaginaZone(ref a, PosCandAlt); //, AFVotaz.NPresentatoCDA);
                        a.expr = z; a.ev = evento;
                        a.Text = li.DescrLista;
                        a.Multi = 0; a.pag = li.Pag;
                        Tz.Add(a);
                        PosCandAlt++;
                        // aggiunta successiva
                        if (PosCandAlt > AFVotaz.AreaVoto.CandidatiPerPagina)
                            PosCandAlt = 1;
                    //}
                }
            }
            // Ok, ora la scheda bianca e il non voto
            if (AFVotaz.SkBianca)
            {
                // la scheda bianca ( che è sempre l'ultima, quindi ntasti)
                a = new TTZone();
                // cambio la posizione nel caso ho Avanti
                if (AFVotaz.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
                    GetZone(ref a, 25, 81, 65, 91);
                else
                    GetZone(ref a, 30, 81, 70, 91);                
                a.expr = VSDecl.VOTO_SCHEDABIANCA;
                a.Text = ""; a.ev = TTEvento.steSkBianca; a.pag = 0; 
                Tz.Add(a);
            }

            // Attenzione, nel caso la votazione sia di tipo Multicandidato, devo Aggiungere un tasto
            // "Avanti" o "Conferma" per continuare
            if (AFVotaz.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
            {
                // devo aggiungere il tasto con evento           
                a = new TTZone();
                GetZone(ref a, 68, 81, 99, 91); a.expr = VSDecl.VOTO_MULTIAVANTI;
                a.Text = ""; a.ev = TTEvento.steMultiAvanti; a.pag = 0; 
                Tz.Add(a);
            }
        }

        // ------------------ CANDIDATO CDA ----------------------------

        private void GetCandidatoCdaZone(ref TTZone a, int APosPresCda, int ATotPresCda)
        {
            float x, y, r, b;

            // qua setto la posizione del candidato, in funzione del totale
            // NOTA: In questa versione sono possibili fino a 3 Candidati presentati dal CDA
            // l'area disponibile in base 1280x1024 è:
            // x:540 y:180 w:614 h:76 la larghezza del rettangolo è 400x3 600x2 800x1
            float yy = 48 / (ATotPresCda);
            float ms = (yy - GetDimensioneCda(ATotPresCda)) / 2;
            x = 42 + ms + (yy * (APosPresCda - 1));

            y = 18;
            r = x + GetDimensioneCda(ATotPresCda);
            b = 25;
            GetZone(ref a, (int)x, (int)y, (int)r, (int)b);

        }

        private float GetDimensioneCda(int ATotPresCda)
        {
            float[] zz = new float[] { 0, 46, 34, 31, 20, 17 };
            return zz[ATotPresCda];
        }

        // ------------------ CANDIDATO ALTERNATIVO PAGINE ----------------------------

        private void GetCandidatoAltPaginaZone(ref TTZone a, int APosCandAlt) //, int ATotPresCda)
        {
            // so che ho il max di CANDIDATI_PER_PAGINA per pagina, quindi in funzione
            // della posizione li metto
            float x, y, r, b;

            //calcolo che ne ho max 5 in colonna, su 2 colonne
            // innanzitutto quante colonne ho?
            int col = 2;  // fisse
            // calcolo le colonne
            float yy = 74 / (col);
            float ms = (yy - GetDimensioneCda(col)) / 2;
            // ora devo calcolarmi la posizione sulla colonna
            x = 1 + ms + (yy * ((GetColonneAltPagina(APosCandAlt)) - 1));
            // devo calcolarmi la y
            y = 33 + ((GetYAltPag(APosCandAlt) * 10) - 2);
            r = x + GetDimensioneCda(col);
            b = y + 7;
            GetZone(ref a, (int)x, (int)y, (int)r, (int)b);
        }

        private int GetColonneAltPagina(int ATotCandAlt)
        {
            int[] zz = new int[] { 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2 };
            return zz[ATotCandAlt];
        }

        private float GetYAltPag(int ATotCandAlt)
        {
            int[] zz = new int[] { 0, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4 };
            return zz[ATotCandAlt];
        }
        
        #endregion
        
        // --------------------------------------------------------------
        //  CALCOLO DEL TOUCH CANDIDATO SINGOLA PAGINA
        // --------------------------------------------------------------

        #region CALCOLO DEL TOUCH CANDIDATO SINGOLA PAGINA

        public void CalcolaTouchCandidatoSingola(TAppStato AStato, ref TVotazione AFVotaz,
                                                        bool ADiffer, ref ArrayList Tz)
        {
            TTZone a;
            TLista li;
            int z, PosPresCda, PosCandAlt, TotCandAlt;

            string nomeEtruria = "";

            PosPresCda = 1;
            PosCandAlt = 1;
            TotCandAlt = AFVotaz.NListe - AFVotaz.NPresentatoCDA;

            // ok qua ci sono dua modi di intendere il voto

            // MODO UNO, non ci sono candidati CDA, quindi posso mettere fino ad un massimo di 21 candidati per pagina
            if (AFVotaz.NPresentatoCDA == 0)
            {

                for (z = 0; z < AFVotaz.NListe; z++)
                {
                    li = (TLista) AFVotaz.Liste[z];
                    // non è presentato, va nell'area in basso
                    a = new TTZone();
                    GetCandidatoAltSingolaPagina21(ref a, PosCandAlt, TotCandAlt, AFVotaz.NPresentatoCDA);
                    a.expr = z;
                    a.ev = TTEvento.steVotoValido;
                    a.Text = li.DescrLista;
                    a.cda = false;
                    a.Multi = 0;
                    a.pag = 0;
                    Tz.Add(a);
                    PosCandAlt++;                               
                }
            }
            else
            {
                // MODO DUE, ci sono candidati CDA

                // ok, ciclo lungo i candidati per metterli nell'area giusta
                for (z = 0; z < AFVotaz.NListe; z++)
                {
                    li = (TLista) AFVotaz.Liste[z];

                    // Devo testare se il candidato è presentato dal cda
                    if (li.PresentatodaCDA)
                    {
                        // ok, è presentato, va nell'area in alto
                        a = new TTZone();
                        GetCandidatoCdaSingolaZone(ref a, PosPresCda, AFVotaz.NPresentatoCDA, AFVotaz.SelezionaTuttiCDA);
                        a.expr = z;
                        a.ev = TTEvento.steVotoValido;
                        a.Text = li.DescrLista;
                        a.Multi = 0;
                        a.pag = 0;
                        a.cda = true;
                        Tz.Add(a);
                        PosPresCda++;

                        nomeEtruria += li.DescrLista + "      ";
                    }
                    else
                    {
                        // non è presentato, va nell'area in basso
                        a = new TTZone();
                        GetCandidatoAltSingolaZone(ref a, PosCandAlt, TotCandAlt, AFVotaz.NPresentatoCDA);
                        a.expr = z;
                        a.ev = TTEvento.steVotoValido;
                        a.Text = li.DescrLista;
                        a.cda = false;
                        a.Multi = 0;
                        a.pag = 0;
                        Tz.Add(a);
                        PosCandAlt++;
                    }
                    //lbVersion.Items.Add("        Lista: " + a.IDLista.ToString() + ", IdSk: " +
                    //    a.IDScheda.ToString() + ", " + a.DescrLista + ", cda: " + a.PresentatodaCDA.ToString());
                }
            }
            // Ok, ora la scheda bianca e il non voto
            //    if (AFVotaz.SkBianca)
            //    {
            //        // la scheda bianca ( che è sempre l'ultima, quindi ntasti)
            //        a = new TTZone();
            //        GetZone(ref a, 30, 81, 70, 91); a.expr = VSDecl.VOTO_SCHEDABIANCA;
            //        a.Text = ""; a.ev = TTEvento.steSkBianca; a.pag = 0; 
            //        Tz.Add(a);
            //    }
            // Ok, ora la scheda bianca e il non voto
            if (AFVotaz.SkBianca && !AFVotaz.SkNonVoto)
            {
                // la scheda bianca ( che è sempre l'ultima, quindi ntasti)
                a = new TTZone();
                GetZone(ref a, 23, 79, 78, 92);
                a.expr = VSDecl.VOTO_SCHEDABIANCA;
                a.pag = 0;
                a.cda = false;
                a.Multi = 0;
                a.Text = "";
                a.ev = TTEvento.steSkBianca;
                Tz.Add(a);
            }
            else
            {
                // Ok, ora la scheda bianca
                if (AFVotaz.SkBianca)
                {
                    a = new TTZone();
                    // se c'è anche non voto devo spostarla
                    if (!AFVotaz.SkNonVoto)
                        GetZone(ref a, 32, 76, 67, 90); // non la sposto sta in centro
                    else
                        GetZone(ref a, 10, 82, 44, 95); //la sposto a sinistra
                    a.expr = VSDecl.VOTO_SCHEDABIANCA;
                    a.Text = "";
                    a.ev = TTEvento.steSkBianca;
                    a.pag = 0;
                    a.Multi = 0;
                    Tz.Add(a);
                }
                // il non voto, se presente (caso BPM)
                if (AFVotaz.SkNonVoto)
                {
                    a = new TTZone();
                    // se c'è anche SkBianca devo spostarla
                    if (!AFVotaz.SkBianca)
                        GetZone(ref a, 32, 82, 67, 95); // non la sposto, sta in centro
                    else
                        GetZone(ref a, 55, 82, 89, 95); //la sposto a destra
                    a.expr = VSDecl.VOTO_NONVOTO;
                    a.Text = "";
                    a.ev = TTEvento.steSkNonVoto;
                    a.pag = 0;
                    a.Multi = 0;
                    Tz.Add(a);
                }
            }

            //// TODO: FATTO PER ETRURIA
            //// ok, ora una cosa strana, il seleziona tuttiCDA, in alcuni casi servere un tasto che
            //// selezioni tutti come se fosse un multi candidato
            //// se nella votazione è presente il seleziona TuttoCDA
            //if (AFVotaz.SelezionaTuttiCDA && AFVotaz.NPresentatoCDA > 0)
            //{
            //    // devo mettere il tasto
            //    a = new TTZone();
            //    //int y;
            //    // devo fare attenzione a quante righe ha il cda e spostare il tasto
            //    GetZone(ref a, 27, 26, 73, 35);

            //    //if (AFVotaz.NPresentatoCDA <= 3)
            //    //    GetZone(ref a, (AFVotaz.AreaVoto.RCda() - 24), (AFVotaz.AreaVoto.BCda() + 2),
            //    //        AFVotaz.AreaVoto.RCda(), (AFVotaz.AreaVoto.BCda() + 8));
            //    //else
            //    //    GetZone(ref a, (AFVotaz.AreaVoto.RCda() - 24), (AFVotaz.AreaVoto.BCda() - 4),
            //    //        AFVotaz.AreaVoto.RCda(), (AFVotaz.AreaVoto.BCda() + 3));
            //    //a.expr = 999; a.cda = false;
            //    a.expr = VSDecl.VOTO_ETRURIA;
            //    a.pag = 0;
            //    a.Text = nomeEtruria;
            //    a.Multi = 0;
            //    a.ev = TTEvento.steVotoValido; a.pag = 0;
            //    Tz.Add(a);
            //}            


        }

        // ------------------ CANDIDATO CDA SINGOLA ----------------------------

        private void GetCandidatoCdaSingolaZone(ref TTZone a, int APosPresCda, int ATotPresCda, bool IsActiveVotaTutti)
        {
            float x, y, r, b;

            // LAYOUT VERTICALE
            // innanzitutto quante colonne ho?
            int col = GetColonneCandSingola(ATotPresCda);
            // calcolo le colonne
            float yy = 97 / (col);
            float ms = (yy - GetDimensioneCdaSingola(col)) / 2;
            // ora devo calcolarmi la posizione sulla colonna
            x = 2 + ms + (yy * ((GetColonneCandSingola(APosPresCda)) - 1));

            int starty = 26;
            if (IsActiveVotaTutti)
                starty = 42;

            y = starty + ((GetYAlt(APosPresCda) * 13) - 2);
            r = x + GetDimensioneCdaSingola(col);
            b = y + 9;
            GetZone(ref a, (int)x, (int)y, (int)r, (int)b);

            //return;
            //// LAYOUT ORIZZONTALE
            //// qua setto la posizione del candidato, in funzione del totale
            //// NOTA: In questa versione sono possibili fino a 3 Candidati presentati dal CDA
            //// l'area disponibile in base 1280x1024 è:
            //// x:540 y:180 w:614 h:76 la larghezza del rettangolo è 400x3 600x2 800x1
            //float yy = 48 / (ATotPresCda);
            //float ms = (yy - GetDimensioneCdaSingola(ATotPresCda)) / 2;
            //x = 42 + ms + (yy * (APosPresCda - 1));

            //y = 18;
            //r = x + GetDimensioneCdaSingola(ATotPresCda);
            //b = 25;
            //GetZone(ref a, (int)x, (int)y, (int)r, (int)b);

        }

        private float GetDimensioneCdaSingola(int ATotPresCda)
        {
            float[] zz = new float[] { 0, 46, 34, 29, 20, 17 };
            return zz[ATotPresCda];
        }

        // ------------------ CANDIDATO ALTERNATIVO SINGOLA ----------------------------

        private void GetCandidatoAltSingolaZone(ref TTZone a, int APosCandAlt, int ATotCandAlt, int ANPresentatoCDA)
        {
            // qui è più difficile, perchè bisogna dividere in colonne
            float x, y, r, b;

            //calcolo che ne ho max 3 in colonna, posso poi veder x 4
            // innanzitutto quante colonne ho?
            int col = GetColonneCandSingola(ATotCandAlt);
            // calcolo le colonne
            float yy = 97 / (col);
            float ms = (yy - GetDimensioneCdaSingola(col)) / 2;
            // ora devo calcolarmi la posizione sulla colonna
            x = 2 + ms + (yy * ((GetColonneCandSingola(APosCandAlt)) - 1));
            // devo calcolarmi la y
            int starty = 21;
            if (ANPresentatoCDA > 0 && ANPresentatoCDA <= 3)
                starty = 33;
            if (ANPresentatoCDA > 3 && ANPresentatoCDA <= 6)
                starty = 46;

            y = starty + ((GetYAlt(APosCandAlt) * 11) - 2);
            r = x + GetDimensioneCdaSingola(col);
            b = y + 8;
            GetZone(ref a, (int)x, (int)y, (int)r, (int)b);
        }

        private int GetColonneCandSingola(int ATotCandAlt)
        {
            //int[] zz = new int[] { 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4 };
            int[] zz = new int[] { 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3 };
            return zz[ATotCandAlt];
        }

        private float GetYAlt(int ATotCandAlt)
        {
            int[] zz = new int[] { 0, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4 };
            //int[] zz = new int[] { 0, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };
            return zz[ATotCandAlt];
        }

        private float GetDimensioneSingola(int ATotPresCda)
        {
            float[] zz = new float[] { 0, 46, 34, 29, 20, 17 };
            return zz[ATotPresCda];
        }

        // ------------------ Singola pagina di 21 candidati ----------------------------

        private void GetCandidatoAltSingolaPagina21(ref TTZone a, int APosCandAlt, int ATotCandAlt, int ANPresentatoCDA)
        {
            // qui è più difficile, perchè bisogna dividere in colonne
            float x, y, r, b;

            //calcolo che ne ho max 3 in colonna, posso poi veder x 4
            // innanzitutto quante colonne ho?
            int col = GetTotColonneCandSingolaPagina21(ATotCandAlt);
            // calcolo le colonne
            float yy = 97 / (col);
            float ms = (yy - GetDimensioneSingolaPagina21(col)) / 2;
            // ora devo calcolarmi la posizione sulla colonna
            x = 2 + ms + (yy * ((GetColonneCandSingolaPagina21(APosCandAlt, ATotCandAlt)) - 1));
            // devo calcolarmi la y
            int starty = 15;
            //if (ANPresentatoCDA > 0 && ANPresentatoCDA <= 3)
            //    starty = 33;
            //if (ANPresentatoCDA > 3 && ANPresentatoCDA <= 6)
            //    starty = 46;

            float heig = (float)7.3;
            float dist = (float)9.5;
            y = starty + ((GetYAltPagina21(APosCandAlt, ATotCandAlt) * dist) - 2);
            r = x + GetDimensioneSingolaPagina21(col);
            b = y + heig;
            GetZoneFloat(ref a, x, y, r, b);
        }

        private int GetTotColonneCandSingolaPagina21(int ATotCandAlt)
        {            
            int[] zz = new int[] { 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3 };
            return zz[ATotCandAlt];
        }

        private int GetColonneCandSingolaPagina21(int AposCand,  int ATotCandAlt)
        {
            if (ATotCandAlt == 19)
            {
                int[] zz = new int[]  {0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3};
                return zz[AposCand];
            }
            else
            {
                int[] zzz = new int[] { 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3 };
                return zzz[ATotCandAlt];
            }
                
        }

        private float GetYAltPagina21(int AposCandAlt, int ATotCandAlt)
        {
            if (ATotCandAlt == 19)
            {
                int[] zz = new int[] {0, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 6};
                return zz[AposCandAlt];
            }
            else
            {
                int[] zz = new int[] { 0, 0, 1, 2, 3, 4, 5, 6, 0, 1, 2, 3, 4, 5, 6, 0, 1, 2, 3, 4, 5, 6 };
                return zz[AposCandAlt];
            }
        }

        private float GetDimensioneSingolaPagina21(int ATotPresCda)
        {
            //float[] zz = new float[] { 0, 46, 34, 29, 20, 17 };
            //return zz[ATotPresCda];
            return 30;
        }

         */
        #endregion


    }
}
