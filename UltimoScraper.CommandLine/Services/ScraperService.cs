using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using UltimoScraper.Dictionary;
using UltimoScraper.Interfaces;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;

namespace UltimoScraper.CommandLine.Services
{
    public class ScraperService : IScraperService
    {
        private readonly IWebParser _webParser;
        private readonly IRobotsTxtRetriever _robotsTxtRetriever;

        private IList<IgnoreRule> _ignoreRules = new List<IgnoreRule>();

        public ScraperService(IWebParser webParser, IRobotsTxtRetriever robotsTxtRetriever)
        {
            _webParser = webParser;
            _robotsTxtRetriever = robotsTxtRetriever;
        }

        public async Task ScrapeSite(string domain, string[] keywords)
        {
            var ignoreRules = await _robotsTxtRetriever.GetRobotsTxt(new Uri(domain));
            var result = await _webParser.ParseSite(domain, ignoreRules, keywords.Select(x => new Keyword() { Value = x }).ToList());
        }

        public async Task<IList<string>> KeywordSearch(string domain, string[] keywords)
        {
            var ignoreRules = await _robotsTxtRetriever.GetRobotsTxt(new Uri(domain));
            return await _webParser.KeywordSearch(domain, ignoreRules, keywords.Select(x => new Keyword() { Value = x }).ToList(), new List<Keyword>());
        }

        public async Task ScrapePage(string domain, string path, string[] keywords)
        {
            var parsedPageDto = await _webParser.ParsePage(domain, path, _ignoreRules, keywords.Select(x => new Keyword() { Value = x }).ToList());
        }
    }
}