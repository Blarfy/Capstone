namespace CryptKeyAPI.Models
{
    public class DecryptedPayment
    {
        public string plaintextCardName { get; set; }
        public string plaintextCardNumber { get; set; }
        public string plaintextCardCVV { get; set; }
        public string plaintextCardExpMonth { get; set; }
        public string plaintextCardExpYear { get; set; }

        public DecryptedPayment(string plaintextCardName, string plaintextCardNumber, string plaintextCardCVV, string plaintextCardExpMonth, string plaintextCardExpYear)
        {
            this.plaintextCardName = plaintextCardName;
            this.plaintextCardNumber = plaintextCardNumber;
            this.plaintextCardCVV = plaintextCardCVV;
            this.plaintextCardExpMonth = plaintextCardExpMonth;
            this.plaintextCardExpYear = plaintextCardExpYear;
        }

        public DecryptedPayment()
        {
        }
    }
}
