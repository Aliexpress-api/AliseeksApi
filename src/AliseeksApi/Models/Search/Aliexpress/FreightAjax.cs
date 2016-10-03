using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search.Aliexpress
{
    public class FreightAjax
    {
        public int ProcessingTime { get; set; }
        public int CommitDay { get; set; }
        public string Company { get; set; }
        public string CompanyDisplayName { get; set; }
        public string Currency { get; set; }
        public string LocalCurrency { get; set; }
        public decimal LocalPrice { get; set; }
        public bool IsDefault { get; set; }
    }
}
