using System;
using System.Text.RegularExpressions;
using Crawler.Core;

namespace douyu
{
    class Program
    {
        private static Crawler.Core.Crawler douyu;
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
            #region config
            var c = new Config
            {
                Name = "douyu",
                ScanUrls = "https://www.douyu.com/directory/all",
                Domains = new[]
                {
                    ".douyu.com",
                    ".douyucdn.cn",
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
                    },new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.owner_name"
                    },new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.online",
                        Type = FieldType.String,
                    },new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.fans_num",
                        Type = FieldType.String,
                    },new Field
                    {
                        Name = "cate",
                        Selector = "$.data.cate_name",
                        Selectortype = SelectorType.JsonPath
                    }, new Field
                    {
                        Name = "startat",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.data.start_time",
                        Type = FieldType.String,
                    },
                },
                RepeatWhen = RepeatWhenEver.hour,
                RepeatAt = DateTime.Now + new TimeSpan(0, 0, 0, 5),
            };
            #endregion
            douyu = new Crawler.Core.Crawler();

            douyu.Setup(c);
            douyu.Processor.OnProcessHelperPage = p =>
            {

                var r = new Regex("\" data-rid=\'([1-9]*)\'");
                var ms = r.Matches(p.Html);
                foreach (Match m in ms)
                {
                    douyu.Schduler.AddUrl("http://open.douyucdn.cn/api/RoomApi/room/" + m.Groups[1].Value, p.Request.Deth + 1);
                }

                p.SkipFindUrl = true;

            };
            douyu.Processor.OnProcessScanPage = p =>
            {
                var r = new Regex(@"count:(.+),");

                var m = r.Match(p.Html);
                var count = int.Parse(m.Groups[1].Value.Replace("\"", string.Empty));
#if DEBUG
               // count = 0;
#endif
                for (int i = 0; i < count; i++)
                {
                    douyu.Schduler.AddUrl($"https://www.douyu.com/directory/all?page={ i + 1}&isAjax=1", p.Request.Deth + 1);
                }
                p.SkipFindUrl = true;

            };
            douyu.Start();

        }
    }
}
