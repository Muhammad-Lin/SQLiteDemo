using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteDemo
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }

        public EncryptionSettings Encryption { get; set; }
    }
}
