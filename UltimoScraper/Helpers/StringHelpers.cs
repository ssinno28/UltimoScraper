using System;
using System.Text.RegularExpressions;
using System.Web;
using UltimoScraper.Models;

namespace UltimoScraper.Helpers
{
    public static class StringHelpers
    {
        public static bool IsValidUrl(this string url)
        {
            bool result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        public static string GetUrlFromText(this string value)
        {
            var decodedValue = HttpUtility.HtmlDecode(value);
            var match = Regex.Match(decodedValue, @"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?");
            return match.Value;
        }

        public static bool MatchesKeyword(this string input, Keyword keyword)
        {
            input = input.ToLower();

            string pattern = $@"\b{Regex.Escape(keyword.Value.ToLower())}\b";
            if (!string.IsNullOrEmpty(keyword.Regex))
            {
                pattern = keyword.Regex;
            }
            
            var match = Regex.Match(input, pattern);

            return match.Success;
        }

        public static bool IsSiteDomain(this string url, Uri domain)
        {
            bool uriCreated = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri);
            return uriCreated && (!uri.IsAbsoluteUri || uri.Authority.Equals(domain.Authority));
        }

        public static string StripDuplicateForwardSlashes(this string path)
        {
            var uriCreated = Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var uri);
            if (!uriCreated)
            {
                return null;
            }

            if (uri.IsAbsoluteUri)
            {
                return path;
            }

            return Regex.Replace(path, @"/+", @"/"); ;
        }
    }
}