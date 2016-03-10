using System;
using System.Configuration;
using System.IO;
using System.Windows; //.Forms.Design;
using System.Windows.Forms;
using Microsoft.Win32;

// -----------------------------------------------------------------------
//			VOTE MANAGER - TSTConfig Class
//  Classe di gestione della configurazione del sistema
// -----------------------------------------------------------------------
//		AUTH	: M.Binello
//		VER		: 1.0 
//		DATE	: Feb 2006
// -----------------------------------------------------------------------
//	History
// -----------------------------------------------------------------------


namespace VotoSegreto
{

	// --------------------------------------------------------------
	//  CLASSE
	// --------------------------------------------------------------
	/// <summary>
	/// Summary description for TSTConfig.
	/// </summary>
	public class TVSConfig
	{

		private string DriveM = @"M:\";
		private string DriveN = @"N:\";
		private string fversione;
		private Boolean fusefile;
		private ConfigDbData DBConfig;

		// costruttore
		public TVSConfig()
		{
			//
			// TODO: Add constructor logic here
			//
			fversione = "0.9B - 02/2006";
			fusefile = true;
			// setto i parametri di default di DBConfig
			DBConfig.DB_ConfigOK = false;
			DBConfig.DB_Type = "ODBC";
			DBConfig.DB_Dsn = "GEAS";
			DBConfig.DB_Name = "GEAS_TEST";
			DBConfig.DB_Uid = "geas";
			DBConfig.DB_Pwd = "geas";
			DBConfig.DB_Server = @"TOGTA-SRVSQL2k\SQL2kGTA";

		}

		// --------------------------------------------------------------
		//  METODI
		// --------------------------------------------------------------

		// carica la configurazione 
		public Boolean CaricaConfig()
		{
			string ss;

			// controllo se esistono i drive
			if (Directory.Exists(DriveM) && Directory.Exists(DriveN))
			{
				// ok, posso fare le cose che voglio, devo cercare GEAS.sql e caricare le stringhe
				// che mi diranno la connessione al database
				if (File.Exists(DriveM + "geas.sql"))
				{
					// leggo cosa c'è dentro
					try 
					{	
						StreamReader file1;
						file1 = File.OpenText(DriveM + "geas.sql");
						ss = file1.ReadLine();
						// testo se il file è giusto
						if (ss == "GEAS 2000 -- Stringa Connesione a SQL") 
						{
							// tutto ok leggo
							DBConfig.DB_Type = file1.ReadLine();
							DBConfig.DB_Dsn = file1.ReadLine();
							DBConfig.DB_Name = file1.ReadLine();
							DBConfig.DB_Uid = file1.ReadLine();
							DBConfig.DB_Pwd = file1.ReadLine();
							DBConfig.DB_Server = file1.ReadLine();
							DBConfig.DB_ConfigOK = true;
							file1.Close();
							return true;
						}
						else
							return false;
					}     
					catch (Exception e) 
					{
						MessageBox.Show("The file could not be read:\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}		
				}
				else
					return false;
			}
			else
			{
				return false;
			}
		}

		// salva la configurazione del database nel registro
		public void SalvaConfigNelRegistro()
		{
			RegistryKey rk, rkl, rkl2, rklOk;

			rk = Registry.CurrentUser;
			rkl = rk.OpenSubKey("Software", true);
            rkl.OpenSubKey("ServizioTitoli\\VoteManager", true);
			//			if (rkl.OpenSubKey("ServizioTitoli\\VoteManager", true) == null) 
			//			{
				rkl2 = rkl.CreateSubKey("ServizioTitoli\\VoteManager");
				// = rkl.OpenSubKey("ServizioTitoli\\VoteManager", true);
				// ok ho aperto la chiave
				rklOk = rkl2.CreateSubKey("Database");
				// posso scrivere i valori
				rklOk.SetValue("DB_Type", DBConfig.DB_Type);
				rklOk.SetValue("DB_Dsn", DBConfig.DB_Dsn);
				rklOk.SetValue("DB_Uid", DBConfig.DB_Uid);
				rklOk.SetValue("DB_Name", DBConfig.DB_Name);
				rklOk.SetValue("DB_Pwd", DBConfig.DB_Pwd);
				rklOk.SetValue("DB_Server", DBConfig.DB_Server);
				rklOk.Close();
			//			}
			rk.Close();
			
		}
		// --------------------------------------------------------------
		//  PROPRIETA'
		// --------------------------------------------------------------

		// Configurazioen
		public string DB_Type  { get { return DBConfig.DB_Type; } }
		public string DB_Dsn  { get { return DBConfig.DB_Dsn; } }
		public string DB_Uid  { get { return DBConfig.DB_Uid; } }
		public string DB_Name  { get { return DBConfig.DB_Name; } }
		public string DB_Pwd  { get { return DBConfig.DB_Pwd; } }
		public string DB_Server  { get { return DBConfig.DB_Server; } }
		public ConfigDbData DB_Config { get { return DBConfig; } }

		// proprietà File
		public Boolean UseFile
		{
			get { return fusefile; }
			set { fusefile = value; }
		}
        // proprietà versione
		public string Versione 
		{
			get { return fversione; }
			set { fversione = value; }
		}

	}
}
