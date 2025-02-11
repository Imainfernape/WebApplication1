using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Helper
{
    public static class EncryptionHelper
    {
        private static readonly string SecretKey = "potatopotatopotatopotatopotatoes";

        public static (string EncryptedData, string AESKey) EncryptWithKey(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return (plainText, string.Empty);

            using (var aes = Aes.Create())
            {
                aes.GenerateIV();
                byte[] iv = aes.IV;

                aes.Key = Encoding.UTF8.GetBytes(SecretKey.PadRight(32).Substring(0, 32));

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    string encryptedData = Convert.ToBase64String(ms.ToArray());
                    string aesKey = Convert.ToBase64String(aes.Key); 

                    return (encryptedData, aesKey);
                }
            }
        }
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return "Error: CipherText is empty";

            try
            {
                byte[] fullCipher = Convert.FromBase64String(cipherText);

                using (var aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(SecretKey.PadRight(32).Substring(0, 32)); // Ensure 32-byte key

                    byte[] iv = new byte[16];

                    // Extract IV from the first 16 bytes
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    // Extract encrypted data (excluding IV)
                    byte[] encryptedData = new byte[fullCipher.Length - iv.Length];
                    Array.Copy(fullCipher, iv.Length, encryptedData, 0, encryptedData.Length);

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(encryptedData))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error decrypting NRIC: " + ex.Message;
            }
        }
    }
}
