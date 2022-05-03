using System.Collections.Generic;
using HtmlAgilityPack;

namespace UltimoScraper.Models
{
    public class ParsedList
    {
        public string XPath { get; set; }
        public string PageId { get; set; }
        public string SiteId { get; set; }
        public IList<ParsedListItem> ListItems { get; set; }
        public HtmlNode Parent { get; set; }
        public string ListRetrieverType { get; set; }
    }
}