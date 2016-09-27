using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Search
{
    public class SavedSearchModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public DateTime Created { get; set; }
        public SearchCriteria Criteria { get; set; }
    }
}
