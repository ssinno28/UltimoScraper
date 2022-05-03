using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UltimoScraper.Models;

namespace UltimoScraper.Interfaces.Retrievers
{
    public interface ILinkRetriever
    {
        Task<IList<ParsedWebLink>> GetLinks(
            HtmlNode url, 
            IList<IgnoreRule> linkIgnoreRules,
            IList<string> keywords);
    }
}