using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using UltimoScraper.Dictionary;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Interfaces.Threaders;
using UltimoScraper.Models;
using UltimoScraper.Processors.LinkProcessors;

namespace UltimoScraper.Parsers
{
    public class DefaultWebParser : IWebParser
    {
        private readonly object _thisLock = new object();

        private readonly IEnumerable<ILinkRetriever> _linkRetrievers;
        private readonly IListRetriever _listRetriever;
        private readonly IEnumerable<IHtmlThreader> _htmlThreaders;
        private readonly IEnumerable<IHtmlDocThreader> _htmlDocThreaders;
        private readonly IEnumerable<ITitleRetriever> _titleRetrievers;
        private readonly IEnumerable<ILinkProcessor> _linkProcessors;
        private readonly IEnumerable<IPageInteraction> _pageInteractions;
        private readonly ILogger<DefaultWebParser> _logger;
        private readonly IPageManager _viewManager;
        private readonly Action<string> _throttleFunc;
        private readonly ScraperConfig _scraperConfig;

        public DefaultWebParser(
            IEnumerable<ILinkRetriever> linkRetrievers,
            IListRetriever listRetriever,
            IEnumerable<IHtmlThreader> htmlThreaders,
            IEnumerable<ITitleRetriever> titleRetrievers,
            IEnumerable<IHtmlDocThreader> htmlDocThreaders,
            IEnumerable<ILinkProcessor> linkProcessors,
            IEnumerable<IPageInteraction> pageInteractions,
            ILogger<DefaultWebParser> logger,
            IPageManager viewManager,
            Action<string> throttleFunc,
            IOptions<ScraperConfig> scraperConfigOptions)
        {
            _linkRetrievers = linkRetrievers;
            _listRetriever = listRetriever;
            _htmlThreaders = htmlThreaders;
            _titleRetrievers = titleRetrievers;
            _htmlDocThreaders = htmlDocThreaders;
            _linkProcessors = linkProcessors;
            _pageInteractions = pageInteractions;
            _logger = logger;
            _viewManager = viewManager;
            _throttleFunc = throttleFunc;
            _scraperConfig = scraperConfigOptions.Value;
        }

        public async Task<ParsedSite> ParseSite(
            string domain,
            IList<IgnoreRule> ignoreRules,
            IList<string> keywords,
            string sessionName = null)
        {
            var siteLink = new ParsedWebLink { Value = domain };
            var knownLinks = new List<ParsedWebLink>();
            var pagesPerKeyword = new Dictionary<string, IList<ParsedPage>>();

            if (string.IsNullOrEmpty(sessionName))
            {
                sessionName = $"scrape-run-{DateTime.Now:O}";
            }

            var parsedPages =
                await ParsePages(
                    new Uri(domain),
                    siteLink,
                    knownLinks,
                    ignoreRules,
                    keywords,
                    pagesPerKeyword,
                    sessionName);

            return new ParsedSite
            {
                Domain = domain,
                StartPage = parsedPages,
                FacebookLink = await GetFacebookLink(knownLinks),
                InstagramLink = await GetInstagramLink(knownLinks),
                TwitterLink = await GetTwitterLink(knownLinks),
                PagesPerKeyword = pagesPerKeyword
            };
        }

        private async Task<ParsedWebLink> GetFacebookLink(IList<ParsedWebLink> knownLinks)
        {
            var linkProcessor = _linkProcessors.First(x => x.GetType() == typeof(FacebookLinkProcessor));
            foreach (var parsedWebLink in knownLinks)
            {
                if (await linkProcessor.Process(parsedWebLink, new List<string>())) return parsedWebLink;
            }

            return null;
        }

        private async Task<ParsedWebLink> GetInstagramLink(IList<ParsedWebLink> knownLinks)
        {
            var linkProcessor = _linkProcessors.First(x => x.GetType() == typeof(InstagramLinkProcessor));
            foreach (var parsedWebLink in knownLinks)
            {
                if (await linkProcessor.Process(parsedWebLink, new List<string>())) return parsedWebLink;
            }

            return null;
        }

        private async Task<ParsedWebLink> GetTwitterLink(IList<ParsedWebLink> knownLinks)
        {
            var linkProcessor = _linkProcessors.First(x => x.GetType() == typeof(TwitterLinkProcessor));
            foreach (var parsedWebLink in knownLinks)
            {
                if (await linkProcessor.Process(parsedWebLink, new List<string>())) return parsedWebLink;
            }

            return null;
        }

        public async Task<ParsedPage> ParsePage(string domain, string path, IList<IgnoreRule> ignoreRules, IList<string> keywords, string sessionName = null)
        {
            sessionName =
                string.IsNullOrEmpty(sessionName) ? $"scrape-run-{DateTime.Now:O}" : sessionName;

            HtmlDocument doc = await GetPageHtml(new Uri(domain), path, sessionName);
            if (doc == null) return null;

            var bodyHtmlNode = doc.DocumentNode.QuerySelector("body");
            var parsedPage = new ParsedPage
            {
                BodyHtmlNode = bodyHtmlNode,
                Url = path,
                Title = GetDocumentTitle(doc),
                WebLinks = await GetDocumentLinks(doc, ignoreRules, keywords),
                ParsedLists = await _listRetriever.GetParsedLists(bodyHtmlNode, ignoreRules)
            };

            return parsedPage;
        }

        private string GetDocumentTitle(HtmlDocument doc)
        {
            string title = null;
            foreach (var titleRetriever in _titleRetrievers.OrderByDescending(x => x.Priority))
            {
                string result = titleRetriever.GetTitle(doc.DocumentNode);
                if (string.IsNullOrEmpty(result)) continue;

                title = result;
                break;
            }

            return title;
        }

        private async Task<List<ParsedWebLink>> GetDocumentLinks(HtmlDocument doc, IList<IgnoreRule> ignoreRules, IList<string> keywords)
        {
            var linkIgnoreRules =
                ignoreRules.Where(x => x.IgnoreRuleType == IgnoreRuleType.Link)
                    .ToList();

            List<ParsedWebLink> webLinks = new List<ParsedWebLink>();
            foreach (var linkRetriever in _linkRetrievers)
            {
                webLinks.AddRange(await linkRetriever.GetLinks(doc.DocumentNode, linkIgnoreRules, keywords));
            }

            return webLinks;
        }

        private async Task<HtmlDocument> GetPageHtml(Uri domain, string url, string sessionName)
        {
            var uriCreated = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri);
            if (!uriCreated)
            {
                return null;
            }

            try
            {
                string urlWithScheme = uri.IsAbsoluteUri
                        ? uri.AbsoluteUri
                        : $"{domain.Scheme}://{domain.Authority}/{url}";

                var doc = new HtmlDocument();

                _throttleFunc(sessionName);

                var page = await _viewManager.GetPage(sessionName);
                string decodedString = HttpUtility.HtmlDecode(urlWithScheme);

                _logger.LogDebug($"Starting parse of {decodedString} for domain {domain}");

                var pageTimeout = _scraperConfig.PageTimeout == 0 ? 5000 : _scraperConfig.PageTimeout;
                await page.GoToAsync(decodedString, pageTimeout, new[]
                {
                    WaitUntilNavigation.DOMContentLoaded
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
                _logger.LogError(ex, $"Could not download page for url {url}");
                return null;
            }
        }

        private async Task<ParsedPage> ParsePages(
            Uri domain,
            ParsedWebLink linkToPage,
            List<ParsedWebLink> knownLinks,
            IList<IgnoreRule> ignoreRules,
            IList<string> keywords,
            IDictionary<string, IList<ParsedPage>> pagesPerKeyword,
            string sessionName)
        {

            HtmlDocument doc = await GetPageHtml(domain, linkToPage.Value, sessionName);
            if (doc == null) return null;

            ParsedPage parsedPage = new ParsedPage { ChildPages = new List<ParsedPage>() };

            List<ParsedWebLink> webLinks = await GetDocumentLinks(doc, ignoreRules, keywords);
            webLinks =
                webLinks.Where(x => 
                        !string.IsNullOrEmpty(x.Value) && knownLinks.All(kl => kl.Value.NotSameUri(x.Value, domain)) && 
                        !x.Value.StartsWith("#"))
                    .GroupBy(x => x.Value)
                    .Select(x => x.First())
                    .ToList();

            lock (_thisLock)
            {
                knownLinks.AddRange(webLinks);
            }

            foreach (var webLink in webLinks)
            {
                if (!webLink.Value.IsSiteDomain(domain))
                {
                    continue;
                }

                if(webLink.Value.StartsWith("mailto")) continue;

                parsedPage.ChildPages.Add(await ParsePages(domain, webLink, knownLinks, ignoreRules, keywords, pagesPerKeyword, sessionName));
            }

            string title = GetDocumentTitle(doc) ?? linkToPage.Value;
            var bodyHtmlNode = doc.DocumentNode.QuerySelector("body");

            string text =
                bodyHtmlNode != null
                ? bodyHtmlNode.InnerText.ToLower()
                : String.Empty;

            List<string> matchedKeywords = new List<string>();
            foreach (var keyword in keywords)
            {
                if (!text.Contains(keyword.ToLower()))
                {
                    continue;
                }

                matchedKeywords.Add(keyword.ToLower());
                if (pagesPerKeyword.TryGetValue(keyword, out var matchedPages))
                {
                    matchedPages.Add(parsedPage);
                }
                else
                {
                    pagesPerKeyword.Add(keyword, new List<ParsedPage> { parsedPage });
                }
            }

            parsedPage.WebLinks = webLinks;
            parsedPage.BodyHtmlNode = bodyHtmlNode;
            parsedPage.Url = linkToPage.Value;
            parsedPage.Title = title;
            parsedPage.MatchedKeywords = matchedKeywords;
            parsedPage.LinkToPage = linkToPage;

            return parsedPage;
        }
    }
}