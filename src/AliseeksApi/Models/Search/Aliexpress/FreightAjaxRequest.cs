using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search.Aliexpress
{
    public class FreightAjaxRequest
    {
        public string ProductID { get; set; }
        public string CurrencyCode { get; set; }
        public string Country { get; set; }
    }
}
