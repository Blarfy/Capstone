using System.Security.Cryptography.X509Certificates;

namespace CryptKeyAPI.Controllers
{
    public class AccountController
    {
        public AccountController()
        {
        }
        /// Send request to create a new user account
        /// <param name="email">E-Mail of new account</param>
        /// <param name="masterPass">Master password of new account</param>
        public async Task<string> CreateAccount(string email, string masterPass)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync($"https://localhost:7124/api/CreateAccount?email={email}&masterPass={masterPass}", null);
                return await response.Result.Content.ReadAsStringAsync();
            }
        }

        /// Login function returns lockbox key
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <returns></returns>
        public async Task<byte[]> Login(string email, string password)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync($"https://localhost:7124/api/Login?email={email}&password={password}");
                var recieved = await response.Result.Content.ReadAsStringAsync();

                // Extract key from response
                recieved = recieved.Substring(1, recieved.Length - 2); // Remove surrounding quotes from string
                byte[] key = new byte[32];
                if (recieved != "Invalid credentials")
                {
                    key = Convert.FromBase64String(recieved);
                }

                return key;
            }
        }
    }
}