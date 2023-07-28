using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;

namespace UltimoScraper.Retrievers.Links
{
    public class DefaultLinkRetriever : ALinkRetriever, ILinkRetriever
    {
        public DefaultLinkRetriever(IEnumerable<ILinkProcessor> linkProcessors) : base(linkProcessors)
        {
        }

        public async Task<IList<ParsedWebLink>> GetLinks(
            HtmlNode htmlNode, 
            IList<IgnoreRule> linkIgnoreRules,
            IList<Keyword> keywords)
        {
            var links =
                htmlNode.QuerySelectorAll("a")
                        .Select(a => new ParsedWebLink
                        {
                            Value = HttpUtility.HtmlDecode(a.GetAttributeValue("href", null)),
                            Text = a.GetOnlyInnerText()
                        }).Where(u => !string.IsNullOrEmpty(u.Value));

            return await ProcessLinks(links, linkIgnoreRules, keywords);
        }
    }
}