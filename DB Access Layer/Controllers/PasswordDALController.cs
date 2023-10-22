using Microsoft.AspNetCore.Mvc;

namespace DB_Access_Layer.Controllers
{
    using Npgsql;
    using BCrypt.Net;
    using System.Security.Cryptography;
    using DB_Access_Layer.Models;

    [Route("api")]
    [ApiController]
    public class PasswordDALController : ControllerBase
    {

        private static string getConnString()
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
            return connString;
        }

        static string connString = getConnString();

        [HttpGet ("TestConnection")]
        public string TestConnection()
        {
            return "Hello World!";
        }

        /// <summary>
        /// Create new account with a random key
        /// </summary>
        /// <param name="email">Email of new account</param>
        /// <param name="masterPass">Master password of new account</param>
        /// <returns></returns>
        [HttpPost("CreateAccount")]
        public async Task<string> CreateAccount([FromQuery] string email, [FromQuery] string masterPass) {
            // TODO: Ensure that user does not already exist in DB

            // Generate random key for user encryption
            var userKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(userKey);
            }

            // Add user to DB
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("INSERT INTO users (user_name, user_pass, user_key) VALUES (@user_name, @user_pass, @user_key)");
            command.Parameters.AddWithValue("user_name", email);
            command.Parameters.AddWithValue("user_pass", masterPass);
            command.Parameters.AddWithValue("user_key", userKey);
            await command.ExecuteNonQueryAsync();
            return "Account created successfully!";
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
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT user_id, user_pass FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", email);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync() && BCrypt.Verify(password, reader.GetString(1)))
            {
                return reader.GetInt32(0);
            } 
            else return -1;
        }

        /// <summary>
        /// Login function returns lockbox key
        /// </summary>
        /// <param name="email">E-Mail of user</param>
        /// <param name="password">Password of user</param>
        /// <returns></returns>
        [HttpGet("Login")]
        public async Task<byte[]> Login([FromQuery] string email, [FromQuery] string password) // Merge with GetUserID function? Check if moving key around is secure
        {
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT user_pass, user_key FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", email);
            await using var reader = await command.ExecuteReaderAsync();
            byte[] key = new byte[32];

            if (await reader.ReadAsync() && BCrypt.Verify(password, reader.GetString(0)))
            {
                key = reader.GetFieldValue<byte[]>(1);
                return key;
            } 
            else return key;
        }        


        /// <summary>
        /// Encrypt and add password to DB
        /// </summary>
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <param name="encPass">Encrypted Password</param>
        /// <returns></returns>
        [HttpPost("AddPassword")]
        public async Task<string> AddPassword([FromQuery] string email, [FromQuery] string password, [FromBody] EncryptedPassword encPass) 
        {
            // TODO: Ensure that password is not already in DB

            int ownerID = GetUserID(email, password).Result;

            // Add password to DB        
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("INSERT INTO passwords (owner_id, login_location, login_user, login_email, login_password) VALUES (@owner_id, @login_location, @login_user, @login_email, @login_password)");
            command.Parameters.AddWithValue("owner_id", ownerID);
            command.Parameters.AddWithValue("login_location", encPass.encryptedLocation);
            command.Parameters.AddWithValue("login_user", encPass.encryptedUsername);
            command.Parameters.AddWithValue("login_email", encPass.encryptedEmail);
            command.Parameters.AddWithValue("login_password", encPass.encryptedIVPass);
            await command.ExecuteNonQueryAsync();
            return "Password added successfully!";
        }

        /// <summary>
        /// Retrieve all encrypted passwords from DB
        /// Passwords are made of a username, email, password, and website/location
        /// </summary>
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <returns></returns>
        [HttpGet("GetPasswords")]
        public async Task<List<EncryptedPassword>> GetPasswords([FromQuery] string email, [FromQuery] string password) 
        {
            {
                int owner_id = GetUserID(email, password).Result;

                await using var dataSource = NpgsqlDataSource.Create(connString);
                await using var command = dataSource.CreateCommand("SELECT login_location, login_user, login_email, login_password FROM passwords WHERE owner_id = @owner_id");
                command.Parameters.AddWithValue("owner_id", owner_id);
                await using var reader = await command.ExecuteReaderAsync();

                List<EncryptedPassword> passwords = new List<EncryptedPassword>();

                while (await reader.ReadAsync())
                {
                    var encPass = new EncryptedPassword
                    {
                        // Turn into byte array
                        encryptedLocation = reader.GetFieldValue<byte[]>(0),
                        encryptedUsername = reader.GetFieldValue<byte[]>(1),
                        encryptedEmail = reader.GetFieldValue<byte[]>(2),
                        encryptedIVPass = reader.GetFieldValue<byte[]>(3)
                    };
                    passwords.Add(encPass);
                }
                return passwords;
            }        
        }
    }
}
