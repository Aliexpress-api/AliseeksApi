using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Models.User
{
    public class UserOverviewModel
    {
        public string Username { get; set; }
        public SavedSearchModel[] SavedSearches { get; set; }
    }
}
