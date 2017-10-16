using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crawler.Core.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Crawler.Core.Downloader;
using Crawler.Core.Scheduler;
using HtmlAgilityPack;

namespace Crawler.Core.Processor.Tests
{



    [TestClass()]
    public class baseProcessorTests:BaseProcessor
    {
        HttpHelper http = new HttpHelper();
        [TestMethod()]
        public void DoRegexTest()
        {
            var p = new Page();
            DoRegex(p, "id=\"test\" name=\"test\"", new Field
            {
                Name = "id",
                Selectortype = SelectorType.Regex,
                Selector = "id=\"([a-z]*)\""
            });
           // Assert.AreEqual(p.Results["id"],"test");
        }
        [TestMethod()]
        public void DoHtmlTest()
        {
            var p = new Page();
            var r = new HttpItem()
            {
                URL = "http://www.huya.com/dongxiaosa",
            };
            var htmlstr = http.GetHtml(r).Html;
            DoHtml(p, htmlstr, new Field
            {
                Name = "name",
                Selector = "//*[@id=\"J_roomHeader\"]/div[1]/div[2]/div[2]/div/h3"
            });

           // Assert.AreEqual(p.Results["name"], "董导丶董小飒");

        }
        [TestMethod()]
        public void DoJsonTest()
        {
            var p = new Page();
            
            var r = new HttpItem()
            {
                URL = "http://open.douyucdn.cn/api/RoomApi/room/1229",
            };

            var htmlstr = http.GetHtml(r).Html;
            DoJson(p, htmlstr, new Field
            {
                Name = "name",
                Selectortype = SelectorType.JsonPath,
                Selector = "$.data.owner_name"
            });
            foreach (var rr in p.Results)
            {
                Logger.Info(rr);
            }
            //嗨氏
          //  Assert.AreEqual(p.Results["name"], "嗨氏");

        }
        [TestMethod]
        public void FindUrlTest()
        {
            Config=new Config{ScanUrls = "http://www.cnbeta.com" };
            var r = new HttpItem()
            {
                URL = "http://www.cnbeta.com",
            };

            var p = http.GetHtml(r);
            var pg = new Page(p);
            var s = new DefaultSchduler();
            s.Config = Config;
            pg.Request=new Request(s);
            FindUrl(pg);

        }



    }
}