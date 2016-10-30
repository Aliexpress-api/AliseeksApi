using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using AliseeksApi.Models.Search.Aliexpress;
using HtmlAgilityPack;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Models.Search;
using Newtonsoft.Json;

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

        //Matches all numbers in string
        const string quantityStringRegex = @"([0-9])\w+";

        //Matches all numbers in string
        const string stockStringRegex = @"([0-9])\w+";

        public ItemDetail ScrapeSingleItem(string html)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var itemNode = doc.DocumentNode.GetNodesByCssClass("detail");

                var item = scrapeSingleItem(itemNode);

                return item;
            }
            catch(Exception e)
            {
                //Log this as a warning
                return new ItemDetail();
            }
        }

		public SearchResultOverview ScrapeSearchResults(string html)
		{
			try
			{
				var doc = new HtmlDocument();
				doc.LoadHtml(html);

                var searchResultOverview = scrapeSearchResultsOverview(doc.DocumentNode);

				//select element holding items
				var itemElements = doc.DocumentNode.Descendants().First(p => p.Id.Contains("page")).Descendants().Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "item");

				//Cycle through all the elements
				foreach (var element in itemElements)
				{
					var item = scrapeSearchResultsItemNode(element);

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

        public FreightAjax[] ScrapeFreightResults(string html)
        {
            try
            {
                var jsonText = html.Substring(12, html.Length - 2 - 12);
                var freights = JsonConvert.DeserializeObject<FreightAjax[]>(jsonText);

                return freights;
            }
            catch(Exception e)
            {
                //Log as exception
            }

            return new FreightAjax[0];
        }

		SearchResultOverview scrapeSearchResultsOverview(HtmlNode node)
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
		Item scrapeSearchResultsItemNode(HtmlNode node)
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
            var shippingTypeElement = node.Descendants().FirstOrDefault(x => x.Name == "dd");

            if(shippingTypeElement != null)
            {
                var shippingTypeText = shippingTypeElement.InnerText;
                var viaIndex = shippingTypeText.IndexOf("via");
                shippingTypeText = shippingTypeText.Substring(viaIndex).Replace("via", "").Trim();
                item.ShippingType = shippingTypeText;
            }

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

        ItemDetail scrapeSingleItem(HtmlNode node)
        {
            ItemDetail item = new ItemDetail()
            {
                Source = "Aliexpress"
            };

            //<h1 class="product-name" itemprop="name">10PCS LOT Cooler 40 x 40 x 10mm 4010s DC 2Pin 12V 40mm Computer Cooling Fan</h1>
            var nameElement = node.GetNodesByCssClass("product-name");

            //<span class="order-num" id="j-order-num">227 orders</span>
            var ordersElement = node.GetNodesByCssClass("order-num");

            //<span id="j-sku-price" class="p-price">8.79</span>
            var priceElement = node.GetNodesByCssClass("p-price");

            //<span class="p-symbol">US $</span>
            var currencyElement = node.GetNodesByCssClass("p-symbol");

            //<span class="p-unit"> lot </span>
            var unitElement = node.GetNodesByCssClass("p-unit");

            //Must use calculate freight to get this info
            /*    //<span class="logistics-cost" data-role="shipping-price">Free Shipping</span>
                var shippingPriceElement = node.GetNodesByDataRole("shipping-price");

                //<span id="j-shipping-company">ePacket</span>
                var shippingCompanyElement = node.GetNodesByID("j-shipping-company"); */

            //<div class="description-content" data-role="description">
            var descriptionElement = node.GetNodesByCssClass("product-property-list");

            //<span class="packaging-des">lot (10 pieces/lot)</span>
            var quantityElement = node.GetNodesByCssClass("packaging-des");

            //<ul class="image-thumb-list" id="j-image-thumb-list">
            var imageElement = node.GetNodesByCssClass("image-thumb-list");

            //<em data-role="stock-num" id="j-sell-stock-num">5 lots</em>
            var stockNumElement = node.GetJavascriptParam("window.runParams.totalAvailQuantity");

            var itemIdElement = node.GetJavascriptParam("window.runParams.productId");

            if (itemIdElement != null)
            {
                item.ItemID = itemIdElement.ExtractNumerical();
            }

            if(nameElement != null)
            {
                item.Name = nameElement.InnerText;
            }

            if(ordersElement != null)
            {
                string orderNum = ordersElement.InnerText.ExtractNumerical();
                int orderTemp = 0;
                if(int.TryParse(orderNum, out orderTemp))
                {
                    item.Orders = orderTemp;
                }
                else
                {
                    //Log as warning
                }
            }

            if(priceElement != null)
            {
                var priceString = priceElement.InnerText;
                var priceRegex = Regex.Matches(priceString, priceStringRegex);
                decimal priceTemp = 0;

                if(priceRegex.Count > 0)
                {
                    if(decimal.TryParse(priceRegex[0].Value, out priceTemp))
                    {
                        item.Price = priceTemp;
                    }
                    else
                    {
                        //Log as a warning
                    }
                }
            }

            if (currencyElement != null)
            {
                item.Currency = currencyElement.InnerText;
            }

            if(unitElement != null)
            {
                item.Unit = unitElement.InnerText.Trim();
            }

/*            if(shippingPriceElement != null)
            {
                var shippingPriceText = shippingPriceElement.InnerText;
                if (shippingPriceText == "Free Shipping")
                    item.ShippingPrice = 0;
                else
                {
                    var shippingRegex = Regex.Match(shippingPriceText, shippingPriceStringRegex);
                    if(shippingRegex.Success)
                    {
                        decimal shippingTemp = 0;
                        if(decimal.TryParse(shippingRegex.Value, out shippingTemp))
                        {
                            item.ShippingPrice = shippingTemp;
                        }
                        else
                        {
                            //Log as warning
                            item.ShippingPrice = 0;
                        }
                    }
                }
            }
            else
            {
                item.ShippingPrice = 0;
            }

            if(shippingCompanyElement != null)
            {
                item.ShippingType = shippingCompanyElement.InnerText.Trim();
            }

            if(descriptionElement != null)
            {
                var properties = descriptionElement.Descendants().Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("property-item"));

                foreach(var prop in properties)
                {
                    item.Description += $"{prop.InnerText}<br/>";
                }
            } */

            if(quantityElement != null)
            {
                var quantityString = quantityElement.InnerText;
                int quantityTemp = 0;
                var quantityRegex = Regex.Matches(quantityString, quantityStringRegex);

                if(quantityRegex.Count > 0)
                {
                    if(!int.TryParse(quantityRegex[0].Value, out quantityTemp))
                    {
                        item.Quantity = quantityTemp;
                    }
                    else
                    {
                        item.Quantity = 1;
                        //Log as warning
                    }
                }
                else
                {
                    item.Quantity = 1;
                }
            }

            if(imageElement != null)
            {
                var images = imageElement.Descendants().Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("img-thumb-item"));
                var imageLinks = new List<string>();

                foreach(var image in images)
                {
                    var imgElement = image.Descendants().First(p => p.Name == "img");
                    var imageLink = imgElement.Attributes["src"].Value;
                    imageLink = imageLink.Replace("50x50", "640x640");

                    imageLinks.Add(imageLink);
                }

                item.ImageUrls = imageLinks.ToArray();
            }

            if(stockNumElement != null)
            {
                var stockString = stockNumElement;
                int stockTemp = 0;
                var stockRegex = Regex.Matches(stockString, stockStringRegex);

                if(stockRegex.Count > 0)
                {
                    if(int.TryParse(stockRegex[0].Value, out stockTemp))
                    {
                        item.Stock = stockTemp;
                    }
                    else
                    {
                        //Log as a warning
                        item.Stock = 1;
                    }
                }
                else
                {
                    item.Stock = 1;
                }
            }
            else
            {
                item.Stock = 1;
            }

            return item;
        }
    }
}
