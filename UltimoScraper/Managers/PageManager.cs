using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PuppeteerSharp;
using UltimoScraper.Interfaces;

namespace UltimoScraper.Managers
{
    public class PageManager : IPageManager
    {
        private readonly Lazy<ConcurrentDictionary<string, Page>> _pageSessions =
            new Lazy<ConcurrentDictionary<string, Page>>(() => new ConcurrentDictionary<string, Page>());

        public async Task<Page> GetPage(string name)
        {
            if (!_pageSessions.Value.TryGetValue(name, out var pageSession))
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions()
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox" }
                });

                pageSession = await browser.NewPageAsync();
                _pageSessions.Value.TryAdd(name, pageSession);
            }

            return pageSession;
        }

        public void DisposePage(string name)
        {
            if (!_pageSessions.Value.TryGetValue(name, out var pageSession))
            {
                return;
            }

            pageSession.Dispose();

            _pageSessions.Value.TryRemove(name, out _);
        }
    }
}