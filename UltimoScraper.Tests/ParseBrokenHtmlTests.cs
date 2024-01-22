using System.IO;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;
using NUnit.Framework;
using UltimoScraper.Tests.Helpers;
using UltimoScraper.Threaders;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class ParseBrokenHtmlTests
    {
        private string _brokenParagraphList;
        private string _unopenedElementsHtml;

        [SetUp]
        public void FixtureSetup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "p-list-broken-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _brokenParagraphList = reader.ReadToEnd();
            }

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "unopened-elements-sample.html")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _unopenedElementsHtml = reader.ReadToEnd();
            }
        }

        [Test]
        public void FixBrokenHtmlTest()
        {
            string brokenParagraphList = new BrokenDivThreader().Thread(_brokenParagraphList);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(brokenParagraphList);

            Assert.That(doc.ParseErrors.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Fix_Unopened_Elements_Test()
        {
            var threader = new UnopenedElementThreader();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_unopenedElementsHtml);

            doc = threader.Thread(doc);

            Assert.That(doc.ParseErrors.Count(), Is.EqualTo(0));
        }
    }
}