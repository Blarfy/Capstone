using CryptKeyAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CryptKeyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NoteController 
    {
        public NoteController()
        {
        }

        [HttpGet("TestConnection")]
        public string TestConnection()
        {
            return "Hello World!";
        }

        /// Encrypt and add note to DB
        [HttpPost(Name = "AddNote")]
        public async Task<string> AddNote([FromQuery]string email, [FromQuery]string password, [FromQuery] string strKey, [FromBody]DecryptedNote plaintextNote)
        {
            byte[] key = new byte[32];
            key = Convert.FromBase64String(strKey);
            // TODO: Ensure there is no note with the same title in DB
            // Do this in DB Access and return error message if note already exists

            // Encrypt title and note with key
            using (Aes myAes = Aes.Create())
            {
                // Generate IV
                myAes.GenerateIV();

                // Encrypt information
                CryptController sepulchre = new CryptController();
                byte[] encryptedTitle = sepulchre.EncryptStringToBytes_Aes(plaintextNote.plaintextTitle, key, myAes.IV);
                byte[] encryptedNote = sepulchre.EncryptStringToBytes_Aes(plaintextNote.plaintextNote, key, myAes.IV);

                // Concatenate IV and encrypted note
                byte[] encryptedIVNote = new byte[4 + myAes.IV.Length + encryptedNote.Length];
                BitConverter.GetBytes(myAes.IV.Length).CopyTo(encryptedIVNote, 0); // Add IV length to beginning of array to allow for IV extraction later
                myAes.IV.CopyTo(encryptedIVNote, 4);
                encryptedNote.CopyTo(encryptedIVNote, myAes.IV.Length + 4);

                // Turn into EncryptedNote object
                var encNote = new EncryptedNote
                {
                    encryptedTitle = encryptedTitle,
                    encryptedNote = encryptedIVNote
                };

                // Serialize EncryptedNote object
                var json = System.Text.Json.JsonSerializer.Serialize(encNote);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Send to DB Access API
                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.PostAsync($"https://localhost:7124/NotesDAL/AddNote?email={email}&password={password}", content);
                    return await response.Result.Content.ReadAsStringAsync();
                }
            }

        }

        [HttpGet(Name = "GetNotes")]
        public async Task<List<EncryptedNote>> GetNotes([FromQuery]string email, [FromQuery]string password)
        {
            using (var httpClient = new HttpClient())
            {
                List<EncryptedNote> encryptedNotes = new List<EncryptedNote>();
                var response = await httpClient.GetAsync($"https://localhost:7124/NotesDAL/GetNotes?email={email}&password={password}");
                if (response.IsSuccessStatusCode)
                {
                    var recieved = await response.Content.ReadAsStringAsync();
                    encryptedNotes = JsonConvert.DeserializeObject<List<EncryptedNote>>(recieved);
                }
                return encryptedNotes;
            }
        }

        [HttpPost("DecryptNotes")]
        public List<DecryptedNote> DecryptNotes([FromQuery]string key, [FromBody]List<EncryptedNote> encryptedNotes)
        {
            byte[] keyBytes = new byte[32];
            keyBytes = Convert.FromBase64String(key);

            List<DecryptedNote> decryptedNotes = new List<DecryptedNote>();

            foreach (EncryptedNote entry in encryptedNotes)
            {
                var encNote = entry;

                // Extract IV from encrypted note
                byte[] iv = new byte[BitConverter.ToInt32(encNote.encryptedNote, 0)];
                Array.Copy(encNote.encryptedNote, 4, iv, 0, iv.Length);
                byte[] encryptedNoteNoIV = new byte[encNote.encryptedNote.Length - iv.Length - 4];
                Array.Copy(encNote.encryptedNote, iv.Length + 4, encryptedNoteNoIV, 0, encryptedNoteNoIV.Length);

                CryptController sepulchre = new CryptController();
                string plaintextTitle = sepulchre.DecryptStringFromBytes_Aes(encNote.encryptedTitle, keyBytes, iv);
                string plaintextNote = sepulchre.DecryptStringFromBytes_Aes(encryptedNoteNoIV, keyBytes, iv);
                decryptedNotes.Add(new DecryptedNote(plaintextTitle, plaintextNote));
            }
            return decryptedNotes;
        }
    }
}
