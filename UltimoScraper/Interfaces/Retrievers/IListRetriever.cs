using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UltimoScraper.Models;

namespace UltimoScraper.Interfaces.Retrievers
{
    public interface IListRetriever
    {
        Task<IList<ParsedList>> GetParsedLists(HtmlNode node, IList<IgnoreRule> ignoreRules);
    }
}