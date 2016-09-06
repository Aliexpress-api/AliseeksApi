using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models
{
    public class Item
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string Price { get; set; }
        public string Unit { get; set; }
        public bool FreeShipping { get; set; }
        public string ImageURL { get; set; }
        public string MobileOnly { get; set; }
        public string StoreName { get; set; }
        public int Feedback { get; set; }
        public int Orders { get; set; }
    }
}
