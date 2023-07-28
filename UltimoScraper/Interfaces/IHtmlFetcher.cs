using HtmlAgilityPack;
using System.Threading.Tasks;
using System;

namespace UltimoScraper.Interfaces;

public interface IHtmlFetcher
{
    Task<HtmlDocument> GetPageHtml(Uri domain, string url, string sessionName);
}