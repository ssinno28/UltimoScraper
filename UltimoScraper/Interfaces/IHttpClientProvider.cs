using System.Threading.Tasks;

namespace UltimoScraper.Interfaces
{
    public interface IHttpClientProvider
    {
        Task<string> GetStringFromUrl(string urlWithScheme);
    }
}