using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using HtmlAgilityPack;
using AliseeksApi.Utility.Extensions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;

namespace AliseeksApi.Services.DHGate
{
    public class DHGatePageDecoder
    {
        public string PagingKey { get; set; }

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

                //If zero1 then 
                if (doc.DocumentNode.GetNodesByCssClass("zero1") == null)
                {
                    var searchResultOverview = extractResultsOverview(doc.DocumentNode);

                    PagingKey = extractPagingKey(doc.DocumentNode);

                    //select element holding items
                    var itemElements = doc.DocumentNode.Descendants().Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("listitem"));

                    //Cycle through all the elements
                    foreach (var element in itemElements)
                    {
                        var item = decodeItemnode(element);

                        searchResultOverview.Items.Add(item);
                    }

                    searchResultOverview.Extra.Add("PagingKey", PagingKey);

                    return searchResultOverview;
                }
                else
                {
                    var searchResultsOverview = new SearchResultOverview()
                    {
                        SearchCount = 0,
                        Items = new List<Item>()
                    };

                    return searchResultsOverview;
                }
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

        string extractPagingKey(HtmlNode node)
        {
            /* <span class="pagelist">
             * <a href="http://www.dhgate.com/w/40mm+12v/1.html?leftpars=c2hpcGNvbXBhbmllcz1zNG8tc2o5LWRobC1zZDQtc2JpLXVwcy10bnQtc2FvLXN1NC1zOHAtczcyLXNlZGRoZ2F0ZQ==" rel="nofollow">2</a>
             * </span>
             */ 

            var pagelist = node.GetNodesByCssClass("pagelist");
            var page = pagelist.Descendants().First(x => x.Name == "a");
            var key = page.Attributes.Contains("href") ? page.Attributes["href"].Value : "";
            var uri = new Uri(key);

            var qs = QueryHelpers.ParseQuery(uri.Query);

            return qs.ContainsKey("leftpars") ? String.Join("", qs["leftpars"]) : "";           
        }

        //Where the magic happens
        Item decodeItemnode(HtmlNode node)
        {
            Item item = new Item()
            {
                Source = "DHGate"
            };

            //Get name node

            /*<a href="http://www.dhgate.com/product/2015-new-hot-brand-larsson-jennings-watch/377143614.html#s1-0-1b;searl|0952724766" class="subject">
             * 2015 New Hot Brand Larsson Jennings Watch For Mens   Women Fashion Casual Quartz-Watch Leather Watch 40mm Relojes LJ Watches
             * </a>
             */
            var nameElement = node.GetNodesByCssClass("subject");

            /*<img src="http://www.dhresource.com/200x200s/f2-albu-g4-M01-E6-34-rBVaEFbyvs6ANN1fAAHQSjo9I-I825.jpg/2015-new-hot-brand-larsson-jennings-watch.jpg"
             * alt="Wholesale 2015 New Hot Brand Larsson Jennings Watch For Mens Women Fashion Casual Quartz Watch Leather Watch mm Relojes LJ Watches" class="lthumbnail" style="width: 160px; height: 160px;">
             */
            var imageElement = node.GetNodesByCssClass("lthumbnail");

            //<li class="price"><span>US $ 11.36 - 12.92</span> / Piece</li>
            var priceElement = node.Descendants().First(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "price");

            //<li class="freeico"><span class="free">Free shipping</span> 
            var freesElement = node.GetNodesByCssClass("free");

            /*<span class="seller">Seller:
             *  <a href = "http://www.dhgate.com/store/20018058" > huangxuerui </a> </ span >
             */
            var storeElement = node.Descendants().First(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "seller");

            //<li class="feedback"><span>97.3%</span> Positive Feedback</li>
            var feedbackElement = node.GetNodesByCssClass("feedback");

            //<li class="feedback"><span>97.3%</span> Positive Feedback</li>
            var ordersElement = node.GetNodesByCssClass("min");

            //<span class="m-word">Premium Merchant </ span >
            var topRatedSellerElement = node.GetNodesByCssClass("m-word");

            /*<li class="cart" onclick="goog_report_conversion();" itemcodeid="387017480" impressioninfo="s1-1-1b;searl|0952724766" supplierid="ff8080813ef436e4013f1c8f40236130">
             *    < span class="n-yellow-button">Add to Cart</span>
		     * </li>
             */					
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

                item.Unit = new String(priceElement.InnerText.Substring(priceString.Length).Where(x => Char.IsLetter(x)).ToArray()).ToLower();
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
                decimal feedbackInt = 0;

                if (decimal.TryParse(priceRegex.Value, out feedbackInt))
                {
                    item.Feedback = (int)feedbackInt;
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

                if(int.TryParse(quantityString.Value, out quantityTemp))
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

            if(item.Price.Length > 0)
            {
                item.LotPrice = item.Quantity * item.Price[0];
            }

            return item;
        }
    }
}
