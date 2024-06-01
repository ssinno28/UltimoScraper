using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;

namespace UltimoScraper.Retrievers.Lists
{
    public class DefaultListRetriever : IListRetriever
    {
        private readonly IEnumerable<IListItemRetriever> _listItemRetrievers;
        private readonly IEnumerable<IListProcessor> _listProcessors;
        private readonly ILoggerFactory _loggerFactory;

        private ILogger Logger => CreateLogger();
        private ILogger CreateLogger()
        {
            return _loggerFactory.CreateLogger<DefaultListRetriever>();
        }

        public DefaultListRetriever(
            IEnumerable<IListItemRetriever> listItemRetrievers,
            ILoggerFactory loggerFactory,
            IEnumerable<IListProcessor> listProcessors)
        {
            _listItemRetrievers = listItemRetrievers;
            _loggerFactory = loggerFactory;
            _listProcessors = listProcessors;
        }

        public async Task<IList<ParsedList>> GetParsedLists(HtmlNode node, IList<IgnoreRule> ignoreRules)
        {
            return await GetLists(node, new HashSet<string>(), new HashSet<string>(), ignoreRules);
        }

        private async Task<IList<ParsedList>> GetLists(HtmlNode node, HashSet<string> parsedPaths, HashSet<string> listItemPaths, IList<IgnoreRule> ignoreRules)
        {
            Logger.LogDebug($"Getting lists for {node.XPath}");

            if (parsedPaths.Contains(node.XPath))
            {
                return new List<ParsedList>();
            }

            parsedPaths.Add(node.XPath);
            List<ParsedList> lists = new List<ParsedList>();

            var listItems = new List<ParsedListItem>();
            string listRetrieverType = string.Empty;
            if (!listItemPaths.Contains(node.XPath))
            {
                foreach (var listItemRetriever in _listItemRetrievers)
                {
                    var nodeListItems = await listItemRetriever.GetListItems(node);
                    if (!nodeListItems.Any())
                    {
                        continue;
                    }

                    listRetrieverType = listItemRetriever.GetType().Name;
                    listItems.AddRange(nodeListItems);
                    foreach (var parsedListItem in nodeListItems)
                    {
                        listItemPaths.Add(parsedListItem.XPath);
                        foreach (var content in parsedListItem.Contents)
                        {
                            listItemPaths.Add(content.XPath);
                        }
                    }
                }
            }

            // only check the first child as subsequent logic will get the child's siblings
            var firstChild = node.GetFirstChild();
            if (firstChild != null && !parsedPaths.Contains(firstChild.XPath))
            {
                lists.AddRange(await GetLists(firstChild, parsedPaths, listItemPaths, ignoreRules));
            }

            var nextSibling = node.GetNextSibling();
            while (nextSibling != null)
            {
                if (!parsedPaths.Contains(nextSibling.XPath))
                {
                    lists.AddRange(await GetLists(nextSibling, parsedPaths, listItemPaths, ignoreRules));
                }

                nextSibling = nextSibling.GetNextSibling();
            }

            if (!listItems.Any()) return lists;

            var parsedList = new ParsedList
            {
                ListItems = listItems,
                Parent = node.ParentNode,
                XPath = node.XPath,
                ListRetrieverType = listRetrieverType
            };

            bool addList = true;
            foreach (var listProcessor in _listProcessors)
            {
                if (!await listProcessor.Process(parsedList, ignoreRules))
                {
                    addList = false;
                }
            }

            if (addList) lists.Add(parsedList);
            return lists;
        }
    }
}