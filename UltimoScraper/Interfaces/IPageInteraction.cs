using System.Threading.Tasks;
using PuppeteerSharp;

namespace UltimoScraper.Interfaces
{
    public interface IPageInteraction
    {
        bool IsMatch(string url);
        Task Interact(IPage page);
    }
}