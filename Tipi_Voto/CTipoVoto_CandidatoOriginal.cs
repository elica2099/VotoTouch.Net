using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VotoTouch
{
    public class CTipoVoto_CandidatoOriginal: CBaseTipoVoto
    {

        // CLASSE DELLA votazione di candidato
		// Versione ORIGINALE da VotoSegreto
        
        public CTipoVoto_CandidatoOriginal(Rectangle AFormRect) : base(AFormRect)
        {
            // costruttore
        }

        //override public void GetTouchVoteZone(TAppStato AStato, TNewVotazione AFVotaz, 
        //                                                bool ADiffer, ref ArrayList Tz )
        override public void GetTouchVoteZone(TNewVotazione AVotazione)
        {
            // DR12 OK
            Tz.Clear();
            // metto i rettangoli del candidato a singola pagina
            NewCalcolaTouchCandidatoPagina(AVotazione);
            // ora devo mettere i tabs
            CalcolaTouchTabsPagina(AVotazione);
            // nella classe base c'è qualcosa
            base.GetTouchVoteZone(AVotazione);
        }

        // --------------------------------------------------------------
        //  CALCOLO DEL TOUCH CANDIDATO/MULTICANDIDATO PAGINA NUOVO
        // --------------------------------------------------------------

        #region calcolo candidato/multicandidato touch nuovo

        public void NewCalcolaTouchCandidatoPagina(TNewVotazione AFVotaz)
        {
            // DR12 OK
            TTZone a;
            TNewLista li;
            int z, PosPresCda, PosCandAlt;
            TTEvento evento;

            // in funzione della votazione seleziono l'evento corrispondente al tocco del voto
            // se è Multicandidato, l'evento sarà solo locale e setterà un flag nella collection, 
            // altrimenti richiamerà il voto valido all'esterno
            if (AFVotaz.TipoVoto == TTipoVoto.stvMultiCandidato) // VSDecl.VOTO_MULTICANDIDATO)
                evento = TTEvento.steMultiValido;
            else
                evento = TTEvento.steVotoValido;

            PosPresCda = 1;
            PosCandAlt = 1;

            // ok, ciclo lungo i candidati per metterli nell'area giusta
            //foreach(TNewLista li in AFVotaz.Liste)
            for (z = 0; z < AFVotaz.NListe; z++)
            {
                li = (TNewLista)AFVotaz.Liste[z];

                // Devo testare se il candidato è presentato dal cda
                if (li.PresentatodaCDA)
                {
                    // ok, è presentato, va nell'area in alto
                    a = new TTZone();
                    GetNew_CandidatoCdaZone(ref a, PosPresCda, AFVotaz.NPresentatoCDA, AFVotaz.AreaVoto);
                    a.expr = z; a.ev = evento;
                    a.Text = li.DescrLista;
                    a.Multi = 0; a.cda = true;
                    a.pag = 0;
                    Tz.Add(a);
                    PosPresCda++;
                }
                else
                {
                    // una voltra controllavo la pagina corrente /if (li.Pag == CurrPag) ora non si usa più
                    // perché aggiungo tutti i candidati e 
                    // non è presentato, va nell'area in basso
                    a = new TTZone();
                    GetNew_CandidatoAltPaginaZone(ref a, PosCandAlt, AFVotaz.AreaVoto);
                    a.expr = z; a.ev = evento;
                    a.Text = li.DescrLista;
                    a.Multi = 0; a.cda = false;
                    a.pag = li.Pag;
                    Tz.Add(a);
                    PosCandAlt++;
                    // aggiunta successiva
                    if (PosCandAlt > AFVotaz.AreaVoto.CandidatiPerPagina)
                        PosCandAlt = 1;
                }
            }

            // Le schede Speciali
            MettiSchedeSpeciali(AFVotaz);

            //// Ok, ora la scheda bianca e il non voto
            //if (AFVotaz.SkBianca && !AFVotaz.SkNonVoto)
            //{
            //    // la scheda bianca ( che è sempre l'ultima, quindi ntasti)
            //    a = new TTZone();
            //    // cambio la posizione nel caso ho Avanti
            //    if (AFVotaz.TipoVoto == VSDecl.VOTO_MULTICANDIDATO)
            //        GetZone(ref a, 12, 81, 44, 91);
            //    else
            //        GetZone(ref a, 23, 79, 78, 92);
            //    a.expr = VSDecl.VOTO_SCHEDABIANCA;
            //    a.pag = 0;
            //    a.cda = false;
            //    a.Multi = 0;
            //    a.Text = "";
            //    a.ev = TTEvento.steSkBianca;
            //    Tz.Add(a);
            //}
            //else
            //{
            //    // Ok, ora la scheda bianca
            //    if (AFVotaz.SkBianca)
            //    {
            //        a = new TTZone();
            //        // se c'è anche non voto devo spostarla
            //        if (!AFVotaz.SkNonVoto)
            //            GetZone(ref a, 32, 76, 67, 90); // non la sposto sta in centro
            //        else
            //            GetZone(ref a, 10, 76, 44, 90); //la sposto a sinistra
            //        a.expr = VSDecl.VOTO_SCHEDABIANCA;
            //        a.Text = "";
            //        a.ev = TTEvento.steSkBianca;
            //        a.pag = 0;
            //        a.Multi = 0;
            //        Tz.Add(a);
            //    }
            //    // il non voto, se presente (caso BPM)
            //    if (AFVotaz.SkNonVoto)
            //    {
            //        a = new TTZone();
            //        // se c'è anche SkBianca devo spostarla
            //        //if (!AFVotaz.SkBianca)
            //        //    GetZone(ref a, 32, 76, 67, 90); // non la sposto, sta in centro
            //        //else
            //        //    GetZone(ref a, 55, 76, 89, 90); //la sposto a destra
            //        GetZone(ref a, 75, 88, 97, 100); // in bass a sx
            //        a.expr = VSDecl.VOTO_NONVOTO;
            //        a.Text = "";
            //        a.ev = TTEvento.steSkNonVoto;
            //        a.pag = 0;
            //        a.Multi = 0;
            //        Tz.Add(a);
            //    }
            //}
            // Attenzione, nel caso la votazione sia di tipo Multicandidato, devo Aggiungere un tasto
            // "Avanti" o "Conferma" per continuare ed è possibile che ci sia un tasto SelezionaTuttiCDA
            if (AFVotaz.TipoVoto == TTipoVoto.stvMultiCandidato) //VSDecl.VOTO_MULTICANDIDATO)
            {
                // devo aggiungere il tasto con evento           
                a = new TTZone();
                GetZone(ref a, 640, 810, 970, 910); a.expr = VSDecl.VOTO_MULTIAVANTI;
                a.Text = ""; a.ev = TTEvento.steMultiAvanti; a.pag = 0; a.cda = false; a.Multi = 0;
                Tz.Add(a);

                // se nella votazione è presente il seleziona TuttoCDA
                if (AFVotaz.SelezionaTuttiCDA && AFVotaz.NPresentatoCDA > 0)
                {
                    // devo mettere il tasto
                    a = new TTZone();
                    //int y;
                    // devo fare attenzione a quante righe ha il cda e spostare il tasto
                    if (AFVotaz.NPresentatoCDA <= 3)
                        GetZone(ref a, (AFVotaz.AreaVoto.RCda() - 240), (AFVotaz.AreaVoto.BCda() + 20),
                            AFVotaz.AreaVoto.RCda(), (AFVotaz.AreaVoto.BCda() + 8));
                    else
                        GetZone(ref a, (AFVotaz.AreaVoto.RCda() - 240), (AFVotaz.AreaVoto.BCda() - 40),
                            AFVotaz.AreaVoto.RCda(), (AFVotaz.AreaVoto.BCda() + 3));
                    a.expr = 999; a.cda = false;
                    a.Text = ""; a.ev = TTEvento.steMultiSelezTuttiCDA; a.pag = 0;
                    Tz.Add(a);
                }
            }
        }

        private void GetNew_CandidatoCdaZone(ref TTZone a, int APosPresCda, int ATotPresCda, TAreaVotazione Area)
        {
            // ho una zona di voto che è in Area, ho il totale di quanti sono, ela posizione 
            float x, y, r, b, ax, ar;
            int nct, nrt, npc, npr;

            // PER ETRURIA
            // se ho un candidato solo, come nel 99% dei casi di voto singolo, metto 
            //if (ATotPresCda == 1)
            //{
            //    ax = Area.XCda +16;
            //    y = Area.YCda;
            //    ar = Area.RCda() -16;
            //    b = Area.BCda() +1;

            //    GetZone(ref a, (int)ax, (int)y, (int)ar, (int)b);
            //    return;
            //}


            // definisco il n. di colonne totali / per il numero totale di cda
            int[] ncolt = new int[] { 0, 1, 2, 3, 2, 3, 3 };
            nct = ncolt[ATotPresCda];
            // definisco il n. di righe totali
            if (ATotPresCda <= 3) nrt = 1; else nrt = 2;
            // stabilisco la posizione reale del rett, in che colonna è, in funzione di APosPresCda
            // c'è un correttivo, perchè nel caso di 4 cvandidati non vado su tre file, ma su due
            int[] nposc = new int[] { 0, 1, 2, 3, 1, 2, 3 };
            npc = nposc[APosPresCda];
            if (ATotPresCda == 4 && APosPresCda == 3) npc = 1;
            if (ATotPresCda == 4 && APosPresCda == 4) npc = 2;
            // stabilisco la posizione nella riga
            int[] nposr = new int[] { 0, 1, 1, 1, 2, 2, 2 };
            npr = nposr[APosPresCda];
            if (ATotPresCda == 4 && APosPresCda == 3) npr = 2;
            if (ATotPresCda == 4 && APosPresCda == 4) npr = 2;

            // calcolo il rettangolo globale
            x = Area.XCda + ((Area.WCda / nct) * (npc - 1));
            y = Area.YCda + ((Area.HCda / nrt) * (npr - 1));
            r = x + (Area.WCda / nct);
            b = y + (Area.HCda / nrt);
            // devo ora calcolarmi il vero rettangolo interno
            if (npr == 1)
                b = y + HRETT_CANDIDATO;
            else
                y = b - HRETT_CANDIDATO;
            // devo centrare i rettangoli
            float[] dimr = new float[] { 0, 38, 34, 29 };
            ax = x + ((r - x - dimr[nct]) / 2);
            ar = r - ((r - x - dimr[nct]) / 2);

            GetZone(ref a, (int)ax, (int)y, (int)ar, (int)b);
        }

        private void GetNew_CandidatoAltPaginaZone(ref TTZone a, int APosCandAlt, TAreaVotazione Area)
        {
            // ho il n. di candidati per pagina in Area.CandidatiPerPagina

            float x, y, r, b, ax, ar;
            int nct, nrt, npc, npr;

            // definisco il n. di colonne totali in funzione del numero di candidati
            if (Area.CandidatiPerPagina == 1) nct = 1; else nct = 2;
            // definisco il n. di righe totali
            int[] nrowt = new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
            nrt = nrowt[Area.CandidatiPerPagina];
            // posizione del rettangolo sulla riga
            int[] nposc = new int[] { 0, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            npc = nposc[APosCandAlt];

            // posizione sulla riga
            npr = nrowt[APosCandAlt];

            // calcolo il rettangolo globale
            x = Area.XAlt + ((Area.WAlt / nct) * (npc - 1));
            y = Area.YAlt + ((Area.HAlt / nrt) * (npr - 1));
            r = x + (Area.WAlt / nct);
            b = y + (Area.HAlt / nrt);
            // devo ora calcolarmi il vero rettangolo interno
            b = y + HRETT_CANDIDATO;
            // devo centrare i rettangoli
            float[] dimr = new float[] { 0, 380, 340, 280 };
            ax = x + ((r - x - dimr[nct]) / 2);
            ar = r - ((r - x - dimr[nct]) / 2);
            GetZone(ref a, (int)ax, (int)y, (int)ar, (int)b);
        }

        // --------------------------------------------------------------
        //  CALCOLO DEL TOUCH TABS
        // --------------------------------------------------------------

        public void CalcolaTouchTabsPagina(TNewVotazione AFVotaz)
        {
            // DR12 OK
            // se non ho bisogno di tab è inutile, tanto vale uscire
            if (!AFVotaz.AreaVoto.NeedTabs) return;

            // mette i tabs che ci sono in funzione delle pagine contenute in Pagina
            int i, ncol, acol, arow;
            TIndiceListe il;
            int x, y, ax, ay, w, h;
            TTZone a;

            // x:995 y:320 w:295 h:600
            x = 77;
            y = AFVotaz.AreaVoto.YAlt; // 31;
            h = 5; // altezza dei tabs fissa
            // bisogna stabilire quante colonne ci sono
            ncol = ((AFVotaz.Pagine.Count - 1) / 8) + 1;
            w = 24 / ncol;

            acol = 0;
            arow = 0;
            // parto da 1 perche la pagina 0 è quella dei candidati cda
            for (i = 1; i < AFVotaz.Pagine.Count; i++)
            {
                il = (TIndiceListe)AFVotaz.Pagine[i];
                // ok, ora inserisco in funzione della posizione
                a = new TTZone();
                ax = x + (w * acol);
                ay = y + (6 * arow);

                GetZone(ref a, ax, ay, ax + w - 1, ay + h);

                a.expr = il.pag; a.ev = TTEvento.steTabs;
                a.Text = il.indice.ToLower();
                a.Multi = 0; a.pag = 0;
                Tz.Add(a);

                arow++;
                if (arow >= 8)
                {
                    arow = 0;
                    acol++;
                }
            }
        }

        #endregion



    }
}
