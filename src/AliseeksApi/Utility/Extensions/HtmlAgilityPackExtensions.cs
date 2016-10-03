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

        public static HtmlNode GetNodesByDataRole(this HtmlNode node, string dataRole)
        {
            return node.Descendants().FirstOrDefault(p => p.Attributes.Contains("data-role") && p.Attributes["data-role"].Value.Contains(dataRole));
        }

        public static HtmlNode GetNodesByID(this HtmlNode node, string id)
        {
            return node.Descendants().FirstOrDefault(p => p.Attributes.Contains("id") && p.Attributes["id"].Value.Contains(id));
        }

        public static string GetJavascriptParam(this HtmlNode node, string param)
        {
            var innerText = node.InnerText;
            var indexStart = innerText.IndexOf(param) + param.Length + 1;
            var indexEnd = innerText.IndexOf(';', indexStart);

            return innerText.Substring(indexStart, indexEnd - indexStart);
        }
    }
}
