using CryptKeyAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Web;

namespace CryptKeyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController
    {
        public PaymentController()
        {
        }

        /// Encrypt and add payment to DB
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <param name="key">Lockbox key</param>
        /// <param name="payLocation">Website or location of payment</param>
        /// <param name="payUserName">Username of payment</param>
        /// <param name="payEmail">Email of payment</param>
        /// <param name="payPassword">Password of payment</param>
        [HttpPost(Name = "AddPayment")]
        public async Task<string> AddPayment([FromQuery] string email, [FromQuery] string password, [FromQuery] string strKey, [FromBody] DecryptedPayment plaintextPay)
        {
            byte[] key = new byte[32];
            key = Convert.FromBase64String(strKey);

            // Encrypt location, username, email, and password with key
            using (Aes myAes = Aes.Create())
            {
                // Generate IV
                myAes.GenerateIV();

                // Encrypt information
                CryptController sepulchre = new CryptController();
                byte[] encryptedName = sepulchre.EncryptStringToBytes_Aes(plaintextPay.plaintextCardName, key, myAes.IV);
                byte[] encryptedNumber = sepulchre.EncryptStringToBytes_Aes(plaintextPay.plaintextCardNumber, key, myAes.IV);
                byte[] encryptedCVV = sepulchre.EncryptStringToBytes_Aes(plaintextPay.plaintextCardCVV, key, myAes.IV);
                byte[] encryptedExpMonth = sepulchre.EncryptStringToBytes_Aes(plaintextPay.plaintextCardExpMonth, key, myAes.IV);
                byte[] encryptedExpYear = sepulchre.EncryptStringToBytes_Aes(plaintextPay.plaintextCardExpYear, key, myAes.IV);

                // Concatenate IV and CVV
                byte[] encryptedIVCVV = new byte[4 + myAes.IV.Length + encryptedCVV.Length];
                BitConverter.GetBytes(myAes.IV.Length).CopyTo(encryptedIVCVV, 0); // Add IV length to beginning of array to allow for IV extraction later
                myAes.IV.CopyTo(encryptedIVCVV, 4);
                encryptedCVV.CopyTo(encryptedIVCVV, myAes.IV.Length + 4);

                // Turn into EncryptedPassword object
                var encPay = new EncryptedPayment
                {
                    encryptedCardName = encryptedName,
                    encryptedCardNumber = encryptedNumber,
                    encryptedCardCVV = encryptedIVCVV,
                    encryptedCardExpMonth = encryptedExpMonth,
                    encryptedCardExpYear = encryptedExpYear
                };

                // Serialize EncryptedPassword object
                var json = System.Text.Json.JsonSerializer.Serialize(encPay);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Asymmetrically encrypt email and password
                byte[] emailBytes = sepulchre.EncryptStringAsym(email);
                byte[] passwordBytes = sepulchre.EncryptStringAsym(password);

                email = HttpUtility.UrlEncode(Convert.ToBase64String(emailBytes));
                password = HttpUtility.UrlEncode(Convert.ToBase64String(passwordBytes));

                // Send to DB Access API
                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.PostAsync($"https://localhost:7124/PaymentsDAL/AddPayment?email={email}&password={password}", content);
                    return await response.Result.Content.ReadAsStringAsync();
                }

            }
        }

        [HttpGet(Name = "GetPayments")]
        public async Task<List<EncryptedPayment>> GetPayments([FromQuery] string email, [FromQuery] string password)
        {
            CryptController sepulchre = new CryptController();
            byte[] emailBytes = sepulchre.EncryptStringAsym(email);
            byte[] passwordBytes = sepulchre.EncryptStringAsym(password);

            email = HttpUtility.UrlEncode(Convert.ToBase64String(emailBytes));
            password = HttpUtility.UrlEncode(Convert.ToBase64String(passwordBytes));

            using (var httpClient = new HttpClient())
            {
                List<EncryptedPayment> encryptedPayments = new List<EncryptedPayment>();
                var response = await httpClient.GetAsync($"https://localhost:7124/PaymentsDAL/GetPayments?email={email}&password={password}");

                if(response.IsSuccessStatusCode)
                {
                    var recieved = await response.Content.ReadAsStringAsync();
                    encryptedPayments = JsonConvert.DeserializeObject<List<EncryptedPayment>>(recieved);
                }
                return encryptedPayments;
            }
        }

        [HttpPost("DecryptPayments")]
        public List<DecryptedPayment> DecryptPayments([FromQuery] string key, [FromBody] List<EncryptedPayment> encryptedPayments)
        {
            byte[] keyBytes = new byte[32];
            keyBytes = Convert.FromBase64String(key);

            List<DecryptedPayment> decryptedPayments = new List<DecryptedPayment>();

            foreach (EncryptedPayment encryptedPayment in encryptedPayments)
            {
                byte[] iv = new byte[BitConverter.ToInt32(encryptedPayment.encryptedCardCVV, 0)];
                Array.Copy(encryptedPayment.encryptedCardCVV, 4, iv, 0, iv.Length);
                byte[] encryptedCVVNoIV = new byte[encryptedPayment.encryptedCardCVV.Length - iv.Length - 4];
                Array.Copy(encryptedPayment.encryptedCardCVV, iv.Length + 4, encryptedCVVNoIV, 0, encryptedCVVNoIV.Length);

                CryptController sepulchre = new CryptController();
                string decryptedName = sepulchre.DecryptStringFromBytes_Aes(encryptedPayment.encryptedCardName, keyBytes, encryptedPayment.encryptedCardName);
                string decryptedNumber = sepulchre.DecryptStringFromBytes_Aes(encryptedPayment.encryptedCardNumber, keyBytes, encryptedPayment.encryptedCardNumber);
                string decryptedCVV = sepulchre.DecryptStringFromBytes_Aes(encryptedCVVNoIV, keyBytes, iv);
                string decryptedExpMonth = sepulchre.DecryptStringFromBytes_Aes(encryptedPayment.encryptedCardExpMonth, keyBytes, encryptedPayment.encryptedCardExpMonth);
                string decryptedExpYear = sepulchre.DecryptStringFromBytes_Aes(encryptedPayment.encryptedCardExpYear, keyBytes, encryptedPayment.encryptedCardExpYear);

                var decPay = new DecryptedPayment
                {
                    plaintextCardName = decryptedName,
                    plaintextCardNumber = decryptedNumber,
                    plaintextCardCVV = decryptedCVV,
                    plaintextCardExpMonth = decryptedExpMonth,
                    plaintextCardExpYear = decryptedExpYear
                };

                decryptedPayments.Add(decPay);
            }
            return decryptedPayments;
        }

    }
}
