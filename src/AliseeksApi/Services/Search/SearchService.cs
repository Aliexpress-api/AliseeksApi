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

namespace AliseeksApi.Services.Search
{
    public class SearchService : ISearchService
    {
        private const int resultsPerPage = 27;

        private readonly IAliexpressService aliexpress;
        private readonly IDHGateService dhgate;
        private readonly IRavenClient raven;
        private readonly ISearchPostgres db;
        private readonly IApplicationCache cache;

        public SearchService(IAliexpressService aliexpress, IDHGateService dhgate, ISearchPostgres db,
            IApplicationCache cache, IRavenClient raven)
        {
            this.aliexpress = aliexpress;
            this.dhgate = dhgate;
            this.raven = raven;
            this.db = db;
            this.cache = cache;
        }

        public async Task CacheItems(SearchCriteria search)
        {

        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = JsonConvert.SerializeObject(search);

            if (await cache.Exists(key))
            {
                var cachedResult = JsonConvert.DeserializeObject<SearchResultOverview>(await cache.GetString(key));

                //Make sure the next page range is cached

                return cachedResult;
            }
            else
            {
                //First page search
                //Send out search to seperate providers
                var ali = aliexpress.SearchItems(search);
                var dh = dhgate.SearchItems(search);

                var results = await Task.WhenAll(ali, dh);

                //Retrieve and organize the search by price
                var items = new SearchResultOverview();
                foreach (var result in results)
                {
                    items.SearchCount += result.SearchCount;
                    items.Items.AddRange(result.Items);
                }

                var entire = new SearchResultOverview()
                {
                    Items = new List<Item>(items.Items),
                    SearchCount = items.SearchCount
                };

                //Sort by price
                items.Items = items.Items.OrderBy(x =>
                {
                    return x.Price.Length > 0 ? x.Price[0] : 1000;
                }).Take(resultsPerPage).ToList();

                //Cache the search and remaining pages
                try
                {
                    await cache.StoreString(key, JsonConvert.SerializeObject(items));
                }
                catch (Exception e)
                {
                    var sentry = new SentryEvent(e);
                    sentry.Message = $"Error when saving to cache: {e.Message}";

                    await raven.CaptureNetCoreEventAsync(sentry);
                }

                //Cache the next pages 2 -> 5 & Store results in Postgres
                int from = search.Page == null ? 2 : (int)search.Page;

                var services = new List<SearchServiceModel>();

                services.Add(new SearchServiceModel()
                {
                    Criteria = search,
                    MaxPage = ali.Result.SearchCount / 47,
                    Page = 1,
                    Type = SearchServiceType.Aliexpress
                });

                services.Add(new SearchServiceModel()
                {
                    Criteria = search,
                    MaxPage = dh.Result.SearchCount / 27,
                    Page = 1,
                    Type = SearchServiceType.DHGate
                });

                var cacheModel = new SearchCacheModel()
                {
                    Criteria = search,
                    Items = entire.Items,
                    PageFrom = from,
                    PageTo = from + 5,
                    Services = services.ToArray()
                };

                AppTask.Forget(async (cache, );
                AppTask.Forget(async () => await storeSearch(JsonConvert.SerializeObject(search), entire.Items));

                //Return the search
                return items;
            }
        }

        private async Task startCacheJob(IApplicationCache cache, ISearchPostgres db, IAliexpressService aliexpress, IDHGateService dhgate,SearchCacheModel model)
        {
            var cacheJob = new SearchCache(cache, db, aliexpress, dhgate);
            await cacheJob.RunCacheJob(model);
        }

        private async Task storeSearch(string searchid, IEnumerable<Item> items)
        {
            await db.AddSearchCacheAsync(searchid, items);
        }
    }
}
