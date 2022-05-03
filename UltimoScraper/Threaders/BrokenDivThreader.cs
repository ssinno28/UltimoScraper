using System.Text.RegularExpressions;
using UltimoScraper.Interfaces.Threaders;

namespace UltimoScraper.Threaders
{
    public class BrokenDivThreader : IHtmlThreader
    {
        public string Thread(string html)
        {
            string threadedHtml =
                Regex.Replace(html, "(<div)(.*)(<b=\"\")(>)", m => $"{m.Groups[1]} {m.Groups[2]} {m.Groups[4]}");

            return threadedHtml;
        }
    }
}