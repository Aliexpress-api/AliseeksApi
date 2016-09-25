using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Services.DHGate;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using AliseeksApi.Models.Search;
using AliseeksApi.Storage.Postgres.Search;
using AliseeksApi.Storage.Cache;
using Newtonsoft.Json;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Models;
using Hangfire;
using AliseeksApi.Utility;

namespace AliseeksApi.Services.Search
{
    public class SearchService : ISearchService
    {
        private const int resultsPerPage = 27;

        private readonly WebSearchService[] services;
        private readonly IRavenClient raven;
        private readonly ISearchPostgres db;
        private readonly IApplicationCache cache;
        private readonly SearchCache searchCache;

        public SearchService(WebSearchService[] services, ISearchPostgres db,
            IApplicationCache cache, IRavenClient raven, SearchCache searchCache)
        {
            this.raven = raven;
            this.db = db;
            this.cache = cache;
            this.services = services;
            this.searchCache = searchCache;
        }

        public async Task CacheItems(SearchCriteria search)
        {

        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = RedisKeyConvert.Serialize(search);

            SearchResultOverview result = null;

            if (await cache.Exists(key))
            {
                var cachedResult = JsonConvert.DeserializeObject<SearchResultOverview>(await cache.GetString(key));

                result = cachedResult;
            }
            else
            {
                await SearchServiceProvider.RetrieveSearchServices(services, cache, search, allNew: true);

                var results = await SearchDispatcher.Search(services, new int[] { search.Page });

                var formattedResults = SearchFormatter.FormatResults(0, results);
                                                               
                await searchCache.CacheSearch(search, services, formattedResults.Results);

                result = formattedResults.Results;
            }

            searchCache.StartCacheJob(search, null);

            return result;
        }
    }
}
