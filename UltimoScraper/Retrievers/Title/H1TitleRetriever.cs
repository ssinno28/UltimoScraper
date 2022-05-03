using HtmlAgilityPack;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces.Retrievers;

namespace UltimoScraper.Retrievers.Title
{
    public class H1TitleRetriever : ITitleRetriever
    {
        public int Priority => 50;
        public string GetTitle(HtmlNode node)
        {
            var titleNode = node.SelectSingleNode("//h1");
            return titleNode?.GetOnlyInnerText();
        }
    }
}