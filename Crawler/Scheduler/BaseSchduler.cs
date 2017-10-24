using System;
using System.Collections.Generic;
using System.Net;
using NLog;

namespace Crawler.Core.Scheduler
{
    public abstract class BaseSchduler : ISchduler
    {
        public int Left
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public Config Config => Crawler.Config;

        public Logger Logger => Crawler.Logger;

        private readonly Queue<Request> _queue = new Queue<Request>();

        private static readonly object _lock = new object();

        /// <summary>
        /// 全局使用的头
        /// </summary>
        private readonly WebHeaderCollection _headers = new WebHeaderCollection();


        private readonly CookieContainer _cookies = new CookieContainer();
        public Request GetNext()
        {
            lock (_lock)
            {
                if (_queue.Count == 0) return null;

                var r = _queue.Dequeue();
                Logger.Info($"{r.Url} 出队 剩余:{_queue.Count}");
                return r;
            }

        }

        public string GetCookie(string name, string domain)
        {
            var cs = _cookies.GetCookies(new Uri("http://" + domain));
            return cs[name] == null ? null : cs[name].Value;
        }

        public void AddRequest(Request r)
        {
            lock (_lock)
            {
                //去重
                if (_queue.Contains(r))
                {
                    Logger.Warn($"{r.Url} 入队失败,重复");
                }
                else
                {
                    _queue.Enqueue(r);
                    Logger.Info($"{r.Url} 入队 深度:{r.Deth} 剩余:{_queue.Count}");
                }
            }

        }
        public void AddHeader(string key, string value)
        {
            _headers.Add(key, value);
        }

        public void AddCookie(CookieCollection cc)
        {
            lock (_lock)
            {
                if (cc.Count == 0) return;

                foreach (Cookie c in cc)
                {
                    _cookies.Add(c);
                }
                
            }

        }

        public void AddCookie(string key, string value, string domain = null)
        {
            if (domain == null)
                foreach (var d in Config.Domains)
                {
                    _cookies.Add(new Cookie(key, value, "/", d));
                }
            else
                _cookies.Add(new Cookie(key, value, "/", domain));

        }

        public void AddUrl(string url, int deth = 0, Options options = null)
        {

            if (!url.Contains("http"))
                throw new Exception("没有加http or https");

            var domainchcke = false;
            foreach (var domain in Config.Domains)
            {
                domainchcke = url.Contains(domain);
                if (domainchcke)
                    break;

            }

            //过滤不合法域
            if (!domainchcke)
            {
                Logger.Warn($"{url} 入队失败,不合法域");
                return;
            }


            if (options == null)
            {
                options = new Options();//使用默认值
            }

            

            var r = new Request(this)
            {
                Method = options.Method,
                Url = url,
                Deth = deth,
                Postdata = options.Data,
                Timeout = Config.Timeout,
                UserAgent = Config.UserAgent,
                Header = new WebHeaderCollection
                {
                    _headers,
                    options.Headers
                },
                CookieCollection = _cookies.GetCookies(new Uri(url)),
            };

            AddRequest(r);



        }

        public void AddUrls(IEnumerable<string> urls, int deth = 0, Options os = null)
        {
            foreach (var u in urls)
            {
                AddUrl(u, deth, os);
            }
        }

        public void AddScanUrl(string url, Options options = null)
        {
            AddUrl(url, 0, options);
        }

        public string RequestUrl(string url, Options options = null)
        {
            throw new NotImplementedException();
        }

        public void SetUserAgent(string userAgent)
        {
            throw new NotImplementedException();
        }

        public void SetCharset(string charset)
        {
            throw new NotImplementedException();
        }

    }

    public class DefaultSchduler : BaseSchduler
    {

    }

}