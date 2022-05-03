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
    public class OnClickLinkRetriever : ALinkRetriever, ILinkRetriever
    {
        public OnClickLinkRetriever(IEnumerable<ILinkProcessor> linkProcessors) 
            : base(linkProcessors)
        {
        }

        public async Task<IList<ParsedWebLink>> GetLinks(
            HtmlNode htmlNode, 
            IList<IgnoreRule> linkIgnoreRules,
            IList<string> keywords)
        {
            var onClicks =
                htmlNode.QuerySelectorAll("[onclick]")
                    .Where(x => x.Attributes.Any(a => !string.IsNullOrEmpty(a.Value.GetUrlFromText())))
                    .Select(x => new ParsedWebLink
                    {
                        Value = HttpUtility.HtmlDecode(x.GetAttributeValue("onclick", string.Empty).GetUrlFromText()),
                        Text = x.GetOnlyInnerText()
                    });

            return await ProcessLinks(onClicks, linkIgnoreRules, keywords);
        }
    }
}