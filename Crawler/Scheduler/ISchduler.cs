﻿using System;
using System.Collections.Generic;
using System.Net;
using NLog;

namespace CrawlerDotNet.Core.Scheduler
{
    public interface ISchduler
    {
        Logger Logger { get; }
        int Left { get; }
        Config Config { get; }


        Request GetNext();

        /// <summary>
        /// 一般在beforeCrawl回调函数中调用, 用来给的所有HTTP请求添加一个Header
        /// </summary>
        /// <param name="key">Header的属性, String类型, 必填项, 无默认值. 常见属性有User-Agent, Referer等</param>
        /// <param name="value"> Header的属性对应的值, String类型, 必填项, 无默认值</param>
        void AddHeader(string key, string value);
        void AddCookie(CookieCollection cookieCollection);

        /// <summary>
        /// 一般在beforeCrawl回调函数中调用, 用来给的所有HTTP请求添加一个Cookie
        /// </summary>
        /// <param name="key">Cookie的名称, String类型, 必填项, 无默认值</param>
        /// <param name="value">Cookie的值, String类型, 必填项, 无默认值</param>
        /// <param name="domain">添加Cookie到该域名, String类型, 可不填, 无默认值. 如果不传入参数, 表示给domains中的第一个域名添加Cookie</param>
        void AddCookie(string key, string value, string domain = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Cookie的名称, String类型, 必填项, 无默认值</param>
        /// <param name="domain">域名, String类型, 可不填, 无默认值. 如果不传入参数, 表示通过domains中的域名得到第一个符合name的Cookie</param>
        /// <returns>Cookie的值, String类型. 如果在域名下未找到Cookie, 返回值是null</returns>
        string GetCookie(string name, string domain);
        /// <summary>
        /// 一般在onProcessScanPage, onProcessHelperPage和onProcessContentPage回调函数中调用, 用来往待爬队列中添加网页url
        /// </summary>
        /// <param name="url">待添加的网页url, String类型, 必填项, 无默认值</param>
        /// <param name="options">网页url的请求参数等设置, JS对象, 可不填, 无默认值. 成员包括: Method, Data, ContextData, Headers, Reserve, NoProxy, Charset, Events, DupValue, EnableJs, IgnoreCookies, UrlEncodeData</param>
        void AddUrl(string url, PageType type,int deth = 0, Options options = null);
        void AddUrl(string url, int deth);


        /// <summary>
        /// 一般在beforeCrawl回调函数中调用, 用来添加入口页url
        /// </summary>
        /// <param name="url">待添加的入口页url, String类型, 必填项, 无默认值</param>
        /// <param name="options">入口页url的请求参数等设置, JS对象, 可不填, 无默认值. 成员包括: Method, Data, ContextData, Headers, Reserve, NoProxy, Charset, Events, DupValue, EnableJs, IgnoreCookies, UrlEncodeData</param>
        void AddScanUrl(string url, Options options = null);

        /// <summary>
        /// 下载网页, 返回网页内容,
        /// 一般在
        /// beforeCrawl, afterDownloadPage, onProcessScanPage, onProcessHelperPage和onProcessContentPage
        /// 回调函数中调用, 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        string RequestUrl(string url, Options options = null);

        /// <summary>
        /// 一般在beforeCrawl回调函数中调用, 设置浏览器userAgent
        /// </summary>
        /// <param name="userAgent"></param>
        void SetUserAgent(string userAgent);

        /// <summary>
        /// 一般在beforeCrawl回调函数中调用, 设置请求网页的编码格式
        /// </summary>
        /// <param name="charset">网页编码格式, String类型, 必填项, 无默认值. 常用编码格式有”UTF-8”, “GBK”, “gb2312”等</param>
        void SetCharset(string charset);

        void AddRequest(Request r);
    }
}