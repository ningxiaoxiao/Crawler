using System;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace Crawler.Core.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            douyuSample();
            Console.WriteLine("end");
            Console.ReadKey();
        }


        static void douyuSample()
        {
            //https://www.douyu.com/directory/all
            //https://www.douyu.com/directory/all?page=1&isAjax=1
            //http://open.douyucdn.cn/api/RoomApi/room/
            var c = new Config
            {
                ScanUrls = "https://www.douyu.com/directory/all",
                Domains = new[]
                {
                    "douyu.com",
                    "douyucdn.cn",
                },
                ContentUrlRegexes = new Regex("room"),
                HelperUrlRegexes = new Regex("page"),

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
                        Selector = "$.data.online",
                        Type = typeof(int)
                    },
                    new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.fans_num",
                        Type = typeof(int)
                    },
                },
                Interval = 0
            };

            var crawler = new Crawler();

            crawler.Setup(c);
            crawler.Processor.OnProcessHelperPage = p =>
            {

                var r = new Regex("\" data-rid=\'([1-9]*)\'");
                var ms = r.Matches(p.Html);
                foreach (Match m in ms)
                {
                    crawler.Schduler.AddUrl("http://open.douyucdn.cn/api/RoomApi/room/" + m.Groups[1].Value, p.Request.Deth + 1);
                }

                p.SkipFindUrl = true;

            };
            crawler.Processor.OnProcessScanPage = p =>
            {
                var r = new Regex(@"count:(.+),");

                var m = r.Match(p.Html);
                var count = int.Parse(m.Groups[1].Value.Replace("\"", string.Empty));

                for (int i = 0; i < 2; i++)
                {
                    crawler.Schduler.AddUrl($"https://www.douyu.com/directory/all?page={ i + 1}&isAjax=1", p.Request.Deth + 1);
                }
            };
            crawler.Downloader.AfterDownloadPage = p =>
              {
                  Console.WriteLine("AfterDownloadPage:" + p.Html.Substring(0, 10));
              };
            crawler.Processor.OnProcessContentPage = (p) =>
            {
                Console.WriteLine("OnProcessContentPage" + p.Html.Substring(0, 10));
            };

            crawler.Run();
        }
    }
}
