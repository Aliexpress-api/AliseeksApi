using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models
{
    public class ItemDetail
    {
        public string Name { get; set; }
        public string ItemID { get; set; }
        public string Link { get; set; }
        public string Currency { get; set; }
        public decimal Price { get; set; }
        public decimal LotPrice { get; set; }
        public decimal ShippingPrice { get; set; }
        public string Unit { get; set; }
        public string ShippingType { get; set; }
        public int Quantity { get; set; }
        public string[] ImageUrls { get; set; }
        public string StoreName { get; set; }
        public int Feedback { get; set; }
        public int Orders { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
    }
}
