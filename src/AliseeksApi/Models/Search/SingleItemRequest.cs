using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AliseeksApi.Models.Search
{
    public class SingleItemRequest
    {
        public string Title { get; set; }
        public string ID { get; set; }
        public string Source { get; set; }
        public string Link { get; set; }
        public string ShipCountry { get; set; } = "US";

        [JsonIgnore]
        public string Username { get; set; }
    }
}
