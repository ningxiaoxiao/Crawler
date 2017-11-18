using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Core;
using Crawler.Core.Processor;
using Newtonsoft.Json.Linq;

namespace zhanqi
{
    class Program
    {
        private static Crawler.Core.Crawler crawler;
        static void Main(string[] args)
        {
            var config = new Config
            {
                Name = "zhanqi",
                ScanUrls = "http://www.zhanqi.tv/api/static/v2.1/live/list/200/1.json",
                Domains = new[] {".zhanqi.tv"},
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
                        Selector = "$.nickname"
                    },
                    new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.online",
                        Type = FieldType.Int,
                    },
                    new Field
                    {
                        ////*[@id="js-room-anchor-info-area"]/div[2]/div[1]/div/span[1]
                        Name = "fanscount",
                        Selectortype = SelectorType.Regex,
                        Selector = "js-room-follow-num\">([0-9]*)<",
                        Type = FieldType.Int,
                    },
                    new Field
                    {
                        Name = "cate",
                        Selector = "$.gameName",
                        Selectortype = SelectorType.JsonPath
                    }
                },
                RepeatWhen = RepeatWhenEver.hour,
                RepeatAt = DateTime.Now + new TimeSpan(0, 0, 0, 5),

            };
            crawler = new Crawler.Core.Crawler();

            var curPage = 1;
            crawler.BeforeCrawl = () =>
            {
                curPage = 1;
            };

            crawler.Downloader.AfterDownloadPage = p =>
            {
                //是不是有数据.有数据加入下一个json

                var rooms = p.GetJson("$.data.rooms");

                if (rooms != "[]")
                {
                    curPage++;
                    crawler.Schduler.AddUrl($"http://www.zhanqi.tv/api/static/v2.1/live/list/200/{curPage}.json");
                }
                
            };
            crawler.Processor.OnCustomExtract = p =>
            {
                var j = JObject.Parse(p.Html);
                var jr = JArray.FromObject(j["data"]["rooms"]);
               

                for (int i = 0; i < jr.Count; i++)
                {
                    var exres = new ExtractResults();
                    var info = jr[i];
                    foreach (var f in config.Fields)
                    {

                        if (f.Name == "fanscount")
                        {
                            //请求订阅


                            var fanspage = crawler.Downloader.DownloaderOnly(new Request(crawler.Schduler)
                            {
                                Url =
                                    "https://www.zhanqi.tv" +
                                    info.SelectToken("$.url").ToString()
                            });
                            var r = BaseProcessor.DoRegex(fanspage.Html, f);
                            if (r.Value == "")
                                r.Value = "0";
                            exres.Add(r);

                            continue;
                        }



                        var res = new Result(f.Name, info.SelectToken(f.Selector).ToString());
                        exres.Add(res);
                    }
                    p.Results.Add(exres);
                }


                
            };
            crawler.Setup(config);
            crawler.Start();
            Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
