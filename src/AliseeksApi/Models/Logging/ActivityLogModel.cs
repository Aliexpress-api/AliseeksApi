using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Logging
{
    public class ActivityLogModel
    {
        public string IP { get; set; }
        public string User { get; set; }
        public string Request { get; set; }
    }
}
