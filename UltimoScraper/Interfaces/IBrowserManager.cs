using System.Threading.Tasks;
using PuppeteerSharp;

namespace UltimoScraper.Interfaces
{
    public interface IBrowserManager
    {
        Task<Browser> GetBrowser(string name);
        void DisposeBrowser(string name);
    }
}