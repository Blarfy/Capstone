namespace DB_Access_Layer.Models
{
    public class EncryptedNote
    {
        public byte[] encryptedTitle { get; set; }
        public byte[] encryptedNote { get; set; }

        public EncryptedNote(byte[] encryptedTitle, byte[] encryptedNote)
        {
            this.encryptedTitle = encryptedTitle;
            this.encryptedNote = encryptedNote;
        }

        public EncryptedNote()
        {
            this.encryptedTitle = new byte[0];
            this.encryptedNote = new byte[0];
        }
    }
}
