using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace VotoTouch
{
    public partial class FVSStart : Form
    {
        public FVSStart()
        {
            InitializeComponent();

            listBox1.Items.Clear();
            listBox1.Items.Add("Informazioni sulla versione: ");
            listBox1.Items.Add("");
            listBox1.Items.Add(VSDecl.VTS_VERSION);
#if _DBClose
            listBox1.Items.Add("DBClose version");
#endif

            if (System.IO.File.Exists("c:\\data\\VTS_DEMO.txt"))
            {
                listBox1.Items.Add("");
                listBox1.Items.Add("Versione DEMO");
                return;
            }

            if (System.IO.File.Exists("c:\\data\\VTS_STANDALONE.txt"))
            {
                listBox1.Items.Add("");
                listBox1.Items.Add("Versione STANDALONE");
                listBox1.Items.Add("Usa GEAS.sql in locale");
                CaricaConfig(true);
                return;
            }

            // alcuni controlli : disco m:
            if (!System.IO.Directory.Exists(@"M:\"))
            {
                listBox1.Items.Add("");
                listBox1.Items.Add("ATTENZIONE: DISCO M non presente!");
                listBox1.Items.Add("Mappatura errata dischi.");
            }
            else
                CaricaConfig(false);

            // versione demo
            if (System.IO.File.Exists("c:\\data\\VTS_DEMO.txt"))
            {
                listBox1.Items.Add("");
                listBox1.Items.Add("Versione DEMO");
            }
        }


        // carica la configurazione 
        public Boolean CaricaConfig(bool ADataLocal)
        {
            //DR11 OK
            string ss, GeasFileName;

            // verifica se è locale oppure no
            if (ADataLocal)
            {
                if (File.Exists("c:\\data\\geas.sql"))
                    GeasFileName = "c:\\data\\geas.sql";
                else
                    return false;
            }
            else
            {
                if (File.Exists("M:\\geas.sql"))
                    GeasFileName = "M:\\geas.sql";
                else
                    return false;
            }

            // leggo cosa c'è dentro
            try
            {
                StreamReader file1;
                file1 = File.OpenText(GeasFileName);
                ss = file1.ReadLine();
                // testo se il file è giusto
                if (ss != "") 
                //if (ss == "GEAS 2000 -- Stringa Connesione a SQL")
                {
                    // tutto ok leggo
                    ss = file1.ReadLine();
                    ss = file1.ReadLine(); //DB_Dsn
                    ss = file1.ReadLine(); // DB_Name
                    listBox1.Items.Add( "Db:  " + ss);
                    ss = file1.ReadLine(); //DB_Uid
                    ss = file1.ReadLine(); // DB_Pwd
                    ss = file1.ReadLine(); // DB_Server
                    listBox1.Items.Add("Srv:  " + ss);
                    file1.Close();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

    }
}
