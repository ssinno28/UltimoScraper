using System.Collections.Generic;

namespace UltimoScraper.Models
{
    public class ParsedWebLink
    {
        public ParsedWebLink()
        {
            MatchedKeywords = new List<string>();
        }

        public string Value { get; set; }
        public string Text { get; set; }
        public IList<string> MatchedKeywords { get; set; }
    }
}