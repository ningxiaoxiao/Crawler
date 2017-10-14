using System;
using System.Net;

namespace Crawler.Core
{
    /// <summary>
    /// options对象中定义了HTTP请求网页时可配置的参数, 以及能进行的操作, 下面会逐一介绍可调用的成员.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// HTTP请求类型, String类型, 可不填, 默认值是GET
        /// </summary>
        public string Method { get; set; } = "GET";
        /// <summary>
        /// post请求的请求参数,  可不填, 无默认值
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 是否对POST请求的data编码, 布尔类型, 可不填, 默认值是true
        /// 如果urlEncodeData值设为true, 表示会自动对POST请求的data编码.编码类型会按照下表顺序查找, 并优先使用最先查到的编码类型:
        /// </summary>
        public bool UrlEncodeData { get; set; } = true;
        /// <summary>
        /// 网页附加数据, String类型, 可不填, 无默认值
        /// 建议内容为HTML代码, 方便使用XPath抽取
        /// </summary>
        public string ContextData { get; set; }
        /// <summary>
        /// HTTP请求的Headers, JS对象, 可不填, 无默认值
        /// 可添加cookie, referer等Headers
        /// 如果cookie值设为空字符串, 表示HTTP请求不使用Cookies
        /// </summary>
        public WebHeaderCollection Headers { get; set; }=new WebHeaderCollection();

        /// <summary>
        /// 在将待爬网页url添加到待爬队列时使用, 设置该url是否自动去重, 布尔类型, 可不填, 默认值是false
        /// </summary>
        public bool Reserve { get; set; } = false;

        /// <summary>
        /// HTTP请求时是否使用代理IP, 布尔类型, 可不填, 默认值是false
        /// 如果noProxy值设为false, 表示使用代理IP; 如果noProxy值设为true, 表示不使用代理IP
        /// </summary>
        public bool NoProxy { get; set; } = false;

        /// <summary>
        /// 网页的编码类型, String类型, 可不填, 默认值是UTF-8
        /// </summary>
        public string Charset { get; set; } = "UTF-8";

        /// <summary>
        /// 是否对网页使用base64编码, 布尔类型, 可不填, 默认值是false
        /// </summary>
        public bool Base64 { get; set; } = false;
        /// <summary>
        /// 设置JS渲染网页后需要触发的事件, 数组类型, 可不填, 无默认值
        /// </summary>
        [Obsolete("未实现,不可用")]
        public string Events { get;}
        /// <summary>
        /// 在将待爬网页url添加到待爬队列时使用, 设置url自动去重的判断依据
        /// </summary>
        [Obsolete("未实现,不可用")]
        public bool DupValue { get; }

        /// <summary>
        /// HTTP请求时是否自动JS渲染, 布尔类型, 可不填, 默认值是false
        /// </summary>
        [Obsolete("未实现,不可用")]
        public bool EnableJs { get; set; } = false;
        /// <summary>
        /// 调用hostFile函数托管文件时, 设置托管的文件大小, 整型, 可不填, 无默认值, 托管的文件大小上限是16MB
        /// </summary>
        [Obsolete("未实现,不可用")]
        public int FileSize { get; set; }

        /// <summary>
        /// 是否不保存本次HTTP请求返回的Cookies, 布尔类型, 可不填, 默认值是false表示保存
        /// 如果ignoreCookies值设为true, 表示不保存本次HTTP请求返回的Cookies
        /// </summary>
        [Obsolete("未实现,不可用")]
        public bool IgnoreCookies { get; set; } = false;



    }
}