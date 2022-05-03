using HtmlAgilityPack;

namespace UltimoScraper.Interfaces.Retrievers
{
    public interface ITitleRetriever
    {
        int Priority { get; }
        string GetTitle(HtmlNode node);
    }
}