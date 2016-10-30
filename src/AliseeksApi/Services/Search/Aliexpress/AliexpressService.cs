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
using AliseeksApi.Services.Search;
using AliseeksApi.Services.Aliexpress;
using AliseeksApi.Models.Search.Aliexpress;
using Hangfire;
using Microsoft.AspNetCore.Routing;
using AliseeksApi.Exceptions;

namespace AliseeksApi.Services
{
    public class AliexpressService : WebSearchService, IAliexpressService
    {
        public const int ResultsPerPage = 40;

        IHttpService http;
        ISearchPostgres db;
        IRavenClient raven;

        public AliexpressService(IHttpService http, ISearchPostgres db, IRavenClient raven)
            :base(SearchServiceType.Aliexpress)
        {
            this.http = http;
            this.db = db;
            this.raven = raven;
        }

        public override async Task<SearchResultOverview> SearchItems()
        {
            var items = await searchItems(this.ServiceModel);

            return items;
        }

        public override async Task<ItemDetail> SearchItem(SingleItemRequest item)
        {
            if(item.ID.EmptyOrNull() && item.Title.EmptyOrNull())
            {
                if (item.Link.EmptyOrNull())
                    throw new AllowedException("ID, Title and Link cannot be empty");

                item = new AliexpressQueryString().DecodeItemLink(item.Link);

                //Means there was a translation error in the URI format
                if (item.ID.EmptyOrNull() & item.Title.EmptyOrNull())
                    return null;
            }

            string endpoint = SearchEndpoints.AliexpressItemUrl(item.Title, item.ID);

            var response = "";
            ItemDetail detail = null;

            try
            {
                var responseMessage = await http.Get(endpoint);
                response = await responseMessage.Content.ReadAsStringAsync();
            }
            catch(Exception e)
            {

            }

            try
            {
                detail = new AliexpressPageDecoder().ScrapeSingleItem(response);

                var freights = await CalculateFreight(new FreightAjaxRequest()
                {
                    ProductID = item.ID,
                    Country = item.ShipCountry,
                    CurrencyCode = "USD"
                });

                if(freights.Length > 0)
                {
                    var def = freights.FirstOrDefault(x => x.IsDefault == true);
                    detail.ShippingPrice = def.LocalPrice;
                    detail.ShippingType = def.CompanyDisplayName;
                }
            }
            catch(Exception e)
            {
                await raven.CaptureNetCoreEventAsync(e);
            }

            return detail;
        }

        public async Task<FreightAjax[]> CalculateFreight(FreightAjaxRequest model)
        {
            string endpoint = SearchEndpoints.AliexpressFreight(model.ProductID, model.CurrencyCode, model.Country);

            var response = await (await http.Get(endpoint)).Content.ReadAsStringAsync();

            try
            {
                var freights = new AliexpressPageDecoder().ScrapeFreightResults(response);

                return freights;
            }
            catch(Exception e)
            {
                await raven.CaptureNetCoreEventAsync(e);
            }

            return null;
        }

        //Function that actually goes out and gets Aliexpress search and converts it
        async Task<SearchResultOverview> searchItems(SearchServiceModel search)
        {
            var resultTasks = new List<Task<string>>();

            SearchResultOverview results = null;

            foreach (var page in search.Pages)
            {
                search.Page = page;
                string endpoint = new AliexpressQueryString().Convert(search);

                #region Error Tracking
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
                #endregion

                try
                {
                    resultTasks.Add((await http.Get(endpoint)).Content.ReadAsStringAsync());
                }
                catch (Exception e)
                {
                    var sentry = new SentryEvent(e);
                    await raven.CaptureNetCoreEventAsync(sentry);
                }
            }

            var responses = await Task.WhenAll(resultTasks);

            foreach(var response in responses)
            {
                try
                {
                    var result = new AliexpressPageDecoder().ScrapeSearchResults(response);

                    if (results == null)
                    {
                        results = result;
                        this.ServiceModel.PopulateState(results);
                    }
                    else
                    {
                        results.Items.AddRange(result.Items);
                    }
                }
                catch(Exception e)
                {
                    var sentry = new SentryEvent(e);
                    await raven.CaptureNetCoreEventAsync(sentry);
                }
            }

            if(results != null && results.Items.Count > 0)
                StoreSearchItems(results.Items);

            return (results != null) ? results : new SearchResultOverview();
        }

        public Task CacheItems(SearchCriteria search)
        {
            throw new NotImplementedException();
        }

        private SingleItemRequest ConvertAliexpressURL(SingleItemRequest item)
        {
            var link = item.Link;

            return new AliexpressQueryString().DecodeItemLink(link);
        }
    }
}
