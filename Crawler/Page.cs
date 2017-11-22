using System;
using System.Collections.Generic;
using CrawlerDotNet.Core.Downloader;
using CrawlerDotNet.Core.Processor;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace CrawlerDotNet.Core
{
    /// <summary>
    /// 表示当前正在爬取的网页对象
    /// </summary>
    public class Page : HttpResult
    {
        public Page() { }
        public Page(HttpResult r)
        {
            Html = r.Html;
        }

        /// <summary>
        /// 当前网页的内容. 通过自动JS渲染的网页, page.raw是JS渲染后的网页内容
        /// </summary>
        /// <summary>
        /// 请求page网页时使用的请求参数等数据
        /// </summary>
        public Request Request { get; set; }
        /// <summary>
        /// 请求page网页返回的响应信息
        /// </summary>
        public System.Net.HttpWebResponse Response { get; set; }
        /// <summary>
        /// 当前网页的附加数据, 是开发者自定义的一段代码, 例如, HTML代码.
        /// </summary>
        public string ContextData { get; set; }


        public List<ExtractResults> Results { get; } = new List<ExtractResults>();

        /// <summary>
        /// 跳过抽取
        /// </summary>
        public bool SkipExtractField { get; private set; }

        public void SkipExtract()
        {
            SkipExtractField = true;
        }

        /// <summary>
        /// 只能在afterExtractField回调函数中使用, 
        /// 用来过滤抽取项中不需要的抽取结果, 
        /// 结果不会被保存到数据库中
        /// </summary>
        public bool SkipSave { get; private set; }

        public void SkipSaveData()
        {
            SkipSave = true;
        }

        private HtmlDocument _doc;
        public string GetXPath(string pathtext)
        {
            if (_doc == null)
            {
                _doc = new HtmlDocument();
                _doc.LoadHtml(Html);
            }
               
            var n = _doc.DocumentNode.SelectSingleNode(pathtext);
            return n?.InnerText;

        }


        private JObject _json;

        public string GetJson(string jpath)
        {
            if (_json == null) _json = JObject.Parse(Html);

            var v = _json.SelectToken(jpath);
            return v?.ToString();
        }

        public override string ToString()
        {
            return $"url:{Request.Url},\r\nhtml:{Html}";
        }
    }
    public class ExtractResults : Dictionary<string, Result>, IEnumerable<Result>
    {
        public ExtractResults()
        {
            Timestamp = Crawler.CurStartTime.ToString("yyyy-MM-dd HH:mm:ss");
            
        }

        public void Add(Result r)
        {
            if (r == null) return;
            this.Add(r.Key, r);
        }

        public new IEnumerator<Result> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public string Timestamp { get; }
    }
    public enum PageType
    {
        ScanUrl,
        HelperUrl,
        ContextUrl,

    }
}