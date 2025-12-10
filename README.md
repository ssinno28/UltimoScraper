# UltimoScraper

A web scraper designed with ethics and responsibility at its core. UltimoScraper provides a flexible, extensible framework for web scraping while respecting website resources and following best practices.

## Ethical Scraping

### Robots.txt Compliance
UltimoScraper includes `RobotsTxtRetriever.cs` to fetch and parse robots.txt files. Always retrieve and respect the ignore rules:

```csharp
var robotsRetriever = serviceProvider.GetRequiredService<IRobotsTxtRetriever>();
var ignoreRules = await robotsRetriever.GetIgnoreRulesAsync("https://example.com");
```

### Request Throttling
Built-in throttling prevents overwhelming target websites, ensuring scraping doesn't negatively impact their performance.

## Extensibility

### Core Retrievers
Extend scraping functionality by implementing:

- **`ILinkRetriever`** - Extract links from pages
- **`ITitleRetriever`** - Extract page titles
- **`IListRetriever`** - Extract lists of elements
- **`IListItemRetriever`** - Extract individual items from lists

Retrievers query the DOM for specific types of elements, while processors filter out those elements based on your specific needs. Here is an example of the default link retriever:

```csharp
public class DefaultLinkRetriever : ALinkRetriever, ILinkRetriever
    {
        public DefaultLinkRetriever(IEnumerable<ILinkProcessor> linkProcessors) : base(linkProcessors)
        {
        }

        public async Task<IList<ParsedWebLink>> GetLinks(
            HtmlNode htmlNode, 
            IList<IgnoreRule> linkIgnoreRules,
            IList<Keyword> keywords)
        {
            var links =
                htmlNode.QuerySelectorAll("a")
                        .Select(a => new ParsedWebLink
                        {
                            Value = HttpUtility.HtmlDecode(a.GetAttributeValue("href", null)),
                            Text = a.GetOnlyInnerText()
                        }).Where(u => !string.IsNullOrEmpty(u.Value));

            return await ProcessLinks(links, linkIgnoreRules, keywords);
        }
    }
```

### Processors
Retrievers work with processors to filter and refine results, ensuring you only capture the data you need.

```csharp
public class FacebookLinkProcessor : ILinkProcessor
    {
        public async Task<bool> Process(ParsedWebLink webLink, IList<Keyword> keywords)
        {
            var match = Regex.Match(webLink.Value, @"(?:(?:http|https):\/\/)?(?:www.)?facebook.com\/(?:(?:\w)*#!\/)?(?:pages\/)?(?:[?\w\-]*\/)?(?:profile.php\?id=(?=\d.*))?([\w\-]*)?");
            return match.Success;
        }
    }
```

### Custom Ignore Rules
Create ignore rules targeting:
- **Links** - Filter specific URLs
- **Lists** - Ignore HTML elements matching criteria

### Page Interactions
Execute JavaScript during scraping to wait for dynamic content:

```csharp
// Wait for content to load before scraping
 public class LPageInteraction : IPageInteraction
    {
        public bool IsMatch(string url)
        {
            var uri = new Uri(url);
            return uri.Host.Contains("example");
        }

        public async Task Interact(IPage page)
        {
           await page.WaitForFunctionAsync(@"() => {" +
                                           "var items = document.querySelector(\"#blogposts\"); " +
                                           "return items !== null && items.innerHTML !== ''" +
                                           "}");
        }
    }
```

### Threaders
In the scenario that you are scraping a site that just happens to have broken or incompelte HTML, you can create threaders to ensure the website can be parsed properly:

```csharp
 public class BrokenDivThreader : IHtmlThreader
    {
        public string Thread(string html)
        {
            string threadedHtml =
                Regex.Replace(html, "(<div)(.*)(<b=\"\")(>)", m => $"{m.Groups[1]} {m.Groups[2]} {m.Groups[4]}");

            return threadedHtml;
        }
    }
```

## Usage

### Dependency Injection

Register all scraper services using `ServiceCollectionHelper`:

```csharp
services.AddWebScraper();
```

## Getting Started

1. Register services with `AddWebScraper()`
2. Configure scraper settings in appsettings:
```json
  "Scraper": {
    "PageThrottle": 5000,
    "PageTimeout": 5000,
    "MaxProcesses": 10,
    "Headless": true
  }
```
2. Retrieve and respect robots.txt rules
3. Configure custom retrievers and processors
4. Implement page interactions if needed
5. Start scraping via `IWebParser` after injecting it

```csharp
    var ignoreRules = await _robotsTxtRetriever.GetRobotsTxt(new Uri(domain));
    var result = await _webParser.ParseSite(domain, ignoreRules, keywords.Select(x => new Keyword() { Value = x }).ToList(), 5, 20);
```

Scrape responsibly.