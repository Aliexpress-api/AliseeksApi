using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Configuration
{
    public class ShopifyOptions
    {
        public string APIKey { get; set; }
        public string Password { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string SharedSecret { get; set; }
    }
}
