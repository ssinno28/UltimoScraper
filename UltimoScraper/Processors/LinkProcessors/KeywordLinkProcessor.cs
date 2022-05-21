using System.Collections.Generic;
using System.Threading.Tasks;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;

namespace UltimoScraper.Processors.LinkProcessors
{
    public class KeywordLinkProcessor : ILinkProcessor
    {
        public async Task<bool> Process(ParsedWebLink webLink, IList<string> keywords)
        {
            bool passes = false;
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrEmpty(webLink.Text))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }

                var containsKeyword = webLink.Text.ToLower().Contains(keyword) ||
                                      webLink.Value.ToLower().Contains(keyword);

                if (!containsKeyword)
                {
                    continue;
                }

                if (webLink.MatchedKeywords == null)
                {
                    webLink.MatchedKeywords = new List<string>();
                }

                webLink.MatchedKeywords.Add(keyword);
                passes = true;
            }

            return passes;
        }
    }
}