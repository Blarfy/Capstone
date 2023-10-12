namespace Backend.Models
{
    public class DecryptedPassword
    {
        public string plaintextLocation { get; set; }
        public string? plaintextUsername { get; set; }
        public string? plaintextEmail {  get; set; }
        public string plaintextIVPass { get; set;}

        public DecryptedPassword(string plaintextLocation, string? plaintextUsername, string? plaintextEmail, string plaintextIVPass)
        {
            this.plaintextLocation = plaintextLocation;
            this.plaintextUsername = plaintextUsername;
            this.plaintextEmail = plaintextEmail;
            this.plaintextIVPass = plaintextIVPass;
        }
    }
}
