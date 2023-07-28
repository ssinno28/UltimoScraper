using System.Collections.Generic;
using System.Threading.Tasks;
using UltimoScraper.Models;

namespace UltimoScraper.Interfaces.Processors
{
    public interface ILinkProcessor
    {
        Task<bool> Process(ParsedWebLink webLink, IList<Keyword> keywords);
    }
}