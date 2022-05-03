using System;
using System.Collections.Generic;
using System.IO;
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
    public class DefaultListItemRetrieverTests
    {
        private IServiceProvider _applicationContainer;
        private string _tableList;
        private string _fullPageSample;

        [SetUp]
        public void FixtureSetup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "table-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _tableList = reader.ReadToEnd();
            }

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "DefaultListItemRetriever", "full-page-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _fullPageSample = reader.ReadToEnd();
            }

            // Create the container builder.
            var serviceCollection = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddScoped<IListRetriever, DefaultListRetriever>()
                .AddScoped<IListItemRetriever, DefaultListItemRetriever>();

            _applicationContainer = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task Test_Get_Table()
        {
            var listRetriever = _applicationContainer.GetService<IListRetriever>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_tableList);

            var result = 
                await listRetriever.GetParsedLists(doc.DocumentNode.SelectSingleNode("/html[1]/body[1]"), new List<IgnoreRule>());

            Assert.Greater(result.Count, 0);
        }

        [Test]
        public async Task Parses_Table_Out_Of_Full_Page()
        {
            var listRetriever = _applicationContainer.GetService<IListRetriever>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_fullPageSample);

            var result = await listRetriever.GetParsedLists(doc.DocumentNode.SelectSingleNode("/html[1]/body[1]"), new List<IgnoreRule>());
            Assert.Greater(result.Count, 0);
        }
    }
}
