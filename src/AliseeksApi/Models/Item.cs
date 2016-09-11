using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models
{
    public class Item
    {
        public string Name { get; set; }
        public string ItemID { get; set; }
        public string Link { get; set; }
        public string Currency { get; set; }
        public decimal[] Price { get; set; }
        public decimal LotPrice { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public bool FreeShipping { get; set; }
        public string ImageURL { get; set; }
        public string MobileOnly { get; set; }
        public string StoreName { get; set; }
        public bool TopRatedSeller { get; set; }
        public int Feedback { get; set; }
        public int Orders { get; set; }
        public string Source { get; set; }

        public Item()
        {
            Quantity = 1;
        }
    }
}
