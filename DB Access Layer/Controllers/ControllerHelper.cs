using BCrypt.Net;
using Npgsql;

namespace DB_Access_Layer.Controllers
{
    using BCrypt.Net;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;

    public class ControllerHelper
    {
        public static IConfigurationRoot getConfig()
        {
            string appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "appsettings.json");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true)
                .Build();
            return configuration;
        }
        public static string getConnString()
        {
            // Add configuration file
            var configuration = getConfig();

            // Get environment variables and construct connection string
            string DBhost = configuration.GetConnectionString("DefaultConnection") ?? "localhost";
            string DBuser = configuration.GetConnectionString("DefaultUser") ?? "postgres";
            string DBpass = configuration.GetConnectionString("DefaultPassword") ?? "postgres";
            string DBname = configuration.GetConnectionString("DefaultDatabase") ?? "postgres";
            var connString = $"Host={DBhost};Username={DBuser};Password={DBpass};Database={DBname}";
            return connString;
        }

        /// <summary>
        /// Get user ID from DB
        /// </summary>
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <returns></returns>
        /// <remarks>
        /// Returns -1 if user does not exist or password is incorrect
        /// </remarks>
        public async Task<int> GetUserID(string email, string password) // Merge with Login function?
        {
            await using var dataSource = NpgsqlDataSource.Create(getConnString());
            await using var command = dataSource.CreateCommand("SELECT user_id, user_pass FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", email);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() && BCrypt.Verify(password, reader.GetString(1)))
            {
                return reader.GetInt32(0);
            }
            else return -1;
        }
        
        // Used for getting user ID from just a username
        public async Task<int> GetUserID(string username)
        {
            await using var dataSource = NpgsqlDataSource.Create(getConnString());
            await using var command = dataSource.CreateCommand("SELECT user_id FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", username);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            else return -1;
        }

        public string DecryptStringAsymmetrically(string encryptedData)
        {
            var configuration = getConfig();
            string privateKey = configuration.GetValue<string>("PrivateKey");

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(privateKey);

                    // Decrypt data
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                    byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, false);

                    // Convert decrypted data back to string
                    string decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public string DecryptBytesAsymmetrically(byte[] encryptedData)
        {
            var configuration = getConfig();
            string privateKey = configuration.GetValue<string>("PrivateKey");

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(privateKey);

                    // Decrypt data
                    byte[] decryptedBytes = rsa.Decrypt(encryptedData, false);

                    // Convert decrypted data back to string
                    string decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
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

        public byte[] GenerateIV ()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateIV();
                return aesAlg.IV;
            }
        }
    }
}
