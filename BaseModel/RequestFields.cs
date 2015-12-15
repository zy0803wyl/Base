using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseModel
{
    public class RequestFields
    {
        public string id { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string type { get; set; }
        public string values { get; set; }
        public Nullable<int> inuse { get; set; }
    }
}
