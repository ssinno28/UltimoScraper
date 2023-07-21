using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltimoScraper.CommandLine.Services
{
    public interface IScraperService
    {
        Task ScrapeSite(string domain, string[] keywords);
        Task ScrapePage(string domain, string path, string[] keywords);
        Task<IList<string>> KeywordSearch(string domain, string[] keywords);
    }
}