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
using AliseeksApi.Services.Search;
using AliseeksApi.Models;

namespace AliseeksApi.Services.DHGate
{
    public class DHGateService : WebSearchService, IDHGateService
    {
        public const int ResultsPerPage = 27;

        private readonly IHttpService http;
        private readonly IApplicationCache cache;
        private readonly IRavenClient raven;

        private string pageKey { get; set; }

        public DHGateService(IHttpService http, IApplicationCache cache, IRavenClient raven)
            :base(SearchServiceType.DHGate)
        {
            this.http = http;
            this.cache = cache;
            this.raven = raven;
        }

        public override async Task<SearchResultOverview> SearchItems()
        {
            var search = this.ServiceModel;
            string endpoint = String.Empty;
            string pagingKey = search.PageKey;

            var resultTasks = new List<Task<string>>();

            SearchResultOverview overallResult = null;

            //Get the first page if we have no pagekey
            if(this.ServiceModel.PageKey == null || this.ServiceModel.PageKey == String.Empty)
            {
                endpoint = new DHGateQueryString().Convert(search.Criteria);

                try
                {
                    var response = await http.Get(endpoint);

                    overallResult = new DHGatePageDecoder().DecodePage(response);

                    this.ServiceModel.PopulateState(overallResult);
                }
                catch (Exception e)
                {
                    await raven.CaptureNetCoreEventAsync(e);
                    return new SearchResultOverview();
                }
            }

            foreach (int page in search.Pages)
            {
                if (page < search.Page)
                    continue;

                if (page == 0)
                    continue;

                search.Page = page;
                endpoint = new DHGateQueryString().ConvertWithPage(search, this.ServiceModel.PageKey);

                try
                {
                    resultTasks.Add(http.Get(endpoint));
                }
                catch (Exception e)
                {
                    await raven.CaptureNetCoreEventAsync(e);
                }
            }

            var responses = await Task.WhenAll(resultTasks);

            foreach(var response in responses)
            {
                var oneResult = new DHGatePageDecoder().DecodePage(response);
                if (overallResult == null)
                {
                    overallResult = oneResult;
                    this.ServiceModel.PopulateState(oneResult);
                }
                else
                    overallResult.Items.AddRange(oneResult.Items);
            }

            return (overallResult != null) ? overallResult : new SearchResultOverview();
        }

        public override async Task<ItemDetail> SearchItem(ItemDetail item)
        {
            return item;
        }
    }
}
