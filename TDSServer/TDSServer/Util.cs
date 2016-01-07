using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;

namespace TDSServer
{
    public class Util
    {
        internal const string ENCRYPT_DECRYPT_KEY = "19541954";
        private static byte[] Encrypt1(byte[] clearData, byte[] Key, byte[] IV)
        {
            // Create a MemoryStream to accept the encrypted bytes 
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();

            return encryptedData;
        }

        internal static string Encrypt(string EncryptText, string key)
        {

            byte[] clearBytes =
              System.Text.Encoding.Unicode.GetBytes(EncryptText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(key,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 
                            0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            byte[] encryptedData = Encrypt1(clearBytes,
                     pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);

        }
        private static byte[] Decrypt1(byte[] cipherData,
                                   byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();

            return decryptedData;
        }

        internal static string Decrypt(string CipherText, string key)
        {

            byte[] cipherBytes = Convert.FromBase64String(CipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(key,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 
                            0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
            byte[] decryptedData = Decrypt1(cipherBytes,
                pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }
        public static string CretaeGuid()
        {
            Guid g = Guid.NewGuid();
            string guid = g.ToString();

            return guid.Replace("-", "");
        }

        public static string ConvertDateToString14(DateTime curDateTime)
        {
            string retval = curDateTime.ToString("yyyyMMddHHmmss");

            return retval;
        }
        public static DateTime ConvertString2Date(string strdate)
        {
            DateTime retDate;

            int Year;
            int Month;
            int Day;
            int Hour;
            int Minute;
            int Second;

            try
            {



                Year = int.Parse(strdate.Substring(0, 4));
                Month = int.Parse(strdate.Substring(4, 2));
                Day = int.Parse(strdate.Substring(6, 2));
                Hour = int.Parse(strdate.Substring(8, 2));
                Minute = int.Parse(strdate.Substring(10, 2));
                Second = int.Parse(strdate.Substring(12, 2));

                retDate = new DateTime(Year, Month, Day, Hour, Minute, Second);
                return retDate;
            }
            catch (Exception ex)
            {
                throw ex;
                // retDate = DateTime.MinValue;
            }


        }
    }
}
