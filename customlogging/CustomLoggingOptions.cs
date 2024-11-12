using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace customlogging
{
    public class CustomLoggingOptions
    {
        public string ApplicationName { get; set; }
        public string ServiceName { get; set; }
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
    }
}
