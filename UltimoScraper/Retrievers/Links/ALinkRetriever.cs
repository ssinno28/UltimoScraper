using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;

namespace UltimoScraper.Retrievers.Links
{
    public abstract class ALinkRetriever
    {
        private readonly IEnumerable<ILinkProcessor> _linkProcessors;

        protected ALinkRetriever(IEnumerable<ILinkProcessor> linkProcessors)
        {
            _linkProcessors = linkProcessors;
        }

        protected async Task<IList<ParsedWebLink>> ProcessLinks(
            IEnumerable<ParsedWebLink> webLinks,
            IList<IgnoreRule> linkIgnoreRules,
            IList<Keyword> keywords)
        {
            if (!webLinks.Any()) return new List<ParsedWebLink>();

            IList<ParsedWebLink> parsedWebLinks = new List<ParsedWebLink>();
            foreach (var linkProcessor in _linkProcessors)
            {
                foreach (var webLink in webLinks)
                {
                    bool ignoreLink = false;
                    bool ignoreText = false;

                    try
                    {
                        ignoreLink = linkIgnoreRules.Any(x => Regex.Match(webLink.Value, x.Rule).Success);
                        ignoreText = linkIgnoreRules.Any(x => Regex.Match(webLink.Text, x.Rule).Success);
                    }
                    catch (Exception ex)
                    {
                        // swallow for now;
                    }

                    if (!await linkProcessor.Process(webLink, keywords) || ignoreLink || ignoreText) continue;

                    parsedWebLinks.Add(webLink);
                }
            }

            return parsedWebLinks;
        }
    }
}