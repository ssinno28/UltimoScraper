using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;

namespace UltimoScraper.Processors.LinkProcessors
{
    public class TwitterLinkProcessor : ILinkProcessor
    {
        public async Task<bool> Process(ParsedWebLink webLink, IList<Keyword> keywords)
        {
            var match = Regex.Match(webLink.Value, @"(?:(?:http|https):\/\/)?(?:www.)?twitter.com\/(?:(?:\w)*#!\/)?");
            return match.Success;
        }
    }
}