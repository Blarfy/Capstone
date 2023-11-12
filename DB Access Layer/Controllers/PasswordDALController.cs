using Microsoft.AspNetCore.Mvc;

namespace DB_Access_Layer.Controllers
{
    using Npgsql;
    using BCrypt.Net;
    using System.Security.Cryptography;
    using DB_Access_Layer.Models;

    [Route("[controller]")]
    [ApiController]
    public class PasswordDALController : ControllerBase
    {
        static string connString = ControllerHelper.getConnString();
        ControllerHelper controllerHelper = new ControllerHelper();   

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

            int ownerID = controllerHelper.GetUserID(email, password).Result;

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
                int owner_id = controllerHelper.GetUserID(email, password).Result;

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
