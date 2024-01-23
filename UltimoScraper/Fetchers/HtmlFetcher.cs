using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using UltimoScraper.Interfaces;
using UltimoScraper.Interfaces.Threaders;
using UltimoScraper.Models;
using UltimoScraper.Parsers;

namespace UltimoScraper.Fetchers;

public class HtmlFetcher : IHtmlFetcher
{
    private readonly ILogger<HtmlFetcher> _logger;
    private readonly IBrowserManager _viewManager;
    private readonly Action<string> _throttleFunc;
    private readonly ScraperConfig _scraperConfig;
    private readonly IEnumerable<IHtmlThreader> _htmlThreaders;
    private readonly IEnumerable<IHtmlDocThreader> _htmlDocThreaders;
    private readonly IEnumerable<IPageInteraction> _pageInteractions;

    public HtmlFetcher(ILogger<HtmlFetcher> logger,
        IBrowserManager viewManager,
        Action<string> throttleFunc,
        IOptions<ScraperConfig> scraperConfig,
        IEnumerable<IHtmlThreader> htmlThreaders,
        IEnumerable<IHtmlDocThreader> htmlDocThreaders,
        IEnumerable<IPageInteraction> pageInteractions)
    {
        _logger = logger;
        _viewManager = viewManager;
        _throttleFunc = throttleFunc;
        _scraperConfig = scraperConfig.Value;
        _htmlThreaders = htmlThreaders;
        _htmlDocThreaders = htmlDocThreaders;
        _pageInteractions = pageInteractions;
    }

    public async Task<HtmlDocument> GetPageHtml(Uri domain, string url, string sessionName)
    {
        var uriCreated = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri);
        if (!uriCreated)
        {
            return null;
        }

        string urlWithScheme = uri.IsAbsoluteUri
                ? uri.AbsoluteUri
                : $"{domain.Scheme}://{domain.Authority}/{url}";

        var doc = new HtmlDocument();

        _throttleFunc(sessionName);

        var browser = await _viewManager.GetBrowser(sessionName);
        string decodedString = HttpUtility.HtmlDecode(urlWithScheme);
        _logger.LogDebug($"Starting parse of {decodedString} for domain {domain}");

        var pageTimeout = _scraperConfig.PageTimeout == 0 ? 5000 : _scraperConfig.PageTimeout;
        var page = await browser.NewPageAsync();

        try
        {
            await page.GoToAsync(decodedString, pageTimeout, new[]
            {
                WaitUntilNavigation.Load
            });

            var pageInteraction =
                _pageInteractions.FirstOrDefault(x => x.IsMatch(urlWithScheme));

            if (pageInteraction != null)
            {
                await pageInteraction.Interact(page);
            }

            string html = await page.EvaluateExpressionAsync<string>("document.documentElement.innerHTML");
            html = _htmlThreaders.Aggregate(html, (current, htmlThreader) => htmlThreader.Thread(current));

            doc.LoadHtml(html);

            foreach (var htmlDocThreader in _htmlDocThreaders)
            {
                doc = htmlDocThreader.Thread(doc);
            }

            _logger.LogDebug($"Finished parse of page {decodedString} for domain {domain}");

            return doc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Could not go to page {decodedString}");
            return null;
        }
        finally
        {
            if (!page.IsClosed)
            {
                await page.GoToAsync("about:blank");
                await page.CloseAsync();
                string closed = page.IsClosed ? "Closed" : "Not Closed";
                _logger.LogInformation($"Page {page.Url} is {closed}.");
            }
        }
    }
}