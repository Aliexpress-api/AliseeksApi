using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Services.DHGate
{
    public class DHGateQueryString
    {
        public string Convert(SearchCriteria search)
        {
            var qs = new QueryString();

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

            return qs.ToUriComponent().Substring(1);
        }
    }
}
