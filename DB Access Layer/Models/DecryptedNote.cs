namespace DB_Access_Layer.Models
{
    public class DecryptedNote
    {
        public string plaintextTitle { get; set; }
        public string plaintextNote { get; set; }

        public DecryptedNote(string plaintextTitle, string plaintextNote)
        {
            this.plaintextTitle = plaintextTitle;
            this.plaintextNote = plaintextNote;
        }
    }
}
