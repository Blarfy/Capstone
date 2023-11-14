using System.Security.Cryptography;

namespace CryptKeyAPI.Controllers
{

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

        private string publicKey = "<RSAKeyValue><Modulus>yg1PXo31Xnisnx6vWLzuNTzE9Zvm7UEQk1v11thoj2HMnPT/06K7LNS0z21dBZpJokkf35EJaIMoGNnyPM3aWbu0+et4Qx9gENIebHpb+JnjOGzJtLbgpb3V473DOdIvFQ2En+p20nsI2By/UjOZpB9NjSirKJWBGkwzw79notE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public byte[] EncryptStringAsym(string plaintextData)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(publicKey);
                byte[] encryptedData = rsa.Encrypt(System.Text.Encoding.UTF8.GetBytes(plaintextData), false);
                return encryptedData;
            }
        }

        public byte[] EncryptBytesAsym(byte[] byteData)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(publicKey);
                byte[] encryptedData = rsa.Encrypt(byteData, false);
                return encryptedData;
            }
        }
    }
}