using DB_Access_Layer.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace DB_Access_Layer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NotesDALController
    {
        static string connString = ControllerHelper.getConnString();
        ControllerHelper controllerHelper = new ControllerHelper();

        [HttpPost("AddNote")]
        public async Task<string> AddNote([FromQuery] string email, [FromQuery] string password, [FromBody] EncryptedNote encNote)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);

            int ownerID = controllerHelper.GetUserID(email, password).Result; // Email and Password are to be asymmetrically decrypted later

            // Add note to DB
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("INSERT INTO notes (owner_id, note_title, note_text) VALUES (@owner_id, @note_title, @note_text)");
            command.Parameters.AddWithValue("owner_id", ownerID);
            command.Parameters.AddWithValue("note_title", encNote.encryptedTitle);
            command.Parameters.AddWithValue("note_text", encNote.encryptedNote);
            var response = await command.ExecuteNonQueryAsync();
            if (response == 1) return "Note added successfully!";
            else return "Error while adding Note.";
        }

        [HttpGet("GetNotes")]
        public async Task<List<EncryptedNote>> GetNotes([FromQuery] string email, [FromQuery] string password)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);

            int owner_id = controllerHelper.GetUserID(email, password).Result;

            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT note_title, note_text FROM notes WHERE owner_id = @owner_id");
            command.Parameters.AddWithValue("owner_id", owner_id);
            await using var reader = await command.ExecuteReaderAsync();

            List<EncryptedNote> notes = new List<EncryptedNote>();

            while (await reader.ReadAsync())
            {
                var encNote = new EncryptedNote
                {
                    // Turn into byte array
                    encryptedTitle = reader.GetFieldValue<byte[]>(0),
                    encryptedNote = reader.GetFieldValue<byte[]>(1)
                };
                notes.Add(encNote);
            }
            return notes;
        }
    }
}
