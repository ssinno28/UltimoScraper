using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltimoScraper.Models;

namespace UltimoScraper.Interfaces
{
    public interface IWebParser
    {
        Task<ParsedSite> ParseSite(
            string domain, 
            IList<IgnoreRule> ignoreRules, 
            IList<Keyword> keywords,
            string sessionName = null,
            int? maxDepth = null);

        Task<IList<string>> KeywordSearch(
            string domain,
            IList<IgnoreRule> ignoreRules,
            IList<Keyword> keywords,
            IList<Keyword> searchKeywords,
            string sessionName = null);
        Task<ParsedPage> ParsePage(string domain, string path, IList<IgnoreRule> ignoreRules, IList<Keyword> keywords, string sessionName = null);
    }
}