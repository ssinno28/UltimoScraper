using System;
using NUnit.Framework;
using UltimoScraper.Helpers;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class UriHelperTests
    {
        [Test]
        public void Test_Not_Same_Uri_Mixed()
        {
            string path = "https://fakesite.com/event/concert";
            string path2 = "/event/concert/tomorrow";
            Assert.True(path.NotSameUri(path2, new Uri("https://fakesite.com")));
        }
        
        [Test]
        public void Test_Not_Same_Uri_Relative()
        {
            string path = "/event/concert";
            string path2 = "/event/concert/tomorrow";
            Assert.True(path.NotSameUri(path2, new Uri("https://fakesite.com")));
        }
        
        [Test]
        public void Test_Not_Same_Uri_Absolute()
        {
            string path = "https://fakesite.com/event/concert";
            string path2 = "https://fakesite.com/event/concert/tomorrow";
            Assert.True(path.NotSameUri(path2, new Uri("https://fakesite.com")));
        }
        
        [Test]
        public void Test_Same_Uri_Absolute()
        {
            string path = "https://fakesite.com/event/concert";
            string path2 = "https://fakesite.com/event/concert";
            Assert.False(path.NotSameUri(path2, new Uri("https://fakesite.com")));
        }

        [Test]
        public void Test_Is_Same_Uri_Mixed()
        {
            string path = "https://fakesite.com/event/concert";
            string path2 = "/event/concert";
            Assert.False(path.NotSameUri(path2, new Uri("https://fakesite.com")));
        }

        [Test]
        public void Test_Same_Uri_Relative()
        {
            string path = "/event/concert";
            string path2 = "/event/concert";
            Assert.False(path.NotSameUri(path2, new Uri("https://fakesite.com")));
        }
    }
}