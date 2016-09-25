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
        public int Pages
        {
            get
            {
                return (Items != null && Items.Count > 0) ? SearchCount / Items.Count : 0;
            }
        }
        public Dictionary<string, string> Extra { get; set; }

        public SearchResultOverview()
        {
            Items = new List<Item>();
            Extra = new Dictionary<string, string>();
        }

        public SearchServiceModel ToSearchServiceModel(SearchCriteria criteria, SearchServiceType type, string pagekey = null)
        {
            return new SearchServiceModel()
            {
                Criteria = criteria,
                Page = criteria.Page,
                MaxPage = Pages,
                PageKey = pagekey,
                Type = type,
                Searched = true
            };
        }
    }
}
