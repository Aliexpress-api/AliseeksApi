using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Utility;

namespace AliseeksApi.Services.DHGate
{
    public class DHGateQueryString
    {
        public string Convert(SearchCriteria search)
        {
            var qs = new Dictionary<string, string>();

            qs.Add("searchkey", search.SearchText.Replace(" ", "+"));

            var priceRange = new List<double>();
            if (search.PriceFrom.HasValue)
                priceRange.Add(search.PriceFrom.Value);

            if (search.PriceTo.HasValue)
                priceRange.Add(search.PriceTo.Value);

            if (priceRange.Count > 0)
                qs.Add("finfo", String.Join("-", priceRange));

            if (search.ShipTo != null)
                qs.Add("shipcountry", search.ShipTo);

            if (search.FreeShipping.HasValue)
                qs.Add("freeshipping", search.FreeShipping.Value.OneOrZero());

            var strings = new List<string>();
            foreach (var key in qs.Keys)
            {
                strings.Add($"{key}={qs[key]}");
            }

            return SearchEndpoints.DHGateSearchUrl + String.Join("&", strings);
        }

        public string ConvertWithPage(SearchServiceModel search, string pagekey)
        {
            return SearchEndpoints.DHGatePageUrl.Replace("{SEARCH}", search.Criteria.SearchText.Replace(" ", "+")).Replace("{PAGE}", (search.Page + 1).ToString()).Replace("{KEY}", pagekey);
        }
    }
}
