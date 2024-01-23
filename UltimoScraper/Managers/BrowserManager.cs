using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;
using UltimoScraper.Interfaces;

namespace UltimoScraper.Managers
{
    public class BrowserManager : IBrowserManager
    {
        private readonly Lazy<ConcurrentDictionary<string, IBrowser>> _browsers =
            new Lazy<ConcurrentDictionary<string, IBrowser>>(() => new ConcurrentDictionary<string, IBrowser>());

        public List<IBrowser> Browsers => _browsers.Value.Select(x => x.Value).ToList();

        public async Task<IBrowser> GetBrowser(string name)
        {
            if (!_browsers.Value.TryGetValue(name, out var browser))
            {
                await new BrowserFetcher().DownloadAsync();
                browser = await Puppeteer.LaunchAsync(new LaunchOptions()
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox" }
                });
                
                _browsers.Value.TryAdd(name, browser);
            }

            return browser;
        }

        public async Task DisposeBrowser(string name)
        {
            if (!_browsers.Value.TryGetValue(name, out var browser))
            {
                return;
            }

            var pages = await browser.PagesAsync();
            foreach (var page in pages)
            {
                if (!page.IsClosed)
                {
                    await page.CloseAsync();
                }
            }

            await browser.CloseAsync();

            var chromeProcess = Process.GetProcesses().FirstOrDefault(x => x.Id == browser.Process.Id);
            if (chromeProcess != null)
            {
                chromeProcess.Kill();
            }

            _browsers.Value.TryRemove(name, out _);
        }
    }
}