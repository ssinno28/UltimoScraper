using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UltimoScraper.Dictionary;
using UltimoScraper.Interfaces;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;

namespace UltimoScraper.Retrievers
{
    public class RobotsTxtRetriever : IRobotsTxtRetriever
    {
        private readonly ILogger<RobotsTxtRetriever> _logger;
        private readonly IHttpClientProvider _httpClientProvider;

        public RobotsTxtRetriever(ILogger<RobotsTxtRetriever> logger, IHttpClientProvider httpClientProvider)
        {
            _logger = logger;
            _httpClientProvider = httpClientProvider;
        }

        public async Task<IList<IgnoreRule>> GetRobotsTxt(Uri domain)
        {
            string urlWithScheme = $"{domain.Scheme}://{domain.Authority}/robots.txt";
            var uriCreated = Uri.TryCreate(urlWithScheme, UriKind.RelativeOrAbsolute, out var uri);
            if (!uriCreated)
            {
                return null;
            }

            string robotsTxt = await _httpClientProvider.GetStringFromUrl(urlWithScheme);
            if (string.IsNullOrEmpty(robotsTxt)) return new List<IgnoreRule>();

            robotsTxt = Regex.Replace(robotsTxt, @"\r\n|\r|\n", "\r\n");
            string[] ignorePaths = robotsTxt.Split(Environment.NewLine);

            var ignoreRules = new List<IgnoreRule>();
            string disallow = "disallow:";
            foreach (var ignorePath in ignorePaths)
            {
                if (!ignorePath.StartsWith(disallow)) continue;

                var ignoreRule = ignorePath.Substring(disallow.Length, ignorePath.Length - disallow.Length);
                ignoreRules.Add(new IgnoreRule
                {
                    IgnoreRuleType = IgnoreRuleType.Link,
                    Rule = ignoreRule.Trim()
                });
            }

            return ignoreRules;
        }
    }
}