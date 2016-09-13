using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility;
using AliseeksApi.Storage.Cache;
using Newtonsoft.Json;
using AliseeksApi.Storage.Postgres.Search;
using System.Text.RegularExpressions;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Storage.Postgres.Logging;
using AliseeksApi.Models.Aliexpress;

namespace AliseeksApi.Services
{
    public class AliexpressService : IAliexpressService
    {
        IHttpService http;
        IApplicationCache cache;
        ISearchPostgres db;

        public AliexpressService(IHttpService http, IApplicationCache cache, ISearchPostgres db)
        {
            this.http = http;
            this.cache = cache;
            this.db = db;
        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = JsonConvert.SerializeObject(search);
            if (await cache.Exists(key))
            {
                AppTask.Forget(async () => await storeSearch(search, new List<Item>()));
                return JsonConvert.DeserializeObject<SearchResultOverview>(await cache.GetString(key));
            }

            var items = await searchItems(search);

            try
            {
                await cache.StoreString(key, JsonConvert.SerializeObject(items));
            }
            catch
            {
                //TODO: Log unable to cache expection
            }

            //Cache the next pages 2 -> 5 & Store results in Postgres
            int from = search.Page == null ? 2 : (int)search.Page;
            AppTask.Forget(async () => await cacheSearchPages(search, from, from + 3));
            AppTask.Forget(async () => await storeSearch(search, items.Items));

            return items;
        }

        public async Task CacheItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = JsonConvert.SerializeObject(search);
            if (await cache.Exists(key))
                return;

            var items = await searchItems(search);

            await cache.StoreString(key, JsonConvert.SerializeObject(items));
        }

        //Store Search in DB
        async Task storeSearch(SearchCriteria criteria, IEnumerable<Item> items)
        {
            var criteriaModel = new SearchHistoryModel()
            {
                User = criteria.Meta == null || criteria.Meta.User == null ? "Guest" : criteria.Meta.User,
                Search = criteria.SearchText,
                Meta = new SearchHistoryModelMeta()
                {
                    Criteria = criteria
                }
            };

            var itemModels = new List<ItemModel>();

            foreach(var item in items)
            {
                itemModels.Add(new ItemModel()
                {
                    ItemID = item.ItemID,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Seller = item.StoreName,
                    Currency = item.Currency,
                    LotPrice = item.LotPrice,
                    Title = item.Name,
                    Source = item.Source
                });
            }

            try
            {
                await db.AddSearchAsync(criteriaModel, itemModels);
            }
            catch
            {
                //TODO: Log this as a warning
            }
        }

        //Store Search in DB if we took the search from the cache
        async Task storeSearchCached(SearchCriteria criteria)
        {
            var criteriaModel = new SearchHistoryModel()
            {
                User = criteria.Meta == null || criteria.Meta.User == null ? "Guest" : criteria.Meta.User,
                Search = criteria.SearchText,
                Meta = new SearchHistoryModelMeta()
                {
                    Criteria = criteria
                }
            };

            try
            {
                await db.AddSearchAsync(criteriaModel, null);
            }
            catch
            {
                //TODO: Log this as a warning
            }
        }

        async Task cacheSearchPages(SearchCriteria criteria, int from, int to)
        {
            //Create clone
            SearchCriteria search = JsonConvert.DeserializeObject<SearchCriteria>(JsonConvert.SerializeObject(criteria));

            //Cache search results for pages FROM to TO
            for(int i = from; i != to; i++)
            {
                search.Page = i;
                string key = JsonConvert.SerializeObject(search);
                if (await cache.Exists(key)) { continue; }
                var items = await searchItems(search);
                await cache.StoreString(JsonConvert.SerializeObject(search), JsonConvert.SerializeObject(items));
            }
        }

        //Function that actually goes out and gets Aliexpress search and converts it
        async Task<SearchResultOverview> searchItems(SearchCriteria search)
        {
            string qs = new AliSearchEncoder().CreateQueryString(search);
            string endpoint = AliexpressEndpoints.SearchUrl + qs;

            var response = await http.Get(endpoint);

            var items = new AliexpressPageDecoder().DecodePage(response);

            return items;
        }
    }
}
