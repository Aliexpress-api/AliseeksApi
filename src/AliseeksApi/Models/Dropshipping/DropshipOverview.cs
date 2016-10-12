using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Dropshipping.Orders;

namespace AliseeksApi.Models.Dropshipping
{
    public class DropshipOverview
    {
        public DropshipAccount Account { get; set; }
        public int IntegrationCount { get; set; }
        public int ProductCount { get; set; }
    }
}
