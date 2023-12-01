namespace DB_Access_Layer.Models
{
    public class EncryptedPayment
    {
        public byte[] encryptedCardName { get; set; }
        public byte[] encryptedCardNumber { get; set; }
        public byte[] encryptedCardCVV { get; set; }
        public byte[] encryptedCardExpMonth { get; set; }
        public byte[] encryptedCardExpYear { get; set; }

        public EncryptedPayment(byte[] encryptedCardName, byte[] encryptedCardNumber, byte[] encryptedCardCVV, byte[] encryptedCardExpMonth, byte[] encryptedCardExpYear)
        {
            this.encryptedCardName = encryptedCardName;
            this.encryptedCardNumber = encryptedCardNumber;
            this.encryptedCardCVV = encryptedCardCVV;
            this.encryptedCardExpMonth = encryptedCardExpMonth;
            this.encryptedCardExpYear = encryptedCardExpYear;
        }
        public EncryptedPayment()
        {
        }
    }
}
