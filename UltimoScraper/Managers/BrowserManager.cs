using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PuppeteerSharp;
using UltimoScraper.Interfaces;

namespace UltimoScraper.Managers
{
    public class BrowserManager : IBrowserManager
    {
        private readonly Lazy<ConcurrentDictionary<string, Browser>> _browsers =
            new Lazy<ConcurrentDictionary<string, Browser>>(() => new ConcurrentDictionary<string, Browser>());

        public async Task<Browser> GetBrowser(string name)
        {
            if (!_browsers.Value.TryGetValue(name, out var browser))
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                browser = await Puppeteer.LaunchAsync(new LaunchOptions()
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox" }
                });
                
                _browsers.Value.TryAdd(name, browser);
            }

            return browser;
        }

        public void DisposeBrowser(string name)
        {
            if (!_browsers.Value.TryGetValue(name, out var browser))
            {
                return;
            }

            browser.Dispose();

            _browsers.Value.TryRemove(name, out _);
        }
    }
}