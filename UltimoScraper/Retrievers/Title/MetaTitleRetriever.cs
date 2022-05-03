using HtmlAgilityPack;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces.Retrievers;

namespace UltimoScraper.Retrievers.Title
{
    public class MetaTitleRetriever : ITitleRetriever
    {
        public int Priority => 100;
        public string GetTitle(HtmlNode node)
        {
            var titleNode = node.SelectSingleNode("//title");
            return titleNode?.GetOnlyInnerText();
        }
    }
}