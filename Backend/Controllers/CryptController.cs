namespace Backend.Controllers
{
    using System.Security.Cryptography;
    public class CryptController 
    {
        /// <summary>
        /// Encrypts a string using AES_256
        /// </summary>
        /// <param name="plainText">String to be encrypted</param>
        /// <param name="key">Lockbox Key</param>
        /// <param name="iv">Initialization Vector</param>
        /// <returns></returns>
        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypts an AES-256 encrypted byte array into a string
        /// </summary>
        /// <param name="cipherText">Encrypted password byte array</param>
        /// <param name="key">Lockbox Key</param>
        /// <param name="iv">Initialization Vector</param>
        /// <returns></returns>
        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                return null;

            string plaintext = "";

            using (Aes aesAlg = Aes.Create()) 
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}