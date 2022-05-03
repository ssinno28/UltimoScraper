using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UltimoScraper.Interfaces;

namespace UltimoScraper.Providers
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly ILogger<HttpClientProvider> _logger;

        public HttpClientProvider(ILogger<HttpClientProvider> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetStringFromUrl(string urlWithScheme)
        {
            try
            {
                string robotsTxt;
                using (var client = new HttpClient())
                {
                    robotsTxt = await client.GetStringAsync(urlWithScheme);
                }

                return robotsTxt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not download file for url {urlWithScheme}");
                return null;
            }
        }
    }
}