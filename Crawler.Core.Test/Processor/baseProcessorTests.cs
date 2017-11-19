using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrawlerDotNet.Core.Downloader;
using CrawlerDotNet.Core.Scheduler;

namespace CrawlerDotNet.Core.Processor.Tests
{



    [TestClass()]
    public class baseProcessorTests : BaseProcessor
    {
        HttpHelper http = new HttpHelper();
        [TestMethod()]
        public void DoRegexTest()
        {

            var r = DoRegex("id=\"test\" name=\"test\"", new Field
            {
                Name = "id",
                Selectortype = SelectorType.Regex,
                Selector = "id=\"([a-z]*)\""
            });
            var p = new Page();
            p.Results.Add(new ExtractResults{r});
            Assert.AreEqual("test", p.Results[0]["id"].Value);
        }
        [TestMethod()]
        public void DoHtmlTest()
        {
            var p = new Page();
            var r = new HttpItem()
            {
                Url = "http://www.huya.com/dongxiaosa",
            };
            var htmlstr = http.GetHtml(r).Html;

            var res = DoHtml(htmlstr, new Field
            {
                Name = "name",
                Selector = "//*[@id=\"J_roomHeader\"]/div[1]/div[2]/div[2]/div/h3"
            });
            p.Results.Add(new ExtractResults{res});
            Assert.AreEqual("董导丶董小飒", p.Results[0]["name"].Value);

        }
        [TestMethod()]
        public void DoJsonTest()
        {
            var p = new Page();

            var r = new HttpItem()
            {
                Url = "http://open.douyucdn.cn/api/RoomApi/room/1229",
            };

            var htmlstr = http.GetHtml(r).Html;
            var res = DoJson(htmlstr, new Field
            {
                Name = "name",
                Selectortype = SelectorType.JsonPath,
                Selector = "$.data.owner_name"
            });
            p.Results.Add(new ExtractResults { res });

            //嗨氏
            Assert.AreEqual("嗨氏", p.Results[0]["name"].Value);

        }
      



    }
}