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
            return;
        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = JsonConvert.SerializeObject(search);
            if (await cache.Exists(key))
            {
                //AppTask.Forget(async () => await storeSearch(search, new List<Item>()));
                return JsonConvert.DeserializeObject<SearchResultOverview>(await cache.GetString(key));
            }

            //Send out search to seperate providers
            var ali = aliexpress.SearchItems(search);
            var dh = dhgate.SearchItems(search);

            var results = await Task.WhenAll(ali, dh);

            //Retrieve and organize the search by price
            var items = new SearchResultOverview();
            foreach(var result in results)
            {
                items.SearchCount += result.SearchCount;
                items.Items.AddRange(result.Items);
            }

            //Sort by price
            items.Items = items.Items.OrderBy(x => {
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
            //AppTask.Forget(async () => await cacheSearchPages(search, from, from + 3));
            //AppTask.Forget(async () => await storeSearch(search, items.Items));

            //Return the search
            return items;
        }
    }
}
