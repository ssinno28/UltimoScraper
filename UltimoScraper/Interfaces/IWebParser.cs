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
            IList<string> keywords,
            string sessionName = null);

        Task<IList<string>> KeywordSearch(
            string domain,
            IList<IgnoreRule> ignoreRules,
            IList<string> keywords,
            IList<string> searchKeywords,
            string sessionName = null);
        Task<ParsedPage> ParsePage(string domain, string path, IList<IgnoreRule> ignoreRules, IList<string> keywords, string sessionName = null);
    }
}