using Microsoft.AspNetCore.Mvc;
using Npgsql;
using DB_Access_Layer.Models;

namespace DB_Access_Layer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentsDALController
    {
        static string connString = ControllerHelper.getConnString();
        ControllerHelper controllerHelper = new ControllerHelper();

        [HttpPost("AddPayment")]
        public async Task<string> AddPayment([FromQuery] string email, [FromQuery] string password, [FromBody] EncryptedPayment encPayment)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);

            int ownerID = controllerHelper.GetUserID(email, password).Result;

            // Add payment to DB        
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("INSERT INTO payments (owner_id, pay_name, pay_card, pay_cvv, pay_month, pay_year) VALUES (@owner_id, @pay_name, @pay_card, @pay_cvv, @pay_month, @pay_year)");
            command.Parameters.AddWithValue("owner_id", ownerID);
            command.Parameters.AddWithValue("pay_name", encPayment.encryptedCardName);
            command.Parameters.AddWithValue("pay_card", encPayment.encryptedCardNumber);
            command.Parameters.AddWithValue("pay_cvv", encPayment.encryptedCardCVV);
            command.Parameters.AddWithValue("pay_month", encPayment.encryptedCardExpMonth);
            command.Parameters.AddWithValue("pay_year", encPayment.encryptedCardExpYear);
            await command.ExecuteNonQueryAsync();
            return "Payment added successfully!";
        }

        [HttpGet("GetPayments")]
        public async Task<List<EncryptedPayment>> GetPayments([FromQuery] string email, [FromQuery] string password)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);

            int owner_id = controllerHelper.GetUserID(email, password).Result;

            // Get all payments from DB
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("SELECT * FROM payments WHERE owner_id = @owner_id");
            command.Parameters.AddWithValue("owner_id", owner_id);
            await using var reader = await command.ExecuteReaderAsync();
            List<EncryptedPayment> payments = new List<EncryptedPayment>();
            while (await reader.ReadAsync())
            {
                payments.Add(new EncryptedPayment
                {
                    encryptedCardName = (byte[])reader["pay_name"],
                    encryptedCardNumber = (byte[])reader["pay_card"],
                    encryptedCardCVV = (byte[])reader["pay_cvv"],
                    encryptedCardExpMonth = (byte[])reader["pay_month"],
                    encryptedCardExpYear = (byte[])reader["pay_year"]
                });
            }
            return payments;
        }

        [HttpDelete("DeletePayment")]
        public async Task<string> DeletePayment([FromQuery] string email, [FromQuery] string password, [FromQuery] string cardName)
        {
            email = controllerHelper.DecryptStringAsymmetrically(email);
            password = controllerHelper.DecryptStringAsymmetrically(password);

            int owner_id = controllerHelper.GetUserID(email, password).Result;

            // Delete payment from DB
            await using var dataSource = NpgsqlDataSource.Create(connString);
            await using var command = dataSource.CreateCommand("DELETE FROM payments WHERE owner_id = @owner_id AND pay_name = @pay_name");
            command.Parameters.AddWithValue("owner_id", owner_id);
            command.Parameters.AddWithValue("pay_name", cardName);
            await command.ExecuteNonQueryAsync();
            return "Payment deleted successfully!";
        }
    }
}
