using System.Collections.Generic;
using UltimoScraper.Dictionary;
using UltimoScraper.Models;

namespace UltimoScraper.Helpers;

public static class IgnoreRulesHelpers
{
    public static void AddDefaultIgnoreRules(this IList<IgnoreRule> ignoreRules)
    {
        string[] extensionsToIgnore = { ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".svg", ".mp4", ".avi", ".mov", ".mkv", "mailto", "webcal" };
        foreach (var ext in extensionsToIgnore)
        {
            ignoreRules.Add(new IgnoreRule
            {
                IgnoreRuleType = IgnoreRuleType.Link,
                Rule = ext
            });
        }
    }
}