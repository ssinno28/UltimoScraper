using System.Diagnostics;
using System.Threading.Tasks;

namespace UltimoScraper.Interfaces
{
    public interface IStopWatchManager
    {
        Task<Stopwatch> GetStopWatch(string name);
    }
}