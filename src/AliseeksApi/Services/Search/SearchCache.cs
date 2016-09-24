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

namespace AliseeksApi.Services.Search
{
    public class SearchCache
    {
        private const int resultsPerPage = 27;
        private readonly IApplicationCache cache;
        private readonly ISearchPostgres db;
        private readonly IAliexpressService ali;
        private readonly IDHGateService dhgate;

        public SearchCache(IApplicationCache cache, ISearchPostgres db, IAliexpressService ali, IDHGateService dhgate)
        {
            this.cache = cache;
            this.db = db;
            this.ali = ali;
            this.dhgate = dhgate;
        }

        public async Task RunCacheJob(SearchCacheModel model)
        {
            List<Task<SearchResultOverview>> searchJobs = new List<Task<SearchResultOverview>>();
            
            //Go through each search service
            foreach(SearchServiceModel service in model.Services)
            { 
                //Start tasks to update item list from currently cached item page to either max page or page to
                for(int i = service.Page + 1; i <= model.PageTo && i <= service.MaxPage; i++)
                {
                    switch(service.Type)
                    {
                        case SearchServiceType.Aliexpress:
                            searchJobs.Add(ali.SearchItems(model.Criteria));
                            break;

                        case SearchServiceType.DHGate:
                            searchJobs.Add(dhgate.SearchItems(model.Criteria, service.PageKey));
                            break;
                    }                    
                }

                //Update the current service page
                service.Page = model.PageTo;

                //Store the latest search service state
                await cache.StoreString(RedisKeyConvert.Serialize(model.Criteria, "servicekey"), JsonConvert.SerializeObject(service));
            }

            //Retrive current cached items in postgres
            var items = (await db.RetriveSearchCacheAsync(RedisKeyConvert.Serialize(model, schema: "servicekey"))).ToList();
            var newItems = new List<Item>();
            var results = await Task.WhenAll<SearchResultOverview>(searchJobs);

            //Add new items to new list and existing list
            foreach(var result in results)
            {
                items.AddRange(result.Items);
                newItems.AddRange(result.Items);
            }

            //Sort by price
            items = items.OrderBy(x => x.Price.Length > 0 ? x.Price[0] : int.MaxValue).ToList();

            //Sum the distinct search counts
            var searchCount = 0;
            var searchDistinct = results.GroupBy(x => x.SearchCount).Select(grp => grp.First());
            foreach(var s in searchDistinct)
            {
                searchCount += s.SearchCount;
            }

            //Create a shallow copy of the criteria model
            var crit = JsonConvert.DeserializeObject<SearchCriteria>(JsonConvert.SerializeObject(model.Criteria));

            //Cache the pages asked
            for (int from = model.PageFrom; from < model.PageTo; from++)
            {
                var result = new SearchResultOverview()
                {
                    SearchCount = searchCount,
                    Items = items.Skip(from * resultsPerPage).Take(resultsPerPage).ToList()
                };

                crit.Page = from;

                await cache.StoreString(JsonConvert.SerializeObject(crit), JsonConvert.SerializeObject(result));
            }

            //Store the new items into the Postgres cache
            await db.AddSearchCacheAsync(RedisKeyConvert.Serialize(model.Criteria, "servicekey"), newItems);
        }
    }
}
