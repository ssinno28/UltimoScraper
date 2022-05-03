using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UltimoScraper.Interfaces;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Interfaces.Threaders;
using UltimoScraper.Managers;
using UltimoScraper.Models;
using UltimoScraper.Parsers;
using UltimoScraper.Retrievers.Lists;

namespace UltimoScraper.Helpers
{
    public static class ServiceCollectionHelpers
    {
        public static readonly Func<IServiceProvider, Action<string>> ThrottleFunc = (provider) =>
            async (sessionName) =>
            {
                var stopWatchManager = provider.GetService<IStopWatchManager>();
                var stopWatch = await stopWatchManager.GetStopWatch(sessionName);
                var configOptions = provider.GetService<IOptions<ScraperConfig>>();

                if (!stopWatch.IsRunning) stopWatch.Start();

                while (stopWatch.ElapsedMilliseconds < configOptions.Value.PageThrottle)
                {
                    // wait
                }

                stopWatch.Reset();
                stopWatch.Start();
            };

        public static IServiceCollection AddWebScraper(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddScoped<IWebParser, DefaultWebParser>();
            serviceCollection.AddScoped<IListRetriever, DefaultListRetriever>();
            serviceCollection.AddSingleton<IPageManager, PageManager>();
            serviceCollection.AddSingleton<IStopWatchManager, StopWatchManager>();
            serviceCollection.Configure<ScraperConfig>(config =>
            {
                config.PageThrottle = Convert.ToInt32(configuration["Scraper:PageThrottle"]);
                config.PageTimeout = Convert.ToInt32(configuration["Scraper:PageTimeout"]);
            });

            serviceCollection.AddTransient(ThrottleFunc);

            var assembly = Assembly.GetAssembly(typeof(ILinkRetriever));
            foreach (var exportedType in assembly.DefinedTypes)
            {
                if (exportedType.ImplementedInterfaces.Contains(typeof(ILinkRetriever)))
                {
                    serviceCollection.AddScoped(typeof(ILinkRetriever), exportedType);
                }
                
                if (exportedType.ImplementedInterfaces.Contains(typeof(ITitleRetriever)))
                {
                    serviceCollection.AddScoped(typeof(ITitleRetriever), exportedType);
                }

                if (exportedType.ImplementedInterfaces.Contains(typeof(ILinkProcessor)))
                {
                    serviceCollection.AddScoped(typeof(ILinkProcessor), exportedType);
                }
                
                if (exportedType.ImplementedInterfaces.Contains(typeof(IListProcessor)))
                {
                    serviceCollection.AddScoped(typeof(IListProcessor), exportedType);
                }

                if (exportedType.ImplementedInterfaces.Contains(typeof(IHtmlThreader)))
                {
                    serviceCollection.AddScoped(typeof(IHtmlThreader), exportedType);
                }
            }

            return serviceCollection;
        }
    }
}