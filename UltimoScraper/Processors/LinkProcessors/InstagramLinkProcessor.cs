using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;

namespace UltimoScraper.Processors.LinkProcessors
{
    public class InstagramLinkProcessor : ILinkProcessor
    {
        public async Task<bool> Process(ParsedWebLink webLink, IList<Keyword> keywords)
        {
            var match = Regex.Match(webLink.Value, @"(?:(?:http|https):\/\/)?(?:www.)?instagram.com\/(?:(?:\w)*#!\/)?");
            return match.Success;
        }
    }
}