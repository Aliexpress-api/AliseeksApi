using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using HtmlAgilityPack;
using AliseeksApi.Utility.Extensions;
using System.Text.RegularExpressions;

namespace AliseeksApi.Services.DHGate
{
    public class DHGatePageDecoder
    {
        //Matches all decimal numbers with OPTIONAL decimal point
        const string priceStringRegex = @"\d+\.?\d*";

        const string feedbackStringRegex = @"\d+\.?\d*";

        const string quantityStringRegex = @"\d+\.?\d*";



        public SearchResultOverview DecodePage(string html)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var searchResultOverview = extractResultsOverview(doc.DocumentNode);

                //select element holding items
                var itemElements = doc.DocumentNode.Descendants().First(p => p.Id.Contains("page")).Descendants().Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("listitem"));

                //Cycle through all the elements
                foreach (var element in itemElements)
                {
                    var item = decodeItemnode(element);

                    searchResultOverview.Items.Add(item);
                }

                return searchResultOverview;
            }
            catch(Exception e)
            {
                //TODO: Log this as a warning, high priority, make sure that we aren't returning nothing for accurate searches
                return new SearchResultOverview();
            }
        }

        SearchResultOverview extractResultsOverview(HtmlNode node)
        {
            var searchResult = new SearchResultOverview();

            /*
			<p class="result-overview">
				<strong class="search-count">132,700</strong> Results
			</p>              
			*/
            var searchCountElement = node.GetNodesByCssClass("searchresult-note").GetNodesByCssClass("num");

            if (searchCountElement != null)
            {
                var searchResultInt = 0;
                var searchResultString = searchCountElement.InnerText.Replace(",", String.Empty); //Remove commas
                if (!int.TryParse(searchResultString, out searchResultInt))
                {
                    //TODO: Log this as a warning
                }

                searchResult.SearchCount = searchResultInt;
            }

            return searchResult;
        }

        //Where the magic happens
        Item decodeItemnode(HtmlNode node)
        {
            Item item = new Item()
            {
                Source = "DHGate"
            };

            //Get name node

            var nameElement = node.GetNodesByCssClass("subject");

            var imageElement = node.GetNodesByCssClass("lthumbnail");

            var priceElement = node.GetNodesByCssClass("pricewrap");

            //<strong class="free-s">Free Shipping</strong>
            var freesElement = node.GetNodesByCssClass("free");

            //<a href="//www.aliexpress.com/store/1041550" title="Just Beading" class="store "   >Just Beading</a>
            var storeElement = node.GetNodesByCssClass("seller");

            //<span class="score-icon-new score-level-21" id="score20" feedBackScore="784" sellerPositiveFeedbackPercentage="97.8"></span>
            var feedbackElement = node.Descendants().FirstOrDefault(x => x.Attributes.Contains("feedback"));

            /*
			<span rel="nofollow" class="order-num">
				 <a class="order-num-a " href="//www.aliexpress.com/item/Wholesale-Factory-Price-Approx-2200pcs-lot-Jewelry-Head-Pin-Findings-Gold-Plated-45MM-Eye-Pin-for/32263213196.html?ws_ab_test=searchweb201556_0,searchweb201602_1_10065_10068_10067_112_10069_110_111_10017_109_108_10060_10061_10062_10057_10056_10055_10054_10059_10058_10073_10070_10052_10053_10050_10051,searchweb201603_1&amp;btsid=8eae686d-1bf4-4944-acb8-ee998e6a1998#thf" rel="nofollow" >
					<em title="Total Orders"> Orders (0)</em>
				 </a>
			</span>
			*/
            var ordersElement = node.GetNodesByCssClass("min");

            //<span title="Top-rated Seller" class="top-rated-seller"></span>
            var topRatedSellerElement = node.GetNodesByCssClass("m-word");

            //<input class="atc-product-id" type="hidden" value="32493137804" />
            var itemIdElement = node.GetNodesByCssClass("cart");

            /*
			 <dd class="price">
				<span class="value">US $0.15</span> <span class="separator">/</span>
				<span class="unit"> lot</span> via Fedex IE     
			 </dd>
			 */
            var shippingPriceElement = node.Descendants().FirstOrDefault(x => x.Name == "dd" && x.Attributes.Contains("class") && x.Attributes["class"].Value == "price");

            if (nameElement != null)
            {
                item.Name = nameElement.InnerText;
                item.Link = nameElement.Attributes["href"].Value;
            }

            if (imageElement != null)
            {
                //Check if null, if null add a String Empty
                item.ImageURL = imageElement != null ? imageElement.Attributes.First(p => p.Name.Contains("src")).Value : String.Empty;
            }

            if (priceElement != null && priceElement.HasChildNodes)
            {
                //Get the first element that has [itemprop=price]
                var priceString = priceElement.Descendants().First(p => p.Name == "span").InnerText;
                var priceRegex = Regex.Matches(priceString, priceStringRegex);

                item.Price = new decimal[priceRegex.Count];
                for (int i = 0; i != priceRegex.Count; i++)
                {
                    if (!decimal.TryParse(priceRegex[i].ToString(), out item.Price[i]))
                    {
                        //TODO: Log this as a warning
                    }
                }

                //Extract currency from beginning of priceString
                for (int i = 0; i != priceString.Length; i++)
                {
                    if (Char.IsNumber(priceString[i])) { break; }
                    item.Currency += priceString[i];
                }
                item.Currency = item.Currency.Trim();

                item.Unit = priceElement.InnerText;
            }

            item.FreeShipping = (freesElement != null);

            if (storeElement != null)
            {
                item.StoreName = storeElement.Descendants().First(p => p.Name == "a").InnerText;
            }

            if (feedbackElement != null)
            {
                var feedbackString = feedbackElement.Descendants().First(p => p.Name == "span").InnerText;
                var priceRegex = Regex.Match(feedbackString, feedbackStringRegex);
                var feedbackInt = 0;

                if (int.TryParse(priceRegex.Value, out feedbackInt))
                {
                    item.Feedback = feedbackInt;
                }
                else
                {
                    //TODO: Log this as a warning
                }
            }

            if (ordersElement != null)
            {
                var quantityString = Regex.Match(ordersElement.InnerText, quantityStringRegex);
                var quantityTemp = 0;

                if(!int.TryParse(quantityString.Value, out quantityTemp))
                {
                    item.Quantity = quantityTemp;
                }
                else
                {
                    //TODO: Log this as a warning
                }
            }
            else
            {
                item.Quantity = 1;
            }

            if (itemIdElement != null && itemIdElement.Attributes.Contains("itemcodeid"))
            {
                item.ItemID = itemIdElement.Attributes["itemcodeid"].Value;
            }
            else
            {
                item.ItemID = "";
                //TODO: Log this as a warning
            }

            if (shippingPriceElement != null)
            {
                //Get shipping price
            }
            else
            {
                item.ShippingPrice = 0;
            }

            return item;
        }
    }
}
