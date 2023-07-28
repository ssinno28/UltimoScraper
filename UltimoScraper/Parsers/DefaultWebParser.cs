using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using UltimoScraper.Dictionary;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;
using UltimoScraper.Processors.LinkProcessors;

namespace UltimoScraper.Parsers
{
    public class DefaultWebParser : IWebParser
    {
        private readonly object _thisLock = new object();

        private readonly IEnumerable<ILinkRetriever> _linkRetrievers;
        private readonly IEnumerable<ITitleRetriever> _titleRetrievers;
        private readonly IEnumerable<ILinkProcessor> _linkProcessors;
        private readonly ILogger<DefaultWebParser> _logger;
        private readonly IHtmlFetcher _htmlFetcher;

        public DefaultWebParser(
            IEnumerable<ILinkRetriever> linkRetrievers,
            IEnumerable<ITitleRetriever> titleRetrievers,
            IEnumerable<ILinkProcessor> linkProcessors,
            ILogger<DefaultWebParser> logger,
            IHtmlFetcher htmlFetcher)
        {
            _linkRetrievers = linkRetrievers;
            _titleRetrievers = titleRetrievers;
            _linkProcessors = linkProcessors;
            _logger = logger;
            _htmlFetcher = htmlFetcher;
        }

        public async Task<ParsedSite> ParseSite(
            string domain,
            IList<IgnoreRule> ignoreRules,
            IList<Keyword> keywords,
            string sessionName = null)
        {
            var siteLink = new ParsedWebLink { Value = domain };
            var knownLinks = new List<ParsedWebLink>();
            var pagesPerKeyword = new Dictionary<string, IList<ParsedPage>>();

            if (string.IsNullOrEmpty(sessionName))
            {
                sessionName = $"scrape-run-{DateTime.Now:O}";
            }

            ignoreRules.AddDefaultIgnoreRules();
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

        public async Task<IList<string>> KeywordSearch(
            string domain,
            IList<IgnoreRule> ignoreRules,
            IList<Keyword> keywords,
            IList<Keyword> searchKeywords,
            string sessionName = null)
        {
            ignoreRules.AddDefaultIgnoreRules();

            var siteLink = new ParsedWebLink { Value = domain };
            var knownLinks = new List<ParsedWebLink>();

            if (string.IsNullOrEmpty(sessionName))
            {
                sessionName = $"scrape-run-{DateTime.Now:O}";
            }

            return await FindKeywords(
                new Uri(domain),
                siteLink,
                knownLinks,
                ignoreRules,
                keywords,
                searchKeywords,
                sessionName
            );
        }

        private async Task<ParsedWebLink> GetFacebookLink(IList<ParsedWebLink> knownLinks)
        {
            var linkProcessor = _linkProcessors.First(x => x.GetType() == typeof(FacebookLinkProcessor));
            foreach (var parsedWebLink in knownLinks)
            {
                if (await linkProcessor.Process(parsedWebLink, new List<Keyword>())) return parsedWebLink;
            }

            return null;
        }

        private async Task<ParsedWebLink> GetInstagramLink(IList<ParsedWebLink> knownLinks)
        {
            var linkProcessor = _linkProcessors.First(x => x.GetType() == typeof(InstagramLinkProcessor));
            foreach (var parsedWebLink in knownLinks)
            {
                if (await linkProcessor.Process(parsedWebLink, new List<Keyword>())) return parsedWebLink;
            }

            return null;
        }

        private async Task<ParsedWebLink> GetTwitterLink(IList<ParsedWebLink> knownLinks)
        {
            var linkProcessor = _linkProcessors.First(x => x.GetType() == typeof(TwitterLinkProcessor));
            foreach (var parsedWebLink in knownLinks)
            {
                if (await linkProcessor.Process(parsedWebLink, new List<Keyword>())) return parsedWebLink;
            }

            return null;
        }

        public async Task<ParsedPage> ParsePage(string domain, string path, IList<IgnoreRule> ignoreRules, IList<Keyword> keywords, string sessionName = null)
        {
            sessionName =
                string.IsNullOrEmpty(sessionName) ? $"scrape-run-{DateTime.Now:O}" : sessionName;

            HtmlDocument doc = await _htmlFetcher.GetPageHtml(new Uri(domain), path, sessionName);
            if (doc == null) return null;

            var bodyHtmlNode = doc.DocumentNode.QuerySelector("body");
            var parsedPage = new ParsedPage
            {
                BodyHtmlNode = bodyHtmlNode,
                Url = path,
                Title = GetDocumentTitle(doc),
                WebLinks = await GetDocumentLinks(doc, ignoreRules, keywords)
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

        private async Task<List<ParsedWebLink>> GetDocumentLinks(HtmlDocument doc, IList<IgnoreRule> ignoreRules, IList<Keyword> keywords)
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

        private async Task<ParsedPage> ParsePages(
            Uri domain,
            ParsedWebLink linkToPage,
            List<ParsedWebLink> knownLinks,
            IList<IgnoreRule> ignoreRules,
            IList<Keyword> keywords,
            IDictionary<string, IList<ParsedPage>> pagesPerKeyword,
            string sessionName)
        {

            HtmlDocument doc = await _htmlFetcher.GetPageHtml(domain, linkToPage.Value, sessionName);
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

                if (webLink.Value.StartsWith("mailto") ||
                    webLink.Value.StartsWith("webcal") ||
                    webLink.Value.Contains("twitter") ||
                    webLink.Value.Contains("instagram") ||
                    webLink.Value.Contains("facebook")
                    ) continue;

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
                if (!text.MatchesKeyword(keyword))
                {
                    continue;
                }

                matchedKeywords.Add(keyword.Value.ToLower());
                if (pagesPerKeyword.TryGetValue(keyword.Value, out var matchedPages))
                {
                    matchedPages.Add(parsedPage);
                }
                else
                {
                    pagesPerKeyword.Add(keyword.Value, new List<ParsedPage> { parsedPage });
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

        private async Task<IList<string>> FindKeywords(
            Uri domain,
            ParsedWebLink linkToPage,
            List<ParsedWebLink> knownLinks,
            IList<IgnoreRule> ignoreRules,
            IList<Keyword> keywords,
            IList<Keyword> searchKeywords,
            string sessionName)
        {
            HtmlDocument doc = await _htmlFetcher.GetPageHtml(domain, linkToPage.Value, sessionName);
            if (doc == null) return null;

            var linkKeywords = new List<Keyword>();
            linkKeywords.AddRange(keywords);
            linkKeywords.AddRange(searchKeywords);

            List<ParsedWebLink> webLinks = await GetDocumentLinks(doc, ignoreRules, linkKeywords);
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

            var matchedKeywords =
                webLinks
                    .SelectMany(x => x.MatchedKeywords)
                    .Distinct()
                    .ToList();

            var bodyHtmlNode = doc.DocumentNode.QuerySelector("body");
            string text =
                bodyHtmlNode != null
                ? bodyHtmlNode.InnerText.ToLower()
                : String.Empty;

            foreach (var keyword in keywords)
            {
                if (!text.MatchesKeyword(keyword))
                {
                    continue;
                }

                if (!matchedKeywords.Any(x => x.Equals(keyword.Value.ToLower())))
                {
                    matchedKeywords.Add(keyword.Value.ToLower());
                }
            }

            foreach (var matchedKeyword in matchedKeywords)
            {
                _logger.LogInformation($"Keyword {matchedKeyword} has been matched!");
                ((List<Keyword>)keywords).RemoveAll(x => x.Value.Equals(matchedKeyword));
            }

            var ignoreLinks = new List<string>() { "instagram", "facebook", "twitter" };
            foreach (var webLink in webLinks)
            {
                if (!webLink.Value.IsSiteDomain(domain))
                {
                    continue;
                }

                if (ignoreLinks.Any(x => webLink.Value.Contains(x)))
                {
                    continue;
                }

                if (matchedKeywords.Intersect(webLink.MatchedKeywords).Any())
                {
                    continue;
                }

                matchedKeywords.AddRange(await FindKeywords(domain, webLink, knownLinks, ignoreRules, keywords, searchKeywords, sessionName));
            }

            return matchedKeywords;
        }
    }
}