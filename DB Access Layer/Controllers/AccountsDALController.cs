﻿using Microsoft.AspNetCore.Components;

namespace DB_Access_Layer.Controllers
{
    using Npgsql;
    using BCrypt.Net;
    using System.Security.Cryptography;
    using DB_Access_Layer.Models;
    using Microsoft.AspNetCore.Mvc;
    [Route("[controller]")]
    [ApiController]
    public class AccountsDALController : ControllerBase
    {
        static string connString = ControllerHelper.getConnString();
        ControllerHelper controllerHelper = new ControllerHelper();

        /// <summary>
        /// Create new account with a random key
        /// </summary>
        /// <param name="email">Email of new account</param>
        /// <param name="masterPass">Master password of new account</param>
        /// <returns></returns>
        [HttpPost("CreateAccount")]
        public async Task<string> CreateAccount([FromQuery] string email, [FromQuery] string masterPass)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            masterPass = controllerHelper.DecryptStringAsymmetrically(masterPass);

            var hashPass = BCrypt.HashPassword(masterPass);

            // TODO: Ensure that user does not already exist in DB
            await using var checkCommand = NpgsqlDataSource.Create(connString).CreateCommand("SELECT user_name FROM users WHERE user_name = @user_name");
            checkCommand.Parameters.AddWithValue("user_name", email);
            await using var reader = await checkCommand.ExecuteReaderAsync();
            if (await reader.ReadAsync()) throw new Exception("User already exists!");

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
            command.Parameters.AddWithValue("user_pass", hashPass);
            command.Parameters.AddWithValue("user_key", userKey);
            var resp = await command.ExecuteNonQueryAsync();
            if (resp == 1) return "Account created successfully!";
            else throw new Exception("Account creation failed!");
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
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);

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
            else return null;
        }
    }
}
