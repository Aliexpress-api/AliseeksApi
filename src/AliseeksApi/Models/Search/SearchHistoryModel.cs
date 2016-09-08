using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search
{
    public class SearchHistoryModel
    {
        public int ID { get; set; }
        public string Search { get; set; }
        public string User { get; set; }
        public SearchHistoryModelMeta Meta { get; set;}
    }

    public class SearchHistoryModelMeta
    {
        public SearchCriteria Criteria { get; set; }
    }
}
