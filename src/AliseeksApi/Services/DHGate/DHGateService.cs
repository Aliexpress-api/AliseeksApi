using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility;
using Microsoft.AspNetCore.Http;
using AliseeksApi.Storage.Cache;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Services.DHGate
{
    public class DHGateService : IDHGateService
    {
        private readonly IHttpService http;
        private readonly IApplicationCache cache;
        private readonly IRavenClient raven;

        private string pageKey { get; set; }

        public DHGateService(IHttpService http, IApplicationCache cache, IRavenClient raven)
        {
            this.http = http;
            this.cache = cache;
            this.raven = raven;
        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            return await searchItems(search);
        }

        async Task<SearchResultOverview> searchItems(SearchCriteria search)
        {
            string qs = new DHGateQueryString().Convert(search);
            string pagingKey = "";
            string endpoint = SearchEndpoints.DHGateSearchUrl;

            //Get Paging Key if Page isn't 1
            if (search.Page != 1)
            {
                pagingKey = pageKey;
                endpoint = SearchEndpoints.DHGatePageUrl.Replace("{SEARCH}", search.SearchText.Replace(" ", "+")).Replace("{PAGE}", search.Page.ToString()).Replace("{KEY}", pagingKey);
            }
            else
            {
                endpoint += SearchEndpoints.DHGateSearchUrl + qs;
            }

            var items = new SearchResultOverview();

            try
            {
                var response = await http.Get(endpoint);

                items = new DHGatePageDecoder().DecodePage(response);
            }
            catch(Exception e)
            {
                await raven.CaptureNetCoreEventAsync(e);
            }

            return items;
        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search, string pagekey)
        {
            this.pageKey = pageKey;
            var items = await searchItems(search);

            return items;
        }
    }
}
