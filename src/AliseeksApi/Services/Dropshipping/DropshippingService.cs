using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.Dropshipping;
using AliseeksApi.Models.Shopify;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Services.Dropshipping.Shopify;
using AliseeksApi.Services.Search;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;

namespace AliseeksApi.Services.Dropshipping
{
    public class DropshippingService
    {
        private readonly ShopifyService shopify;
        private readonly ISearchService search;
        private readonly DropshipItemsPostgres db;

        public DropshippingService(ShopifyService shopify, ISearchService search, DropshipItemsPostgres db)
        {
            this.shopify = shopify;
            this.search = search;
            this.db = db;
        }

        public async Task DropshipProduct(SingleItemRequest item)
        {
            var detail = await search.ItemSearch(new ItemDetail()
            {
                Source = item.Source,
                ItemID = item.ID,
                Name = item.Title
            });

            var shopifyModel = new ShopifyProductModel()
            {
                BodyHtml = detail.Description,
                Title = detail.Name,
                Vendor = "Me",
                ProductType = detail.Source,

                Variants = new List<object>()
                {
                    new ShopifyVariant().Price((detail.Price + detail.ShippingPrice) * (decimal)1.05)
                    .InventoryQuantity(detail.Quantity - 100 > 0 ? detail.Quantity - 100 : 0)
                    .InventoryPolicy(InventoryPolicy.Deny)
                    .InventoryManagement(InventoryManagement.Shopify)
                    .RequireShipping(true)
                    .Taxable(false)
                    .Build()
                }
            };

            var images = new List<ShopifyImageType>();
            foreach(var image in detail.ImageUrls)
            {
                images.Add(new ShopifyImageType()
                {
                    Src = image
                });
            }

            shopifyModel.Images = images.ToArray();

            //var products = shopify.GetProducts();
            var product = await shopify.AddProduct(shopifyModel);

            await db.Save(new DropshipItemModel()
            {
                Listing = "Shopify",
                ListingID = product.ID,
                Source = detail.Source,
                ItemID = detail.ItemID
            });
        }
    }
}
