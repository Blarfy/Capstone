using CryptKeyAPI.Models;

namespace CryptKeyAPI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Cryptography;
    using Newtonsoft.Json;
    using System.Web;

    [ApiController]
    [Route("[controller]")]
    public class PassController 
    {
        public PassController()
        {
        }

        [HttpGet("TestConnection")]
        public string TestConnection()
        {
            return "Hello World!";
        }

        /// Encrypt and add password to DB
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <param name="key">Lockbox key</param>
        /// <param name="passLocation">Website or location of password</param>
        /// <param name="passUserName">Username of password</param>
        /// <param name="passEmail">Email of password</param>
        /// <param name="passPassword">Password Password</param>
        [HttpPost(Name = "AddPassword")]
        public async Task<string> AddPassword(string email, string password, byte[] key, string passLocation, string passUserName, string passEmail, string passPassword)
        {
            // TODO: Ensure that password is not already in DB

            // Encrypt location, username, email, and password with key
            using (Aes myAes = Aes.Create())
            {
                // Generate IV
                myAes.GenerateIV();

                // Encrypt information
                CryptController sepulchre = new CryptController();
                byte[] encryptedLocation = sepulchre.EncryptStringToBytes_Aes(passLocation, key, myAes.IV);
                byte[] encryptedUsername = sepulchre.EncryptStringToBytes_Aes(passUserName, key, myAes.IV);
                byte[] encryptedEmail = sepulchre.EncryptStringToBytes_Aes(passEmail, key, myAes.IV);
                byte[] encryptedPassword = sepulchre.EncryptStringToBytes_Aes(passPassword, key, myAes.IV);

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

        /// <summary>
        /// Retrieve all encrypted passwords from DB
        /// </summary>
        /// <param name="email">E-Mail of user</param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetPasswords")]
        public async Task<List<EncryptedPassword>> GetPasswords([FromQuery]string email, [FromQuery]string password)
        {
            using (var httpClient = new HttpClient())
            {
                List<EncryptedPassword> encryptedPasswords = new List<EncryptedPassword>();
                var response = await httpClient.GetAsync($"https://localhost:7124/api/GetPasswords?email={email}&password={password}");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    encryptedPasswords = JsonConvert.DeserializeObject<List<EncryptedPassword>>(json);
                }
                return encryptedPasswords;
                
            }
        }

        /// <summary>
        /// Decrypt all passwords using lockbox key
        /// </summary>
        /// TODO: Keep in memory (cache) for duration of session
        /// <param name="key">Lockbox key</param>
        /// <param name="passwords">List of encrypted passwords</param>
        /// <returns></returns>
        [HttpPost("DecryptPasswords")]
        public List<DecryptedPassword> DecryptPasswords([FromQuery]string key, [FromBody]List<EncryptedPassword> passwords)
        {
            // convert key string back into byte array
            byte[] keyBytes = new byte[32];
            keyBytes = Convert.FromBase64String(key);

            var decryptedPasswords = new List<DecryptedPassword>();
            // example key input: %20/5atYQXMIZPBD3jTpNVjpMfKMmupzh+/BkZ/XfwRrvs=
            // need to convert to byte array
            //var sepPasswords = JsonConvert.DeserializeObject<List<EncryptedPassword>>(passwords);
            foreach (EncryptedPassword entry in passwords)
            {
                // Deserialize JSON string to EncryptedPassword object
                var encPass = entry;

                // Extract IV from encryptedIVPass
                byte[] iv = new byte[BitConverter.ToInt32(encPass.encryptedIVPass, 0)];
                Array.Copy(encPass.encryptedIVPass, 4, iv, 0, iv.Length);
                byte[] encryptedPassword = new byte[encPass.encryptedIVPass.Length - 4 - iv.Length];
                Array.Copy(encPass.encryptedIVPass, iv.Length + 4, encryptedPassword, 0, encryptedPassword.Length);

                // Convert EncryptedPassword object to DecryptedPassword object
                CryptController sepulchre = new CryptController();
                string plaintextLocation = sepulchre.DecryptStringFromBytes_Aes(encPass.encryptedLocation, keyBytes, iv);
                string? plaintextUsername = sepulchre.DecryptStringFromBytes_Aes(encPass.encryptedUsername, keyBytes, iv);
                string? plaintextEmail = sepulchre.DecryptStringFromBytes_Aes(encPass.encryptedEmail, keyBytes, iv);
                string plaintextIVPass = sepulchre.DecryptStringFromBytes_Aes(encryptedPassword, keyBytes, iv);
                var decPass = new DecryptedPassword(plaintextLocation, plaintextUsername, plaintextEmail, plaintextIVPass);
                decryptedPasswords.Add(decPass);
            }

            return decryptedPasswords;
        }

    }
}