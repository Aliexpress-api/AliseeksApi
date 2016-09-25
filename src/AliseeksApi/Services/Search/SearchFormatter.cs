using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Services.Search
{
    public class SearchFormatter
    {
        public const int ResultsPerPage = 48;

        public static SearchFormatterResult FormatResults(int page, SearchResultOverview results)
        {
            var formatedResults = new SearchResultOverview()
            {
                Extra = results.Extra,
                Items = results.Items,
                SearchCount = results.SearchCount
            };

            formatedResults.Items = formatedResults.Items
                .OrderBy(x => x.Price != null && x.Price.Length > 0 ? x.Price[0] : decimal.MaxValue)
                .Skip((page - 1) * ResultsPerPage)
                .Take(ResultsPerPage)
                .ToList();

            var ret = new SearchFormatterResult()
            {
                UncertainItems = null,
                Results = formatedResults
            };

            return ret;
        }

        public static SearchFormatterResult ComputeUncertain(SearchResultOverview results)
        {
            var maxes = new Dictionary<string, decimal>();
            foreach (var item in results.Items)
            {
                if (!maxes.ContainsKey(item.Source))
                    maxes.Add(item.Source, item.LotPrice);

                if (maxes[item.Source] < item.LotPrice)
                    maxes[item.Source] = item.LotPrice;
            }

            decimal minMax = maxes.Min(x => x.Value);
            var uncertainItems = results.Items.Where(x => x.LotPrice >= minMax);

            var ret = new SearchFormatterResult()
            {
                UncertainItems = uncertainItems,
                Results = null
            };

            return ret;
        }
    }
}
