using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UltimoScraper.Interfaces.Processors;
using UltimoScraper.Models;
using UltimoScraper.Processors.LinkProcessors;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class LinkProcessorTests
    {
        private IServiceProvider _applicationContainer;

        [SetUp]
        public void FixtureSetup()
        {
            // Create the container builder.
            var serviceCollection = new ServiceCollection()
                .AddScoped<ILinkProcessor, FacebookLinkProcessor>()
                .AddScoped<ILinkProcessor, TwitterLinkProcessor>()
                .AddScoped<ILinkProcessor, InstagramLinkProcessor>();

            _applicationContainer = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task TestTwitterLinkProcessor()
        {
            var twitterLinkProcessor =
                _applicationContainer.GetService<IEnumerable<ILinkProcessor>>()
                    .First(x => x.GetType() == typeof(TwitterLinkProcessor));


            var twitterLink = new ParsedWebLink()
            {
                Text = "Twitter",
                Value = "https://twitter.com/TestSite"
            };
            var result = await twitterLinkProcessor.Process(twitterLink, new List<Keyword>());
            Assert.Equals(true, result);
        }

        [Test]
        public async Task TestFacebookLinkProcessor()
        {
            var facebookLinkProcessor =
                _applicationContainer.GetService<IEnumerable<ILinkProcessor>>()
                    .First(x => x.GetType() == typeof(FacebookLinkProcessor));


            var facebookLink = new ParsedWebLink()
            {
                Text = "Facebook",
                Value = "https://www.facebook.com/FakeSite/"
            };

            var result = await facebookLinkProcessor.Process(facebookLink, new List<Keyword>());
            Assert.Equals(true, result);
        }

        [Test]
        public async Task TestInstagramLinkProcessor()
        {
            var instagramLinkProcessor =
                _applicationContainer.GetService<IEnumerable<ILinkProcessor>>()
                    .First(x => x.GetType() == typeof(InstagramLinkProcessor));


            var instagramLink = new ParsedWebLink()
            {
                Text = "Instagram",
                Value = "https://www.instagram.com/Fake_Site/"
            };

            var result = await instagramLinkProcessor.Process(instagramLink, new List<Keyword>());
            Assert.Equals(true, result);
        }
    }
}
