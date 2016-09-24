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
using SharpRaven.Core.Data;
using SharpRaven.Core;
using AliseeksApi.Services.Aliexpress;

namespace AliseeksApi.Services
{
    public class AliexpressService : IAliexpressService
    {
        IHttpService http;
        ISearchPostgres db;
        IRavenClient raven;

        public AliexpressService(IHttpService http, ISearchPostgres db, IRavenClient raven)
        {
            this.http = http;
            this.db = db;
            this.raven = raven;
        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            var items = await searchItems(search);

            AppTask.Forget(async () => await storeSearch(search, items.Items));

            return items;
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
            catch(Exception e)
            {
                var sentry = new SentryEvent(e)
                {
                    Level = ErrorLevel.Warning,
                    Message = $"Error when saving search results: {e.Message}"
                };
                await raven.CaptureNetCoreEventAsync(sentry);
            }
        }

        //Function that actually goes out and gets Aliexpress search and converts it
        async Task<SearchResultOverview> searchItems(SearchCriteria search)
        {
            string qs = new AliexpressQueryString().Convert(search);
            string endpoint = SearchEndpoints.AliexpressSearchUrl + qs;
            var items = new SearchResultOverview();

            //Add breadcrumb for error monitoring
            var crumb = new Breadcrumb("AliexpressService")
            {
                Message = $"GET {endpoint}",
                Data = new Dictionary<string, string>()
                {
                    { "Aliexpress URL", endpoint }
                }
            };
            raven.AddTrail(crumb);

            try
            {
                var response = await http.Get(endpoint);

                items = new AliexpressPageDecoder().DecodePage(response);
            }
            catch(Exception e)
            {
                var sentry = new SentryEvent(e);
                await raven.CaptureNetCoreEventAsync(sentry);
            }

            return items;
        }

        public Task CacheItems(SearchCriteria search)
        {
            throw new NotImplementedException();
        }
    }
}
