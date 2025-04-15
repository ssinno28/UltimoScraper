namespace UltimoScraper.Models
{
    public class ScraperConfig
    {
        public int PageTimeout { get; set; }
        public int PageThrottle { get; set; }
        public int? MaxProcesses { get; set; }
        public bool Headless { get; set; }
    }
}