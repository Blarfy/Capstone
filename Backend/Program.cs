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
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT user_name FROM users");
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {   
                Console.WriteLine(reader.GetString(0));
            }
            
            // Non-static version
            // var program = new Program();
            // var key = program.Login(connString, "awagga", "gweeble").Result;

            var key = Login(connString, "awagga", "gweeble").Result.Item1;
            
            Console.WriteLine(key);
        }

        // Login function returns tuple of lockbox key and user ID
        // Move to DB Access later
        public static async Task<Tuple<string, int>> Login(string connection, string email, string password)
        {
            await using var dataSource = NpgsqlDataSource.Create(connection);
            await using var command = dataSource.CreateCommand("SELECT user_pass, user_key, user_id FROM users WHERE user_name = @user_name");
            command.Parameters.AddWithValue("user_name", email);
            await using var reader = await command.ExecuteReaderAsync();

            //if (await reader.ReadAsync() && BCrypt.Verify(password, reader.GetString(0)))
            if (await reader.ReadAsync())
            {
                return new Tuple<string, int>(reader.GetString(1), reader.GetInt32(2));
            } 
            else return new Tuple<string, int>("", -1);
        }

        // Retrieve all encrypted passwords from DB
        // Passwords are made of a username, email, password, and website/location
        // Move to DB Access later
        public static async Task<Dictionary<string, List<string>>> GetPasswords(string connection, int owner_id)
        {
            await using var dataSource = NpgsqlDataSource.Create(connection);
            await using var command = dataSource.CreateCommand("SELECT login_location, login_user, login_email, login_password FROM passwords WHERE owner_id = @owner_id");
            command.Parameters.AddWithValue("owner_id", owner_id);
            await using var reader = await command.ExecuteReaderAsync();

            var passwords = new Dictionary<string, List<string>>();

            while (await reader.ReadAsync())
            {
                List<string> passData = new List<string>
                {
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3)
                };
                passwords.Add(reader.GetString(0), passData); 
            }

            return passwords;
        }

        // // Retrieve user_id from DB
        // // Move to DB Access later
        // public static async Task<int> GetUserID(string connection, string email)
        // {
        //     await using var dataSource = NpgsqlDataSource.Create(connection);
        //     await using var command = dataSource.CreateCommand("SELECT user_id FROM users WHERE user_name = @user_name");
        //     command.Parameters.AddWithValue("user_name", email);
        //     await using var reader = await command.ExecuteReaderAsync();

        //     if (await reader.ReadAsync())
        //     {
        //         return reader.GetInt32(0);
        //     } 
        //     else return -1;
        // }


        // Decrypt all passwords using lockbox key
        // TODO: Store lockbox key and passwords in cache  
    }
}