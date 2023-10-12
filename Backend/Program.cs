namespace Backend
{
    using System.IO;
    using System;
    using System.Security.Cryptography;
    using BCrypt.Net;
    using Backend.Models;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal class Program
    {
        public static async Task Main(string[] args)
        {            
            // Use program for non-static methods
            var program = new Program();
            string pass = BCrypt.HashPassword("password");
            // Console.WriteLine(await program.CreateAccount("Gweppy", pass)); // WORKS SUCCESSFULLY

            byte[] key = program.Login("Gweppys", "password").Result; // WORKS SUCCESSFULLY
            Console.WriteLine(Convert.ToBase64String(key));
            //string addPassVerify = program.AddPassword("Gweppys", "password", key, "www.google.com", "username", "awagga@beemail", "myPassword").Result; // WORKS SUCCESSFULLY
            var encPasswords = program.GetPasswords("Gweppys", "password").Result; // WORKS SUCCESSFULLY
            foreach (var entry in encPasswords)
            {
                Console.WriteLine(entry);
            }
            var decPasswords = program.DecryptPasswords(key, encPasswords);
            foreach (DecryptedPassword decPass in decPasswords)
            {
                Console.WriteLine($"Location: {decPass.plaintextLocation}");
                Console.WriteLine($"Username: {decPass.plaintextUsername}");
                Console.WriteLine($"Email: {decPass.plaintextEmail}");
                Console.WriteLine($"Password: {decPass.plaintextIVPass}");
            }
        }

        /// Send request to create a new user account
        /// <param name="email">E-Mail of new account</param>
        /// <param name="masterPass">Master password of new account</param>
        private async Task<string> CreateAccount(string email, string masterPass) {
            using (var httpClient = new HttpClient()) 
            {
                var response = httpClient.PostAsync($"https://localhost:7124/api/CreateAccount?email={email}&masterPass={masterPass}", null);
                return await response.Result.Content.ReadAsStringAsync();
            }
        }

        /// Login function returns lockbox key
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <returns></returns>
        private async Task<byte[]> Login(string email, string password)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync($"https://localhost:7124/api/Login?email={email}&password={password}");
                var recieved = await response.Result.Content.ReadAsStringAsync();

                // Extract key from response
                recieved = recieved.Substring(1, recieved.Length - 2); // Remove surrounding quotes from string
                byte[] key = new byte[32];
                if (recieved != "Invalid credentials")
                {
                    key = Convert.FromBase64String(recieved);
                }

                return key;
            }
        }

        /// Encrypt and add password to DB
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <param name="key">Lockbox key</param>
        /// <param name="passLocation">Website or location of password</param>
        /// <param name="passUserName">Username of password</param>
        /// <param name="passEmail">Email of password</param>
        /// <param name="passPassword">Password Password</param>
        private async Task<string> AddPassword(string email, string password, byte[] key, string passLocation, string passUserName, string passEmail, string passPassword) 
        {
            // TODO: Ensure that password is not already in DB

            // Encrypt location, username, email, and password with key
            using (Aes myAes = Aes.Create())
            {
                // Generate IV
                myAes.GenerateIV();

                // Encrypt information
                byte[] encryptedLocation = EncryptStringToBytes_Aes(passLocation, key, myAes.IV);
                byte[] encryptedUsername = EncryptStringToBytes_Aes(passUserName, key, myAes.IV);
                byte[] encryptedEmail = EncryptStringToBytes_Aes(passEmail, key, myAes.IV);
                byte[] encryptedPassword = EncryptStringToBytes_Aes(passPassword, key, myAes.IV);

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

                // Serialize EncryptedPassword object to JSON string
                var json = System.Text.Json.JsonSerializer.Serialize(encPass);

                // Create StringContent object from JSON string
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Send to DB Access layer at localhost:7124
                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.PostAsync($"https://localhost:7124/api/AddPassword?email={email}&password={password}", content);
                    return await response.Result.Content.ReadAsStringAsync();
                }
            }
        }

        // Retrieve all encrypted passwords from DB
        // Passwords are made of a username, email, password, and website/location
        /// <summary>
        /// Retrieve all encrypted passwords from DB
        /// </summary>
        /// <param name="email">E-Mail of user</param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<List<string>> GetPasswords(string email, string password)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync($"https://localhost:7124/api/GetPasswords?email={email}&password={password}");
                var recieved = await response.Result.Content.ReadAsStringAsync();

                // Deserialize JSON string to string list
                var encPasswords = System.Text.Json.JsonSerializer.Deserialize<List<string>>(recieved);
                return encPasswords;
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

        // Decrypt all passwords using lockbox key
        // TODO: Keep in memory (cache) for duration of session
        // <param name="key">Lockbox key</param>
        // <param name="passwords">Dictionary of encrypted passwords</param>
        public List<DecryptedPassword> DecryptPasswords(byte[] key, List<string> passwords)
        {
            var decryptedPasswords = new List<DecryptedPassword>();
            foreach (string entry in passwords)
            {
                // Deserialize JSON string to EncryptedPassword object
                var encPass = System.Text.Json.JsonSerializer.Deserialize<EncryptedPassword>(entry);

                // Extract IV from encryptedIVPass
                byte[] iv = new byte[BitConverter.ToInt32(encPass.encryptedIVPass, 0)];
                Array.Copy(encPass.encryptedIVPass, 4, iv, 0, iv.Length);
                byte[] encryptedPassword = new byte[encPass.encryptedIVPass.Length - 4 - iv.Length];
                Array.Copy(encPass.encryptedIVPass, iv.Length + 4, encryptedPassword, 0, encryptedPassword.Length);

                // Convert EncryptedPassword object to DecryptedPassword object
                string plaintextLocation = DecryptStringFromBytes_Aes(encPass.encryptedLocation, key, iv);
                string? plaintextUsername = DecryptStringFromBytes_Aes(encPass.encryptedUsername, key, iv);
                string? plaintextEmail = DecryptStringFromBytes_Aes(encPass.encryptedEmail, key, iv);
                string plaintextIVPass = DecryptStringFromBytes_Aes(encryptedPassword, key, iv);
                var decPass = new DecryptedPassword(plaintextLocation, plaintextUsername, plaintextEmail, plaintextIVPass);
                decryptedPasswords.Add(decPass);
            }

            return decryptedPasswords;
        }

        // TODO: Method for storing lockbox key and passwords in cache  
    }
}