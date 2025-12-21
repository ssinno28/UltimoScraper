using System.Collections.Generic;
using HtmlAgilityPack;

namespace UltimoScraper.Models
{
    public class ParsedPage
    {
        public HtmlDocument Document { get; set; }
        public List<ParsedWebLink> WebLinks { get; set; }
        public IList<string> MatchedKeywords { get; set; }
        public HtmlNode BodyHtmlNode { get; set; }
        public IList<ParsedList> ParsedLists { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public IList<ParsedPage> ChildPages { get; set; }
        public ParsedWebLink LinkToPage { get; set; }
    }
}