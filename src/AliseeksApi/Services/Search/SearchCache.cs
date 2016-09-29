using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.Search;
using AliseeksApi.Storage.Cache;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using AliseeksApi.Services.Aliexpress;
using AliseeksApi.Services.DHGate;
using AliseeksApi.Utility;
using Newtonsoft.Json;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using AliseeksApi.Utility.Extensions;
using Hangfire;

namespace AliseeksApi.Services.Search
{
    public class SearchCache
    {
        private readonly IApplicationCache cache;
        private readonly ISearchPostgres db;
        private readonly IRavenClient raven;
        private readonly WebSearchService[] webServices;

        public SearchCache(IApplicationCache cache, ISearchPostgres db, WebSearchService[] services, IRavenClient raven)
        {
            this.cache = cache;
            this.db = db;
            this.raven = raven;
            this.webServices = services;
        }

        public async Task CacheSearch(SearchCriteria criteria, WebSearchService[] services, SearchResultOverview result)
        {
            var key = RedisKeyConvert.Serialize(criteria);

            //Cache the search & service models
            try
            {
                await cacheItemSearch(criteria, result);
                await cacheServices(services);

            }
            catch (Exception e)
            {
                var sentry = new SentryEvent(e);
                sentry.Message = $"Error when saving to cache: {e.Message}";

                await raven.CaptureNetCoreEventAsync(sentry);
            }
        }

        public void StartCacheJob(SearchCriteria criteria, IEnumerable<Item> items = null, int pages = 4)
        {
            var cacheModel = new SearchCacheModel()
            {
                Criteria = criteria,
                Items = (items != null) ? items : new List<Item>(),
                PageFrom = criteria.Page,
                PageTo = criteria.Page + pages,
                Services = null
            };

            BackgroundJob.Enqueue<SearchCache>(x => x.RunCacheJob(cacheModel));
        }

        public async Task RunCacheJob(SearchCacheModel model)
        {
            List<Task<SearchResultOverview>> searchJobs = new List<Task<SearchResultOverview>>();

            var criteria = JsonConvert.DeserializeObject<SearchCriteria>(JsonConvert.SerializeObject(model.Criteria));
            criteria.Page = model.PageFrom;

            await SearchServiceProvider.RetrieveSearchServices(webServices, cache, criteria);

            var pages = new List<int>();
            for(int from = model.PageFrom; from <= model.PageTo; from++)
            {
                if (!await existItemSearch(criteria, from))
                    pages.Add(from);
            }

            var resultsTask = SearchDispatcher.Search(webServices, pages.ToArray());

            var uncertain = JsonConvert.DeserializeObject<Item[]>(await getUncertain(criteria));

            var results = await resultsTask;

            results.Items.AddRange(uncertain);

            //Cache the pages asked
            int index = 1;
            foreach(int page in pages)
            {
                criteria.Page = page;
                await cacheItemSearch(criteria, SearchFormatter.FormatResults(index++, results).Results);
            }

            var leftOver = results.Items.Skip((results.Items.Count / SearchFormatter.ResultsPerPage) * SearchFormatter.ResultsPerPage);

            //Cache the states of the web services
            await cacheServices(webServices);

            await cacheUncertain(criteria, leftOver);
        }

        async Task cacheServices(WebSearchService[] services)
        {
            foreach (WebSearchService service in services)
            {
                var serviceKey = service.ServiceModel.GetRedisKey();
                await cache.StoreString(serviceKey, JsonConvert.SerializeObject(service.ServiceModel));
            }
        }

        async Task cacheItemSearch(SearchCriteria criteria, SearchResultOverview result)
        {
            var key = RedisKeyConvert.Serialize(criteria);

            await cache.StoreString(key, JsonConvert.SerializeObject(result));
        }

        async Task<bool> existItemSearch(SearchCriteria criteria, int page)
        {
            var crit = JsonConvert.DeserializeObject<SearchCriteria>(JsonConvert.SerializeObject(criteria));

            crit.Page = page;

            var key = RedisKeyConvert.Serialize(crit);

            return await cache.Exists(key);
        }

        async Task cacheUncertain(SearchCriteria criteria, IEnumerable<Item> items)
        {
            var key = $"{RedisKeyConvert.Serialize(criteria, "servicekey")}:uncertain";

            await cache.StoreString(key, JsonConvert.SerializeObject(items));
        }

        async Task<string> getUncertain(SearchCriteria criteria)
        {
            var key = $"{RedisKeyConvert.Serialize(criteria, "servicekey")}:uncertain";

            var items = await cache.GetString(key);

            return items ?? "[]";
        }
    }
}
