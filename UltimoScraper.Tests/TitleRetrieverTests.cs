using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Retrievers.Title;
using UltimoScraper.Tests.Helpers;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class TitleRetrieverTests
    {
        private IServiceProvider _applicationContainer;
        private string _tableList;

        [SetUp]
        public void FixtureSetup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "table-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _tableList = reader.ReadToEnd();
            }

            // Create the container builder.
            var serviceCollection = new ServiceCollection()
                .AddScoped<ITitleRetriever, MetaTitleRetriever>()
                .AddScoped<ITitleRetriever, H1TitleRetriever>();

            _applicationContainer = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public void TestMetaTitleRetriever()
        {
            var metaTitleRetriever =
                _applicationContainer.GetService<IEnumerable<ITitleRetriever>>()
                    .First(x => x.GetType() == typeof(MetaTitleRetriever));

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_tableList);

            var result = metaTitleRetriever.GetTitle(doc.DocumentNode);
            Assert.Equals("Title Test", result);
        }

        [Test]
        public void TestH1TitleRetriever()
        {
            var h1TitleRetriever =
                _applicationContainer.GetService<IEnumerable<ITitleRetriever>>()
                    .First(x => x.GetType() == typeof(H1TitleRetriever));

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_tableList);

            var result = h1TitleRetriever.GetTitle(doc.DocumentNode);
            Assert.Equals("Title H1 Test", result);
        }
    }
}
