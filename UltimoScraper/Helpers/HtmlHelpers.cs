using System.Web;
using HtmlAgilityPack;

namespace UltimoScraper.Helpers
{
    public static class HtmlHelpers
    {
        public static HtmlNode GetNextSibling(this HtmlNode node)
        {
            HtmlNode sibling = null;
            while (sibling == null && node.NextSibling != null)
            {
                if (node.NextSibling.NodeType == HtmlNodeType.Text || node.NextSibling.NodeType == HtmlNodeType.Comment || node.NextSibling.Name.Equals("br") || node.NextSibling.Name.Equals("script"))
                {
                    node = node.NextSibling;
                    continue;
                }

                sibling = node.NextSibling;
            }

            return sibling;
        }

        public static HtmlNode GetFirstChild(this HtmlNode node)
        {
            HtmlNode firstChild = null;
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.NodeType == HtmlNodeType.Text || childNode.NodeType == HtmlNodeType.Comment || childNode.Name.Equals("br"))
                {
                    continue;
                }

                firstChild = childNode;
                break;
            }

            return firstChild;
        }

        public static HtmlNode GetClosestParentByTagName(this HtmlNode htmlNode, string tagName)
        {
            var node = htmlNode.ParentNode;
            for (; node != null && !node.Name.Equals("html"); node = node.ParentNode)
            {
                if (node.Name.Equals(tagName)) return node;
            }

            return null;
        }
        
        public static HtmlNode GetClosestParentByClassName(this HtmlNode htmlNode, string @class)
        {
            var node = htmlNode.ParentNode;
            for (; node != null && !node.Name.Equals("html"); node = node.ParentNode)
            {
                if (node.HasClass(@class)) return node;
            }

            return null;
        }

        public static string GetOnlyInnerText(this HtmlNode htmlNode)
        {
            return HttpUtility.HtmlDecode(htmlNode.InnerText).Trim();
        }
    }
}