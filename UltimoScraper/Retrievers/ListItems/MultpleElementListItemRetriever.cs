using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;

namespace UltimoScraper.Retrievers.ListItems
{
    public class MultpleElementListItemRetriever : IListItemRetriever
    {
        private readonly HashSet<string> _nonMultipleElListTypes = new HashSet<string>
        {
            "li",
            "td",
            "tr",
            "th",
            "br",
            "b",
            "font",
            "table",
            "a",
            "center"
        };

        private int _numOfElements = 3;
        public async Task<IList<ParsedListItem>> GetListItems(HtmlNode node)
        {
            if (_nonMultipleElListTypes.Contains(node.Name)) return new List<ParsedListItem>();

            int numOfElements = 0;
            int numOfListItems = 0;

            for (var i = 2; i <= _numOfElements; i++)
            {
                numOfListItems = SameSiblings(node, i);
                if (numOfListItems > 1)
                {
                    numOfElements = i;
                    break;
                }
            }

            if (numOfListItems <= 1)
            {
                return new List<ParsedListItem>();
            }
            
            var parsedListItems = new List<ParsedListItem>();
            for (var i = 0; i <= numOfListItems; i++)
            {
                var listItem = GetNextListItem(node, numOfElements);
                parsedListItems.Add(new ParsedListItem
                {
                    Contents = listItem,
                    XPath = node.XPath
                });

                node = listItem.Last().GetNextSibling();
            }

            return parsedListItems;
        }

        private int SameSiblings(HtmlNode node, int numOfElements)
        {
            var listItem = GetNextListItem(node, numOfElements);
            var sibling = listItem;

            bool isSameNodeType = true;
            int count = 0;
            while (isSameNodeType)
            {
                sibling = GetNextListItem(sibling.Last().GetNextSibling(), numOfElements);
                if (IsSameSibling(listItem, sibling))
                {
                    count++;

                    if (sibling == null || sibling.Count == 0)
                    {
                        break;
                    }
                }
                else
                {
                    isSameNodeType = false;
                }
            }

            return count;
        }

        private bool IsSameSibling(List<HtmlNode> node, List<HtmlNode> sibling)
        {
            if (sibling.Count == 0 || sibling.Count != node.Count) return false;

            int count = node.Count;
            bool isSameSibling = true;
            for (var i = 0; i < count; i++)
            {
                if (!node[i].Name.Equals(sibling[i].Name))
                {
                    isSameSibling = false;
                    break;
                }
            }

            return isSameSibling;
        }

        private List<HtmlNode> GetNextListItem(HtmlNode node, int numOfElements)
        {
            var listItem = new List<HtmlNode>();
            for (var i = 0; i < numOfElements; i++)
            {
                if (node == null) break;

                if (_nonMultipleElListTypes.Contains(node.Name)) break;

                listItem.Add(node);
                node = node.GetNextSibling();
            }

            return listItem;
        }
    }
}