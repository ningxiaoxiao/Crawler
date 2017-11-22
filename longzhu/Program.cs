using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlerDotNet.Core;
using CrawlerDotNet.Core.Processor;
using Newtonsoft.Json.Linq;

namespace longzhu
{
    class Program
    {
        private static CrawlerDotNet.Core.Crawler crawler;
        static void Main(string[] args)
        {
            var config = new Config
            {
                Name = "longzhu",
                ScanUrls = "http://api.plu.cn/tga/streams?max-results=50&start-index=0&sort-by=views&filter=0&game=0",
                Fields = new[]
                {
                    new Field
                    {
                        Name = "title",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.channel.status"
                    },
                    new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.channel.name"
                    },
                    new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.viewers",
                        Type = FieldType.Int,
                    },
                    new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.channel.followers",
                        Type = FieldType.Int,
                    },
                    new Field
                    {
                        Name = "cate",
                        Selector = "$.game[0].name",
                        Selectortype = SelectorType.JsonPath
                    }
                },
                RepeatWhen = RepeatWhenEver.hour,
                RepeatAt = new TimeSpan(0, 25, 0),

            };
            crawler = new Crawler();

          

            crawler.Downloader.AfterDownloadPage = p =>
            {
               

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
            crawler.Processor.OnProcessScanPage = p =>
            {
                var totalcount = p.GetJson("$.data.totalItems");
                var pagecount = int.Parse(totalcount) / 50 + 1;

                for (int i = 1; i <= pagecount; i++)
                {
                    crawler.Schduler.AddUrl($"http://api.plu.cn/tga/streams?max-results=200&start-index={i * 50}&sort-by=views&filter=0&game=0");
                }

            };
            crawler.Setup(config);
            crawler.Start();
            Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
