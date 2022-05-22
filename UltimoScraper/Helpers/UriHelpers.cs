using System;
using System.ComponentModel.DataAnnotations;

namespace UltimoScraper.Helpers
{
    public static class UriHelpers
    {
        public static bool NotSameUri(this string url1, string url2, Uri domain)
        {
            var uri1Created = Uri.TryCreate(url1, UriKind.RelativeOrAbsolute, out var uri1);
            var uri2Created = Uri.TryCreate(url2, UriKind.RelativeOrAbsolute, out var uri2);

            if (uri1Created != uri2Created) return true;

            var result = Uri.Compare(uri1.MakeAbsolute(domain), uri2.MakeAbsolute(domain), UriComponents.AbsoluteUri, UriFormat.Unescaped,
                StringComparison.Ordinal) != 0;

            return result;
        }

        public static Uri MakeAbsolute(this Uri uri, Uri domain)
        {
            string path = uri.ToString();
            if (path.StartsWith("/")) path = path.Substring(1, path.Length - 1);

            string urlWithScheme = uri.IsAbsoluteUri
                ? uri.AbsoluteUri
                : $"{domain.Scheme}://{domain.Authority}/{path}";

            return new Uri(urlWithScheme);
        }
    }
}