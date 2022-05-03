using Microsoft.Extensions.Logging;

namespace UltimoScraper.Tests.Logger
{
    public class NUnitLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = new NUnitLogger();
            return logger;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }
    }
}