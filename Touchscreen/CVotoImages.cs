using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace VotoTouch
{
    public class CVotoImages
    {

        private const string IMG_EXT = ".png";

        public string Img_path;

        public CVotoImages()		
        {
            // costruttore
        }

        // -----------------------------------------------------------------
        // carico delle immagini nella finestra
        // -----------------------------------------------------------------

        public void LoadImages(string AImage) //ref System.Windows.Forms.Form fForm,
        {
            // cancello l'immagine prima perché sennò aumenta la memoria a palla;
            if (mainForm.BackgroundImage != null)
                mainForm.BackgroundImage.Dispose();


            // prima la cerco nella cartella data
            if (System.IO.File.Exists(Img_path + AImage + IMG_EXT))
            {
                mainForm.BackgroundImage = Image.FromFile(Img_path + AImage + IMG_EXT);
                return;
            }
        }

        // -----------------------------------------------------------------
        // controllo della cartella data 
        // -----------------------------------------------------------------

        public bool CheckDataFolder(ref string AData_Path)
        {
            // ok, per prima cosa verifico se c'è la cartella c:\data, se si ok
            // sennò devo considerare la cartella dell'applicazione, se non c'è esco
            string SourceExePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            // se ci solo nel immagini in c:\data\VtsNETImg
            if (System.IO.Directory.Exists("c:" + VSDecl.DATA_PATH_ABS) &&      // "\\Data\\";
                System.IO.Directory.Exists("c:" + VSDecl.IMG_PATH_ABS))         // "\\Data\\VtsNETImg\\";
            {
                // allora i path sono quelli assoluti  c:\data\VtsNETImg
                AData_Path = "c:" + VSDecl.DATA_PATH_ABS;
                Img_path = "c:" + VSDecl.IMG_PATH_ABS;
                // qua però dovrebbe controllare se le immagini sono cambiate
                CheckImageFolder();
            }
            else
            {
                // controllo se esistono le cartelle locali nella cartella applicazione cioè la 
                // cartella \\VtsNETImgLocali\\ nel caso il VotoSegreto es. fosse sotto c:\Programmi
                if (System.IO.Directory.Exists(SourceExePath + VSDecl.IMG_PATH_LOC))  // "\\VtsNETImgLocali\\";
                {
                    // metto i corretti path
                    AData_Path = SourceExePath + "\\";
                    Img_path = SourceExePath + VSDecl.IMG_PATH_LOC;
                }
                else
                {
                    // Non ho trovato nessuna cartella, quindi mi creo il ramo c:\\Data\\VtsNETImg\\
                    // e tento di copiare le immagini dalla cartella Source, es. h:\\Data\\VtsNETImg\\
                    // in caso di esecuzione da server
                    //splash.SetSplash(11, "Copio immagini...");
                    if (CopyImages())
                    {
                        AData_Path = "c:" + VSDecl.DATA_PATH_ABS;       // "\\Data\\";
                        Img_path = "c:" + VSDecl.IMG_PATH_ABS;          // "\\Data\\VtsNETImg\\";
                    }
                    else
                    {
                        MessageBox.Show("Impossibile trovare le immagini", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //CtrlPrimoAvvio = false;
                        return false;
                    }
                }
            }
            return true;
        }

        // -----------------------------------------------------------------
        // copia delle immagini nella cartella Data
        // -----------------------------------------------------------------

        public bool CopyImages()
        {
            // DR11 OK
            string SourcePath, dstName;

            try
            {
                // mi trovo il path dell'eseguibile
                SourcePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                // e poi aggiungo il path immagini \\Data\\VtsNETImg\\ da dove copierò le immagini
                SourcePath = SourcePath + VSDecl.SOURCE_IMG_PATH;

                // allora, devo testare se c'è la cartella data in locale, se non c'è devo crearla
                // e copiare le immagini
                if (!System.IO.Directory.Exists("c:" + VSDecl.IMG_PATH_ABS) && System.IO.Directory.Exists(SourcePath))
                {
                    // creao la cartella in locale, siccome sono in loading mi setto la splash screen
                    //if (splash != null) splash.SetSplash(12, "Creazione cartella immagini...");
                    // creo la sotto cartella
                    System.IO.Directory.CreateDirectory("c:" + VSDecl.IMG_PATH_ABS);
                }
                // copia immagini
                //if (splash != null) splash.SetSplash(12, "Copia immagini...");

                // Process the list of files found in the directory.
                string[] fileEntries = System.IO.Directory.GetFiles(SourcePath);
                foreach (string fileName in fileEntries)
                {
                    dstName = "c:" + VSDecl.IMG_PATH_ABS + System.IO.Path.GetFileName(fileName);
                    //if (splash != null) splash.SetSplash(12, "Copio " + dstName + "...");
                    System.IO.File.Copy(fileName, dstName, true);
                }
                // ok, tutto a posto
                if (fileEntries.Length == 0)
                    return false;
                else
                    return true;
                //}
                //else
                //    return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }


        // -----------------------------------------------------------------
        // controllo e copia delle immagini nella cartella Data, controlla
        // le dimensioni e la data del file
        // -----------------------------------------------------------------

        public bool CheckImageFolder()
        {
            string SourcePath, dstName;
            FileInfo src_f, dst_f;
            DateTime s_time, d_time;

            try
            {
                // mi trovo il path dell'eseguibile
                SourcePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                // e poi aggiungo il path immagini \\Data\\VtsNETImg\\ da dove copierò le immagini
                SourcePath = SourcePath + VSDecl.SOURCE_IMG_PATH;

                // se le cartelle hanno lo stesso nome, vuol dire che sono in locale
                // quindi è inutile comparare
                if (SourcePath == Img_path) return false;
                if (!System.IO.Directory.Exists(SourcePath)) return false;

                // ora devo controllare tutti i files sotto Img_path
                // Process the list of files found in the directory.
                string[] fileEntries = System.IO.Directory.GetFiles(SourcePath);
                foreach (string fileName in fileEntries)
                {
                    // verifico se c'è nel corrispondente path
                    dstName = "c:" + VSDecl.IMG_PATH_ABS + System.IO.Path.GetFileName(fileName);
                    if (System.IO.File.Exists(dstName))
                    {
                        // devo testare se sono uguale
                        src_f = new FileInfo(fileName);
                        dst_f = new FileInfo(dstName);
                        //// testo se sono diversi in dimensione
                        //if (src_f.Length != dst_f.Length)
                        //    System.IO.File.Copy(fileName, dstName, true);
                        // testo se sono diversi come data di modifica
                        s_time = src_f.LastWriteTime;
                        d_time = dst_f.LastWriteTime;
                        if (d_time < s_time)
                            System.IO.File.Copy(fileName, dstName, true);

                    }
                    else
                        // copio il file
                        System.IO.File.Copy(fileName, dstName, true);

                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
        }


        // -----------------------------------------------------------------
        //  Proprietà
        // -----------------------------------------------------------------
        
        private Form mainForm;
        public Form MainForm
        {
            get { return mainForm; }
            set { mainForm = value; }
        }

    }
}
