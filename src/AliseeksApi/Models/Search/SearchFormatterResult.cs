using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search
{
    public class SearchFormatterResult
    {
        public SearchResultOverview Results { get; set; }
        public IEnumerable<Item> UncertainItems { get; set; }
    }
}
