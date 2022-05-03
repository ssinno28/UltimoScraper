using System.Collections.Generic;

namespace UltimoScraper.Models
{
    public class ParsedSite
    {
        public string Domain { get; set; }
        public ParsedPage StartPage { get; set; }
        public ParsedWebLink FacebookLink { get; set; }
        public ParsedWebLink InstagramLink { get; set; }
        public ParsedWebLink TwitterLink { get; set; }
        public IDictionary<string, IList<ParsedPage>> PagesPerKeyword { get; set; }
    }
}