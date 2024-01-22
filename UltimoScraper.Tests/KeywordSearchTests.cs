using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using UltimoScraper.Helpers;
using UltimoScraper.Interfaces;
using UltimoScraper.Models;
using UltimoScraper.Tests.Helpers;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class KeywordSearchTests
    {
        private IServiceProvider _applicationContainer;
        private string _fullPageSample;
        private Mock<IHtmlFetcher> _mockHtmlFetcher;

        [SetUp]
        public void FixtureSetup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "full-page-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _fullPageSample = reader.ReadToEnd();
            }

            var configuration = new Mock<IConfiguration>();
            _mockHtmlFetcher = new Mock<IHtmlFetcher>();

            // Create the container builder.
            var serviceCollection = new ServiceCollection()
                .AddWebScraper(configuration.Object)
                .AddLogging(x => x.AddConsole())
                .AddScoped<IHtmlFetcher>((sp) => _mockHtmlFetcher.Object);

            _applicationContainer = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task FindKeywords_DoesNotMatcheOnHockey()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_fullPageSample);

            _mockHtmlFetcher.Setup(x => x.GetPageHtml(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(doc);

            var webParser = _applicationContainer.GetService<IWebParser>();
            var keywords = new List<Keyword>()
            {
                new Keyword
                {
                    Value = "Hockey",
                    Regex = @"\b(?<!field\s)hockey\b",
                }
            };

            var result = 
                await webParser.KeywordSearch("https://www.example.com", new List<IgnoreRule>(), keywords, new List<Keyword>());
            Assert.Equals(0, result.Count);
        }
        
        [Test]
        public async Task FindKeywords_MatchesOnFieldHockey()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_fullPageSample);

            _mockHtmlFetcher.Setup(x => x.GetPageHtml(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(doc);

            var webParser = _applicationContainer.GetService<IWebParser>();
            var keywords = new List<Keyword>()
            {
                new Keyword
                {
                    Value = "Hockey",
                    Regex = @"\b(?<!field\s)hockey\b",
                },
                new Keyword
                {
                    Value = "Field Hockey"
                },
            };

            var result = 
                await webParser.KeywordSearch("https://www.example.com", new List<IgnoreRule>(), keywords, new List<Keyword>());
            Assert.Equals(1, result.Count);
        }
    }
}
