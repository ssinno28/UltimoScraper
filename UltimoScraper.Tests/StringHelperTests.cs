using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;
using UltimoScraper.Helpers;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class StringHelperTests
    {
        [Test]
        public void Test_Strip_Extra_Forward_Slashes()
        {
            string path = "//event/concert";
            path = path.StripDuplicateForwardSlashes();
            Assert.AreEqual("/event/concert", path);
        }
        
        [Test]
        public void Test_Does_Not_Strip_Http_Slashes()
        {
            string path = "https://fakesite.com/event/concert";
            path = path.StripDuplicateForwardSlashes();
            Assert.AreEqual("https://fakesite.com/event/concert", path);
        }
    }
}