using Microsoft.AspNetCore.Mvc;
using Npgsql;
using DB_Access_Layer.Models;
using System.Text.Json;

namespace DB_Access_Layer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SharedDALController
    {
        static string connString = ControllerHelper.getConnString();
        ControllerHelper controllerHelper = new ControllerHelper();

        [HttpPost("AddShared")]
        public async Task<string> AddShared([FromQuery] string email, [FromQuery] string password, [FromQuery] string type, [FromQuery] string shareeUsername, [FromBody] Object EncryptedItem)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);
            shareeUsername = controllerHelper.DecryptStringAsymmetrically(shareeUsername);
            type = controllerHelper.DecryptStringAsymmetrically(type);

            int ownerID = controllerHelper.GetUserID(email, password).Result;

            // Find sharee's ID
            int shareeID = controllerHelper.GetUserID(shareeUsername).Result;
            if (shareeID == -1) return "Sharee does not exist!";
            if (shareeID == ownerID) return "Cannot share with yourself!";

            // Find item's ID
            int itemID = -1;
            string itemQuery = "";
            var typeSource = NpgsqlDataSource.Create(connString);
            var itemCommand = typeSource.CreateCommand(itemQuery);
            switch (type)
            {
                case "note":
                    itemQuery = "SELECT note_id FROM notes WHERE owner_id = @owner_id AND note_title = @note_title AND note_text = @note_text";
                    var newNote = JsonSerializer.Deserialize<EncryptedNote>(EncryptedItem.ToString());
                    itemCommand = typeSource.CreateCommand(itemQuery);
                    itemCommand.Parameters.AddWithValue("owner_id", ownerID);
                    itemCommand.Parameters.AddWithValue("note_title", newNote.encryptedTitle);
                    itemCommand.Parameters.AddWithValue("note_text", newNote.encryptedNote);
                    break;
                case "password":
                    itemQuery = "SELECT pass_id FROM passwords WHERE owner_id = @owner_id AND login_location = @login_location AND login_user = @login_user AND login_email = @login_email AND login_password = @login_password";
                    var newPass = JsonSerializer.Deserialize<EncryptedPassword>(EncryptedItem.ToString());
                    itemCommand = typeSource.CreateCommand(itemQuery);
                    itemCommand.Parameters.AddWithValue("owner_id", ownerID);
                    itemCommand.Parameters.AddWithValue("login_location", newPass.encryptedLocation);
                    itemCommand.Parameters.AddWithValue("login_user", newPass.encryptedUsername);
                    itemCommand.Parameters.AddWithValue("login_email", newPass.encryptedEmail);
                    itemCommand.Parameters.AddWithValue("login_password", newPass.encryptedIVPass);
                    break;
                case "payment":
                    itemQuery = "SELECT payment_id FROM payments WHERE owner_id = @owner_id AND pay_name = @pay_name AND pay_card = @pay_card AND pay_cvv = @pay_cvv AND pay_month = @pay_month AND pay_year = @pay_year";
                    var newPayment = JsonSerializer.Deserialize<EncryptedPayment>(EncryptedItem.ToString());
                    itemCommand = typeSource.CreateCommand(itemQuery);
                    itemCommand.Parameters.AddWithValue("owner_id", ownerID);
                    itemCommand.Parameters.AddWithValue("pay_name", newPayment.encryptedCardName);
                    itemCommand.Parameters.AddWithValue("pay_card", newPayment.encryptedCardNumber);
                    itemCommand.Parameters.AddWithValue("pay_cvv", newPayment.encryptedCardCVV);
                    itemCommand.Parameters.AddWithValue("pay_month", newPayment.encryptedCardExpMonth);
                    itemCommand.Parameters.AddWithValue("pay_year", newPayment.encryptedCardExpYear);
                    break;
            }
            await using var itemReader = await itemCommand.ExecuteReaderAsync();
            if (await itemReader.ReadAsync())
            {
                itemID = itemReader.GetInt32(0);
            }
            else return "Item does not exist!";
            itemReader.Close();

            // Add shared to DB        
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("INSERT INTO shared (type, sharer_id, sharee_id, item_id) VALUES (@type, @sharer_id, @sharee_id, @item_id)");
            command.Parameters.AddWithValue("type", type);
            command.Parameters.AddWithValue("sharer_id", ownerID);
            command.Parameters.AddWithValue("sharee_id", shareeID);
            command.Parameters.AddWithValue("item_id", itemID);
            await command.ExecuteNonQueryAsync();
            return "Shared added successfully!";
        }

        [HttpGet("GetShared")]
        public async Task<List<EncryptedShared>> GetShared([FromQuery] string email, [FromQuery] string password)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);

            int owner_id = controllerHelper.GetUserID(email, password).Result;

            // Get all shared from DB
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT item_id, type, sharer_id FROM shared WHERE sharee_id = @owner_id");
            command.Parameters.AddWithValue("owner_id", owner_id);
            await using var reader = await command.ExecuteReaderAsync();
            List<EncryptedShared> shared = new List<EncryptedShared>();
            while (await reader.ReadAsync())
            {
                // Separate items by type and add to shared list
                if (reader.GetString(1) == "note")
                {
                    var noteCommand = dataSource.CreateCommand("SELECT note_title, note_text FROM notes WHERE note_id = @note_id");
                    noteCommand.Parameters.AddWithValue("note_id", reader.GetInt32(0));
                    await using var noteReader = await noteCommand.ExecuteReaderAsync();
                    if (await noteReader.ReadAsync())
                    {
                        shared.Add(RecryptShared(new EncryptedShared("note", new EncryptedNote
                        {
                            encryptedTitle = noteReader.GetFieldValue<byte[]>(0),
                            encryptedNote = noteReader.GetFieldValue<byte[]>(1)
                        }), email, password, reader.GetInt32(2)));
                    }
                }
                else if(reader.GetString(1) == "password")
                {
                    var passCommand = dataSource.CreateCommand("SELECT login_location, login_user, login_email, login_password FROM passwords WHERE pass_id = @pass_id");
                    passCommand.Parameters.AddWithValue("pass_id", reader.GetInt32(0));
                    await using var passReader = await passCommand.ExecuteReaderAsync();
                    if (await passReader.ReadAsync())
                    {
                        shared.Add(RecryptShared(new EncryptedShared("password", new EncryptedPassword
                        {
                            encryptedLocation = passReader.GetFieldValue<byte[]>(0),
                            encryptedUsername = passReader.GetFieldValue<byte[]>(1),
                            encryptedEmail = passReader.GetFieldValue<byte[]>(2),
                            encryptedIVPass = passReader.GetFieldValue<byte[]>(3)
                        }), email, password, reader.GetInt32(2)));
                    }
                }
                else if (reader.GetString(1) == "payment")
                {
                    var payCommand = dataSource.CreateCommand("SELECT pay_name, pay_card, pay_cvv, pay_month, pay_year FROM payments WHERE payment_id = @payment_id");
                    payCommand.Parameters.AddWithValue("payment_id", reader.GetInt32(0));
                    await using var payReader = await payCommand.ExecuteReaderAsync();
                    if (await payReader.ReadAsync())
                    {
                        shared.Add(RecryptShared(new EncryptedShared("payment", new EncryptedPayment
                        {
                            encryptedCardName = payReader.GetFieldValue<byte[]>(0),
                            encryptedCardNumber = payReader.GetFieldValue<byte[]>(1),
                            encryptedCardCVV = payReader.GetFieldValue<byte[]>(2),
                            encryptedCardExpMonth = payReader.GetFieldValue<byte[]>(3),
                            encryptedCardExpYear = payReader.GetFieldValue<byte[]>(4)
                        }), email, password, reader.GetInt32(2)));
                    }
                }
            }
            return shared;
        }

        public EncryptedShared RecryptShared(EncryptedShared sharedItem, string email, string password, int ownerID)
        {
            int shareeID = controllerHelper.GetUserID(email, password).Result;

            // Get sharee's key
            byte[] shareeKey = new byte[32];
            var keySource = NpgsqlDataSource.Create(connString);
            var keyCommand = keySource.CreateCommand("SELECT user_key FROM users WHERE user_id = @user_id");
            keyCommand.Parameters.AddWithValue("user_id", shareeID);
            var keyReader = keyCommand.ExecuteReader();
            keyReader.Read();
            shareeKey = keyReader.GetFieldValue<byte[]>(0);
            keyReader.Close();

            // Get owner's key
            byte[] ownerKey = new byte[32];
            var ownerKeySource = NpgsqlDataSource.Create(connString);
            var ownerKeyCommand = ownerKeySource.CreateCommand("SELECT user_key FROM users WHERE user_id = @user_id");
            ownerKeyCommand.Parameters.AddWithValue("user_id", ownerID);
            var ownerKeyReader = ownerKeyCommand.ExecuteReader();

            if (ownerKeyReader.Read())
            {
                ownerKey = ownerKeyReader.GetFieldValue<byte[]>(0);
            }
            ownerKeyReader.Close();

            EncryptedShared newEncryptedShared = new EncryptedShared();

            // Decrypt item
            switch (sharedItem.itemType)
            {
                case "note":
                    EncryptedNote note = (EncryptedNote)sharedItem.item;
                    // Get IV
                    byte[] iv = new byte[BitConverter.ToInt32(note.encryptedNote, 0)];
                    Array.Copy(note.encryptedNote, 4, iv, 0, iv.Length);
                    byte[] encryptedNoteNoIV = new byte[note.encryptedNote.Length - iv.Length - 4];
                    Array.Copy(note.encryptedNote, iv.Length + 4, encryptedNoteNoIV, 0, encryptedNoteNoIV.Length);

                    DecryptedNote decryptedNote = new DecryptedNote
                    (
                        controllerHelper.DecryptStringFromBytes_Aes(note.encryptedTitle, ownerKey, iv),
                        controllerHelper.DecryptStringFromBytes_Aes(encryptedNoteNoIV, ownerKey, iv)
                    );

                    // Recrypt item with sharee's key
                    byte[] newIV = controllerHelper.GenerateIV();
                    byte[] newEncryptedTitle = controllerHelper.EncryptStringToBytes_Aes(decryptedNote.plaintextTitle, shareeKey, newIV);
                    byte[] newEncryptedNote = controllerHelper.EncryptStringToBytes_Aes(decryptedNote.plaintextNote, shareeKey, newIV);

                    // Store new IV in encryptedNote
                    byte[] newEncryptedNoteWithIV = new byte[newEncryptedNote.Length + newIV.Length + 4];
                    BitConverter.GetBytes(newIV.Length).CopyTo(newEncryptedNoteWithIV, 0);
                    newIV.CopyTo(newEncryptedNoteWithIV, 4);
                    newEncryptedNote.CopyTo(newEncryptedNoteWithIV, newIV.Length + 4);

                    EncryptedNote newEncryptedNoteObj = new EncryptedNote
                    {
                        encryptedTitle = newEncryptedTitle,
                        encryptedNote = newEncryptedNoteWithIV
                    };

                    newEncryptedShared = new EncryptedShared("note", newEncryptedNoteObj);

                    return newEncryptedShared;
                case "password":
                    EncryptedPassword pass = (EncryptedPassword)sharedItem.item;
                    // Get IV from encryptedIVPass
                    byte[] ivPass = new byte[BitConverter.ToInt32(pass.encryptedIVPass, 0)];
                    Array.Copy(pass.encryptedIVPass, 4, ivPass, 0, ivPass.Length);
                    byte[] encryptedPassNoIV = new byte[pass.encryptedIVPass.Length - ivPass.Length - 4];
                    Array.Copy(pass.encryptedIVPass, ivPass.Length + 4, encryptedPassNoIV, 0, encryptedPassNoIV.Length);

                    DecryptedPassword decryptedPass = new DecryptedPassword
                    (
                        controllerHelper.DecryptStringFromBytes_Aes(pass.encryptedLocation, ownerKey, ivPass),
                        controllerHelper.DecryptStringFromBytes_Aes(pass.encryptedUsername, ownerKey, ivPass),
                        controllerHelper.DecryptStringFromBytes_Aes(pass.encryptedEmail, ownerKey, ivPass),
                        controllerHelper.DecryptStringFromBytes_Aes(encryptedPassNoIV, ownerKey, ivPass)
                    );

                    // Recrypt item with sharee's key
                    byte[] newIVPass = controllerHelper.GenerateIV();
                    byte[] newEncryptedLocation = controllerHelper.EncryptStringToBytes_Aes(decryptedPass.plaintextLocation, shareeKey, newIVPass);
                    byte[] newEncryptedUsername = controllerHelper.EncryptStringToBytes_Aes(decryptedPass.plaintextUsername, shareeKey, newIVPass);
                    byte[] newEncryptedEmail = controllerHelper.EncryptStringToBytes_Aes(decryptedPass.plaintextEmail, shareeKey, newIVPass);
                    byte[] newEncryptedPass = controllerHelper.EncryptStringToBytes_Aes(decryptedPass.plaintextIVPass, shareeKey, newIVPass);

                    // Store new IV in encryptedIVPass
                    byte[] newEncryptedPassWithIV = new byte[newEncryptedPass.Length + newIVPass.Length + 4];
                    BitConverter.GetBytes(newIVPass.Length).CopyTo(newEncryptedPassWithIV, 0);
                    newIVPass.CopyTo(newEncryptedPassWithIV, 4);
                    newEncryptedPass.CopyTo(newEncryptedPassWithIV, newIVPass.Length + 4);

                    EncryptedPassword newEncryptedPassObj = new EncryptedPassword
                    {
                        encryptedLocation = newEncryptedLocation,
                        encryptedUsername = newEncryptedUsername,
                        encryptedEmail = newEncryptedEmail,
                        encryptedIVPass = newEncryptedPassWithIV
                    };

                    newEncryptedShared = new EncryptedShared("password", newEncryptedPassObj);

                    return newEncryptedShared;
                case "payment":
                    EncryptedPayment payment = (EncryptedPayment)sharedItem.item;
                    // Get IV from encryptedCardCVV
                    byte[] ivCVV = new byte[BitConverter.ToInt32(payment.encryptedCardCVV, 0)];
                    Array.Copy(payment.encryptedCardCVV, 4, ivCVV, 0, ivCVV.Length);
                    byte[] encryptedCVVNoIV = new byte[payment.encryptedCardCVV.Length - ivCVV.Length - 4];
                    Array.Copy(payment.encryptedCardCVV, ivCVV.Length + 4, encryptedCVVNoIV, 0, encryptedCVVNoIV.Length);

                    DecryptedPayment decryptedPayment = new DecryptedPayment
                    (
                        controllerHelper.DecryptStringFromBytes_Aes(payment.encryptedCardName, ownerKey, ivCVV),
                        controllerHelper.DecryptStringFromBytes_Aes(payment.encryptedCardNumber, ownerKey, ivCVV),
                        controllerHelper.DecryptStringFromBytes_Aes(encryptedCVVNoIV, ownerKey, ivCVV),
                        controllerHelper.DecryptStringFromBytes_Aes(payment.encryptedCardExpMonth, ownerKey, ivCVV),
                        controllerHelper.DecryptStringFromBytes_Aes(payment.encryptedCardExpYear, ownerKey, ivCVV)
                    );

                    // Recrypt item with sharee's key
                    byte[] newIVCVV = controllerHelper.GenerateIV();
                    byte[] newEncryptedCardName = controllerHelper.EncryptStringToBytes_Aes(decryptedPayment.plaintextCardName, shareeKey, newIVCVV);
                    byte[] newEncryptedCardNumber = controllerHelper.EncryptStringToBytes_Aes(decryptedPayment.plaintextCardNumber, shareeKey, newIVCVV);
                    byte[] newEncryptedCVV = controllerHelper.EncryptStringToBytes_Aes(decryptedPayment.plaintextCardCVV, shareeKey, newIVCVV);
                    byte[] newEncryptedCardExpMonth = controllerHelper.EncryptStringToBytes_Aes(decryptedPayment.plaintextCardExpMonth, shareeKey, newIVCVV);
                    byte[] newEncryptedCardExpYear = controllerHelper.EncryptStringToBytes_Aes(decryptedPayment.plaintextCardExpYear, shareeKey, newIVCVV);

                    // Store new IV in encryptedCardCVV
                    byte[] newEncryptedCVVWithIV = new byte[newEncryptedCVV.Length + newIVCVV.Length + 4];
                    BitConverter.GetBytes(newIVCVV.Length).CopyTo(newEncryptedCVVWithIV, 0);
                    newIVCVV.CopyTo(newEncryptedCVVWithIV, 4);
                    newEncryptedCVV.CopyTo(newEncryptedCVVWithIV, newIVCVV.Length + 4);

                    EncryptedPayment newEncryptedPaymentObj = new EncryptedPayment
                    {
                        encryptedCardName = newEncryptedCardName,
                        encryptedCardNumber = newEncryptedCardNumber,
                        encryptedCardCVV = newEncryptedCVVWithIV,
                        encryptedCardExpMonth = newEncryptedCardExpMonth,
                        encryptedCardExpYear = newEncryptedCardExpYear
                    };

                    newEncryptedShared = new EncryptedShared("payment", newEncryptedPaymentObj);

                    return newEncryptedShared;
                default:
                    return null;
            }
            
        }
    }
}
