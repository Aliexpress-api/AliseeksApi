using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using HtmlAgilityPack;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Utility
{
    public class AliexpressPageDecoder
    {
        public List<Item> DecodePage(string html)
        {
            List<Item> items = new List<Item>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //select element holding items
            var itemElements = doc.DocumentNode.Descendants().First(p => p.Id.Contains("page")).Descendants().Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "item");

            //Cycle through all the elements
            foreach (var element in itemElements)
            {
                var item = decodeItemnode(element);

                items.Add(item);
            }

            return items;
        }

        Item decodeItemnode(HtmlNode node)
        {
            Item item = new Item();

            //Get name node
            var nameElement = node.GetNodesByCssClass("history-item product");
            var imageElement = node.GetNodesByCssClass("picCore");
            var priceElement = node.GetNodesByCssClass("price-m");
            var freesElement = node.GetNodesByCssClass("free-s");
            var mobileElement = node.GetNodesByCssClass("mobile-exclusive");
            var storeElement = node.GetNodesByCssClass("store ");
            var feedbackElement = node.GetNodesByCssClass("rate-num");
            var ordersElement = node.GetNodesByCssClass("order-num-a");

            if (nameElement != null)
            {
                item.Name = nameElement.Attributes["title"].Value;
                item.Link = nameElement.Attributes["href"].Value;
            }

            if (imageElement != null)
            {
                item.ImageURL = imageElement != null ? imageElement.Attributes.First(p => p.Name.Contains("src")).Value : "";
            }

            if (priceElement != null && priceElement.HasChildNodes)
            {
                item.Price = priceElement.Descendants().First(p => p.Attributes.Contains("itemprop") && p.Attributes["itemprop"].Value.Contains("price")).InnerText;
                item.Unit = priceElement.Descendants().First(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("unit")).InnerText;
            }

            if (freesElement != null)
            {
                item.FreeShipping = (freesElement != null);
            }

            if (mobileElement != null && mobileElement.ChildNodes[0] != null)
            {
                item.MobileOnly = mobileElement.ChildNodes[1].InnerText;
            }

            if (storeElement != null)
            {
                item.StoreName = storeElement.Attributes["title"].Value;
            }

            if (feedbackElement != null)
            {
                string feedback = Regex.Match(feedbackElement.Attributes["title"].Value, @"\(([^\)]+)\)").Value;
                if(feedback != "")
                {
                    item.Feedback = Convert.ToInt32(feedback.Replace("(", "").Replace(")", ""));
                }
            }

            if (ordersElement != null)
            {
                string orders = Regex.Match(ordersElement.InnerText, @"\(([^\)]+)\)").Value;
                if(orders != "")
                {
                    item.Orders = Convert.ToInt32(orders.Replace("(", "").Replace(")", ""));
                }
            }

            return item;
        }
    }
}
