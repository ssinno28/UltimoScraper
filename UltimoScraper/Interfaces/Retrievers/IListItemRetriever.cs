using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UltimoScraper.Models;

namespace UltimoScraper.Interfaces.Retrievers
{
    public interface IListItemRetriever
    {
        Task<IList<ParsedListItem>> GetListItems(HtmlNode node);
    }
}