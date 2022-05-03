using System;
using Microsoft.Extensions.Logging;

namespace UltimoScraper.Tests.Logger
{
    class NUnitLogger : ILogger, IDisposable
    {
        private readonly Action<string> output = Console.WriteLine;

        public void Dispose()
        {
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter) => output(formatter(state, exception));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => this;
    }
}