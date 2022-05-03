using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UltimoScraper.CommandLine.Helpers;
using UltimoScraper.CommandLine.Services;
using UltimoScraper.Helpers;

namespace UltimoScraper.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            string domain = args.ToList().GetArgument("--Domain");
            string path = args.ToList().GetArgument("--Path");
            string keywords = args.ToList().GetArgument("--Keywords");

            if (!args.Any())
            {
                bool quitNow = false;
                while (!quitNow)
                {
                    var consoleInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(consoleInput)) continue;

                    try
                    {
                        var inputArgs = consoleInput.MakeArgs();
                        domain = inputArgs.GetArgument("--Domain");
                        path = inputArgs.GetArgument("--Path");
                        keywords = inputArgs.GetArgument("--Keywords");

                        quitNow = true;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Could not scrape site!! ex: {ex.Message}");
                        break;
                    }
                }
            }

            MainAsync(GetContainer(), domain, path, !string.IsNullOrEmpty(keywords) ? keywords.Split(',') : new string[] { }).Wait();
        }

        static IServiceProvider GetContainer()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile($"appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();

            var isDevelopment = string.IsNullOrEmpty(env) || env.ToLower() == "Development";
            if (isDevelopment) //only add secrets in development
            {
                builder.AddUserSecrets<ScraperService>();
            }

            // Create the container builder.
            var serviceCollection = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddWebScraper(builder.Build())
                .AddScoped<IScraperService, ScraperService>();

            return serviceCollection.BuildServiceProvider();
        }

        static async Task MainAsync(IServiceProvider container, string domain, string path, string[] keywords)
        {
            var scraperService = container.GetService<IScraperService>();

            if (!string.IsNullOrEmpty(domain))
            {
                Console.WriteLine($"scraping site for site id: {domain}");
                await scraperService.ScrapeSite(domain, keywords);
            }

            if (!string.IsNullOrEmpty(path))
            {
                Console.WriteLine($"Scraping page for page id {path}");
                await scraperService.ScrapePage(domain, path, keywords);
            }
        }
    }
}
