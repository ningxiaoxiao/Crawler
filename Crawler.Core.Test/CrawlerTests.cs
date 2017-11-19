using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace CrawlerDotNet.Core.Tests
{
    [TestClass()]
    public class CrawlerTests
    {
        [TestMethod()]
        public void RunTest()
        {
            var c = new Config
            {
                ScanUrls = "https://www.douyu.com/directory/all?page=1&isAjax=1",
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
            };

            var crawler=new Crawler();

            crawler.Setup(c);

            crawler.Processor.OnProcessScanPage = p =>
            {
                var r=new Regex("data-rid=\'([1-9]*)\'");
                var ms = r.Matches(p.Html);
                foreach (Match m in ms)
                {
                    crawler.Schduler.AddUrl("http://open.douyucdn.cn/api/RoomApi/room/" + m.Groups[1].Value);
                }
            };

        }
    }
}

