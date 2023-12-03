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
                
                foreach(EncryptedShared encryptedShare in encryptedShared)
                {
                    encryptedShare.item = JsonConvert.SerializeObject(encryptedShare.item);
                }
                return encryptedShared;
            }
        }

        [HttpPost("DecryptShared")]
        public List<EncryptedShared> DecryptShared([FromQuery] string key, [FromBody] List<EncryptedShared> encryptedShared)
        {
            byte[] keyBytes = new byte[32];
            keyBytes = Convert.FromBase64String(key);

            List<EncryptedShared> decryptedShared = new List<EncryptedShared>();

            foreach (EncryptedShared encryptedShare in encryptedShared)
            {
                EncryptedShared newEncryptedShared = new EncryptedShared();

                switch(encryptedShare.itemType)
                {
                    case "password":
                        PassController passController = new PassController();
                        encryptedShare.item = JsonConvert.DeserializeObject<EncryptedPassword>(encryptedShare.item.ToString());
                        
                        List<DecryptedPassword> newPass = passController.DecryptPasswords(key, new List<EncryptedPassword> { (EncryptedPassword)encryptedShare.item });
                        newEncryptedShared = new EncryptedShared("password", newPass[0]);
                        break;
                    case "payment":
                        PaymentController paymentController = new PaymentController();
                        encryptedShare.item = JsonConvert.DeserializeObject<EncryptedPayment>(encryptedShare.item.ToString());

                        List<DecryptedPayment> newPayment = paymentController.DecryptPayments(key, new List<EncryptedPayment> { (EncryptedPayment)encryptedShare.item });
                        newEncryptedShared = new EncryptedShared("payment", newPayment[0]);
                        break;
                    case "note":
                        NoteController noteController = new NoteController();
                        encryptedShare.item = JsonConvert.DeserializeObject<EncryptedNote>(encryptedShare.item.ToString());

                        List<DecryptedNote> newNote = noteController.DecryptNotes(key, new List<EncryptedNote> { (EncryptedNote)encryptedShare.item });
                        newEncryptedShared = new EncryptedShared("note", newNote[0]);
                        break;
                }
                decryptedShared.Add(newEncryptedShared);
            }

            return decryptedShared;
        }
    }
}
