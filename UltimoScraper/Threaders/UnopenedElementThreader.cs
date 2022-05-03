using System.Linq;
using HtmlAgilityPack;
using UltimoScraper.Interfaces.Threaders;
using UltimoScraper.Models;

namespace UltimoScraper.Threaders
{
    public class UnopenedElementThreader : IHtmlDocThreader
    {
        public HtmlDocument Thread(HtmlDocument doc)
        {
            NodePositions pos = new NodePositions(doc);

            // browse all tags detected as not opened
            foreach (HtmlParseError error in doc.ParseErrors.Where(e => e.Code == HtmlParseErrorCode.TagNotOpened))
            {
                // find the text node just before this error
                HtmlTextNode last = pos.Nodes.OfType<HtmlTextNode>().LastOrDefault(n => n.StreamPosition < error.StreamPosition);
                if (last != null)
                {
                    // fix the text; reintroduce the broken tag
                    last.Text = error.SourceText.Replace("/", "") + last.Text + error.SourceText;
                }
            }

            var newDoc = new HtmlDocument();
            newDoc.LoadHtml(doc.DocumentNode.OuterHtml);
            return newDoc;
        }
    }
}