using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search
{
    public class SearchCacheModel
    {
        public SearchCriteria Criteria { get; set; }
        public SearchServiceModel[] Services { get; set; }
        public IEnumerable<Item> Items { get; set; }
        public int PageFrom { get; set; }
        public int PageTo { get; set; }
    }
}
