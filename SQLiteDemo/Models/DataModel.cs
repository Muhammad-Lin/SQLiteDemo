using System;

namespace SQLiteDemo.Models
{
    public class DataModel
    {
        public int Id { get; set; }
        public string EncryptedData { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSynced { get; set; }
    }
}
