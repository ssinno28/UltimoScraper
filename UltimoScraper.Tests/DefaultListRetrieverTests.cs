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
using UltimoScraper.Threaders;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class DefaultListRetrieverTests
    {
        private IServiceProvider _applicationContainer;
        private string _fullPageSample;
        private string _noListSample;

        [SetUp]
        public void FixtureSetup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "full-page-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _fullPageSample = reader.ReadToEnd();
            }

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "no-list-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _noListSample = reader.ReadToEnd();
            }

            // Create the container builder.
            var serviceCollection = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddScoped<IListRetriever, DefaultListRetriever>()
                .AddScoped<IListItemRetriever, DefaultListItemRetriever>()
                .AddScoped<IListItemRetriever, MultpleElementListItemRetriever>();

            _applicationContainer = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task Test_Skips_Adding_List_With_No_Items()
        {
            var listRetriever = _applicationContainer.GetService<IListRetriever>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_noListSample);

            var result = 
                await listRetriever.GetParsedLists(doc.DocumentNode.SelectSingleNode("/html[1]/body[1]"), new List<IgnoreRule>());
            Assert.Equals(result.Count, 0);
        }

        [Test]
        public async Task Gets_Correct_List()
        {
            string html = new BrokenDivThreader().Thread(_fullPageSample);  

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var listRetriever = _applicationContainer.GetService<IListRetriever>();
            var result = 
                await listRetriever.GetParsedLists(doc.DocumentNode.SelectSingleNode("/html[1]/body[1]"), new List<IgnoreRule>());

            Assert.That(result.Count, Is.GreaterThan(0));
        }
    }
}
