namespace Backend
{
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using System.IO;
    using System;
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Find configuration
            string appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "appsettings.json");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true)
                .Build();

            // Get environment variables
            string DBhost = configuration.GetConnectionString("DefaultConnection") ?? "localhost";
            string DBuser = configuration.GetConnectionString("DefaultUser") ?? "postgres";
            string DBpass = configuration.GetConnectionString("DefaultPassword") ?? "postgres";
            string DBname = configuration.GetConnectionString("DefaultDatabase") ?? "postgres";

            // Connect to postgres
            var connString = $"Host={DBhost};Username={DBuser};Password={DBpass};Database={DBname}";
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT user_name FROM users");
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {   
                Console.WriteLine(reader.GetString(0));
            }
        }
    }
}