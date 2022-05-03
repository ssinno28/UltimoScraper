using HtmlAgilityPack;

namespace UltimoScraper.Interfaces.Threaders
{
    public interface IHtmlDocThreader
    {
        HtmlDocument Thread(HtmlDocument doc);
    }
}