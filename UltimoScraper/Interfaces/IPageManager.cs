using System.Threading.Tasks;
using PuppeteerSharp;

namespace UltimoScraper.Interfaces
{
    public interface IPageManager
    {
        Task<Page> GetPage(string name);
        void DisposePage(string name);
    }
}