using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltimoScraper.Models;

namespace UltimoScraper.Interfaces.Retrievers
{
    public interface IRobotsTxtRetriever
    {
        Task<IList<IgnoreRule>> GetRobotsTxt(Uri domain);
    }
}