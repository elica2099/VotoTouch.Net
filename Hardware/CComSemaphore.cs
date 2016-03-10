using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace VotoTouch
{
    class CComSemaphore : CBaseSemaphore
    {

        // classe della porta seriale
        private SerialPort Aserial;
        private byte[] SerBuf;
        //private byte serial_written_flag;
        private bool FlipFlop;
        // timer di sostenimento
        private System.Windows.Forms.Timer timSemaforo;
       
        public CComSemaphore()
        {
            Aserial = new SerialPort();

            //configuring the serial port
            // in realtà la porta è diversa
            Aserial.PortName = "COM1";
            Aserial.BaudRate = 9600;
            Aserial.DataBits = 8;
            Aserial.Parity = Parity.None;
            Aserial.StopBits = StopBits.One;

            this.SerBuf = new byte[10];
            //this.serial_written_flag = 0;

            // creo il timer
            timSemaforo = new System.Windows.Forms.Timer();
            timSemaforo.Enabled = false;
            timSemaforo.Interval = 800;
            timSemaforo.Tick += timSemaforo_tick;

            ConnAddress = "COM1";
            SemaforoAttivo = false;
            FlipFlop = false;
            SemStato = TStatoSemaforo.stsNulla;
        }

        ~CComSemaphore()
        {
            if (Aserial.IsOpen)
            {
                //this.SerBuf[0] = 110; // li spengo tutti
                //this.i2c_transmit(1);
                //Aserial.Close();
            }
        }

        // connessione ---------------------------------------------------

        override public bool AttivaSemaforo(bool AAttiva)
        {
            bool result = false;
            // connette la com prendendoi come indirizzo IP_COM_Address
            if (AAttiva)
                result = Connetti();
            else
                result = Disconnetti();
            return result;
        }

        private bool Connetti()
        {

            try
            {
                if (ConnAddress.StartsWith("COM"))
                {
                    if (Aserial.IsOpen)
                    {
                        Aserial.Close();
                    }
                    Aserial.PortName = ConnAddress;
                    Aserial.Parity = Parity.None;
                    Aserial.BaudRate = 0x4b00;
                    Aserial.StopBits = StopBits.Two;
                    Aserial.DataBits = 8;
                    Aserial.ReadTimeout = 50;
                    Aserial.WriteTimeout = 50;
                    Aserial.Open();
                    SerBuf[0] = 90;
                    this.i2c_transmit(1);
                    this.i2c_recieve(2);
                    // ok, vedo se ho trovato quella giusta
                    if (this.SerBuf[0] == 10)
                    {
                        //this.textBox_ver.Text = string.Format("{0}", this.SerBuf[1]);
                        //this.rly02_found = 1;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // non so cosa fare
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return false;
        }

        private bool Disconnetti()
        {
            try
            {
                if (Aserial.IsOpen)
                {
                    Aserial.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private void i2c_recieve(byte read_bytes)
        {
            byte num;
            if (ConnAddress.StartsWith("COM"))
            {
                for (num = 0; num < read_bytes; num = (byte)(num + 1))
                {
                    try
                    {
                        Aserial.Read(this.SerBuf, num, 1);
                    }
                    catch (Exception)
                    {
                        this.SerBuf[0] = 0xff;
                        //MessageBox.Show("read fail");
                        //this.rly16_found = this.rly02_found = (byte)(this.rly08_found = 0);
                    }
                }
            }
        }

        private void i2c_transmit(byte write_bytes)
        {
            if (ConnAddress.StartsWith("COM"))
            {
                try
                {
                    Aserial.Write(this.SerBuf, 0, write_bytes);
                }
                catch (Exception)
                {
                    //this.rly02_found = this.rly08_found = (byte)(this.rly16_found = 0);
                    //MessageBox.Show("write fail");
                }
            }
        }


        // METODI ---------------------------------------------------

        override public void SemaforoOccupato()
        {
            timSemaforo.Enabled = false;
            SemStato = TStatoSemaforo.stsOccupato;
            // apro il secondo rele e spengo il primo
            if (Aserial.IsOpen)
            {
                this.SerBuf[0] = 0x5c;
                this.SerBuf[1] = 170;
                this.i2c_transmit(2);
            }
            // chiamo la classe base per l'evento
            base.SemaforoOccupato();
        }

        override public void SemaforoLibero()
        {
            timSemaforo.Enabled = false;
            SemStato = TStatoSemaforo.stsLibero;
            // apro il primo rele e spengo il secondo
            if (Aserial.IsOpen)
            {
                this.SerBuf[0] = 0x5c;
                this.SerBuf[1] = 0x55;
                this.i2c_transmit(2);
            }
            // chiamo la classe base per l'evento
            base.SemaforoLibero();
        }

        override public void SemaforoErrore()
        {
            timSemaforo.Enabled = false;
            SemStato = TStatoSemaforo.stsErrore;
            // spengo tuuti e due
            if (Aserial.IsOpen)
            {
                //this.SerBuf[0] = 110; o li spengo tutti
                //this.i2c_transmit(1);
                this.SerBuf[0] = 100;
                this.i2c_transmit(1);
                FlipFlop = false;
                timSemaforo.Enabled = true;
            }
            // chiamo la classe base per l'evento
            base.SemaforoErrore();
        }

        override public void SemaforoFineOccupato()
        {
            // ciclo con il timer
            SemStato = TStatoSemaforo.stsFineoccupato;
            if (Aserial.IsOpen)
            {
                //this.SerBuf[0] = 100;
                //this.i2c_transmit(1);
                FlipFlop = false;
                timSemaforo.Enabled = true;
            }
            // chiamo la classe base per l'evento
            base.SemaforoFineOccupato();
        }

        override public void SemaforoChiusoVoto()
        {
            // ciclo con il timer
            timSemaforo.Enabled = false;
            SemStato = TStatoSemaforo.stsChiusoVoto;
            if (Aserial.IsOpen)
            {
                this.SerBuf[0] = 0x5c;
                this.SerBuf[1] = 170;
                this.i2c_transmit(2);
            }
            // chiamo la classe base per l'evento
            base.SemaforoChiusoVoto();
        }

        // semaforo ---------------------------------------------------

        override public void timSemaforo_tick(object sender, EventArgs e)
        {
            // a seconda dello stato faccio
            if (FlipFlop)
            {
                FlipFlop = false;
                if (SemStato == TStatoSemaforo.stsFineoccupato)
                {
                    this.SerBuf[0] = 0x66;
                    this.i2c_transmit(1);
                }
                if (SemStato == TStatoSemaforo.stsErrore)
                {
                    this.SerBuf[0] = 0x5c;
                    this.SerBuf[1] = 0x03; // 0x55;
                    this.i2c_transmit(2);
                }
            }
            else
            {
                FlipFlop = true;
                if (SemStato == TStatoSemaforo.stsFineoccupato)
                {
                    this.SerBuf[0] = 0x6e;
                    this.i2c_transmit(1);
                }
                if (SemStato == TStatoSemaforo.stsErrore)
                {
                    this.SerBuf[0] = 0x5c;
                    this.SerBuf[1] = 0x02;
                    this.i2c_transmit(2);
                }
            }
        }

    }
}
