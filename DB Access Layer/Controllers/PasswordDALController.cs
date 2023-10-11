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

        string connString = getConnString();

        [HttpGet ("TestConnection")]
        public string TestConnection()
        {
            return "Hello World!";
        }

        // TODO: Other functions utilize "email" and "password" while this does not. Uniformize all DB Access methods when moving over.
        // Move to DB Access later
        // <param name="connString">Connection string to postgres database</param>
        // <param name="userName">Username of new account</param>
        // <param name="masterPass">Master password of new account</param>
        [HttpPost("CreateAccount")]
        public async Task CreateAccount([FromQuery] string userName, [FromQuery] string masterPass) {
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
            command.Parameters.AddWithValue("user_name", userName);
            command.Parameters.AddWithValue("user_pass", masterPass);
            command.Parameters.AddWithValue("user_key", userKey);
            await command.ExecuteNonQueryAsync();
        }

        // Retrieve user_id from DB
        // Unsure where to use this as of now
        // <param name="connection">Connection string to postgres database</param>
        public async Task<int> GetUserID(string email)
        {
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT user_id FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", email);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            } 
            else return -1;
        }

        // Login function returns tuple of lockbox key and user ID
        // <param name="email">Email of user</param>
        // <param name="password">Password of user</param>
        [HttpPost("Login")]
        public async Task<Tuple<byte[], int>> Login([FromQuery] string email, [FromQuery] string password)
        {
            await using var dataSource = NpgsqlDataSource.Create(connString);
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

        // Encrypt and add password to DB
        // <param name="key">Lockbox key</param>
        // <param name="ownerID">User ID of owner</param>
        // <param name="encryptedLocation">Encrypted location of login</param>
        // <param name="encryptedUsername">Encrypted username of login</param>
        // <param name="encryptedEmail">Encrypted email of login</param>
        // <param name="encryptedIVPass">Encrypted password of login</param>
        [HttpPost("AddPassword")]
        public async Task AddPassword([FromQuery] int ownerID, [FromBody] EncryptedPassword encPass) 
        {
            // TODO: Ensure that password is not already in DB

            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("INSERT INTO passwords (owner_id, login_location, login_user, login_email, login_password) VALUES (@owner_id, @login_location, @login_user, @login_email, @login_password)");
            command.Parameters.AddWithValue("owner_id", ownerID);
            command.Parameters.AddWithValue("login_location", encPass.encryptedLocation);
            command.Parameters.AddWithValue("login_user", encPass.encryptedUsername);
            command.Parameters.AddWithValue("login_email", encPass.encryptedEmail);
            command.Parameters.AddWithValue("login_password", encPass.encryptedIVPass);
            await command.ExecuteNonQueryAsync();
        }

        // Retrieve all encrypted passwords from DB
        // Passwords are made of a username, email, password, and website/location
        // <param name="owner_id">User ID of owner</param>
        [HttpGet("GetPasswords")]
        public async Task<Dictionary<byte[], List<byte[]>>> GetPasswords([FromQuery] int owner_id)
        {
            await using var dataSource = NpgsqlDataSource.Create(connString);
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
    }
}
