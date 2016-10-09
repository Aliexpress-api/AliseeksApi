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
        public DropshipOrder[] Orders { get; set; }
        public DropshipItem[] Items { get; set; }
    }
}
