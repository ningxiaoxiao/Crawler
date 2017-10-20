using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NLog;

namespace Crawler.Core.Processor
{
    public delegate void ResultDelegate(IEnumerable<KeyValuePair<string, dynamic>> results);

    public delegate void VoidPageDelegate(Page p);

    public delegate void VoidPageResultDelegate(Page p, Result r);

    public interface IProcessor
    {
        int SkipCount { get; }
        int ExtractCount { get; }
        int FailCount { get; }
        Logger Logger { get; }

        Config Config { get; set; }
        VoidPageDelegate OnProcessScanPage { get; set; }
        VoidPageDelegate OnProcessHelperPage { get; set; }
        VoidPageDelegate OnProcessContentPage { get; set; }
        /// <summary>
        /// 从内容页中抽取到一个抽取项的值后进行的回调, 在此回调中可以对该抽取项的值进行处理并返回
        /// </summary>
        VoidPageResultDelegate AfterExtractField { get; set; }
        VoidPageDelegate OnComplete { get; set; }
        void Handle(Page page);
    }

    public abstract class BaseProcessor : IProcessor
    {
        public int SkipCount { get; private set; }
        public int ExtractCount { get; private set; }
        public int FailCount { get; private set; }
        public Logger Logger => Crawler.Logger;
        public Config Config { get; set; }
        public VoidPageDelegate OnProcessScanPage { get; set; }
        public VoidPageDelegate OnProcessHelperPage { get; set; }
        public VoidPageDelegate OnProcessContentPage { get; set; }
        public VoidPageResultDelegate AfterExtractField { get; set; }
        public VoidPageDelegate OnComplete { get; set; }

        public void Handle(Page page)
        {
            if (page.Request.URL == Config.ScanUrls)
            {
                OnProcessScanPage?.Invoke(page);
                page.SkipExtractField = true;
            }
            else if (Config.HelperUrlRegexes.IsMatch(page.Request.URL))
            {
                //识别列表页
                OnProcessHelperPage?.Invoke(page);
                page.SkipExtractField = true;
            }
            else if (Config.ContentUrlRegexes.IsMatch(page.Request.URL))
            {
                //识别内容页
                OnProcessContentPage?.Invoke(page);
            }
            else
            {
               page.SkipExtractField = true;
            }

            if (page.SkipExtractField)
            {
                SkipCount++;
            }
            else
            {
                Extract(page);
            }



            OnComplete?.Invoke(page);
            Logger.Info($"{page.Request.URL} 抽取成功");
            ExtractCount++;

            if (page.Request.Deth >= Config.Deth) return;

            if (!page.SkipFindUrl)
            {
                var t = new Thread(FindUrl);
                t.Start(page);
            }
        }

        private void Extract(Page page)
        {

            foreach (var field in Config.Fields)
            {
                try
                {
                    string source;
                    switch (field.SourceType)
                    {
                        case SourceType.Page:
                            source = page.Raw;
                            break;
                        case SourceType.AttachedUrl:
                            throw new NotImplementedException();
                        case SourceType.UrlContext:
                            source = page.Request.URL;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    Result result;
                    switch (field.Selectortype)
                    {
                        case SelectorType.JsonPath:
                            result = DoJson(page, source, field);
                            break;
                        case SelectorType.XPath:
                            result = DoHtml(page, source, field);
                            break;
                        case SelectorType.Regex:
                            result = DoRegex(page, source, field);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    page.Results.Add(result);
                    AfterExtractField?.Invoke(page, result);

                }
                catch (Exception e)
                {
                    Logger.Error($"{page.Request.URL} 抽取 {field.Selectortype} {field.Name} 失败 \r\n{e}");
                    FailCount++;
                    return;
                }
            }
        }

        public void FindUrl(object pageobj)
        {
            var page = (Page)pageobj;
            var r = new Regex("href=\"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");

            var ms = r.Matches(page.Raw);
            foreach (Match match in ms)
            {
                var url = match.Value.Remove(0, 6);

                if (Config.HelperUrlRegexes.IsMatch(url) || Config.ContentUrlRegexes.IsMatch(url))
                {
                    //Logger.Info($"在[{page.Request.URL}]发现新网页[{url}]");
                    page.Request.Schduler.AddUrl(url, page.Request.Deth + 1);
                }
            }
        }

        protected Result DoRegex(Page page, string source, Field field)
        {
            var r = new Regex(field.Selector);
            var m = r.Match(source);
            if (!m.Success) throw new Exception("正则失败");
            return new Result(field.Name, m.Groups[1].Value);
        }

        protected Result DoHtml(Page page, string source, Field field)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(source);

            var n = doc.DocumentNode.SelectSingleNode(field.Selector);
            return new Result(field.Name, n.InnerText);
        }

        protected Result DoJson(Page page, string source, Field field)
        {
            JObject j;
            try
            {
                j = JObject.Parse(source);
            }
            catch (Exception)
            {
                throw new Exception("解析出现问题:" + source);
            }
            return new Result(field.Name, j.SelectToken(field.Selector).Value<string>());
        }
    }

    public class DefaultProcessor : BaseProcessor
    {

    }

    public class Result
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public Type Type { get; set; } = typeof(string);
        /// <summary>
        /// 跳过存储
        /// </summary>
        public bool Skip { get; set; } = false;

        public Result(string k, string v)
        {
            Key = k;
            Value = v;
        }
    }


}