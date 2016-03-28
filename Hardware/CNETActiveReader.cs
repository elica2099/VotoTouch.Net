using System;
using System.Windows.Forms;
using System.IO.Ports;


namespace VotoTouch
{

    public delegate void DataRead(object source, string data);
    public delegate void SerialError(object source, System.IO.Ports.SerialErrorReceivedEventArgs e);


    public class CNETActiveReader : Object
    {

        // classe della porta seriale
        private SerialPort serial;

        // evento
        public event DataRead ADataRead;
        public event SerialError ASerialError;

        public string PortName;
        private string buffer;
        private int pos;

        public CNETActiveReader()
        {
            serial = new SerialPort();

            //configuring the serial port
            // in realtà la porta è diversa
            serial.PortName = "COM1";
            serial.BaudRate = 9600;
            serial.DataBits = 8;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;

            PortName = "COM1";
            buffer = "";

            serial.DataReceived += serial_DataReceived;

            pos = 0;
        }

        public bool Open()
        {
            bool ret, found;

            try
            {
                // prima testo se è presente la com, se no esco dicendo che non va
                // vedi pistole usb che si creano le com e non sono collegate
                found = false;
                foreach (string sysportname in SerialPort.GetPortNames())
                {
                    if (sysportname == PortName)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    // se è aperta la chiudoi
                    if (serial.IsOpen)
                        serial.Close();
                    // setto la porta
                    serial.PortName = PortName;
                    // apro la porta
                    serial.Open();
                    ret = true;
                }
                else
                    ret = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ret = false;
            }
            return ret;
        }

        public void Close()
        {
            serial.Close();
        }

        public void Flush()
        {
            serial.Close();
            buffer = "";
            pos = 0;
            serial.Open();
        }


       private void serial_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
       {
           //
           string msg, dato;
           int i;

           msg = serial.ReadExisting();

           buffer += msg;

           // ciclo lungo il buffer per vedere se riesco a riconoscere un codice valido
           // che finisce con un #13#10, poi shifto il buffer di quel pacchetto
           for (i = pos; i < (buffer.Length-1); i++)
           {
               // devo cercare il carattere 
               if (buffer[i] == 13 && buffer[i + 1] == 10)
               {
                   // ok, ho trovato, devo copiare la stringa
                   dato = buffer.Substring(pos, i);
                   // devo posizionare i a uno e togliere il tutto
                   if ((i + 2) == buffer.Length)
                       buffer = "";
                   else
                   {
                       string gg = buffer.Substring(i + 2, (buffer.Length - i -2));
                       buffer = gg;
                   }
                   pos = 0;

                   if (ADataRead != null) { ADataRead(this, dato); }

               }

           }
       }


       private void serial_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
       {
          // rispedisco al mittente
           if (ASerialError != null) { ASerialError(this, e); }
       }


       // -------------------------------------------------------------------------------
       // AUTODISCOVER
       // -------------------------------------------------------------------------------

       public bool AutodiscoverBarcode( ref string APorta, ref string ADescription, ref int APortaInt)
       {
           bool found = false;
           string ss = "";
           // carico le seriali
           foreach (COMPortInfo comPort in COMPortInfo.GetCOMPortsInfo())
           {
               ss = comPort.Description.ToLower();

               if (ss.IndexOf("datalogic", System.StringComparison.Ordinal) >= 0 ||
                   ss.IndexOf("barcode", System.StringComparison.Ordinal) >= 0 ||
                   ss.IndexOf("scanner", System.StringComparison.Ordinal) >= 0 ||
                   ss.IndexOf("hyperion", System.StringComparison.Ordinal) >= 0 ||
                   ss.IndexOf("orbit", System.StringComparison.Ordinal) >= 0
                   )
               {
                   // la porta va bene, setto
                   APorta = comPort.Name;
                   ADescription = comPort.Description;

                               // ora devo trovare la stringa COM
                   string ss2 = comPort.Name;
                   const string removeString = "COM";
                   int index = ss2.LastIndexOf(removeString, System.StringComparison.Ordinal);
                   if (index >= 0)
                   {
                       string st = ss2.Remove(ss2.IndexOf(removeString, System.StringComparison.Ordinal), removeString.Length);
                       //string st = ss.Substring(index +3, ss.Length - index +3); // ss[index + 3].ToString();
                       APortaInt = Convert.ToInt16(st);
                   }
                   //// set the property
                   //App.Config.cfg.ReaderUsa = true;
                   //App.Config.cfg.ReaderComPort = comPort.Name;


                   //// set the port hardware
                   //if (evRequestConfiguraLettore != null)
                   //    evRequestConfiguraLettore(this, true, porta);

                   found = true;
                   break;
               }
           }
           return found;
       }


    }

}


