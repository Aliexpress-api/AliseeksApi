using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using Microsoft.AspNetCore.Http;
using AliseeksApi.Utility.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using AliseeksApi.Utility;
using Microsoft.AspNetCore.Routing;

namespace AliseeksApi.Services.Aliexpress
{
    public class AliexpressQueryString
    {
        private const string aliexpressItemLinkTemplate = "item/{itemTitle}/{itemid}.html";

        public string Convert(SearchServiceModel model)
        {
            var qs = new Dictionary<string, string>();
            var search = model.Criteria;

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

            if (search.QuantityMax.HasValue)
                qs.Add("maxQuantity", search.QuantityMax.Value.ToString());

            if (search.QuantityMin.HasValue)
                qs.Add("minQuantity", search.QuantityMin.Value.ToString());

            qs.Add("page", (model.Page).ToString());

            var strings = new List<string>();
            foreach(var key in qs.Keys)
            {
                strings.Add($"{key}={qs[key]}");
            }

            return SearchEndpoints.AliexpressSearchUrl + String.Join("&", strings);
        }

        public SingleItemRequest DecodeItemLink(string link)
        {
            try
            {
                var uri = new Uri(link);
                var routeMatcher = new RouteMatcher();
                var values = routeMatcher.Match(aliexpressItemLinkTemplate, uri.LocalPath);

                var singleItem = new SingleItemRequest()
                {
                    Source = "Aliexpress"
                };

                singleItem.Title = values.ContainsKey("itemTitle") ? values["itemTitle"].ToString() : String.Empty;
                singleItem.ID = values.ContainsKey("itemid") ? values["itemid"].ToString() : String.Empty;
                singleItem.Link = link;

                return singleItem;
            }
            catch(Exception e)
            {
                return new SingleItemRequest()
                {
                    Link = link
                };
            }
        }
    }
}
