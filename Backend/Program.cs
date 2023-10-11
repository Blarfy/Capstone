namespace Backend
{
    using System.IO;
    using System;
    using System.Security.Cryptography;
    using BCrypt.Net;
    using Backend.Models;

    internal class Program
    {
        public static void Main(string[] args)
        {            
            // Non-static version
            var program = new Program();
            var pass = BCrypt.HashPassword("password");
            program.CreateAccount("Gweppy", pass);

            var loginfo = program.Login("Gweppy", "password");
            byte[] key = loginfo.Item1;
            var id = loginfo.Item2;
            program.AddPassword(key, id, "www.google.com", "username", "awagga@beemail", "myPassword");
            var passwords = program.DecryptPasswords(key, program.GetPasswords(id));
            foreach (KeyValuePair<string, List<string>> entry in passwords)
            {
                Console.WriteLine(entry.Key);
                foreach (string value in entry.Value)
                {
                    Console.WriteLine(value);
                }
            }
        }

        // TODO: Other functions utilize "email" and "password" while this does not. Uniformize all DB Access methods when moving over.
        // <param name="connString">Connection string to postgres database</param>
        // <param name="userName">Username of new account</param>
        // <param name="masterPass">Master password of new account</param>
        private void CreateAccount(string userName, string masterPass) {
            // Send data to DB Access layer

        }

        // Encrypt and add password to DB
        // <param name="key">Lockbox key</param>
        // <param name="ownerID">User ID of owner</param>
        // <param name="location">Website or location of password</param>
        // <param name="userName">Username of password</param>
        // <param name="email">Email of password</param>
        // <param name="password">Password</param>
        private void AddPassword(byte[] key, int ownerID, string location, string userName, string email, string password) 
        {
            // TODO: Ensure that password is not already in DB

            // Encrypt location, username, email, and password with key
            using (Aes myAes = Aes.Create())
            {
                // Generate IV
                myAes.GenerateIV();

                // Encrypt information
                byte[] encryptedLocation = EncryptStringToBytes_Aes(location, key, myAes.IV);
                byte[] encryptedUsername = EncryptStringToBytes_Aes(userName, key, myAes.IV);
                byte[] encryptedEmail = EncryptStringToBytes_Aes(email, key, myAes.IV);
                byte[] encryptedPassword = EncryptStringToBytes_Aes(password, key, myAes.IV);

                // Concatenate IV and encrypted password
                byte[] encryptedIVPass = new byte[4 + myAes.IV.Length + encryptedPassword.Length];
                BitConverter.GetBytes(myAes.IV.Length).CopyTo(encryptedIVPass, 0); // Add IV length to beginning of array to allow for IV extraction later
                myAes.IV.CopyTo(encryptedIVPass, 4);
                encryptedPassword.CopyTo(encryptedIVPass, myAes.IV.Length + 4);

                // Turn into EncryptedPassword object
                var encPass = new EncryptedPassword
                {
                    encryptedLocation = encryptedLocation,
                    encryptedUsername = encryptedUsername,
                    encryptedEmail = encryptedEmail,
                    encryptedIVPass = encryptedIVPass
                };
                // Send to DB Access layer at localhost:7124
                
            }
        }

        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
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

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
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

        // Login function returns tuple of lockbox key and user ID
        // Move to DB Access later
        // <param name="connString">Connection string to postgres database</param>
        // <param name="email">Email of user</param>
        // <param name="password">Password of user</param>
        private Tuple<byte[], int> Login(string email, string password)
        {
            // Send data to DB Access layer
            return null;
        }

        // Retrieve all encrypted passwords from DB
        // Passwords are made of a username, email, password, and website/location
        // Move to DB Access later
        // <param name="connString">Connection string to postgres database</param>
        // <param name="owner_id">User ID of owner</param>
        private Dictionary<byte[], List<byte[]>> GetPasswords(int owner_id)
        {
            // Get data from DB Access layer
            return null;
        }

        // Decrypt all passwords using lockbox key
        // TODO: Keep in memory (cache) for duration of session
        // <param name="key">Lockbox key</param>
        // <param name="passwords">Dictionary of encrypted passwords</param>
        public Dictionary<string, List<string>> DecryptPasswords(byte[] key, Dictionary<byte[], List<byte[]>> passwords)
        {
            var decryptedPasswords = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<byte[], List<byte[]>> entry in passwords)
            {
                List<string> passData = new List<string> {};
                // Extract IV length from first 4 bytes of encryptedIVPass
                byte[] encryptedIVPass = entry.Value[2];
                int ivLength = BitConverter.ToInt32(encryptedIVPass, 0);
                byte[] iv = new byte[ivLength];
                Array.Copy(encryptedIVPass, 4, iv, 0, ivLength);

                // Extract encrypted password
                byte[] encryptedPassword = new byte[16];
                Array.Copy(encryptedIVPass, ivLength + 4, encryptedPassword, 0, encryptedPassword.Length);

                // Decrypt all data
                passData.Add(DecryptStringFromBytes_Aes(entry.Value[0], key, iv));
                passData.Add(DecryptStringFromBytes_Aes(entry.Value[1], key, iv));
                passData.Add(DecryptStringFromBytes_Aes(encryptedPassword, key, iv));

                // Decrypt location and add to dictionary
                decryptedPasswords.Add(DecryptStringFromBytes_Aes(entry.Key, key, iv), passData);
            }
            return decryptedPasswords;
        }

        // TODO: Method for storing lockbox key and passwords in cache  
    }
}