using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Security.Cryptography;

namespace VotoTouch
{


	public class Logging
	{

        public static string logFilename = "";

		public static string generateDefaultLogFileName(string BaseDirName, string BaseFileName)
		{
			return  BaseDirName + BaseFileName + "_" +
				DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + 
				DateTime.Now.Year + ".log";
		}

        public static string generateInternalLogFileName(string BaseDirName, string BaseFileName)
        {
            logFilename = BaseDirName + BaseFileName + "_" +
                DateTime.Now.Month + "_" + DateTime.Now.Day + "_" +
                DateTime.Now.Year + ".log";
            return logFilename;
        }
 
		/// <summary>
		/// Pass in the fully qualified name of the log file you want to write to
		/// and the message to write
		/// </summary>
		/// <param name="LogPath"></param>
		/// <param name="Message"></param>

		public static void WriteToLog_no(string LogPath, string Message)
		{
#if RELEASE
			try
			{
				using (StreamWriter s = File.AppendText(LogPath))
				{
					s.WriteLine(DateTime.Now + "\t" + Message);
				}

			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
#endif
        }

        public static void WriteToLog(string Message)
        {
#if RELEASE
            try
            {
                using (StreamWriter s = File.AppendText(logFilename))
                {
                    s.WriteLine(DateTime.Now + "\t" + Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
#endif
        }

		/// <summary>
		/// Writes a message to the application event log
		/// /// </summary>
		/// <param name="Source">Source is the source of the message ususally you will want this to be the application name</param>
		/// <param name="Message">message to be written</param>
		/// <param name="EntryType">the entry type to use to categorize the message like for exmaple error or information</param>

//		public static void WriteToEventLog(string Source, string Message, System.Diagnostics.EventLogEntryType EntryType)
//		{
//
//			try
//			{
//
//				if (!EventLog.SourceExists(Source))
//				{
//					EventLog.CreateEventSource(Source, "Application");
//				}
//
//				EventLog.WriteEntry(Source, Message, EntryType);
//
//			}
//			catch(Exception ex)
//			{
//
//				System.Diagnostics.Debug.WriteLine(ex.Message);
//
//			}
//		}
	}

    // --------------------------------------------------------------------------
    //  CLASSE LOGGING VOTE
    //
    //  Salva i voti criptati per successive elaborazioni
    //
    // --------------------------------------------------------------------------

    public class LogVote
    {
        public const string ASharedSecret = "Elenab-2099";

        public LogVote()
        {
            //ASharedSecret = "Elenab-2099";
        }

        public static string GenerateDefaultLogFileName(string BaseDirName, string BaseFileName)
        {
            return BaseDirName + BaseFileName + "_" +
                DateTime.Now.Month + "_" + DateTime.Now.Day + "_" +
                DateTime.Now.Year + ".log";
        }


        /// <summary>
        /// Pass in the fully qualified name of the log file you want to write to
        /// and the message to write
        /// </summary>
        /// <param name="LogPath"></param>
        /// <param name="Message"></param>

        public static void WriteToLog(string LogPath, string Message)
        {

            try
            {
                using (StreamWriter s = File.AppendText(LogPath))
                {

                    s.WriteLine(DateTime.Now + "\t" + Message);

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public static void WriteToLogCrypt(string LogPath, string Message)
        {

            try
            {
                using (StreamWriter s = File.AppendText(LogPath))
                {
                    //string EncryMessage = EncryptStringAES(Message, ASharedSecret);
                    string EncryMessage = ASCIIEnryptString(Message);

                    s.WriteLine(EncryMessage);

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public static void WriteToLogDeCrypt(string LogPath, string Message)
        {

            try
            {
                using (StreamWriter s = File.AppendText(LogPath))
                {
                    //string EncryMessage = EncryptStringAES(Message, ASharedSecret);
                    string EncryMessage = ASCIIDecryptString(Message);

                    s.WriteLine(EncryMessage);
                    //s.WriteLine(DateTime.Now + "\t" + Message);

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        // ---------------------------------------------------------------------
        //   ROUTINE di CRITTAZZIONE
        // ---------------------------------------------------------------------

        // --------------------- ASCII ------------------------------------------

        private static string ASCIIEnryptString(string strEncrypted)
        {
            try
            {
                byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(strEncrypted);
                string encryptedConnectionString = Convert.ToBase64String(b);
                return encryptedConnectionString;
            }
            catch
            {
                return strEncrypted;
            }
        }

        private static string ASCIIDecryptString(string encrString)
        {
            try
            {
                byte[] b = Convert.FromBase64String(encrString);
                string decryptedConnectionString = System.Text.ASCIIEncoding.ASCII.GetString(b);
                return decryptedConnectionString;
            }
            catch
            {
                return encrString;
            }
        }

        // --------------------- XOR ------------------------------------------

        public static int key = 129;

        public static string XOREncryptDecrypt(string textToEncrypt)
        {
            StringBuilder inSb = new StringBuilder(textToEncrypt);
            StringBuilder outSb = new StringBuilder(textToEncrypt.Length);
            char c;
            for (int i = 0; i < textToEncrypt.Length; i++)
            {
                c = inSb[i];
                c = (char)(c ^ key);
                outSb.Append(c);
            }
            return outSb.ToString();
        }

        // --------------------- AES ------------------------------------------

        private static byte[] _salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
                //throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                return plainText;
                //throw new ArgumentNullException("sharedSecret");

            string outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            catch
            {
                outStr = plainText;
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                    aesAlg.Dispose();
                }
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

    
    }



}