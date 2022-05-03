using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;

namespace UltimoScraper.Processors.ListProcessors
{
    public class NoTableHeaderListProcessor : IListProcessor
    {
        public async Task<bool> Process(ParsedList list, IList<IgnoreRule> ignoreRules)
        {
            if (list.Parent == null || !list.Parent.Name.Equals("table"))
            {
                return true;
            }

            //TODO: extract to separate rules list
            var headers = list.Parent.SelectNodes("//tr[@bgcolor='#000080']");
            if (headers != null && headers.Any())
            {
                return true;
            }

            var tableRows = list.Parent.Elements("tr").ToList();
            var tableHeads = list.Parent.Elements("thead").ToList();
            if (!tableRows.Any() && !tableHeads.Any())
            {
                return false;
            }

            var tableHeaders = tableRows.SelectMany(x => x.Elements("th"));
            var tableHeadHeaders = tableHeads.SelectMany(x => x.Elements("th"));
            if (!tableHeaders.Any() && !tableHeadHeaders.Any())
            {
                return false;
            }

            return true;
        }
    }
}