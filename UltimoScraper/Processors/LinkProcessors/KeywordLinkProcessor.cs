using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;

namespace UltimoScraper.Processors.LinkProcessors
{
    public class KeywordLinkProcessor : ILinkProcessor
    {
        public async Task<bool> Process(ParsedWebLink webLink, IList<Keyword> keywords)
        {
            bool passes = false;
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrEmpty(webLink.Text))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(keyword.Value))
                {
                    continue;
                }

                string text = webLink.Text.ToLower();
                string value = webLink.Value.ToLower();
                var containsKeyword = 
                    text.MatchesKeyword(keyword) ||
                    value.MatchesKeyword(keyword);

                if (!containsKeyword)
                {
                    continue;
                }

                if (webLink.MatchedKeywords == null)
                {
                    webLink.MatchedKeywords = new List<string>();
                }

                webLink.MatchedKeywords.Add(keyword.Value);
                passes = true;
            }

            return passes;
        }
    }
}