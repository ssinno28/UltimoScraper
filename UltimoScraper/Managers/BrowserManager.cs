using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.AnonymizeUa;
using PuppeteerSharp;
using UltimoScraper.Interfaces;
using UltimoScraper.Models;

namespace UltimoScraper.Managers
{
    public class BrowserManager : IBrowserManager
    {
        private readonly ILogger<BrowserManager> _logger;
        private readonly ScraperConfig _scraperConfig;
        private readonly Lazy<ConcurrentDictionary<string, IBrowser>> _browsers =
            new Lazy<ConcurrentDictionary<string, IBrowser>>(() => new ConcurrentDictionary<string, IBrowser>());


        public BrowserManager(ILogger<BrowserManager> logger,
            IOptions<ScraperConfig> scraperConfig)
        {
            _logger = logger;
            _scraperConfig = scraperConfig.Value;
        }

        public List<IBrowser> Browsers => _browsers.Value.Select(x => x.Value).ToList();

        public async Task<IBrowser> GetBrowser(string name)
        {
            var chromeProcesses = Process.GetProcesses().Where(x => x.ProcessName.Contains("chrome"));
            if (chromeProcesses.Count() > (_scraperConfig.MaxProcesses ?? 10))
            {
                _logger.LogDebug($"Max number of processes hit for session {name}, disposing browser and restarting");
                await DisposeBrowser(name);
            }

            if (!_browsers.Value.TryGetValue(name, out var browser))
            {
                // Initialization plugin builder
                var extra = new PuppeteerExtra();

                // Use stealth plugin
                extra.Use(new StealthPlugin());
                extra.Use(new AnonymizeUaPlugin());

                var installedBrowser = await new BrowserFetcher(SupportedBrowser.Chrome).DownloadAsync();
                browser = await extra.LaunchAsync(new LaunchOptions()
                {
                    Headless = true,
                    Browser = SupportedBrowser.Chrome,
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

            int processId = browser.Process.Id;
            var pages = await browser.PagesAsync();
            foreach (var page in pages)
            {
                if (!page.IsClosed)
                {
                    await page.CloseAsync();
                }
            }

            await browser.CloseAsync();

            var chromeProcess = Process.GetProcesses().FirstOrDefault(x => x.Id == processId);
            if (chromeProcess != null)
            {
                _logger.LogDebug($"Chrome process found for browser {processId}, killing now.");
                chromeProcess.Kill();
                _logger.LogDebug($"Chrome process {processId} killed.");
            }

            _browsers.Value.TryRemove(name, out _);
        }
    }
}