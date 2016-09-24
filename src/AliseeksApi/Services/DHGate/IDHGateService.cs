using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Services.DHGate
{
    public interface IDHGateService
    {
        Task<SearchResultOverview> SearchItems(SearchCriteria search);
        Task<SearchResultOverview> SearchItems(SearchCriteria search, string pagekey);
    }
}
