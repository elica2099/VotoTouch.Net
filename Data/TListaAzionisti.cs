using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows.Forms;

namespace VotoTouch
{
    
    public class TAzionista
	{
        // dati dell'utente
        public int IDBadge { get; set; }
        public string CoAz { get; set; }
        public int IDAzion { get; set; }
        public int ProgDeleg { get; set; }
        public string RaSo { get; set; }
        public double NAzioni { get; set; }
        public string Sesso { get; set; }
        public int HaVotato { get; set; }
        
        // dati del voto 
        public int IDVotaz { get; set; }
        // voti
        public List<TVotoEspresso> VotiEspressi;

        // test se sk nonvoto
        public bool HaNonVotato { 
            get
            {
                if (VTConfig.AbilitaDirittiNonVoglioVotare)
                    return VotiEspressi.Count(v => v.VotoExp_IDScheda == VSDecl.VOTO_NONVOTO) > 0;
                else
                    return false;
            } 
        }
        // Raso con Sig
        public string RaSo_Sesso
        {
            get
            {
                if (Sesso == "M")
                    return "Sig. " + RaSo;
                if (Sesso == "F")
                    return "Sig.ra " + RaSo;
                return RaSo;
            }
        }

        public TAzionista()
        {
            HaVotato = TListaAzionisti.VOTATO_NO;
            VotiEspressi = new List<TVotoEspresso>();
        }

        public void CopyFrom(ref TAzionista cp)
        {
            IDBadge = cp.IDBadge; CoAz = cp.CoAz; IDAzion = cp.IDAzion; ProgDeleg = cp.ProgDeleg;
            RaSo = cp.RaSo; NAzioni = cp.NAzioni; Sesso = cp.Sesso; HaVotato = cp.HaVotato;
            IDVotaz = cp.IDVotaz;
        }
	}

    public class TListaAzionisti
    {

        public const int VOTATO_NO = 0;
        public const int VOTATO_SESSIONE = 1;
        public const int VOTATO_DBASE = 2;

        // oggetto lista azionisti
        protected List<TAzionista> _Azionisti;
        public List<TAzionista> Azionisti
        {
            get { return _Azionisti; }
            set
            {
                _Azionisti = value;
            }
        }

        // oggetto voto corrente
        protected List<TAzionista> ListaDiritti_VotoCorrente;
        protected int IDVotazione_VotoCorrente;
        // Oggetto titolare che contene il titolare del badge
        public TAzionista Titolare_Badge;

        private CVotoBaseDati DBDati;

        public TListaAzionisti(CVotoBaseDati ADBDati)
        {
            // costruttore
            DBDati = ADBDati;
            _Azionisti = new List<TAzionista>();
            Titolare_Badge = new TAzionista();

            ListaDiritti_VotoCorrente = new List<TAzionista>();
            IDVotazione_VotoCorrente = -1;
        }

        ~TListaAzionisti()
        {
            // Distruttore
        }

        // --------------------------------------------------------------------------
        //  Ritorno dati Normali
        // --------------------------------------------------------------------------

        #region Ritorno dati Normali 

        public List<TAzionista> DammiDirittiDiVotoPerIDVotazione(int AIDVotazione, bool ASoloDaVotare)
        {
            // mi da la collection di diritti di voto per singola votazione
            // in più se ASoloDaVotare = true mi da solo quello che devono esprimere il voto
            // sennò mi da tutti quanti
            if (_Azionisti == null || _Azionisti.Count == 0) return null;

            //List<TAzionista> newList =  
            if (ASoloDaVotare)
                return _Azionisti.Where(a => a.IDVotaz == AIDVotazione && a.HaVotato == VOTATO_NO).ToList();
            else
                return  _Azionisti.Where(a => a.IDVotaz == AIDVotazione).ToList();
        }

        public int DammiTotaleDirittiRimanentiPerIDVotazione(int AIDVotazione)
        {
            return _Azionisti.Count(a => a.IDVotaz == AIDVotazione && a.HaVotato == VOTATO_NO);
        }

        public int DammiTotaleAzioniRimanentiPerIDVotazione(int AIDVotazione)
        {
            return (int)_Azionisti.Where(a => a.IDVotaz == AIDVotazione && a.HaVotato == VOTATO_NO).Sum(a => a.NAzioni);
        }

        public int DammiTotaleDirittiRimanenti()
        {
            return _Azionisti.Count(a => a.HaVotato == VOTATO_NO);
        }

        public int DammiTotaleAzioniRimanenti()
        {
            return (int)_Azionisti.Where(a => a.HaVotato == VOTATO_NO).Sum(a => a.NAzioni);
        }

        public int DammiQuantiDirittiSonoStatiVotati()
        {
            return _Azionisti.Count(a => a.HaVotato != VOTATO_NO);
        }

        public bool TuttiIDirittiSonoStatiEspressi()
        {
            return _Azionisti.Count(a => a.HaVotato == VOTATO_NO) == 0;
        }

        public bool HaDirittiDiVotoMultipli()
        {
            return (DammiMaxNumeroDirittiDiVotoTotali() > 1);
        }

        public int DammiMaxNumeroDirittiDiVotoTotali()
        {
            // questa funzione non è banale, perchè deve estrarre il numero massimo di ritti di voto, 
            // che nel caso normale è semplice, ma in caso di votazioni differenziate già espresse
            // può essere difficoltoso. In pratica seleziono il conteggio dei diritti per IDVotazioni
            // e prendo il numero maggiore
            if (_Azionisti == null || _Azionisti.Count == 0) return 0;

            // qua modifico in funzione del parametro AbilitaDirittiNonVoglioVotare.
            // se è no, prendo il count bvanale, altrimenti devo fare la media

            if (!VTConfig.AbilitaDirittiNonVoglioVotare)
            {
                var listatemp = _Azionisti.Where(a => a.HaVotato == VOTATO_NO).Take(1);
                TAzionista v = listatemp.ElementAt(0);
                return (int) Azionisti.Count(n => n.HaVotato == VOTATO_NO && n.IDVotaz == v.IDVotaz);
            }
            else
            {
                var AzionNoVotato = Azionisti.Where(n => n.HaVotato == VOTATO_NO);
                if (AzionNoVotato.Count() > 0)
                {
                    var maxDiritti = AzionNoVotato
                        .GroupBy(n => n.IDVotaz)
                        .Select(group =>
                                new
                                    {
                                        IDVotaz = group.Key,
                                        //Diritti = group.ToList(),
                                        Count = group.Count()
                                    })
                        .Max(n => n.Count);
                    return (int) maxDiritti;
                }
                else
                    return 0;
            }
        }

        #endregion

        // --------------------------------------------------------------------------
        //  Gestione della procedura di votazione
        // --------------------------------------------------------------------------

        #region Gestione della procedura di votazione

        public bool EstraiAzionisti_VotoCorrente(bool ADifferenziato)
        {
            // estrae l'azionista  (se diff o se ha 1 solo diritto) o l'elenco di azionisti che sono in voto
            // tipicamente prende i primi della lista che non hanno votato:
            // - Se normale prende il gruppo che non ha votato di una singola votazione
            // - Se differenziato prende il primo record che non ha votato indipendentemente dalla votazione
           
            // ritorna true se ce ne sono, false se è finito il voto
            if (_Azionisti != null && _Azionisti.Count > 0)
            {
                // resetta la lista dei diritti di voto correnti
                ListaDiritti_VotoCorrente.Clear();
                if (ADifferenziato)
                {
                    // LINQ Prende il primo e lo trasferisce nella lista correnti
                    var listatemp = _Azionisti.Where(a => a.HaVotato == VOTATO_NO).Take(1);
                    //var azionistas = listatemp as IList<TAzionista> ?? listatemp.ToList();
                    //ListaDiritti_VotoCorrente = azionistas.ToList();
                    foreach (TAzionista c in listatemp)
                        ListaDiritti_VotoCorrente.Add(c);
                }
                else
                {
                    // LINQ Prende i primi n con idvotazioni contigui partendo dal primo
                    var listatemp = _Azionisti.Where(a => a.HaVotato == VOTATO_NO).Take(1);
                    //var azionistas = listatemp as IList<TAzionista> ?? listatemp.ToList();
                    //if (azionistas.Any())
                    if (listatemp.Count() > 0)
                    {
                        // estrae la votazione del primo
                        //TAzionista v = azionistas.ElementAt(0);
                        TAzionista v = listatemp.ElementAt(0);
                        listatemp = _Azionisti.Where(a => a.HaVotato == VOTATO_NO && a.IDVotaz == v.IDVotaz);
                        foreach (TAzionista c in listatemp)
                            ListaDiritti_VotoCorrente.Add(c);
                    }
                }

                // setto l'idvotazionecorrente
                IDVotazione_VotoCorrente = DammiIDVotazione_VotoCorrente();

                // ritorno se 
                return (ListaDiritti_VotoCorrente.Count > 0);
            }
            else
                return false;

            // NOTA: POTREBBERO ESSERE SOLO GLI ID DEGLI AZIONISTI O I PUNTATORI O LINQ
        }

        public int DammiCountDirittiDiVoto_VotoCorrente()
        {
            // conta i diritti di voto relativi agli azionisti in votazione (lista sopra) 
            if (ListaDiritti_VotoCorrente != null)
            {
                return ListaDiritti_VotoCorrente.Count(a => a.HaVotato == VOTATO_NO);
            }
            else 
                return 0;
        }

        public int DammiCountAzioniVoto_VotoCorrente()
        {
            // conta i diritti di voto relativi agli azionisti in votazione (lista sopra) 
            if (ListaDiritti_VotoCorrente != null)
            {
                return (int)ListaDiritti_VotoCorrente.Where(a => a.HaVotato == VOTATO_NO).Sum(a => a.NAzioni);
            }
            else
                return 0;
        }


        public int DammiTotaleDirittiRimanenti_VotoCorrente()
        {
            if (ListaDiritti_VotoCorrente != null)
            {
                return DammiTotaleDirittiRimanentiPerIDVotazione(IDVotazione_VotoCorrente);
            }
            else 
                return 0;
        }

        public int DammiIDVotazione_VotoCorrente()
        {
            // ritorna l'IDVotazione relativo alla lista o al singolo, in pratica del primo item 
            // NOTA: se è normale prende il primo della lista che sono tutti uguali, se diff prende comunque il primo
            if (ListaDiritti_VotoCorrente != null && ListaDiritti_VotoCorrente.Count > 0)
            {
                TAzionista c = ListaDiritti_VotoCorrente.First();
                return  c != null ? c.IDVotaz : -1;
            }
            else
                return -1;
        }

        public string DammiNomeAzionistaInVoto_VotoCorrente(bool AIsVotazioneDifferenziata)
        {
            // se votazione normale prende il titolare, se differenziata il primo della lista dei voti correnti (che sarà sempre 1)
            if (!AIsVotazioneDifferenziata)
                return Titolare_Badge.RaSo;
            else
            {
                if (ListaDiritti_VotoCorrente != null && ListaDiritti_VotoCorrente.Count > 0)
                {
                    TAzionista c = ListaDiritti_VotoCorrente.First();
                    return c != null ? c.RaSo : "";
                }
                else
                    return "";
            }         
        }

        public int DammiDirittiAzioniDiVotoConferma(bool AIsVotazioneDifferenziata)
        {
            if (VTConfig.ModoAssemblea == VSDecl.MODO_AGM_POP)
            {
                if (AIsVotazioneDifferenziata)
                    return 1; 
                else
                {
                    if (!HaDirittiDiVotoMultipli())
                        return 1; 
                    else
                        return DammiCountDirittiDiVoto_VotoCorrente(); //" diritti di voto per";
                }
                //return AIsVotazioneDifferenziata ? 1 : DammiCountDirittiDiVoto_VotoCorrente();
            }

            if (VTConfig.ModoAssemblea == VSDecl.MODO_AGM_SPA)
            {
                // devo ritornare le azioni
                return DammiCountAzioniVoto_VotoCorrente();
                //return AIsVotazioneDifferenziata ? 1 : DammiCountDirittiDiVoto_VotoCorrente();
            }
            return 0;
        }

        #endregion

        // --------------------------------------------------------------------------
        //  Gestione dei voti, sono demandati a azionisti (bisognerà cambiarlo in diritti di voto)
        // --------------------------------------------------------------------------

        public bool ConfermaVoti_VotoCorrente(ref ArrayList AVotiDaSalvare)
        {
            // salvo i voti nell'array dell'azionista
            foreach (TAzionista a in ListaDiritti_VotoCorrente)
            {
                // resetto i voti, non si sa mai che possano essere doppi
                a.VotiEspressi.Clear();
                // carico i voti sull'array
                foreach (TVotoEspresso v in AVotiDaSalvare)
                {
                    a.VotiEspressi.Add(v);
                }
                a.HaVotato = VOTATO_SESSIONE;
            }

            return true;
        }

        public bool ConfermaVotiDaInterruzione(TVotoEspresso vz)
        {
            foreach (TAzionista a in _Azionisti)
            {
                if (a.HaVotato == VOTATO_NO)
                {
                    a.VotiEspressi.Clear();
                    a.VotiEspressi.Add(vz);
                    a.HaVotato = VOTATO_SESSIONE;
                }
            }

            return true;
        }

        // --------------------------------------------------------------------------
        //  Caricamento dati da database
        // --------------------------------------------------------------------------

        public bool CaricaDirittidiVotoDaDatabase(int AIDBadge, ref TListaVotazioni AVotazioni)
        {
            // ok, questa funziomne carica i diritti di voto in funzione
            // del idbadge, in pratica alla fine avrò una lista di diritti *per ogni votazione*
            // con l'indicazione se sono stati già espressi o no
            _Azionisti.Clear();

            return DBDati.CaricaDirittidiVotoDaDatabase(AIDBadge, ref _Azionisti, ref Titolare_Badge, ref AVotazioni);

        }


    }
}
