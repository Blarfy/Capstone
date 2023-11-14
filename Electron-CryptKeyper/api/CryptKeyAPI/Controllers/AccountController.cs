using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace CryptKeyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController
    {
        public AccountController()
        {
        }
        /// Send request to create a new user account
        /// <param name="email">E-Mail of new account</param>
        /// <param name="masterPass">Master password of new account</param>
        /// <returns></returns>
        [HttpPost(Name = "CreateAccount")]
        public async Task<string> CreateAccount([FromQuery]string email, [FromQuery]string masterPass)
        {
            CryptController sepulchre = new CryptController();
            byte[] emailBytes = sepulchre.EncryptStringAsym(email);
            byte[] passwordBytes = sepulchre.EncryptStringAsym(masterPass);

            email = HttpUtility.UrlEncode(Convert.ToBase64String(emailBytes));
            masterPass = HttpUtility.UrlEncode(Convert.ToBase64String(passwordBytes));

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync($"https://localhost:7124/AccountsDAL/CreateAccount?email={email}&masterPass={masterPass}", null);
                return await response.Result.Content.ReadAsStringAsync();
            }
        }

        /// Login function returns lockbox key
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <returns></returns>
        [HttpGet(Name = "Login")]
        public async Task<string> Login([FromQuery]string email, [FromQuery]string password)
        {
            CryptController sepulchre = new CryptController();
            byte[] emailBytes = sepulchre.EncryptStringAsym(email);
            byte[] passwordBytes = sepulchre.EncryptStringAsym(password);

            email = HttpUtility.UrlEncode(Convert.ToBase64String(emailBytes));
            password = HttpUtility.UrlEncode(Convert.ToBase64String(passwordBytes));

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync($"https://localhost:7124/AccountsDAL/Login?email={email}&password={password}");

                // If login is unsuccessful, return null
                if (!response.Result.IsSuccessStatusCode)
                {
                    return null;
                }
                var recieved = await response.Result.Content.ReadAsStringAsync();


                // Extract key from response
                recieved = recieved.Substring(1, recieved.Length - 2); // Remove surrounding quotes from string
                if (recieved != "Invalid credentials")
                {
                    recieved = HttpUtility.UrlEncode(recieved);
                    return recieved;
                }
                return null;
            }
        }
    }
}