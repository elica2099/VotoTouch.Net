using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace VotoTouch
{

    public class CIPThreadSemaphore : CBaseSemaphore
    {

        public Socket SockSem;

        // timer di sostenimento
        private System.Windows.Forms.Timer timSemaforo;

        private System.Net.IPAddress remoteIPAddress;
        private System.Net.IPEndPoint remoteEndPoint;

        public int IPPort;

        private string sGloSend;

        private static Mutex MyMutex = new Mutex(false, "MyMutex");

        private Thread semaf;

        public CIPThreadSemaphore() 
        {
            ConnAddress = "127.0.0.1";
            IPPort = 1001;

            SemaforoAttivo = false;
            SemStato = TStatoSemaforo.stsNulla;

            // creo il timer
            timSemaforo = new System.Windows.Forms.Timer();
            timSemaforo.Enabled = false;
            timSemaforo.Interval = 30000;
            timSemaforo.Tick += timSemaforo_tick;

            sGloSend = "";
        }

        ~CIPThreadSemaphore()
        {
            // Destructor
            //if (SemaforoAttivo && SockSem.Connected) SockSem.Close();
            //SockSem.Dispose();
        }


        override public bool AttivaSemaforo(bool AAttiva)
        {
            try
            {
                if (AAttiva)
                {
                    semaf = new Thread(new ThreadStart(ThreadSemaphore)) {IsBackground = true};
                    semaf.Start();
                    SemaforoAttivo = true;
                }
                else
                {
                    if (semaf != null) semaf.Abort();
                    semaf = null;
                    timSemaforo.Enabled = false;
                    SemaforoAttivo = false;
                }
            }
            catch (SocketException se)
            {
                //MessageBox.Show(se.Message);
                Logging.WriteToLog("<SemError> : AttivaSemaforo " + se.Message);
                //SemaforoAttivo = false;
            }

            return SemaforoAttivo;
        }


        // -----------------------------------------------------------------------------
        //  Routine del thread
        // -----------------------------------------------------------------------------

        void ThreadSemaphore()
        {
            string ss;
            Byte[] byteSend;

            // questo è il thread, verifica se ci sono comandi e nel caso li manda
            String szIPSelected = ConnAddress;
            remoteIPAddress = System.Net.IPAddress.Parse(szIPSelected);
            remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, IPPort);

            while (true)
            {

                if (sGloSend != "")
                {
                    // leggo la variabile bloccandola 
                    MyMutex.WaitOne();
                    ss = sGloSend;
                    sGloSend = "";
                    MyMutex.ReleaseMutex();

                    try
                    {
                        // crea il socket
                        SockSem = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        // connette il socket
                        SockSem.Connect(remoteEndPoint);
                        //
                        byteSend = Encoding.ASCII.GetBytes(ss);
                        //
                        SockSem.Send(byteSend);
                        //
                        SockSem.Close();
                        SockSem.Dispose();
                        SockSem = null;
                        MyMutex.WaitOne();

                        MyMutex.ReleaseMutex();
                    }
                    catch (Exception ex)
                    {
                        SockSem = null;
                        MyMutex.WaitOne();
                        sGloSend = "";
                        Logging.WriteToLog("<SemError> : ThreadSemaphore " + ex.Message);

                        MyMutex.ReleaseMutex();
                    }
                    
                }

                Thread.Sleep(300);

            }


        }


        // -----------------------------------------------------------------------------
        //  Timer
        // -----------------------------------------------------------------------------

        override public void timSemaforo_tick(object sender, EventArgs e)
        {
            // è un timer di controllo che riattiva sempre il timer
            try
            {
                if (SemaforoAttivo)
                {
                    // ribatto il comando
                    switch (SemStato)
                    {
                        case TStatoSemaforo.stsOccupato:
                            SemaforoOccupato();
                            break;
                        case TStatoSemaforo.stsLibero:
                            SemaforoLibero();
                            break;
                        case TStatoSemaforo.stsErrore:
                            SemaforoErrore();
                            break;
                        case TStatoSemaforo.stsFineoccupato:
                            SemaforoFineOccupato();
                            break;
                        case TStatoSemaforo.stsChiusoVoto:
                            SemaforoChiusoVoto();
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Logging.WriteToLog("<SemError> : timSemaforo_tick " + ex.Message);
                SemaforoAttivo = false;
                timSemaforo.Enabled = false;
            }
        }
        

        private void SendData(char AColor, char AFlash, bool ARound)
        {
            // riattivo il timer
            timSemaforo.Enabled = false;
            timSemaforo.Enabled = true;

            string sSend = new string(' ', 128);

            try
            {
                if (SemaforoAttivo)
                {
                    // mando i dati
                    sSend = ARound ? MakeRoundFull(AColor, AFlash) : MakeCharX(AColor, AFlash);

                    sGloSend = sSend;
                }
            }
            catch (SocketException se)
            {
                //MessageBox.Show(se.Message);
                Logging.WriteToLog("<SemError> : SendData " + se.Message);
                //SemaforoAttivo = false;
            }
        }

        // -----------------------------------------------------------------------------
        // classi relative al semaforo
        // -----------------------------------------------------------------------------

        override public void SemaforoOccupato()
        {
            char SColor = 'R';
            char SFlash = '0';

            SemStato = TStatoSemaforo.stsOccupato;
            if (SemaforoAttivo)
            {
                // mando i dati
                SendData(SColor, SFlash, true);
            }
            // chiamo la classe base per l'evento
            base.SemaforoOccupato();
        }

        override public void SemaforoLibero()
        {
            char SColor = 'V';
            char SFlash = '0';

            SemStato = TStatoSemaforo.stsLibero;
            if (SemaforoAttivo)
            {
                // mando i dati
                SendData(SColor, SFlash, true);
            }
            // chiamo la classe base per l'evento
            base.SemaforoLibero();
        }

        override public void SemaforoErrore()
        {
            char SColor = 'R';
            char SFlash = '1';

            SemStato = TStatoSemaforo.stsErrore;
            if (SemaforoAttivo)
            {
                // mando i dati
                SendData(SColor, SFlash, false);
            }
            // chiamo la classe base per l'evento
            base.SemaforoErrore();
        }

        override public void SemaforoFineOccupato()
        {
            char SColor = 'R';
            char SFlash = '1';

            SemStato = TStatoSemaforo.stsFineoccupato;
            if (SemaforoAttivo)
            {
                // mando i dati
                SendData(SColor, SFlash, true);
            }
            // chiamo la classe base per l'evento
            base.SemaforoFineOccupato();
        }

        override public void SemaforoChiusoVoto()
        {
            char SColor = 'R';
            char SFlash = '0';

            SemStato = TStatoSemaforo.stsChiusoVoto;
            if (SemaforoAttivo)
            {
                // mando i dati
                SendData(SColor, SFlash, true);
            }
            // chiamo la classe base per l'evento
            base.SemaforoChiusoVoto();
        }

        // -----------------------------------------------------------------------------
        // classi relative al semaforo
        // -----------------------------------------------------------------------------

        public String MakeCrc(String sText)
        {
            int nVar;
            int nCrc;
            //char ch;
            char h;
            char l;

            string sTCrc = "";
            nCrc = 0;

            for(nVar=0;nVar < sText.Length; nVar++) {
                nCrc ^= sText[nVar];
            }


            h = (char)((nCrc & 0xF0) / 0x10 + 0x20);
            l = (char)((nCrc & 0x0F) + 0x20);

            sTCrc = '<' + sText + h + l + '>';

            return (sTCrc);
        }

        public String MakeBuffer(char ch,char color, char Effect)
        {
            string sTextOut = "300GC=0;101;" + '0' + Effect + color + Effect + ch + ';';

            sTextOut = MakeCrc(sTextOut);

            return (sTextOut);

        }

        public String MakeRoundFull(char cColor,char Flash)
        {
            
              
            return (MakeBuffer(CHAR_ROUND,cColor, Flash));
        }

        public String MakeRoundEmpty(char cColor,char Flash)
        {

            return (MakeBuffer(CHAR_ROUND_EMPTY, cColor, Flash));
        }

        public String MakeCharX(char cColor,char Flash)
        {

            return (MakeBuffer(CHAR_X, cColor, Flash));
        }
        

    }

    
}
