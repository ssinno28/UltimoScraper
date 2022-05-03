using System.Collections.Generic;
using HtmlAgilityPack;

namespace UltimoScraper.Models
{
    public class ParsedListItem
    {
        public IList<HtmlNode> Contents { get; set; }
        public string XPath { get; set; }
    }
}