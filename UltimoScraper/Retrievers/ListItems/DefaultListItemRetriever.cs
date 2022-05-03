using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;

namespace UltimoScraper.Retrievers.ListItems
{
    public class DefaultListItemRetriever : IListItemRetriever
    {
        private readonly List<string> _nonElListTypes = new List<string>()
        {
            "br",
            "table",
            "td",
            "p",
            "strong",
            "font",
            "a",
            "b",
            "center",
            "img",
            "span",
            "script",
            "param",
            "iframe",
            "button"
        };

        public async Task<IList<ParsedListItem>> GetListItems(HtmlNode node)
        {
            if (_nonElListTypes.Contains(node.Name)) return new List<ParsedListItem>();

            int numOfListItems = SameSiblings(node);
            if (numOfListItems == 0)
            {
                return new List<ParsedListItem>();
            }

            var listItems = new List<ParsedListItem>();
            var listItem = node;
            for (var i = 0; i < numOfListItems + 1; i++)
            {
                listItems.Add(new ParsedListItem
                {
                    Contents = new List<HtmlNode>
                    {
                        listItem
                    },
                    XPath = listItem.XPath
                });

                listItem = listItem.GetNextSibling();
            }

            return listItems;
        }

        private int SameSiblings(HtmlNode node)
        {
            HtmlNode sibling = node.GetNextSibling();
            if (sibling == null) return 0;

            bool isSameNodeType = true;
            int count = 0;
            while (isSameNodeType)
            {
                if (!node.Name.Equals(sibling.Name))
                {
                    isSameNodeType = false;
                }
                else
                {
                    count++;
                    sibling = sibling.GetNextSibling();

                    if (sibling == null)
                    {
                        break;
                    }
                }
            }

            return count;
        }
    }
}