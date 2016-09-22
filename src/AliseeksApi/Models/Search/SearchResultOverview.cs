using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search
{
    public class SearchResultOverview
    {
        public int SearchCount { get; set; }
        public List<Item> Items { get; set; }

        public SearchResultOverview()
        {
            Items = new List<Item>();
        }
    }
}
