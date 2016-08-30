using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AliseeksApi.Utility.Extensions
{
    public static class HtmlAgilityPackExtensions
    {
        public static HtmlNode GetNodesByCssClass(this HtmlNode node, string cssClass)
        {
            return node.Descendants().FirstOrDefault(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains(cssClass));
        }
    }
}
