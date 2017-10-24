using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler.Core.Downloader
{
    public struct pairItem
    {
        public string key;
        public string value;
    };

    /// <summary>
    /// Http连接操作帮助类
    /// </summary>
    public class HttpHelper
    {
        #region 预定义方变量
        //默认的编码
        private Encoding _encoding = Encoding.Default;
        //Post数据编码
        private Encoding _postencoding = Encoding.Default;
        //HttpWebRequest对象用来发起请求
        private HttpWebRequest _request;
        //获取影响流的数据对象
        private HttpWebResponse _response;
        //设置本地的出口ip和端口
        private IPEndPoint _ipEndPoint;
        #endregion

     
        // parse the Set-Cookie string (in http response header) to cookies
        // Note: auto omit to parse the abnormal cookie string
        // normal example for 'setCookieStr':
        // MSPOK= ; expires=Thu, 30-Oct-1980 16:00:00 GMT;domain=login.live.com;path=/;HTTPOnly= ;version=1,PPAuth=Cuyf3Vp2wolkjba!TOr*0v22UMYz36ReuiwxZZBc8umHJYPlRe4qupywVFFcIpbJyvYZ5ZDLBwV4zRM1UCjXC4tUwNuKvh21iz6gQb0Tu5K7Z62!TYGfowB9VQpGA8esZ7iCRucC7d5LiP3ZAv*j4Z3MOecaJwmPHx7!wDFdAMuQUZURhHuZWJiLzHP1j8ppchB2LExnlHO6IGAdZo1f0qzSWsZ2hq*yYP6sdy*FdTTKo336Q1B0i5q8jUg1Yv6c2FoBiNxhZSzxpuU0WrNHqSytutP2k4!wNc6eSnFDeouX; domain=login.live.com;secure= ;path=/;HTTPOnly= ;version=1,PPLState=1; domain=.live.com;path=/;version=1,MSPShared=1; expires=Wed, 30-Dec-2037 16:00:00 GMT;domain=login.live.com;path=/;HTTPOnly= ;version=1,MSPPre= ;domain=login.live.com;path=/;Expires=Thu, 30-Oct-1980 16:00:00 GMT,MSPCID= ; HTTPOnly= ; domain=login.live.com;path=/;Expires=Thu, 30-Oct-1980 16:00:00 GMT,RPSTAuth=EwDoARAnAAAUWkziSC7RbDJKS1VkhugDegv7L0eAAOfCAY2+pKwbV5zUlu3XmBbgrQ8EdakmdSqK9OIKfMzAbnU8fuwwEi+FKtdGSuz/FpCYutqiHWdftd0YF21US7+1bPxuLJ0MO+wVXB8GtjLKZaA0xCXlU5u01r+DOsxSVM777DmplaUc0Q4O1+Pi9gX9cyzQLAgRKmC/QtlbVNKDA2YAAAhIwqiXOVR/DDgBocoO/n0u48RFGh79X2Q+gO4Fl5GMc9Vtpa7SUJjZCCfoaitOmcxhEjlVmR/2ppdfJx3Ykek9OFzFd+ijtn7K629yrVFt3O9q5L0lWoxfDh5/daLK7lqJGKxn1KvOew0SHlOqxuuhYRW57ezFyicxkxSI3aLxYFiqHSu9pq+TlITqiflyfcAcw4MWpvHxm9on8Y1dM2R4X3sxuwrLQBpvNsG4oIaldTYIhMEnKhmxrP6ZswxzteNqIRvMEKsxiksBzQDDK/Cnm6QYBZNsPawc6aAedZioeYwaV3Z/i3tNrAUwYTqLXve8oG6ZNXL6WLT/irKq1EMilK6Cw8lT3G13WYdk/U9a6YZPJC8LdqR0vAHYpsu/xRF39/On+xDNPE4keIThJBptweOeWQfsMDwvgrYnMBKAMjpLZwE=; domain=.live.com;path=/;HTTPOnly= ;version=1,RPSTAuthTime=1328679636; domain=login.live.com;path=/;HTTPOnly= ;version=1,MSPAuth=2OlAAMHXtDIFOtpaK1afG2n*AAxdfCnCBlJFn*gCF8gLnCa1YgXEfyVh2m9nZuF*M7npEwb4a7Erpb*!nH5G285k7AswJOrsr*gY29AVAbsiz2UscjIGHkXiKrTvIzkV2M; domain=.live.com;path=/;HTTPOnly= ;version=1,MSPProf=23ci9sti6DZRrkDXfTt1b3lHhMdheWIcTZU2zdJS9!zCloHzMKwX30MfEAcCyOjVt*5WeFSK3l2ZahtEaK7HPFMm3INMs3r!JxI8odP9PYRHivop5ryohtMYzWZzj3gVVurcEr5Bg6eJJws7rXOggo3cR4FuKLtXwz*FVX0VWuB5*aJhRkCT1GZn*L5Pxzsm9X; domain=.live.com;path=/;HTTPOnly= ;version=1,MSNPPAuth=CiGSMoUOx4gej8yQkdFBvN!gvffvAhCPeWydcrAbcg!O2lrhVb4gruWSX5NZCBPsyrtZKmHLhRLTUUIxxPA7LIhqW5TCV*YcInlG2f5hBzwzHt!PORYbg79nCkvw65LKG399gRGtJ4wvXdNlhHNldkBK1jVXD4PoqO1Xzdcpv4sj68U6!oGrNK5KgRSMXXpLJmCeehUcsRW1NmInqQXpyanjykpYOcZy0vq!6PIxkj3gMaAvm!1vO58gXM9HX9dA0GloNmCDnRv4qWDV2XKqEKp!A7jiIMWTmHup1DZ!*YCtDX3nUVQ1zAYSMjHmmbMDxRJECz!1XEwm070w16Y40TzuKAJVugo!pyF!V2OaCsLjZ9tdGxGwEQRyi0oWc*Z7M0FBn8Fz0Dh4DhCzl1NnGun9kOYjK5itrF1Wh17sT!62ipv1vI8omeu0cVRww2Kv!qM*LFgwGlPOnNHj3*VulQOuaoliN4MUUxTA4owDubYZoKAwF*yp7Mg3zq5Ds2!l9Q$$; domain=.live.com;path=/;HTTPOnly= ;version=1,MH=MSFT; domain=.live.com;path=/;version=1,MHW=; expires=Thu, 30-Oct-1980 16:00:00 GMT;domain=.live.com;path=/;version=1,MHList=; expires=Thu, 30-Oct-1980 16:00:00 GMT;domain=.live.com;path=/;version=1,NAP=V=1.9&E=bea&C=zfjCKKBD0TqjZlWGgRTp__NiK08Lme_0XFaiKPaWJ0HDuMi2uCXafQ&W=1;domain=.live.com;path=/,ANON=A=DE389D4D076BF47BCAE4DC05FFFFFFFF&E=c44&W=1;domain=.live.com;path=/,MSPVis=$9;domain=login.live.com;path=/,pres=; expires=Thu, 30-Oct-1980 16:00:00 GMT;domain=.live.com;path=/;version=1,LOpt=0; domain=login.live.com;path=/;version=1,WLSSC=EgBnAQMAAAAEgAAACoAASfCD+8dUptvK4kvFO0gS3mVG28SPT3Jo9Pz2k65r9c9KrN4ISvidiEhxXaPLCSpkfa6fxH3FbdP9UmWAa9KnzKFJu/lQNkZC3rzzMcVUMjbLUpSVVyscJHcfSXmpGGgZK4ZCxPqXaIl9EZ0xWackE4k5zWugX7GR5m/RzakyVIzWAFwA1gD9vwYA7Vazl9QKMk/UCjJPECcAAAoQoAAAFwBjcmlmYW4yMDAzQGhvdG1haWwuY29tAE8AABZjcmlmYW4yMDAzQGhvdG1haWwuY29tAAAACUNOAAYyMTM1OTIAAAZlCAQCAAB3F21AAARDAAR0aWFuAAR3YW5nBMgAAUkAAAAAAAAAAAAAAaOKNpqLi/UAANQKMk/Uf0RPAAAAAAAAAAAAAAAADgA1OC4yNDAuMjM2LjE5AAUAAAAAAAAAAAAAAAABBAABAAABAAABAAAAAAAAAAA=; domain=.live.com;secure= ;path=/;HTTPOnly= ;version=1,MSPSoftVis=@72198325083833620@:@; domain=login.live.com;path=/;version=1
        // here now support parse the un-correct Set-Cookie:
        // MSPRequ=/;Version=1;version&lt=1328770452&id=250915&co=1; path=/;version=1,MSPVis=$9; Version=1;version=1$250915;domain=login.live.com;path=/,MSPSoftVis=@72198325083833620@:@; domain=login.live.com;path=/;version=1,MSPBack=1328770312; domain=login.live.com;path=/;version=1
        public CookieCollection parseSetCookie(string setCookieStr, string curDomain)
        {
            CookieCollection parsedCookies = new CookieCollection();

            // process for expires and Expires field, for it contains ','
            //refer: http://www.yaosansi.com/post/682.html
            // may contains expires or Expires, so following use xpires
            string commaReplaced = Regex.Replace(setCookieStr, @"xpires=\w{3},\s\d{2}-\w{3}-\d{4}", new MatchEvaluator(_processExpireField));
            string[] cookieStrArr = commaReplaced.Split(',');
            foreach (string cookieStr in cookieStrArr)
            {
                Cookie ck = new Cookie();
                // recover it back
                string recoveredCookieStr = Regex.Replace(cookieStr, @"xpires=\w{3}" + replacedChar + @"\s\d{2}-\w{3}-\d{4}", new MatchEvaluator(_recoverExpireField));
                if (parseSingleCookie(recoveredCookieStr, ref ck))
                {
                    if (needAddThisCookie(ck, curDomain))
                    {
                        parsedCookies.Add(ck);
                    }
                }
            }

            return parsedCookies;
        }//parseSetCookie
         //replace ',' with replacedChar
        private string _processExpireField(Match foundExpire)
        {
            string replacedComma = "";
            replacedComma = foundExpire.Value.ToString().Replace(',', replacedChar);
            return replacedComma;
        }
        // replace the replacedChar back to original ','
        private string _recoverExpireField(Match foundPprocessedExpire)
        {
            string recovedStr = "";
            recovedStr = foundPprocessedExpire.Value.Replace(replacedChar, ',');
            return recovedStr;
        }
        const char replacedChar = '_';
        private bool needAddThisCookie(Cookie ck, string curDomain)
        {
            bool needAdd = false;

            if ((ck == null) || (ck.Name == ""))
            {
                needAdd = false;
            }
            else
            {
                if (ck.Domain != "")
                {
                    needAdd = true;
                }
                else// ck.Domain == ""
                {
                    if (curDomain != "")
                    {
                        ck.Domain = curDomain;
                        needAdd = true;
                    }
                    else // curDomain == ""
                    {
                        // not set current domain, omit this
                        // should not add empty domain cookie, for this will lead execute CookieContainer.Add() fail !!!
                        needAdd = false;
                    }
                }
            }

            return needAdd;
        }
        // parse Set-Cookie string part into cookies
        // leave current domain to empty, means omit the parsed cookie, which is not set its domain value
        public CookieCollection parseSetCookie(string setCookieStr)
        {
            return parseSetCookie(setCookieStr, "");
        }
        // parse cookie field expression
        public bool parseCookieField(string ckFieldExpr, out pairItem pair)
        {
            bool parsedOK = false;

            if (ckFieldExpr == "")
            {
                pair.key = "";
                pair.value = "";
                parsedOK = false;
            }
            else
            {
                ckFieldExpr = ckFieldExpr.Trim();

                //some specials: secure/httponly
                if (ckFieldExpr.ToLower() == "httponly")
                {
                    pair.key = "httponly";
                    //pair.value = "";
                    pair.value = "true";
                    parsedOK = true;
                }
                else if (ckFieldExpr.ToLower() == "secure")
                {
                    pair.key = "secure";
                    //pair.value = "";
                    pair.value = "true";
                    parsedOK = true;
                }
                else // normal cookie field
                {
                    int equalPos = ckFieldExpr.IndexOf('=');
                    if (equalPos > 0) // is valid expression
                    {
                        pair.key = ckFieldExpr.Substring(0, equalPos);
                        pair.key = pair.key.Trim();
                        if (isValidCookieField(pair.key))
                        {
                            // only process while is valid cookie field
                            pair.value = ckFieldExpr.Substring(equalPos + 1);
                            pair.value = pair.value.Trim();
                            parsedOK = true;
                        }
                        else
                        {
                            pair.key = "";
                            pair.value = "";
                            parsedOK = false;
                        }
                    }
                    else
                    {
                        pair.key = "";
                        pair.value = "";
                        parsedOK = false;
                    }
                }
            }

            return parsedOK;
        }
        public bool isValidCookieField(string cookieKey)
        {
            return cookieFieldList.Contains(cookieKey.ToLower());
        }
        List<string> cookieFieldList = new List<string>();
        //add recognized cookie field: expires/domain/path/secure/httponly/version, into cookie
        public bool addFieldToCookie(ref Cookie ck, pairItem pairInfo)
        {
            bool added = false;
            if (pairInfo.key != "")
            {
                string lowerKey = pairInfo.key.ToLower();
                switch (lowerKey)
                {
                    case "expires":
                        DateTime expireDatetime;
                        if (DateTime.TryParse(pairInfo.value, out expireDatetime))
                        {
                            // note: here coverted to local time: GMT +8
                            ck.Expires = expireDatetime;

                            //update expired filed
                            if (DateTime.Now.Ticks > ck.Expires.Ticks)
                            {
                                ck.Expired = true;
                            }

                            added = true;
                        }
                        break;
                    case "domain":
                        ck.Domain = pairInfo.value;
                        added = true;
                        break;
                    case "secure":
                        ck.Secure = true;
                        added = true;
                        break;
                    case "path":
                        ck.Path = pairInfo.value;
                        added = true;
                        break;
                    case "httponly":
                        ck.HttpOnly = true;
                        added = true;
                        break;
                    case "version":
                        int versionValue;
                        if (int.TryParse(pairInfo.value, out versionValue))
                        {
                            ck.Version = versionValue;
                            added = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            return added;
        }//addFieldToCookie
        //parseCookieField
        //parse single cookie string to a cookie
        //example:
        //MSPShared=1; expires=Wed, 30-Dec-2037 16:00:00 GMT;domain=login.live.com;path=/;HTTPOnly= ;version=1
        //PPAuth=CkLXJYvPpNs3w!fIwMOFcraoSIAVYX3K!CdvZwQNwg3Y7gv74iqm9MqReX8XkJqtCFeMA6GYCWMb9m7CoIw!ID5gx3pOt8sOx1U5qQPv6ceuyiJYwmS86IW*l3BEaiyVCqFvju9BMll7!FHQeQholDsi0xqzCHuW!Qm2mrEtQPCv!qF3Sh9tZDjKcDZDI9iMByXc6R*J!JG4eCEUHIvEaxTQtftb4oc5uGpM!YyWT!r5jXIRyxqzsCULtWz4lsWHKzwrNlBRbF!A7ZXqXygCT8ek6luk7rarwLLJ!qaq2BvS; domain=login.live.com;secure= ;path=/;HTTPOnly= ;version=1
        public bool parseSingleCookie(string cookieStr, ref Cookie ck)
        {
            bool parsedOk = true;
            //Cookie ck = new Cookie();
            //string[] expressions = cookieStr.Split(";".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            //refer: http://msdn.microsoft.com/en-us/library/b873y76a.aspx
            string[] expressions = cookieStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            //get cookie name and value
            pairItem pair = new pairItem();
            if (parseCookieNameValue(expressions[0], out pair))
            {
                ck.Name = pair.key;
                ck.Value = pair.value;

                string[] fieldExpressions = getSubStrArr(expressions, 1, expressions.Length - 1);
                foreach (string eachExpression in fieldExpressions)
                {
                    //parse key and value
                    if (parseCookieField(eachExpression, out pair))
                    {
                        // add to cookie field if possible
                        addFieldToCookie(ref ck, pair);
                    }
                    else
                    {
                        // if any field fail, consider it is a abnormal cookie string, so quit with false
                        parsedOk = false;
                        break;
                    }
                }
            }
            else
            {
                parsedOk = false;
            }

            return parsedOk;
        }//parseSingleCookie

        //given a string array 'origStrArr', get a sub string array from 'startIdx', length is 'len'
        public string[] getSubStrArr(string[] origStrArr, int startIdx, int len)
        {
            string[] subStrArr = new string[] { };
            if ((origStrArr != null) && (origStrArr.Length > 0) && (len > 0))
            {
                List<string> strList = new List<string>();
                int endPos = startIdx + len;
                if (endPos > origStrArr.Length)
                {
                    endPos = origStrArr.Length;
                }

                for (int i = startIdx; i < endPos; i++)
                {
                    //refer: http://zhidao.baidu.com/question/296384408.html
                    strList.Add(origStrArr[i]);
                }

                subStrArr = new string[len];
                strList.CopyTo(subStrArr);
            }

            return subStrArr;
        }

        // parse the cookie name and value
        public bool parseCookieNameValue(string ckNameValueExpr, out pairItem pair)
        {
            bool parsedOK = false;
            if (ckNameValueExpr == "")
            {
                pair.key = "";
                pair.value = "";
                parsedOK = false;
            }
            else
            {
                ckNameValueExpr = ckNameValueExpr.Trim();

                int equalPos = ckNameValueExpr.IndexOf('=');
                if (equalPos > 0) // is valid expression
                {
                    pair.key = ckNameValueExpr.Substring(0, equalPos);
                    pair.key = pair.key.Trim();
                    if (isValidCookieName(pair.key))
                    {
                        // only process while is valid cookie field
                        pair.value = ckNameValueExpr.Substring(equalPos + 1);
                        pair.value = pair.value.Trim();
                        parsedOK = true;
                    }
                    else
                    {
                        pair.key = "";
                        pair.value = "";
                        parsedOK = false;
                    }
                }
                else
                {
                    pair.key = "";
                    pair.value = "";
                    parsedOK = false;
                }
            }
            return parsedOK;
        }

        //check whether the cookie name is valid or not
        public bool isValidCookieName(string ckName)
        {
            bool isValid = true;
            if (ckName == null)
            {
                isValid = false;
            }
            else
            {
                string invalidP = @"\W+";
                Regex rx = new Regex(invalidP);
                Match foundInvalid = rx.Match(ckName);
                if (foundInvalid.Success)
                {
                    isValid = false;
                }
            }

            return isValid;
        }
        #region Public

        /// <summary>
        /// 根据相传入的数据，得到相应页面数据
        /// </summary>
        /// <param name="item">参数类对象</param>
        /// <returns>返回HttpResult类型</returns>
        public HttpResult GetHtml(HttpItem item)
        {
            //返回参数
            var result = new Page();
            try
            {
                //准备参数
                SetRequest(item);


            }
            catch (Exception ex)
            {
                //配置参数时出错
                return new HttpResult()
                {
                    Cookie = string.Empty,
                    Header = null,
                    Html = ex.Message,
                    StatusDescription = "配置参数时出错：" + ex.Message,
                    StatusCode = HttpStatusCode.NotFound
                };
            }
            try
            {
                //请求数据
                _response = (HttpWebResponse)_request.GetResponse();

                GetData(item, result);

            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    _response = (HttpWebResponse)ex.Response;

                    GetData(item, result);

                }
                else
                {
                    result.Html = ex.Message;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }
            if (item.IsToLower) result.Html = result.Html.ToLower();
            result.Response = _response;
            return result;
        }
        #endregion

        #region GetData

        /// <summary>
        /// 获取数据的并解析的方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        private void GetData(HttpItem item, HttpResult result)
        {
            if (_response == null)
            {
                return;
            }
            #region base
            //获取StatusCode
            result.StatusCode = _response.StatusCode;
            //获取StatusDescription
            result.StatusDescription = _response.StatusDescription;
            //获取Headers
            result.Header = _response.Headers;
            //获取最后访问的URl
            result.ResponseUri = _response.ResponseUri.ToString();
            //获取CookieCollection
            if (_response.Cookies != null) result.CookieCollection = _response.Cookies;
            //获取set-cookie
            if (_response.Headers["set-cookie"] != null) result.Cookie = _response.Headers["set-cookie"];
            #endregion

            #region byte
            //处理网页Byte
            var responseByte = GetByte();
            #endregion

            #region Html
            if (responseByte != null && responseByte.Length > 0)
            {
                //设置编码
                SetEncoding(item, result, responseByte);
                //得到返回的HTML
                result.Html = _encoding.GetString(responseByte);
            }
            else
            {
                //没有返回任何Html代码
                result.Html = string.Empty;
            }
            #endregion
        }
        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="item">HttpItem</param>
        /// <param name="result">HttpResult</param>
        /// <param name="responseByte">byte[]</param>
        private void SetEncoding(HttpItem item, HttpResult result, byte[] responseByte)
        {
            //是否返回Byte类型数据
            if (item.ResultType == ResultType.Byte) result.ResultByte = responseByte;
            //从这里开始我们要无视编码了
            if (_encoding == null)
            {
                var meta = Regex.Match(Encoding.Default.GetString(responseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                var c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }
                if (c.Length > 2)
                {
                    try
                    {
                        _encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(_response.CharacterSet))
                        {
                            _encoding = Encoding.UTF8;
                        }
                        else
                        {
                            _encoding = Encoding.GetEncoding(_response.CharacterSet);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(_response.CharacterSet))
                    {
                        _encoding = Encoding.UTF8;
                    }
                    else
                    {
                        _encoding = Encoding.GetEncoding(_response.CharacterSet);
                    }
                }
            }
        }
        /// <summary>
        /// 提取网页Byte
        /// </summary>
        /// <returns></returns>
        private byte[] GetByte()
        {
            byte[] responseByte;
            using (var stream = new MemoryStream())
            {
                //GZIIP处理
                if (_response.ContentEncoding != null && _response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    //开始读取流并设置编码方式
                    new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress).CopyTo(stream, 10240);
                }
                else
                {
                    //开始读取流并设置编码方式
                    _response.GetResponseStream().CopyTo(stream, 10240);
                }
                //获取Byte
                responseByte = stream.ToArray();
            }
            return responseByte;
        }


        #endregion

        #region SetRequest
        /// <summary>
        /// 为请求准备参数
        /// </summary>
        ///<param name="item">参数列表</param>
        private void SetRequest(HttpItem item)
        {

            // 验证证书
            SetCer(item);
            if (item.IpEndPoint != null)
            {
                _ipEndPoint = item.IpEndPoint;
                //设置本地的出口ip和端口
                _request.ServicePoint.BindIPEndPointDelegate = BindIpEndPointCallback;
            }
            //设置Header参数
            if (item.Header != null && item.Header.Count > 0) foreach (string key in item.Header.AllKeys)
                {
                    _request.Headers.Add(key, item.Header[key]);
                }
            // 设置代理
            SetProxy(item);
            if (item.ProtocolVersion != null) _request.ProtocolVersion = item.ProtocolVersion;
            _request.ServicePoint.Expect100Continue = item.Expect100Continue;
            //请求方式Get或者Post
            _request.Method = item.Method;
            _request.Timeout = item.Timeout;
            _request.KeepAlive = item.KeepAlive;
            _request.ReadWriteTimeout = item.ReadWriteTimeout;
            if (!string.IsNullOrWhiteSpace(item.Host))
            {
                _request.Host = item.Host;
            }
            if (item.IfModifiedSince != null) _request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
            //Accept
            _request.Accept = item.Accept;
            //ContentType返回类型
            _request.ContentType = item.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            _request.UserAgent = item.UserAgent;
            // 编码
            _encoding = item.Encoding;
            //设置安全凭证
            _request.Credentials = item.Credentials;
            //设置Cookie
            SetCookie(item);
            //来源地址
            _request.Referer = item.Referer;
            //是否执行跳转功能
            _request.AllowAutoRedirect = item.Allowautoredirect;
            if (item.MaximumAutomaticRedirections > 0)
            {
                _request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
            }
            //设置Post数据
            SetPostData(item);
            //设置最大连接
            if (item.Connectionlimit > 0) _request.ServicePoint.ConnectionLimit = item.Connectionlimit;
        }
        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="item"></param>
        private void SetCer(HttpItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.CerPath))
            {
                //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest)WebRequest.Create(item.Url);
                SetCerList(item);
                //将证书添加到请求里
                _request.ClientCertificates.Add(new X509Certificate(item.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest)WebRequest.Create(item.Url);
                SetCerList(item);
            }
        }
        /// <summary>
        /// 设置多个证书
        /// </summary>
        /// <param name="item"></param>
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    _request.ClientCertificates.Add(c);
                }
            }
        }
        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="item">Http参数</param>
        private void SetCookie(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.Cookie)) _request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            //设置CookieCollection
            if (item.ResultCookieType == ResultCookieType.CookieCollection)
            {
                _request.CookieContainer = new CookieContainer();
                if (item.CookieCollection != null && item.CookieCollection.Count > 0)
                    _request.CookieContainer.Add(item.CookieCollection);
            }
        }
        /// <summary>
        /// 设置Post数据
        /// </summary>
        /// <param name="item">Http参数</param>
        private void SetPostData(HttpItem item)
        {
            //验证在得到结果时是否有传入数据
            if (!_request.Method.Trim().ToLower().Contains("get"))
            {
                if (item.PostEncoding != null)
                {
                    _postencoding = item.PostEncoding;
                }
                byte[] buffer = null;
                //写入Byte类型
                if (item.PostDataType == PostDataType.Byte && item.PostdataByte != null && item.PostdataByte.Length > 0)
                {
                    //验证在得到结果时是否有传入数据
                    buffer = item.PostdataByte;
                }//写入文件
                else if (item.PostDataType == PostDataType.FilePath && !string.IsNullOrWhiteSpace(item.Postdata))
                {
                    StreamReader r = new StreamReader(item.Postdata, _postencoding);
                    buffer = _postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //写入字符串
                else if (!string.IsNullOrWhiteSpace(item.Postdata))
                {
                    buffer = _postencoding.GetBytes(item.Postdata);
                }
                if (buffer != null)
                {
                    _request.ContentLength = buffer.Length;
                    _request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
                else
                {
                    _request.ContentLength = 0;
                }
            }
        }
        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="item">参数对象</param>
        private void SetProxy(HttpItem item)
        {
            bool isIeProxy = false;
            if (!string.IsNullOrWhiteSpace(item.ProxyIp))
            {
                isIeProxy = item.ProxyIp.ToLower().Contains("ieproxy");
            }
            if (!string.IsNullOrWhiteSpace(item.ProxyIp) && !isIeProxy)
            {
                //设置代理服务器
                if (item.ProxyIp.Contains(":"))
                {
                    string[] plist = item.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    //建议连接
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //给当前请求对象
                    _request.Proxy = myProxy;
                }
                else
                {
                    WebProxy myProxy = new WebProxy(item.ProxyIp, false);
                    //建议连接
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //给当前请求对象
                    _request.Proxy = myProxy;
                }
            }
            else if (isIeProxy)
            {
                //设置为IE代理
            }
            else
            {
                _request.Proxy = item.WebProxy;
            }
        }


        #endregion

        #region private main
        /// <summary>
        /// 回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>Bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }

        /// <summary>
        /// 通过设置这个属性，可以在发出连接的时候绑定客户端发出连接所使用的IP地址。 
        /// </summary>
        /// <param name="servicePoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        private IPEndPoint BindIpEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            return _ipEndPoint;//端口号
        }
        #endregion
    }

    #region public calss
    /// <summary>
    /// Http请求参考类
    /// </summary>
    public class HttpItem
    {
        /// <summary>
        /// 请求URL必须填写
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求方式默认为GET方式,当为POST方式时必须设置Postdata的值
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        /// 默认请求超时时间
        /// </summary>
        public int Timeout { get; set; } = 100000;

        /// <summary>
        /// 默认写入Post数据超时间
        /// </summary>
        public int ReadWriteTimeout { get; set; } = 30000;

        /// <summary>
        /// 设置Host的标头信息
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///  获取或设置一个值，该值指示是否与 Internet 资源建立持久性连接默认为true。
        /// </summary>
        public Boolean KeepAlive { get; set; } = true;

        /// <summary>
        /// 请求标头值 默认为text/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept { get; set; } = "text/html, application/xhtml+xml, */*";

        /// <summary>
        /// 请求返回类型默认 text/html
        /// </summary>
        public string ContentType { get; set; } = "text/html";

        /// <summary>
        /// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";

        /// <summary>
        /// 返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Post的数据类型
        /// </summary>
        public PostDataType PostDataType { get; set; } = PostDataType.String;

        /// <summary>
        /// Post请求时要发送的字符串Post数据
        /// </summary>
        public string Postdata { get; set; }
        /// <summary>
        /// Post请求时要发送的Byte类型的Post数据
        /// </summary>
        public byte[] PostdataByte { get; set; }
        /// <summary>
        /// Cookie返回类型,默认的是只返回字符串类型
        /// </summary>
        public ResultCookieType ResultCookieType { get; set; } = ResultCookieType.CookieCollection;

        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }
        /// <summary>
        /// 请求时的Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// 来源地址，上次访问地址
        /// </summary>
        public string Referer { get; set; }
        /// <summary>
        /// 证书绝对路径
        /// </summary>
        public string CerPath { get; set; }
        /// <summary>
        /// 设置代理对象，不想使用IE默认配置就设置为Null，而且不要设置ProxyIp
        /// </summary>
        public WebProxy WebProxy { get; set; }

        /// <summary>
        /// 是否设置为全文小写，默认为不转化
        /// </summary>
        public Boolean IsToLower { get; set; } = false;

        /// <summary>
        /// 支持跳转页面，查询结果将是跳转后的页面，默认是不跳转
        /// </summary>
        public Boolean Allowautoredirect { get; set; } = false;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int Connectionlimit { get; set; } = 1024;

        /// <summary>
        /// 代理Proxy 服务器用户名
        /// </summary>
        public string ProxyUserName { get; set; }
        /// <summary>
        /// 代理 服务器密码
        /// </summary>
        public string ProxyPwd { get; set; }
        /// <summary>
        /// 代理 服务IP,如果要使用IE代理就设置为ieproxy
        /// </summary>
        public string ProxyIp { get; set; }

        /// <summary>
        /// 设置返回类型String和Byte
        /// </summary>
        public ResultType ResultType { get; set; } = ResultType.String;

        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

        /// <summary>
        //     获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。
        /// </summary>
        public Version ProtocolVersion { get; set; }

        /// <summary>
        ///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。
        /// </summary>
        public Boolean Expect100Continue { get; set; } = false;

        /// <summary>
        /// 设置509证书集合
        /// </summary>
        public X509CertificateCollection ClentCertificates { get; set; }
        /// <summary>
        /// 设置或获取Post参数编码,默认的为Default编码
        /// </summary>
        public Encoding PostEncoding { get; set; }

        /// <summary>
        /// 获取或设置请求的身份验证信息。
        /// </summary>
        public ICredentials Credentials { get; set; } = CredentialCache.DefaultCredentials;

        /// <summary>
        /// 设置请求将跟随的重定向的最大数目
        /// </summary>
        public int MaximumAutomaticRedirections { get; set; }

        /// <summary>
        /// 获取和设置IfModifiedSince，默认为当前日期和时间
        /// </summary>
        public DateTime? IfModifiedSince { get; set; } = null;

        #region ip-port

        /// <summary>
        /// 设置本地的出口ip和端口
        /// </summary>]
        /// <example>
        ///item.IPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"),80);
        /// </example>
        public IPEndPoint IpEndPoint { get; set; } = null;

        #endregion
    }
    /// <summary>
    /// Http返回参数类
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        /// <summary>
        /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
        /// </summary>
        public string Html { get; set; } = string.Empty;

        /// <summary>
        /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
        /// </summary>
        public byte[] ResultByte { get; set; }
        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; }
        /// <summary>
        /// 返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }
        /// <summary>
        /// 返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// 最后访问的URl
        /// </summary>
        public string ResponseUri { get; set; }
        /// <summary>
        /// 获取重定向的URl
        /// </summary>
        public string RedirectUrl
        {
            get
            {
                try
                {
                    if (Header != null && Header.Count > 0)
                    {
                        if (Header.AllKeys.Any(k => k.ToLower().Contains("location")))
                        {
                            string baseurl = Header["location"].Trim();
                            string locationurl = baseurl.ToLower();
                            if (!string.IsNullOrWhiteSpace(locationurl))
                            {
                                bool b = locationurl.StartsWith("http://") || locationurl.StartsWith("https://");
                                if (!b)
                                {
                                    baseurl = new Uri(new Uri(ResponseUri), baseurl).AbsoluteUri;
                                }
                            }
                            return baseurl;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
                return string.Empty;
            }
        }

    }
    /// <summary>
    /// 返回类型
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 表示只返回字符串 只有Html有数据
        /// </summary>
        String,
        /// <summary>
        /// 表示返回字符串和字节流 ResultByte和Html都有数据返回
        /// </summary>
        Byte
    }
    /// <summary>
    /// Post的数据格式默认为string
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        /// 字符串类型，这时编码Encoding可不设置
        /// </summary>
        String,
        /// <summary>
        /// Byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空
        /// </summary>
        Byte,
        /// <summary>
        /// 传文件，Postdata必须设置为文件的绝对路径，必须设置Encoding的值
        /// </summary>
        FilePath
    }
    /// <summary>
    /// Cookie返回类型
    /// </summary>
    public enum ResultCookieType
    {
        /// <summary>
        /// 只返回字符串类型的Cookie
        /// </summary>
        String,
        /// <summary>
        /// CookieCollection格式的Cookie集合同时也返回String类型的cookie
        /// </summary>
        CookieCollection
    }

    #endregion
}