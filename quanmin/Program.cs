using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrawlerDotNet.Core;
using CrawlerDotNet.Core.Processor;
using CrawlerDotNet.Core.Scheduler;

namespace quanmin
{
    class Program
    {
        private static CrawlerDotNet.Core.Crawler crawler;
        static void Main(string[] args)
        {
            var config = new Config
            {
                Name = "quanmin",
                ScanUrls = "https://www.quanmin.tv/game/all",
                Domains = new[] { ".quanmin.tv" },
                Fields = new[]
                {
                    new Field
                    {
                        Name = "title",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.title"
                    },
                    new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.nick"
                    },
                    new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.view",
                        Type = FieldType.Int,
                    },
                    new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.follow",
                        Type = FieldType.Int,
                    },
                    new Field
                    {
                        Name = "cate",
                        Selector = "$.category_name",
                        Selectortype = SelectorType.JsonPath
                    }
                },
                RepeatWhen = RepeatWhenEver.hour,
                RepeatAt = DateTime.Now + new TimeSpan(0, 0, 0, 5),

            };
            crawler = new CrawlerDotNet.Core.Crawler();

            var curPage = 1;
            crawler.BeforeCrawl = () =>
            {
                curPage = 1;
            };

            crawler.Downloader.AfterDownloadPage = p =>
            {

                if (p.Request.Type != PageType.ContextUrl) return;

                //处理页面
                var r = new Regex("{\"uid([\\s\\S]*)\"ignore_ad\":true}");
                var m = r.Match(p.Html);
                p.Html = m.Value;


            };

            crawler.Processor.OnProcessScanPage = p =>
            {

                var r = new Regex("total:([0-9]*),");
                var m = r.Match(p.Html.Replace(" ", string.Empty));

                //得到页码
                var page = int.Parse(m.Groups[1].Value);

                for (int i = 1; i <= page; i++)
                {
                    crawler.Schduler.AddUrl($"https://www.quanmin.tv/game/all?p={i}", PageType.HelperUrl);
                }

                p.SkipExtract();
                p.SkipFind();
            };
            crawler.Processor.OnProcessHelperPage = p =>
            {
                var r = new Regex("\"evtname\":\"([0-9]*)\"");

                var ms = r.Matches(p.Html);

                foreach (Match m in ms)
                {
                    crawler.Schduler.AddUrl("https://www.quanmin.tv/" + m.Groups[1].Value);
                }

                p.SkipExtract();
                p.SkipFind();
            };

            crawler.Setup(config);
            crawler.Start();
            Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
