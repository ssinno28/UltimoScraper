using System.Collections.Generic;
using System.Threading.Tasks;
using UltimoScraper.Dictionary;
using UltimoScraper.Interfaces;
using UltimoScraper.Models;

namespace UltimoScraper.CommandLine.Services
{
    public class ScraperService : IScraperService
    {
        private readonly IWebParser _webParser;

        private IList<IgnoreRule> _ignoreRules = new List<IgnoreRule>();

        public ScraperService(IWebParser webParser)
        {
            _webParser = webParser;
        }

        public async Task ScrapeSite(string domain, string[] keywords)
        {
            var result = await _webParser.ParseSite(domain, _ignoreRules, keywords);
        }

        public async Task ScrapePage(string domain, string path, string[] keywords)
        {
            var parsedPageDto = await _webParser.ParsePage(domain, path, _ignoreRules, keywords);
        }
    }
}