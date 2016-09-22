using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using HtmlAgilityPack;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Utility
{
	public class AliexpressPageDecoder
	{
		//Matches all decimal numbers with OPTIONAL decimal point
		const string priceStringRegex = @"\d+\.?\d*";

		//Matches number inside paramethesis
		const string orderStringRegex = @"\(([^\)]+)\)";

		//Matches any number from string
		const string lotPriceUnitStringRegex = @"\d+";

		//Matches all decimal numbers with OPTIONAL decimal point
		const string lotPriceStringRegex = @"\d+\.?\d*";

		//Matches all decimal numbers with OPTIONAL decimal point
		const string shippingPriceStringRegex = @"\d+\.?\d*";

		public SearchResultOverview DecodePage(string html)
		{
			try
			{
				var doc = new HtmlDocument();
				doc.LoadHtml(html);

                var searchResultOverview = extractResultsOverview(doc.DocumentNode);

				//select element holding items
				var itemElements = doc.DocumentNode.Descendants().First(p => p.Id.Contains("page")).Descendants().Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "item");

				//Cycle through all the elements
				foreach (var element in itemElements)
				{
					var item = decodeItemnode(element);

                    searchResultOverview.Items.Add(item);
				}

                return searchResultOverview;
			}
			catch
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
			var searchCountElement = node.GetNodesByCssClass("search-count");

			if(searchCountElement != null)
			{
                var searchResultInt = 0;
                var searchResultString = searchCountElement.InnerText.Replace(",", String.Empty); //Remove commas
                if(!int.TryParse(searchResultString, out searchResultInt))
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
				Source = "Aliexpress"
			};

			//Get name node

			//<a  class="history-item product " href="//www.aliexpress.com/item/Wholesale-Factory-Price-Approx-2200pcs-lot-Jewelry-Head-Pin-Findings-Gold-Plated-45MM-Eye-Pin-for/32263213196.html?ws_ab_test=searchweb201556_0,searchweb201602_1_10065_10068_10067_112_10069_110_111_10017_109_108_10060_10061_10062_10057_10056_10055_10054_10059_10058_10073_10070_10052_10053_10050_10051,searchweb201603_1&amp;btsid=8eae686d-1bf4-4944-acb8-ee998e6a1998" title="Wholesale Factory Price Approx 2200pcs/lot Jewelry Head Pin Findings Gold Plated 45MM Eye Pin for Jewelry Making DH-FZB007-19" >Wholesale Factory Price Approx 2200pcs/lot Jewelry Head Pin Findings Gold Plated <font><b>45MM</b></font> Eye Pin for Jewelry Making DH-FZB007-19</a>
			var nameElement = node.GetNodesByCssClass("history-item product");

			//<img   class="picCore" image-src="http://g03.a.alicdn.com/kf/HTB1xSXeIXXXXXaBXXXXq6xXFXXXo/Wholesale-Factory-Price-Approx-2200pcs-lot-Jewelry-Head-Pin-Findings-Gold-Plated-45MM-Eye-Pin-for.jpg_220x220.jpg"  alt="Wholesale Factory Price Approx 2200pcs/lot Jewelry Head Pin Findings Gold Plated 45MM Eye Pin for Jewelry Making DH-FZB007-19(China (Mainland))" />
			var imageElement = node.GetNodesByCssClass("picCore");


			/*<span class="price price-m">
				< span class="value" itemprop="price">US $0.01</span>
				<span class="separator">/</span> 
				<span class="unit">piece</span>
			</span>*/
			var priceElement = node.GetNodesByCssClass("price-m");

			//<strong class="free-s">Free Shipping</strong>
			var freesElement = node.GetNodesByCssClass("free-s");

			var mobileElement = node.GetNodesByCssClass("mobile-exclusive");

			//<a href="//www.aliexpress.com/store/1041550" title="Just Beading" class="store "   >Just Beading</a>
			var storeElement = node.GetNodesByCssClass("store ");

			//<span class="score-icon-new score-level-21" id="score20" feedBackScore="784" sellerPositiveFeedbackPercentage="97.8"></span>
			var feedbackElement = node.Descendants().FirstOrDefault(x => x.Attributes.Contains("feedBackScore"));

			/*
			<span rel="nofollow" class="order-num">
				 <a class="order-num-a " href="//www.aliexpress.com/item/Wholesale-Factory-Price-Approx-2200pcs-lot-Jewelry-Head-Pin-Findings-Gold-Plated-45MM-Eye-Pin-for/32263213196.html?ws_ab_test=searchweb201556_0,searchweb201602_1_10065_10068_10067_112_10069_110_111_10017_109_108_10060_10061_10062_10057_10056_10055_10054_10059_10058_10073_10070_10052_10053_10050_10051,searchweb201603_1&amp;btsid=8eae686d-1bf4-4944-acb8-ee998e6a1998#thf" rel="nofollow" >
					<em title="Total Orders"> Orders (0)</em>
				 </a>
			</span>
			*/
			var ordersElement = node.GetNodesByCssClass("order-num-a");

			//<span title="Top-rated Seller" class="top-rated-seller"></span>
			var topRatedSellerElement = node.GetNodesByCssClass("top-rated-seller");

			//<input class="atc-product-id" type="hidden" value="32493137804" />
			var itemIdElement = node.GetNodesByCssClass("atc-product-id");

			/*
			 <span class="lot-price">
				<span class="value" itemprop="price">US $1.38</span>
				<span class="separator">/</span> 
				<span class="unit">lot (170 pieces)</span>
			</span>
			 */
			var lotPriceElement = node.GetNodesByCssClass("lot-price");

			/*
			 <dd class="price">
				<span class="value">US $0.15</span> <span class="separator">/</span>
				<span class="unit"> lot</span> via Fedex IE     
			 </dd>
			 */
			var shippingPriceElement = node.Descendants().FirstOrDefault(x => x.Name == "dd" && x.Attributes.Contains("class") && x.Attributes["class"].Value == "price");

			if (nameElement != null)
			{
				item.Name = nameElement.Attributes["title"].Value;
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
				var priceString = priceElement.Descendants().First(p => p.Attributes.Contains("itemprop") && p.Attributes["itemprop"].Value.Contains("price")).InnerText;
				var priceRegex = Regex.Matches(priceString, priceStringRegex);

				item.Price = new decimal[priceRegex.Count];
				for(int i = 0; i != priceRegex.Count; i++)
				{
					if(!decimal.TryParse(priceRegex[i].ToString(), out item.Price[i]))
					{
						//TODO: Log this as a warning
					}
				}

				//Extract currency from beginning of priceString
				for(int i = 0; i != priceString.Length; i++)
				{
					if(Char.IsNumber(priceString[i])) { break; }
					item.Currency += priceString[i];
				}
				item.Currency = item.Currency.Trim();

				item.Unit = priceElement.Descendants().First(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("unit")).InnerText;
			}

			if(lotPriceElement != null)
			{
				var lotPriceUnit = lotPriceElement.GetNodesByCssClass("unit");
				var lotPrice = lotPriceElement.GetNodesByCssClass("value");

				var lotPriceString = lotPrice.InnerText;
				var lotPriceUnitString = lotPriceUnit.InnerText;

				decimal lotPriceDecimal = 0;
				if(!decimal.TryParse(Regex.Match(lotPriceString, lotPriceStringRegex).Value, out lotPriceDecimal))
				{
					//TODO: Log this as a warning
				}

				item.LotPrice = lotPriceDecimal;

				int lotQuantity = 1;
				if (!int.TryParse(Regex.Match(lotPriceUnitString, lotPriceUnitStringRegex).Value, out lotQuantity))
				{
					//TODO: Log this as a warning
				}

				item.Quantity = lotQuantity;
			}
			else
			{
				//Probably a single piece item so set LotPrice to price of Item
				if (item.Price.Length > 0)
					item.LotPrice = item.Price[0];
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
				var feedbackInt = 0;
				if (int.TryParse(feedbackElement.Attributes["feedBackScore"].Value, out feedbackInt))
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
				string orders = Regex.Match(ordersElement.InnerText, orderStringRegex).Value;
				orders = orders.Replace("(", "").Replace(")", ""); //Remove parathesis
				if (orders != "")
				{
					var ordersTemp = 0;
					if(int.TryParse(orders, out ordersTemp))
					{
						item.Orders = ordersTemp;
					}
					else
					{
						//TODO: Log this as a warning
					}
				}
			}

			if(itemIdElement != null && itemIdElement.Attributes.Contains("value"))
			{
				item.ItemID = itemIdElement.Attributes["value"].Value;
			}
			else
			{
				item.ItemID = "";
				//TODO: Log this as a warning
			}

			if(shippingPriceElement != null)
			{
				//Get the first element that has [itemprop=price]
				var shippingPriceString = shippingPriceElement.GetNodesByCssClass("value").InnerText;
				var shippingPriceRegex = Regex.Match(shippingPriceString, shippingPriceStringRegex);

				decimal shippingPriceDecimal = 0;
				if (!decimal.TryParse(shippingPriceRegex.Value.ToString(), out shippingPriceDecimal))
				{
					//TODO: Log this as a warning
				}

				item.ShippingPrice = shippingPriceDecimal;
			}
			else
			{
				item.ShippingPrice = 0;
			}

			return item;
		}
	}
}
