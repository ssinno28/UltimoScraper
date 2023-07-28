using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UltimoScraper.Helpers;
using UltimoScraper.Models;

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
        
        [Test]
        public void Test_Properly_Matches_On_Keyword()
        {
            var keyword = new Keyword
            {
                Value = "Field Hockey"
            };

            bool result = "I love Field Hockey".MatchesKeyword(keyword);
            Assert.IsTrue(result);
        }
        
        [Test]
        public void Test_Does_Not_Match_Keyword()
        {
            var keyword = new Keyword
            {
                Regex = @"\b(?<!field\s)hockey\b",
                Value = "Hockey"
            };

            bool result = "I love Field Hockey".MatchesKeyword(keyword);
            Assert.IsFalse(result);
        }
        
        [Test]
        public void Test_Matches_Keyword()
        {
            var keyword = new Keyword
            {
                Regex = @"\b(?<!field\s)hockey\b",
                Value = "Hockey"
            };

            bool result = "I love Hockey".MatchesKeyword(keyword);
            Assert.IsTrue(result);
        }
    }
}