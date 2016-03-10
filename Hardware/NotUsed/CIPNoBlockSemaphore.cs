using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace VotoTouch
{

    public class CIPNoBlockSemaphore : CBaseSemaphore
    {
		// classe dei socket asincroni
		// non bloccanti

        public enum TStatoSocket : int { stsNonConnesso, stsInConnessione, stsPronto, stsInInvio };

        public Socket SockSem;

        // timer di sostenimento
        private System.Windows.Forms.Timer timSemaforo;

        private System.Net.IPAddress remoteIPAddress;
        private System.Net.IPEndPoint remoteEndPoint;

        public int IPPort;

        public TStatoSocket SckStato;

        public CIPNoBlockSemaphore() 
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

            SckStato = TStatoSocket.stsNonConnesso;
        }

        ~CIPNoBlockSemaphore()
        {
            // Destructor
            //if (SemaforoAttivo && SockSem.Connected) SockSem.Close();
            //SockSem.Dispose();
        }

        override public void timSemaforo_tick(object sender, EventArgs e)
        {
            // è un timer di controllo che riattiva sempre il timer
            try
            {
                if (SemaforoAttivo && SockSem.Connected)
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
                    }
                }

                // sono in una condizione di errore
                if (SemaforoAttivo && !SockSem.Connected)
                // se non è connesso bisogna connetterlo
                {
                    Logging.WriteToLog("     >> Semaforo scollegato, tento riconnessione");
                    SockSem = null;
                    AttivaSemaforo(SemaforoAttivo);
                }

            }
            catch (Exception ex)
            {
                Logging.WriteToLog("<SemError> : timSemaforo_tick " + ex.Message);
                SemaforoAttivo = false;
                timSemaforo.Enabled = false;
            }
        }
        
        override public bool AttivaSemaforo(bool AAttiva)
        {
            try
            {
                if (AAttiva)
                {
                    //create a new client socket ...
                    SockSem = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    String szIPSelected = ConnAddress;

                    remoteIPAddress = System.Net.IPAddress.Parse(szIPSelected);
                    remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, IPPort);

                    Connect(remoteEndPoint, SockSem);

                    //SockSem.Connect(remoteEndPoint);
                    //timSemaforo.Enabled = true;
                    SemaforoAttivo = true;
                }
                else
                {
                    if (SockSem != null && SockSem.Connected)
                            SockSem.Close();
                    SockSem = null;
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
        //      METODI DI CONNESSIONE
        // -----------------------------------------------------------------------------

        public void Connect(EndPoint remoteEP, Socket client)
        {
            SockSem.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);

            SckStato = TStatoSocket.stsInConnessione;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                SckStato = TStatoSocket.stsPronto;
            }
            catch (Exception e)
            {
                Logging.WriteToLog("<SemError> : ConnectCallback " + e.Message);
            }
        }

        // -----------------------------------------------------------------------------
        //      METODI DI INVIO DATI
        // -----------------------------------------------------------------------------

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
                new AsyncCallback(SendCallback), client);

            SckStato = TStatoSocket.stsInInvio;
        }

        private void SendData(char AColor, char AFlash, bool ARound)
        {
            String sSend;

            sSend = new string(' ', 128);

            try
            {
                if (SemaforoAttivo && SockSem.Connected)
                {
                    // mando i dati
                    if (ARound)
                        sSend = MakeRoundFull(AColor, AFlash);
                    else
                        sSend = MakeCharX(AColor, AFlash);

                    Byte[] byteSend = Encoding.ASCII.GetBytes(sSend);
                    // Begin sending the data to the remote device.
                    SockSem.BeginSend(byteSend, 0, byteSend.Length, SocketFlags.None,
                        new AsyncCallback(SendCallback), SockSem);
                    SckStato = TStatoSocket.stsInInvio;
                }
            }
            catch (SocketException se)
            {
                //MessageBox.Show(se.Message);
                Logging.WriteToLog("<SemError> : SendData " + se.Message);
                //SemaforoAttivo = false;
            }
            //finally
            //{
            //    SockSem.Blocking = blockingState;
            //}
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                SckStato = TStatoSocket.stsPronto;
            }
            catch (Exception e)
            {
                Logging.WriteToLog("<SemError> : SendCallback " + e.Message);
                //Console.WriteLine(e.ToString());
            }
        }

        // -----------------------------------------------------------------------------
        //      VECCHI METODI
        // -----------------------------------------------------------------------------

        private void SendDataOld(char AColor, char AFlash, bool ARound)
        {
            String sSend;
        
            sSend = new string(' ', 128);

            try
            {
                if (SemaforoAttivo && SockSem.Connected)
                {
                    // mando i dati
                    if (ARound)
                        sSend = MakeRoundFull(AColor, AFlash);
                    else
                        sSend = MakeCharX(AColor, AFlash);

                    Byte[] byteSend = Encoding.ASCII.GetBytes(sSend);
                    //SockSem.Blocking = false;
                    SockSem.Send(byteSend);

                    //Object objData = txtData.Text;
                    //byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                    //m_socClient.Send(byData);
                }
            }
            catch (SocketException se)
            {
                //MessageBox.Show(se.Message);
                Logging.WriteToLog("<SemError> : SendData " + se.Message);
                //SemaforoAttivo = false;
            }
            //finally
            //{
            //    SockSem.Blocking = blockingState;
            //}
        }


                //// This is how you can determine whether a socket is still connected.
                //bool blockingState = client.Blocking;
                //try
                //{
                //    byte [] tmp = new byte[1];

                //    client.Blocking = false;
                //    client.Send(tmp, 0, 0);
                //    Console.WriteLine("Connected!");
                //}
                //catch (SocketException e) 
                //{
                //    // 10035 == WSAEWOULDBLOCK
                //    if (e.NativeErrorCode.Equals(10035))
                //        Console.WriteLine("Still Connected, but the Send would block");
                //    else
                //    {
                //        Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                //    }
                //}
                //finally
                //{
                //    client.Blocking = blockingState;
                //}



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

        // -----------------------------------------------------------------------------
        // classi relative al semaforo
        // -----------------------------------------------------------------------------

        public String MakeCrc(String sText)
        {
            String sTCrc;
            int nVar;
            int nCrc;
            //char ch;
            char h;
            char l;

            sTCrc = "";
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
            String sTextOut;

            sTextOut = "300GC=0;101;" + '0' + Effect + color + Effect + ch + ';';

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
