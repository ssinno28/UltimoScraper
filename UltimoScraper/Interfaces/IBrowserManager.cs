using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace UltimoScraper.Interfaces
{
    public interface IBrowserManager
    {
        Task<IBrowser> GetBrowser(string name);
        Task DisposeBrowser(string name);
        List<IBrowser> Browsers { get; }
    }
}