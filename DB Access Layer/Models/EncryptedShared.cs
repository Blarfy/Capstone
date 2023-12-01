namespace DB_Access_Layer.Models
{
    public class EncryptedShared
    {
        string itemType { get; set; }
        Object item { get; set; }

        public EncryptedShared(string itemType, Object item)
        {
            this.itemType = itemType;
            this.item = item;
        }

        public EncryptedShared()
        {
        }
    }
}
