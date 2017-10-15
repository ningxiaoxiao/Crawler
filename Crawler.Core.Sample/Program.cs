using System;
using System.Text.RegularExpressions;

namespace Crawler.Core.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            cnblogSample();
            Console.ReadKey();
        }
        static void cnblogSample()
        {

            var c = new Config
            {
                ScanUrls = "http://www.cnbeta.com/",
                Domains = new[]
                {
                    "cnbeta.com",
                },
                Deth = 1,
                Fields = new[]
                {
                    new Field
                    {
                        Name = "title",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.room_name"
                    },
                    new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.owner_name"
                    },
                    new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.online"
                    },
                    new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.fans_num"
                    },
                },
                Interval = 0
            };

            var crawler = new Crawler();

            crawler.Setup(c);

            crawler.Processor.OnProcessScanPage = p =>
            {
                var r = new Regex("\" data-rid=\'([1-9]*)\'");
                var ms = r.Matches(p.Raw);
                foreach (Match m in ms)
                {
                    crawler.Schduler.AddUrl("http://open.douyucdn.cn/api/RoomApi/room/" + m.Groups[1].Value);
                }
                p.SkipExtractField = true;
            };
            crawler.Processor.AfterExtractField = (p, r) =>
            {
                r.Skip = true;
            };

            crawler.Run();
        }

        static void douyuSample()
        {

            var c = new Config
            {
                ScanUrls = "https://www.douyu.com/directory/all?page=1&isAjax=1",
                Domains = new[]
                {
                    "douyu.com",
                    "douyucdn.cn",
                },
                Fields = new[]
                {
                    new Field
                    {
                        Name = "title",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.room_name"
                    },
                    new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.owner_name"
                    },
                    new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.online"
                    },
                    new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.fans_num"
                    },
                },
                Interval = 0
            };

            var crawler = new Crawler();

            crawler.Setup(c);

            crawler.Processor.OnProcessScanPage = p =>
            {
                var r = new Regex("\" data-rid=\'([1-9]*)\'");
                var ms = r.Matches(p.Raw);
                foreach (Match m in ms)
                {
                    crawler.Schduler.AddUrl("http://open.douyucdn.cn/api/RoomApi/room/" + m.Groups[1].Value);
                }
                p.SkipExtractField = true;
            };
            crawler.Processor.AfterExtractField = (p, r) =>
            {
                r.Skip = true;
            };

            crawler.Run();
        }
    }
}
