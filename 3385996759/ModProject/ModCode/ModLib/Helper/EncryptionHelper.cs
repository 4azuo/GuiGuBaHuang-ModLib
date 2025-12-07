using ModLib.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for encryption and decryption operations.
    /// Based on A Ghazal's implementation from StackOverflow.
    /// https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
    /// </summary>
    /*
     * https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
     * A Ghazal
     */
    [ActionCat("Encrypt")]
    public static class EncryptionHelper
    {
        private const string KEY = "f8545t";

        /// <summary>
        /// Encrypts a string using AES encryption.
        /// </summary>
        /// <param name="clearText">Plain text to encrypt</param>
        /// <returns>Base64-encoded encrypted string</returns>
        public static string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        /// <summary>
        /// Decrypts an AES-encrypted string.
        /// </summary>
        /// <param name="cipherText">Base64-encoded encrypted text</param>
        /// <returns>Decrypted plain text</returns>
        public static string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(KEY, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        /// <summary>
        /// Computes SHA256 hash of a string and returns as integer.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Hash as 32-bit integer</returns>
        public static int GetSha256HashAsInt(this string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                int hash = BitConverter.ToInt32(hashBytes, 0);
                return hash;
            }
        }

        /// <summary>
        /// Converts string to BigInteger by encoding each byte as 3-digit number.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>BigInteger representation</returns>
        public static BigInteger GetByteCodeAsInt(this string input)
        {
            return BigInteger.Parse(string.Join(string.Empty, Encoding.ASCII.GetBytes(input).Select(x => x.ToString("000"))));
        }
    }
}