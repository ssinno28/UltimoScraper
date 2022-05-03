using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltimoScraper.Dictionary;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;

namespace UltimoScraper.Processors.ListProcessors
{
    public class IgnoreRuleListProcessor : IListProcessor
    {
        public async Task<bool> Process(ParsedList list, IList<IgnoreRule> ignoreRules)
        {
            if (ignoreRules.Any(x => x.Rule.Equals(list.XPath) && x.IgnoreRuleType == IgnoreRuleType.List))
            {
                return false;
            }

            return true;
        }
    }
}