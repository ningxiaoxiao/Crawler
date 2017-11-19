using System;
using System.Text.RegularExpressions;
using CrawlerDotNet.Core;
using CrawlerDotNet.Core.Scheduler;
using Newtonsoft.Json.Linq;


namespace huya
{
    class Program
    {
        public static CrawlerDotNet.Core.Crawler huya { get; private set; }

        static void Main(string[] args)
        {
            #region config
            var c = new Config
            {
                Name = "huya",
                ScanUrls = "http://www.huya.com/cache.php?m=LiveList&do=getLiveListByPage&tagAll=0&page=0",
                Domains = new[]
                {
                    ".huya.com",
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
                        Selector = "$.roomJson.introduction"
                    },new Field
                    {
                        Name = "username",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.profileJson.nick"
                    },new Field
                    {
                        Name = "online",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.roomJson.totalCount",
                        Type = FieldType.Int,
                    },new Field
                    {
                        Name = "fanscount",
                        Selectortype = SelectorType.JsonPath,
                        Selector = "$.profileJson.fans",
                        Type = FieldType.Int,
                    },new Field
                    {
                        Name = "cate",
                        Selector = "$.roomJson.gameFullName",
                        Selectortype = SelectorType.JsonPath
                    }, 
                },
                RepeatWhen = RepeatWhenEver.hour,
                RepeatAt = DateTime.Now + new TimeSpan(0, 0, 0, 5),
            };
            #endregion
            huya = new CrawlerDotNet.Core.Crawler();

            huya.Setup(c);
            huya.Processor.OnProcessHelperPage = p =>
            {


                var pageSize = int.Parse(p.GetJson("$.data.pageSize"));
                for (int i = 0; i < pageSize; i++)
                {
                    p.Request.Schduler.AddUrl("http://www.huya.com/" + p.GetJson($"$.data.datas[{i}].privateHost"), p.Request.Deth + 1);
                }
                p.SkipExtract();
                p.SkipFind();
            };
            huya.Processor.OnProcessScanPage = p =>
            {
                var pagecount = int.Parse(p.GetJson("$.data.totalPage"));

                for (int i = 1; i <= pagecount; i++)
                {
                    p.Request.Schduler.AddUrl("http://www.huya.com/cache.php?m=LiveList&do=getLiveListByPage&tagAll=0&page=" + i, PageType.HelperUrl, p.Request.Deth + 1);
                }

                p.SkipFind();
                p.SkipExtract();

            };
  
            huya.Processor.OnProcessContentPage = p =>
            {
                var room = new Regex("TT_ROOM_DATA = ([\\s\\S]*?);var");
                var profile = new Regex("TT_PROFILE_INFO = ([\\s\\S]*?);var");

                var roomMatch = room.Match(p.Html.Replace("&amp;", " "));
                var profileMatch = profile.Match(p.Html.Replace("&amp;", " "));

                if (!roomMatch.Success || !profileMatch.Success)
                    return;

                var profileJson = JObject.Parse(profileMatch.Groups[1].ToString());

                var roomJson = JObject.Parse(roomMatch.Groups[1].ToString());

                var json = new JObject();
                json["profileJson"] = profileJson;
                json["roomJson"] = roomJson;

                p.Html = json.ToString();
            };
         
            huya.Start();
            Console.ReadKey();
        }
    }
}
