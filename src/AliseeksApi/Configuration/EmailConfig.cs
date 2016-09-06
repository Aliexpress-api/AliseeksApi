using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Configuration
{
    public class EmailConfig
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string ElasticmailUsername { get; set; }
        public string ElasticmailSecret { get; set; }
    }
}
