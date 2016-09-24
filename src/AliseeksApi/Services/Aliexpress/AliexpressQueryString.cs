﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using Microsoft.AspNetCore.Http;
using AliseeksApi.Utility.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace AliseeksApi.Services.Aliexpress
{
    public class AliexpressQueryString
    {
        public string Convert(SearchCriteria search)
        {
            var qs = new Dictionary<string, string>();

            qs.Add("SearchText", search.SearchText.Replace(" ", "+"));

            if(search.PriceFrom.HasValue)
                qs.Add("minPrice", search.PriceFrom.ToString());

            if (search.PriceTo.HasValue)
                qs.Add("maxPrice", search.PriceTo.ToString());

            if (search.ShipFrom != null)
                qs.Add("shipFromCountry", search.ShipFrom);

            if (search.ShipTo != null)
                qs.Add("shipCountry", search.ShipTo);

            if (search.FreeShipping.HasValue)
                qs.Add("isFreeShip", search.FreeShipping.Value.YesOrNo());

            if (search.SaleItems.HasValue)
                qs.Add("isOnSale", search.SaleItems.Value.YesOrNo());

            if (search.AppOnly.HasValue)
                qs.Add("isMobileExclusive", search.AppOnly.Value.YesOrNo());

            if (search.Page.HasValue)
                qs.Add("page", search.Page.Value.ToString());

            var strings = new List<string>();
            foreach(var key in qs.Keys)
            {
                strings.Add($"{key}={qs[key]}");
            }

            return String.Join("&", strings);
        }
    }
}
