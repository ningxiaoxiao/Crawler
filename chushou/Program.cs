using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlerDotNet.Core;
using CrawlerDotNet.Core.Processor;
using CrawlerDotNet.Core.Scheduler;
using Newtonsoft.Json.Linq;

namespace chushou
{
    class Program
    {
        private static CrawlerDotNet.Core.Crawler crawler;
        static void Main(string[] args)
        {
            var config = new Config
            {
                Name = "chushou",
                ScanUrls = "https://chushou.tv/live/down-v2.htm",
                Domains = new[] { "chushou.tv" },
                Fields = new[]
                {
                    new Field
                    {
                        Name = "title",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.name"
                    },
                    new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.meta.creator"
                    },
                    new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.meta.onlineCount",
                        Type = FieldType.Int,
                    },
                    new Field
                    {

                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.meta.subscriberCount",
                        Type = FieldType.Int,
                    },
                    new Field
                    {
                        Name = "cate",
                        Selector = "$.meta.gameName",
                        Selectortype = SelectorType.JsonPath
                    }
                },
                RepeatWhen = RepeatWhenEver.hour,
                RepeatAt = DateTime.Now + new TimeSpan(0, 0, 0, 5),

            };
            crawler = new CrawlerDotNet.Core.Crawler();
            string lastpoint = "";
            crawler.Processor.OnProcessScanPage = p =>
            {
                var point = p.GetJson("$.data.breakpoint");
                crawler.Schduler.AddUrl("https://chushou.tv/live/down-v2.htm?&breakpoint=" + point, point != lastpoint ? PageType.ScanUrl : PageType.ContextUrl);
                lastpoint = point;
            };
            crawler.Processor.OnCustomExtract = p =>
            {
                var j = JObject.Parse(p.Html);
                var jr = JArray.FromObject(j["data"]["items"]);


                for (int i = 0; i < jr.Count; i++)
                {
                    var exres = new ExtractResults();
                    var info = jr[i];
                    foreach (var f in config.Fields)
                    {

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
