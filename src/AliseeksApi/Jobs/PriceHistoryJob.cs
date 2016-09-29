using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Storage.Postgres.Search;
using AliseeksApi.Models;

namespace AliseeksApi.Jobs
{
    public class PriceHistoryJob
    {
        private readonly ISearchPostgres db;

        public PriceHistoryJob(ISearchPostgres db)
        {
            this.db = db;
        }

        public async Task RunJob()
        {
            for(int i = 0; i != 500; i++)
            {
                await job();
                //System.Threading.Thread.Sleep(100);
            }
        }

        async Task job()
        {
            var newHistory = false;

            var itemHistory = await db.PullTopItemHistoryAsync();

            if (itemHistory == null)
                return;

            var items = await db.SelectItemsAsync(itemHistory);

            var priceHistory = await db.SelectItemPriceHistoryAsync(new ItemPriceHistoryModel()
            {
                ItemID = itemHistory.ItemID,
                Source = itemHistory.Source
            });

            if (priceHistory == null)
            {
                newHistory = true;
                priceHistory = new ItemPriceHistoryModel()
                {
                    ItemID = itemHistory.ItemID,
                    Source = itemHistory.Source,
                    Title = itemHistory.Title
                };
            }

            var pricesModel = new List<ItemPriceModel>();

            foreach (var item in items)
            {
                pricesModel.Add(new ItemPriceModel()
                {
                    Date = item.CreatedDate,
                    Price = (item.Price.Length > 0) ? item.Price[0] : 0
                });
            }

            if (priceHistory.Prices != null)
                pricesModel.AddRange(priceHistory.Prices);

            priceHistory.Prices = pricesModel.ToArray();

            if (newHistory)
                await db.AddItemPriceHistoryAsync(priceHistory);
            else
                await db.UpdateItemPriceHistoryAsync(priceHistory);

            await db.DeleteItemHistoryByItemIDAsync(itemHistory);
        }
    }
}
