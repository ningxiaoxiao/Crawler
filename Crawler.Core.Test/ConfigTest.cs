using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Crawler.Core.Test
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void ConfigSerializeTest()
        {
            #region 序列化
            var c = new Config
            {
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

            var j = JsonConvert.SerializeObject(c);
            var output = j.ToString();
            var deC = JsonConvert.DeserializeObject<Config>(output,new JsonSerializerSettings() {NullValueHandling = NullValueHandling.Ignore});

            Assert.AreEqual(c.ScanUrls,deC.ScanUrls);

            Console.WriteLine(output);
        }
    }
}
