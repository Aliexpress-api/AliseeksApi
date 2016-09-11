using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models
{
    public class ItemModel
    {
        public int ID { get; set; }
        public string ItemID { get; set; }
        public string Title { get; set; }
        public decimal[] Price { get; set; }
        public int Quantity { get; set; }
        public string Seller { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Currency { get; set; }
        public decimal LotPrice { get; set; }
        public ItemModelMeta Meta { get; set; }
    }

    public class ItemModelMeta
    {

    }
}
