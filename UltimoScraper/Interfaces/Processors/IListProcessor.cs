using System.Collections.Generic;
using System.Threading.Tasks;
using UltimoScraper.Models;

namespace UltimoScraper.Interfaces.Processors
{
    public interface IListProcessor
    {
        Task<bool> Process(ParsedList list, IList<IgnoreRule> ignoreRules);
    }
}