namespace CryptKeyAPI.Models
{
    public class EncryptedPassword
    {
        public byte[] encryptedLocation { get; set; }
        public byte[]? encryptedUsername { get; set; }
        public byte[]? encryptedEmail {  get; set; }
        public byte[] encryptedIVPass { get; set;}

        public EncryptedPassword(byte[] encryptedLocation, byte[]? encryptedUsername, byte[]? encryptedEmail, byte[] encryptedIVPass)
        {
            this.encryptedLocation = encryptedLocation;
            this.encryptedUsername = encryptedUsername;
            this.encryptedEmail = encryptedEmail;
            this.encryptedIVPass = encryptedIVPass;
        }

        public EncryptedPassword()
        {
            this.encryptedLocation = new byte[0];
            this.encryptedUsername = new byte[0];
            this.encryptedEmail = new byte[0];
            this.encryptedIVPass = new byte[0];
        }
    }
}
