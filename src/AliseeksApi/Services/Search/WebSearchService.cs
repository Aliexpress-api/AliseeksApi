using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using Hangfire;
using AliseeksApi.Storage.Postgres.Search;

namespace AliseeksApi.Services.Search
{
    public abstract class WebSearchService
    {
        public SearchServiceType ServiceType { get; set; }
        public SearchServiceModel ServiceModel { get; set; }

        public WebSearchService(SearchServiceType type)
        {
            ServiceType = type;
        }

        public abstract Task<SearchResultOverview> SearchItems();
        public virtual Task<SearchResultOverview> SearchItems(SearchServiceModel model)
        {
            this.ServiceModel = model;
            return this.SearchItems();
        }

        public abstract Task<ItemDetail> SearchItem(SingleItemRequest item);

        public virtual void StoreSearchItems(IEnumerable<Item> items)
        {
            var itemModels = new List<ItemModel>();

            foreach(Item item in items)
            {
                var model = new ItemModel();

                model.Currency = item.Currency;
                model.ItemID = item.ItemID;
                model.LotPrice = item.LotPrice;
                model.Price = item.Price;
                model.Quantity = item.Quantity;
                model.Seller = item.StoreName;
                model.Source = item.Source;
                model.Title = item.Name;
                model.Meta = new ItemModelMeta();

                itemModels.Add(model);
            }

            BackgroundJob.Enqueue<ISearchPostgres>(db => db.AddItemsAsync(itemModels));
        }
    }
}
