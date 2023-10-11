namespace DB_Access_Layer.Models
{
    public class EncryptedPassword
    {
        public byte[] encryptedLocation { get; set; }
        public byte[]? encryptedUsername { get; set; }
        public byte[]? encryptedEmail {  get; set; }
        public byte[] encryptedIVPass { get; set;}
    }
}
