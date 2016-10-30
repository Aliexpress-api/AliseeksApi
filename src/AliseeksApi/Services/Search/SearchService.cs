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
        private const int maxSavedSearchesPerUser = 20;

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

        public void CacheItems(SearchCriteria search)
        {
            searchCache.StartCacheJob(search, null, 0);
        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = RedisKeyConvert.Serialize(search);

            SearchResultOverview result = null;

            try
            {
                storeSearchHistory(search);

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
            }
            catch(Exception e)
            {
               await raven.CaptureNetCoreEventAsync(e);
            }

            return result;
        }

        public async Task<SavedSearchModel> SaveSearch(SavedSearchModel model)
        {
            //Only allow certain numbers of saves per user
            if(await db.CountSavedSearchs(model.Username) > maxSavedSearchesPerUser)
            {
                var saves = await db.SelectSaveSearches(model.Username);
                await db.DeleteSavedSearch(model.Username, saves.FirstOrDefault().ID);
            }

            await db.AddSavedSearchAsync(model);
            return model;
        }

<<<<<<< HEAD
        public async Task DeleteSearch(SavedSearchModel model)
        {
            await db.DeleteSavedSearchAsync(model);
=======
        public async Task DeleteSearch(int id, string username)
        {
            await db.DeleteSavedSearch(username, id);
        }

        public async Task<ItemPriceHistoryModel[]> GetPriceHistories(PriceHistoryRequestModel[] models)
        {
            return await db.SelectItemPriceHistoriesAsync(models);
        }

        public async Task<ItemDetail> ItemSearch(SingleItemRequest model)
        {
            var detail = new ItemDetail();

            switch(model.Source)
            {
                case "Aliexpress":
                    detail = await services.First(x => x.ServiceType == SearchServiceType.Aliexpress).SearchItem(model);
                    break;
            }

            return detail;
>>>>>>> refs/remotes/origin/feature/itempricetable
        }

        void storeSearchHistory(SearchCriteria search)
        {
            var model = new SearchHistoryModel()
            {
                Search = search.SearchText,
                User = search.Meta.User,
                Meta = new SearchHistoryModelMeta()
                {
                    Criteria = search
                }
            };

            BackgroundJob.Enqueue<ISearchPostgres>(db => db.AddSearchAsync(model));
        }
    }
}
