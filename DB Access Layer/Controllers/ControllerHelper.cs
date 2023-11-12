using BCrypt.Net;
using Npgsql;

namespace DB_Access_Layer.Controllers
{
    using BCrypt.Net;
    public class ControllerHelper
    {
        public static string getConnString()
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

        // Decrypt Asymmetrically Method goes here
    }
}
