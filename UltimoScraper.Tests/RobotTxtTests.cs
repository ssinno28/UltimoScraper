using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using UltimoScraper.Interfaces;
using UltimoScraper.Interfaces.Retrievers;
using UltimoScraper.Retrievers;
using UltimoScraper.Tests.Helpers;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class RobotTxtTests
    {
        private IServiceProvider _serviceProvider;
        private Mock<IHttpClientProvider> _mockHttpClientProvider;
        private string _robotsTxt;

        [SetUp]
        public void FixtureSetup()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = File.OpenRead(Path.Combine(assembly.GetAssemblyDirectory(), "Samples", "robots.txt")))
            using (StreamReader reader = new StreamReader(stream))
            {
                _robotsTxt = reader.ReadToEnd();
            }


            _mockHttpClientProvider = new Mock<IHttpClientProvider>();
            // Create the container builder.
            var services = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddScoped<IRobotsTxtRetriever, RobotsTxtRetriever>();

            services.Add(new ServiceDescriptor(typeof(IHttpClientProvider), _mockHttpClientProvider.Object));

            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public async Task TestRobotsReturnsCorrectIgnoreRules()
        {
            _mockHttpClientProvider
                .Setup(x => x.GetStringFromUrl(It.IsAny<string>()))
                .ReturnsAsync(_robotsTxt);

            var robotsRetriever = _serviceProvider.GetService<IRobotsTxtRetriever>();
            var robots = await robotsRetriever.GetRobotsTxt(new Uri("https://www.fakesite.com"));

            Assert.AreEqual(8, robots.Count);
        }
    }
}
