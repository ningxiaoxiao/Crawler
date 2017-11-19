using System;
using System.Text.RegularExpressions;
using CrawlerDotNet.Core;
using CrawlerDotNet.Core.Processor;
using Newtonsoft.Json.Linq;

namespace panda
{
    class Program
    {
        private static CrawlerDotNet.Core.Crawler crawler;
        static void Main(string[] args)
        {
            #region c
            var c = new Config
            {
                Name = "panda",
                ScanUrls = "https://www.panda.tv/live_lists?status=2&order=person_num&token=&pageno=1&pagenum=120",
                Domains = new[]
            {
                    ".panda.tv",
                },
                ContentUrlRegexes = new Regex("live_lists"),
                HelperUrlRegexes = new Regex("789987"),

                Fields = new[]
            {
                    new Field
                    {
                        Name = "title",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.name"
                    },new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.userinfo.nickName"
                    },new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.person_num",
                        Type = FieldType.Int,
                    },new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.fans",
                        Type = FieldType.Int,
                    },new Field
                    {
                        Name = "cate",
                        Selector = "$.classification.cname",
                        Selectortype = SelectorType.JsonPath
                    }
                },
                RepeatWhen = RepeatWhenEver.hour,
                RepeatAt = DateTime.Now + new TimeSpan(0, 0, 0, 5),
            };
            #endregion
            crawler = new CrawlerDotNet.Core.Crawler();
            ////https://www.panda.tv/room_followinfo?token=&roomid=1042806&_=1509522885105
            //https://www.panda.tv/1042806
            //https://www.panda.tv/live_lists?status=2&order=person_num&token=&pageno=3&pagenum=120&_=1509525309865

            crawler.Processor.OnCustomExtract = p =>
            {
                var j = JObject.Parse(p.Html);

                for (int i = 0; i < 120; i++)
                {
                    var roominfo = j.SelectToken($"$.data.items[{i}]");
                    if (roominfo == null) break;
                    var exres = new ExtractResults();

                    foreach (var f in c.Fields)
                    {
                        if (f.Name == "fanscount")
                        {
                            //请求订阅


                            var fanspage = crawler.Downloader.DownloaderOnly(new Request(crawler.Schduler)
                            {
                                Url =
                                    "https://www.panda.tv/room_followinfo?token=&roomid=" +
                                    roominfo.SelectToken("$.id").ToString()
                            });
                            var r= BaseProcessor.DoJson(fanspage.Html, f);
                            exres.Add(r);

                            continue;
                        }
                      

                        var res = new Result(f.Name, roominfo.SelectToken(f.Selector).ToString());
                        exres.Add(res);
                    }

                   


                    p.Results.Add(exres);
                }
            };

            crawler.Processor.OnProcessScanPage = p =>
            {
                //*[@id="pages-container"]/div/div/a[7]
                var total = int.Parse(p.GetJson("$.data.total"));

                var pageconut = total / 120 + (total % 120 > 0 ? 1 : 0);


                for (int i = 1; i <= pageconut; i++)
                {
                    crawler.Schduler.AddUrl($"https://www.panda.tv/live_lists?status=2&order=person_num&token=&pageno={i}&pagenum=120");
                }

                p.SkipFind();


            };
            crawler.Setup(c);
            crawler.Start();

            Console.ReadLine();

        }
    }
}
