using BCrypt.Net;

namespace Backend
{
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using System.IO;
    using System;
    using System.Security.Cryptography;
    using BCrypt.Net;

    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Add configuration file
            string appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "appsettings.json");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true)
                .Build();

            // Get environment variables and construct connection string
            string DBhost = configuration.GetConnectionString("DefaultConnection") ?? "localhost";
            string DBuser = configuration.GetConnectionString("DefaultUser") ?? "postgres";
            string DBpass = configuration.GetConnectionString("DefaultPassword") ?? "postgres";
            string DBname = configuration.GetConnectionString("DefaultDatabase") ?? "postgres";
            var connString = $"Host={DBhost};Username={DBuser};Password={DBpass};Database={DBname}";

            // Connect to postgres
            // await using var dataSource = NpgsqlDataSource.Create(connString);
            // await using var command = dataSource.CreateCommand("SELECT user_name FROM users");
            // await using var reader = await command.ExecuteReaderAsync();

            // while (await reader.ReadAsync())
            // {   
            //     Console.WriteLine(reader.GetString(0));
            // }
            
            // Non-static version
            // var program = new Program();
            // var key = program.Login(connString, "awagga", "gweeble").Result;

            //await CreateAccount(connString, "Gweppy", "password");
            var loginfo = Login(connString, "Gweppy", "password").Result;
            byte[] key = loginfo.Item1;
            var id = loginfo.Item2;
            // await AddPassword(connString, key, id, "www.google.com", "username", "awagga@beemail", "myPassword");
            var passwords = DecryptPasswords(key, GetPasswords(connString, id).Result);
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
        // Move to DB Access later
        // <param name="connString">Connection string to postgres database</param>
        // <param name="userName">Username of new account</param>
        // <param name="masterPass">Master password of new account</param>
        public static async Task CreateAccount(string connString, string userName, string masterPass) {
            // TODO: Ensure that user does not already exist in DB
            // Generate user key
            var hashPass = BCrypt.HashPassword(masterPass);

            // Generate random key
            // TODO: Establish proper key generation method using masterPass in DB Access
            var userKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(userKey);
            }

            // Add user to DB
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("INSERT INTO users (user_name, user_pass, user_key) VALUES (@user_name, @user_pass, @user_key)");
            command.Parameters.AddWithValue("user_name", userName);
            command.Parameters.AddWithValue("user_pass", hashPass);
            command.Parameters.AddWithValue("user_key", userKey);
            await command.ExecuteNonQueryAsync();
        }

        // Encrypt and add password to DB
        // Move to DB Access later
        // <param name="connString">Connection string to postgres database</param>
        // <param name="key">Lockbox key</param>
        // <param name="ownerID">User ID of owner</param>
        // <param name="location">Website or location of password</param>
        // <param name="userName">Username of password</param>
        // <param name="email">Email of password</param>
        // <param name="password">Password</param>
        public static async Task AddPassword(string connString, byte[] key, int ownerID, string location, string userName, string email, string password) 
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
            
                // Send to DB
                await using var dataSource = NpgsqlDataSource.Create(connString);
                await using var command = dataSource.CreateCommand("INSERT INTO passwords (owner_id, login_location, login_user, login_email, login_password) VALUES (@owner_id, @login_location, @login_user, @login_email, @login_password)");
                command.Parameters.AddWithValue("owner_id", ownerID);
                command.Parameters.AddWithValue("login_location", encryptedLocation);
                command.Parameters.AddWithValue("login_user", encryptedUsername);
                command.Parameters.AddWithValue("login_email", encryptedEmail);
                command.Parameters.AddWithValue("login_password", encryptedIVPass);
                await command.ExecuteNonQueryAsync();
            }
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
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

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
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
        public static async Task<Tuple<byte[], int>> Login(string connection, string email, string password)
        {
            await using var dataSource = NpgsqlDataSource.Create(connection);
            await using var command = dataSource.CreateCommand("SELECT user_pass, user_key, user_id FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", email);
            await using var reader = await command.ExecuteReaderAsync();
            byte[] key = new byte[32];

            if (await reader.ReadAsync() && BCrypt.Verify(password, reader.GetString(0)))
            {
                key = reader.GetFieldValue<byte[]>(1);
                return new Tuple<byte[], int>(key, reader.GetInt32(2));
            } 
            else return new Tuple<byte[], int>(key, -1);
        }

        // Retrieve all encrypted passwords from DB
        // Passwords are made of a username, email, password, and website/location
        // Move to DB Access later
        // <param name="connString">Connection string to postgres database</param>
        // <param name="owner_id">User ID of owner</param>
        public static async Task<Dictionary<byte[], List<byte[]>>> GetPasswords(string connection, int owner_id)
        {
            await using var dataSource = NpgsqlDataSource.Create(connection);
            await using var command = dataSource.CreateCommand("SELECT login_location, login_user, login_email, login_password FROM passwords WHERE owner_id = @owner_id");
            command.Parameters.AddWithValue("owner_id", owner_id);
            await using var reader = await command.ExecuteReaderAsync();

            var passwords = new Dictionary<byte[], List<byte[]>>();

            while (await reader.ReadAsync())
            {
                List<byte[]> passData = new List<byte[]>
                {
                    reader.GetFieldValue<byte[]>(1),
                    reader.GetFieldValue<byte[]>(2),
                    reader.GetFieldValue<byte[]>(3)
                };
                passwords.Add(reader.GetFieldValue<byte[]>(0), passData); 
            }

            return passwords;
        }

        // // Retrieve user_id from DB
        // // Move to DB Access later
        public static async Task<int> GetUserID(string connection, string email)
        {
            await using var dataSource = NpgsqlDataSource.Create(connection);
            await using var command = dataSource.CreateCommand("SELECT user_id FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", email);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            } 
            else return -1;
        }


        // Decrypt all passwords using lockbox key
        // TODO: Keep in memory (cache) for duration of session
        // <param name="key">Lockbox key</param>
        // <param name="passwords">Dictionary of encrypted passwords</param>
        public static Dictionary<string, List<string>> DecryptPasswords(byte[] key, Dictionary<byte[], List<byte[]>> passwords)
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