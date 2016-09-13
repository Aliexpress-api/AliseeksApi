using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using AliseeksApi.Models.Search;
using AliseeksApi.Models.Aliexpress;

namespace AliseeksApi.Services
{
    public interface IAliexpressService
    {
        Task<SearchResultOverview> SearchItems(SearchCriteria search);
        Task CacheItems(SearchCriteria search);
    }
}
