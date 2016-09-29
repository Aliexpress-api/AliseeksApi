using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search
{
    public class ItemPriceHistoryModel
    {
        public int ID { get; set; }
        public string ItemID { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public ItemPriceModel[] Prices { get; set; }
    }

    public class ItemPriceModel
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
