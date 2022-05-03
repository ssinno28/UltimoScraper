using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Models;
using UltimoScraper.Retrievers.ListItems;
using UltimoScraper.Retrievers.Lists;
using UltimoScraper.Tests.Helpers;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class MultpleElementListItemRetrieverTests
    {
        private IServiceProvider _applicationContainer;
        private string _paragraphList;

        [SetUp]
        public void FixtureSetup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "p-list-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _paragraphList = reader.ReadToEnd();
            }

            // Create the container builder.
            var serviceCollection = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddScoped<IListRetriever, DefaultListRetriever>()
                .AddScoped<IListItemRetriever, MultpleElementListItemRetriever>();

            _applicationContainer = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task TestGetParagraphList()
        {
            var listRetriever = _applicationContainer.GetService<IListRetriever>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_paragraphList);

            var result = 
                await listRetriever.GetParsedLists(doc.DocumentNode.SelectSingleNode("/html[1]/body[1]"), new List<IgnoreRule>());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(8, result.First().ListItems.Count);
        }
    }
}
