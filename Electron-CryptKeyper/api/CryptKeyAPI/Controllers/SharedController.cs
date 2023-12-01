using CryptKeyAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Web;

namespace CryptKeyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SharedController
    {
        public SharedController()
        {
        }

        [HttpPost(Name = "AddShared")]
        public async Task<string> AddShared([FromQuery] string email, [FromQuery] string password, [FromQuery] string type, [FromQuery] string shareeUsername, [FromBody] Object encItem)
        {
            // Asymmetrically encrypt email, password, type, and shareeUsername
            CryptController sepulchre = new CryptController();
            byte[] emailBytes = sepulchre.EncryptStringAsym(email);
            byte[] passwordBytes = sepulchre.EncryptStringAsym(password);
            byte[] typeBytes = sepulchre.EncryptStringAsym(type);
            byte[] shareeUsernameBytes = sepulchre.EncryptStringAsym(shareeUsername);

            email = HttpUtility.UrlEncode(Convert.ToBase64String(emailBytes));
            password = HttpUtility.UrlEncode(Convert.ToBase64String(passwordBytes));
            type = HttpUtility.UrlEncode(Convert.ToBase64String(typeBytes));
            shareeUsername = HttpUtility.UrlEncode(Convert.ToBase64String(shareeUsernameBytes));

            // Send to DB Access API
            using (var httpClient = new HttpClient())
            {
                var json = System.Text.Json.JsonSerializer.Serialize(encItem);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"https://localhost:7124/SharedDAL/AddShared?email={email}&password={password}&type={type}&shareeUsername={shareeUsername}", content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        [HttpGet(Name = "GetShared")]
        public async Task<List<EncryptedShared>> GetShared([FromQuery] string email, [FromQuery] string password)
        {
            CryptController sepulchre = new CryptController();
            byte[] emailBytes = sepulchre.EncryptStringAsym(email);
            byte[] passwordBytes = sepulchre.EncryptStringAsym(password);

            email = HttpUtility.UrlEncode(Convert.ToBase64String(emailBytes));
            password = HttpUtility.UrlEncode(Convert.ToBase64String(passwordBytes));

            using (var httpClient = new HttpClient())
            {
                List<EncryptedShared> encryptedShared = new List<EncryptedShared>();
                var response = await httpClient.GetAsync($"https://localhost:7124/SharedDAL/GetShared?email={email}&password={password}");
                if (response.IsSuccessStatusCode)
                {
                    var recieved = await response.Content.ReadAsStringAsync();
                    encryptedShared = JsonConvert.DeserializeObject<List<EncryptedShared>>(recieved);
                }
                return encryptedShared;
            }
        }
    }
}
