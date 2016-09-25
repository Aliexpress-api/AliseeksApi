using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility;
using AliseeksApi.Storage.Cache;
using Newtonsoft.Json;
using AliseeksApi.Services.DHGate;

namespace AliseeksApi.Services.Search
{
    public class SearchServiceProvider
    {
        public static async Task RetrieveSearchServices(WebSearchService[] services, IApplicationCache cache, SearchCriteria criteria, bool allNew = false)
        {
            foreach(var webService in services)
            {
                var type = webService.ServiceType;

                var service = new SearchServiceModel()
                {
                    Criteria = criteria,
                    Type = type
                };

                var key = service.GetRedisKey();

                try
                {
                    if (!allNew && await cache.Exists(key))
                    {
                        webService.ServiceModel = (JsonConvert.DeserializeObject<SearchServiceModel>(await cache.GetString(key)));
                        continue;
                    }
                }
                catch
                {
                    //Something wrong with the service so we'll have to return a new one
                }

                webService.ServiceModel = new SearchServiceModel()
                    {
                        Criteria = criteria,
                        MaxPage = 0,
                        Page = 0,
                        PageKey = "",
                        Type = type,
                        Pages = new int[] { 0 },
                        Searched = false
                    };
            }
        }

        public static async Task SetSearchServices(WebSearchService[] services, IApplicationCache cache)
        {
            foreach(var model in services)
            {
                var key = model.ServiceModel.GetRedisKey();

                await cache.StoreString(key, JsonConvert.SerializeObject(model.ServiceModel));
            }
        }
    }
}
