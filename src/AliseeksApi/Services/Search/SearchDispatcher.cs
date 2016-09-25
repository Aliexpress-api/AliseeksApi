using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Services.Search
{
    public class SearchDispatcher
    {
        public static async Task<SearchResultOverview> Search(WebSearchService[] services)
        {
            var aggregatedResults = new SearchResultOverview();

            var resultTasks = new List<Task<SearchResultOverview>>();

            foreach (WebSearchService service in services)
            {
                resultTasks.Add(service.SearchItems());
            }

            var results = await Task.WhenAll(resultTasks);

            foreach(var result in results)
            {
                aggregatedResults.SearchCount += result.SearchCount;
                aggregatedResults.Items.AddRange(result.Items);
            }

            return aggregatedResults;
        }

        public static async Task<SearchResultOverview> Search(WebSearchService[] services, int[] pages)
        {

            foreach(WebSearchService service in services)
            {
                service.ServiceModel.Pages = pages;
            }
            
            return await Search(services);
        }
    }
}
