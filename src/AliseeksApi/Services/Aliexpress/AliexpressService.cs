using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility;
using AliseeksApi.Storage.Cache;
using Newtonsoft.Json;

namespace AliseeksApi.Services
{
    public class AliexpressService : IAliexpressService
    {
        IHttpService http;
        IApplicationCache cache;

        public AliexpressService(IHttpService http, IApplicationCache cache)
        {
            this.http = http;
            this.cache = cache;
        }

        public async Task<IEnumerable<Item>> SearchItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = JsonConvert.SerializeObject(search);
            if (await cache.Exists(key))
                return JsonConvert.DeserializeObject<IEnumerable<Item>>(await cache.GetString(key));

            var items = await searchItems(search);

            await cache.StoreString(key, JsonConvert.SerializeObject(items));

            int from = search.Page == null ? 2 : (int)search.Page;
            Task.Run(() => cacheSearchPages(search, from, from + 3));

            return items;
        }

        public async Task CacheItems(SearchCriteria search)
        {
            //Check for cached item list
            string key = JsonConvert.SerializeObject(search);
            if (await cache.Exists(key))
                return;

            var items = searchItems(search);

            await cache.StoreString(key, JsonConvert.SerializeObject(items));
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

        async Task<IEnumerable<Item>> searchItems(SearchCriteria search)
        {
            string qs = new AliSearchEncoder().CreateQueryString(search);
            string endpoint = AliexpressEndpoints.SearchUrl + qs;

            var response = await http.Get(endpoint);

            var items = new AliexpressPageDecoder().DecodePage(response);

            return items;
        }
    }
}
