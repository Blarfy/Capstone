using Microsoft.AspNetCore.Mvc;
using Npgsql;
using DB_Access_Layer.Models;

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
                    var newNote = (EncryptedNote)EncryptedItem;
                    itemCommand = typeSource.CreateCommand(itemQuery);
                    itemCommand.Parameters.AddWithValue("owner_id", ownerID);
                    itemCommand.Parameters.AddWithValue("note_title", newNote.encryptedTitle);
                    itemCommand.Parameters.AddWithValue("note_text", newNote.encryptedNote);
                    break;
                case "password":
                    itemQuery = "SELECT pass_id FROM passwords WHERE owner_id = @owner_id AND login_location = @login_location AND login_user = @login_user AND login_email = @login_email AND login_password = @login_password";
                    var newPass = (EncryptedPassword)EncryptedItem;
                    itemCommand = typeSource.CreateCommand(itemQuery);
                    itemCommand.Parameters.AddWithValue("owner_id", ownerID);
                    itemCommand.Parameters.AddWithValue("login_location", newPass.encryptedLocation);
                    itemCommand.Parameters.AddWithValue("login_user", newPass.encryptedUsername);
                    itemCommand.Parameters.AddWithValue("login_email", newPass.encryptedEmail);
                    itemCommand.Parameters.AddWithValue("login_password", newPass.encryptedIVPass);
                    break;
                case "payment":
                    itemQuery = "SELECT pay_id FROM payments WHERE owner_id = @owner_id AND pay_name = @pay_name AND pay_card = @pay_card AND pay_cvv = @pay_cvv AND pay_month = @pay_month AND pay_year = @pay_year";
                    var newPayment = (EncryptedPayment)EncryptedItem;
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
            await using var command = dataSource.CreateCommand("SELECT item_id, type FROM shared WHERE sharee_id = @owner_id");
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
                        shared.Add(new EncryptedShared("note", new EncryptedNote
                        {
                            encryptedTitle = noteReader.GetFieldValue<byte[]>(0),
                            encryptedNote = noteReader.GetFieldValue<byte[]>(1)
                        }));
                    }
                }
                else if(reader.GetString(1) == "password")
                {
                    var passCommand = dataSource.CreateCommand("SELECT login_location, login_user, login_email, login_password FROM passwords WHERE pass_id = @pass_id");
                    passCommand.Parameters.AddWithValue("pass_id", reader.GetInt32(0));
                    await using var passReader = await passCommand.ExecuteReaderAsync();
                    if (await passReader.ReadAsync())
                    {
                        shared.Add(new EncryptedShared("password", new EncryptedPassword
                        {
                            encryptedLocation = passReader.GetFieldValue<byte[]>(0),
                            encryptedUsername = passReader.GetFieldValue<byte[]>(1),
                            encryptedEmail = passReader.GetFieldValue<byte[]>(2),
                            encryptedIVPass = passReader.GetFieldValue<byte[]>(3)
                        }));
                    }
                }
                else if (reader.GetString(1) == "payment")
                {
                    var payCommand = dataSource.CreateCommand("SELECT pay_name, pay_card, pay_cvv, pay_month, pay_year FROM payments WHERE pay_id = @pay_id");
                    payCommand.Parameters.AddWithValue("pay_id", reader.GetInt32(0));
                    await using var payReader = await payCommand.ExecuteReaderAsync();
                    if (await payReader.ReadAsync())
                    {
                        shared.Add(new EncryptedShared("payment", new EncryptedPayment
                        {
                            encryptedCardName = payReader.GetFieldValue<byte[]>(0),
                            encryptedCardNumber = payReader.GetFieldValue<byte[]>(1),
                            encryptedCardCVV = payReader.GetFieldValue<byte[]>(2),
                            encryptedCardExpMonth = payReader.GetFieldValue<byte[]>(3),
                            encryptedCardExpYear = payReader.GetFieldValue<byte[]>(4)
                        }));
                    }
                }
            }
            return shared;
        }
    }
}
