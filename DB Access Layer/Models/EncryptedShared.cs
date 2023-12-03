namespace DB_Access_Layer.Models
{
    public class EncryptedShared
    {
        public string itemType { get; set; }
        public Object item { get; set; }

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
