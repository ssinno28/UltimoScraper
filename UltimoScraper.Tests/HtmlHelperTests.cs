using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;
using UltimoScraper.Helpers;

namespace UltimoScraper.Tests
{
    [TestFixture]
    public class HtmlHelperTests
    {
        [Test]
        public void Test_GetOnlyInnerText()
        {
            string htmlString = "&nbsp; Hello There! &nbsp;";
            var mockNode = new Mock<HtmlNode>(MockBehavior.Default, HtmlNodeType.Element, new HtmlDocument(), 0);
            mockNode.Setup(x => x.InnerText).Returns(htmlString);
            Assert.Equals("Hello There!", mockNode.Object.GetOnlyInnerText());
        }

        [Test]
        public void Test_GetClosestParentByTagName()
        {
            string html = @"<div>
<article>
<div>
<div class=""my-test-class""></div>
</div>
</article>
</div>";

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var testNode = document.DocumentNode.QuerySelector(".my-test-class");
            Assert.Equals("article", testNode.GetClosestParentByTagName("article").Name);
        }   
        
        [Test]
        public void Test_GetClosestParentByClassName()
        {
            string html = @"<div>
<article class=""my-parent-class"">
<div>
<div class=""my-test-class""></div>
</div>
</article>
</div>";

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var testNode = document.DocumentNode.QuerySelector(".my-test-class");
            Assert.Equals("article", testNode.GetClosestParentByClassName("my-parent-class").Name);
        }
    }
}