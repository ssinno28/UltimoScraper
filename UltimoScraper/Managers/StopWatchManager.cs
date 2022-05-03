using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimoScraper.Interfaces;

namespace UltimoScraper.Managers
{
    public class StopWatchManager : IStopWatchManager
    {
        private readonly Lazy<ConcurrentDictionary<string, Stopwatch>> _stopWatches =
            new Lazy<ConcurrentDictionary<string, Stopwatch>>(() => new ConcurrentDictionary<string, Stopwatch>());

        public async Task<Stopwatch> GetStopWatch(string name)
        {
            if (!_stopWatches.Value.TryGetValue(name, out var stopWatch))
            {
                stopWatch = Stopwatch.StartNew();
                stopWatch.Stop();
                _stopWatches.Value.TryAdd(name, stopWatch);
            }

            return stopWatch;
        }
    }
}