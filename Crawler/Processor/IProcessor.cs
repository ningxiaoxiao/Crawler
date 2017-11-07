using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Crawler.Core.Scheduler;
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

        Config Config { get; }
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
        public Config Config => Crawler.Config;
        public VoidPageDelegate OnProcessScanPage { get; set; }
        public VoidPageDelegate OnProcessHelperPage { get; set; }
        public VoidPageDelegate OnProcessContentPage { get; set; }
        public VoidPageResultDelegate AfterExtractField { get; set; }
        public VoidPageDelegate OnComplete { get; set; }
        public VoidPageDelegate OnCustomExtract { get; set; }
        
        public void Handle(Page page)
        {
            Logger.Info($"开始处理 {page.Request.Url}");


            switch (page.Request.Type)
            {
                case PageType.ScanUrl:
                    OnProcessScanPage?.Invoke(page);
                    break;
                case PageType.HelperUrl:
                    Logger.Info($"列表页");
                    //识别列表页
                    OnProcessHelperPage?.Invoke(page);
                    break;
                case PageType.ContextUrl:
                    Logger.Info($"内容页");
                    //识别内容页
                    OnProcessContentPage?.Invoke(page);
                    break;
                default:
                    Logger.Info($"未知页");
                    //什么都不是的网页,跳过抽取,
                    page.SkipExtract();
                    page.SkipSaveData();
                    page.SkipFind();

                    break;
            }

            if (page.SkipExtractField)
            {
                Logger.Info($"跳过抽取");
                SkipCount++;
            }
            else
            {
                Logger.Info($"开始抽取");
                if (OnCustomExtract == null)
                {

                    Extract(page);
                }
                else
                {
                    Logger.Info($"使用自定抽取");
                    OnCustomExtract.Invoke(page);
                }

                Logger.Info($"抽取完成");
                OnComplete?.Invoke(page);
                Logger.Info($"{page.Request.Url} 抽取到{page.Results.Count}结果");
                ExtractCount++;
            }

            Logger.Info($"深度检查,当前深度[{page.Request.Deth}],最大深度[{Config.MaxDeth}]");
            //查看深度 如果大于最大深度,就不会向这个网页中再查找网址
            if (page.Request.Deth > Config.MaxDeth) return;


            if (!page.SkipFindUrl)
            {
                Logger.Info($"启动查找");
                var t = new Thread(FindUrl);
                t.Start(page);
            }
            Logger.Info($"处理结束");
        }

        protected void FindUrl(object pageobj)
        {

            var page = (Page)pageobj;
            Logger.Info($"查找新的URL开始,当前深度: " + page.Request.Deth);
            var r = new Regex("href=\"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");

            var ms = r.Matches(page.Html);
            foreach (Match match in ms)
            {
                var url = match.Value.Remove(0, 6);

                if (Config.HelperUrlRegexes.IsMatch(url) || Config.ContentUrlRegexes.IsMatch(url))
                {
                    Logger.Info($"在[{page.Request.Url}]发现新网页[{url}]");
                    page.Request.Schduler.AddUrl(url, PageType.ContextUrl,page.Request.Deth + 1);
                }
            }
            Logger.Info($"查找新的URL结束");
        }

        protected void Extract(Page page)
        {
            var results = new ExtractResults ();
            
            foreach (var field in Config.Fields)
            {
                try
                {
                    string source;
                    switch (field.SourceType)
                    {
                        case SourceType.Page:
                            source = page.Html;
                            break;
                        case SourceType.AttachedUrl:
                            throw new NotImplementedException();
                        case SourceType.UrlContext:
                            source = page.Request.Url;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    Result result;
                    switch (field.Selectortype)
                    {
                        case SelectorType.JsonPath:
                            result = DoJson(source, field);
                            break;
                        case SelectorType.XPath:
                            result = DoHtml(source, field);
                            break;
                        case SelectorType.Regex:
                            result = DoRegex(source, field);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                   
                    results.Add(result);
                }
                catch (Exception e)
                {
                    Logger.Error($"{page.Request.Url} 抽取 {field.Selectortype} {field.Name} 失败 \r\n{e}");
                    FailCount++;
                    return;
                }
            }
             
             page.Results.Add(results);
             //AfterExtractField?.Invoke(page, result);
        }

        public static Result DoRegex(string source, Field field)
        {
            var r = new Regex(field.Selector);
            var m = r.Match(source);
            if (!m.Success)
               Crawler.Logger.Error("正则失败");
            return new Result(field.Name, m.Groups[1].Value);
        }

        public static Result DoHtml(string source, Field field)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(source);

            var n = doc.DocumentNode.SelectSingleNode(field.Selector);
            return new Result(field.Name, n.InnerText);
        }

        public static Result DoJson(string source, Field field)
        {
            
            try
            {
                var j = JObject.Parse(source);

                var v = j.SelectToken(field.Selector);
                if (v == null) return null;

                return new Result(field.Name, v.Value<string>());
            }
            catch 
            {
                Crawler.Logger.Error("json解析出现问题:" +field);
            }
            return null;
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