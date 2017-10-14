using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NLog;

namespace Crawler.Core.Processor
{
    public delegate void ResultDelegate(IEnumerable<KeyValuePair<string, dynamic>> results);

    public delegate void VoidPageDelegate(Page p);

    public interface IProcessor
    {
        int SkipCount { get; }
        int ExtractCount { get; }
        int FailCount { get; }
        Logger Logger { get; }

        Config Config { get; set; }
        VoidPageDelegate OnProcessScanPage { get; set; }
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
        public VoidPageDelegate OnComplete { get; set; }
        public void Handle(Page page)
        {
            if (page.Request.URL == Config.ScanUrls)
            {
                OnProcessScanPage?.Invoke(page);
            }

            if (page.SkipExtractField)
            {
                SkipCount++;
                return;
            }

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

                    switch (field.Selectortype)
                    {
                        case SelectorType.JsonPath:
                            DoJson(page, source, field);
                            break;
                        case SelectorType.XPath:
                            DoHtml(page, source, field);
                            break;
                        case SelectorType.Regex:
                            DoRegex(page, source, field);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"{page.Request.URL} 抽取 {field.Selectortype} {field.Name} 失败 \r\n{e}");
                    FailCount++;
                    return;
                }
            }



            OnComplete?.Invoke(page);
            Logger.Info($"{page.Request.URL} 抽取成功");
            ExtractCount++;
        }

        protected void DoRegex(Page page, string source, Field field)
        {
            var r = new Regex(field.Selector);
            var m = r.Match(source);
            if (!m.Success) throw new Exception("正则失败");
            var v = m.Groups[1].Value;
            page.Results.Add(field.Name, v);
        }

        protected void DoHtml(Page page, string source, Field field)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(source);

            var n = doc.DocumentNode.SelectSingleNode(field.Selector);
            page.Results.Add(field.Name, n.InnerText);

        }

        protected void DoJson(Page page, string source, Field field)
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

            var v = j.SelectToken(field.Selector).Value<string>();
            page.Results.Add(field.Name, v);


        }
    }

    public class DefaultProcessor : BaseProcessor
    {

    }




}