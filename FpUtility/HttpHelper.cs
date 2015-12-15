using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace FpUtility
{
    /// <summary>
    /// Http杩炴帴鎿嶄綔甯姪绫?
    /// </summary>
    internal class HttpHelper
    {
        #region 棰勫畾涔夋柟鍙橀噺

        //榛樿鐨勭紪鐮?
        private Encoding encoding = Encoding.Default;

        //Post鏁版嵁缂栫爜
        private Encoding postencoding = Encoding.Default;

        //HttpWebRequest瀵硅薄鐢ㄦ潵鍙戣捣璇锋眰
        private HttpWebRequest request = null;

        //鑾峰彇褰卞搷娴佺殑鏁版嵁瀵硅薄
        private HttpWebResponse response = null;

        #endregion 棰勫畾涔夋柟鍙橀噺

        #region Public

        /// <summary>
        /// 鏍规嵁鐩镐紶鍏ョ殑鏁版嵁锛屽緱鍒扮浉搴旈〉闈㈡暟鎹?
        /// </summary>
        /// <param name="item">鍙傛暟绫诲璞?/param>
        /// <returns>杩斿洖HttpResult绫诲瀷</returns>
        public HttpResult GetHtml(HttpItem item)
        {
            //杩斿洖鍙傛暟
            HttpResult result = new HttpResult();
            try
            {
                //鍑嗗鍙傛暟
                SetRequest(item);
            }
            catch (Exception ex)
            {
                result.Cookie = string.Empty;
                result.Header = null;
                result.Html = ex.Message;
                result.StatusDescription = "閰嶇疆鍙傛暟鏃跺嚭閿欙細" + ex.Message;
                //閰嶇疆鍙傛暟鏃跺嚭閿?
                return result;
            }
            try
            {
                //璇锋眰鏁版嵁
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    GetData(item, result);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (response = (HttpWebResponse)ex.Response)
                    {
                        GetData(item, result);
                    }
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
            return result;
        }

        #endregion Public

        #region GetData

        /// <summary>
        /// 鑾峰彇鏁版嵁鐨勫苟瑙ｆ瀽鐨勬柟娉?
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        private void GetData(HttpItem item, HttpResult result)
        {
            #region base

            //鑾峰彇StatusCode
            result.StatusCode = response.StatusCode;
            //鑾峰彇StatusDescription
            result.StatusDescription = response.StatusDescription;
            //鑾峰彇Headers
            result.Header = response.Headers;
            //鑾峰彇CookieCollection
            if (response.Cookies != null) result.CookieCollection = response.Cookies;
            //鑾峰彇set-cookie
            if (response.Headers["set-cookie"] != null) result.Cookie = response.Headers["set-cookie"];

            #endregion base

            #region byte

            //澶勭悊缃戦〉Byte
            byte[] ResponseByte = GetByte();

            #endregion byte

            #region Html

            if (ResponseByte != null & ResponseByte.Length > 0)
            {
                //璁剧疆缂栫爜
                SetEncoding(item, result, ResponseByte);
                //寰楀埌杩斿洖鐨凥TML
                result.Html = encoding.GetString(ResponseByte);
            }
            else
            {
                //娌℃湁杩斿洖浠讳綍Html浠ｇ爜
                result.Html = string.Empty;
            }

            #endregion Html
        }

        /// <summary>
        /// 璁剧疆缂栫爜
        /// </summary>
        /// <param name="item">HttpItem</param>
        /// <param name="result">HttpResult</param>
        /// <param name="ResponseByte">byte[]</param>
        private void SetEncoding(HttpItem item, HttpResult result, byte[] ResponseByte)
        {
            //鏄惁杩斿洖Byte绫诲瀷鏁版嵁
            if (item.ResultType == ResultType.Byte) result.ResultByte = ResponseByte;
            //浠庤繖閲屽紑濮嬫垜浠鏃犺缂栫爜浜?
            if (encoding == null)
            {
                Match meta = Regex.Match(Encoding.Default.GetString(ResponseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                string c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }
                if (c.Length > 2)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(response.CharacterSet))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else
                        {
                            encoding = Encoding.GetEncoding(response.CharacterSet);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(response.CharacterSet))
                    {
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(response.CharacterSet);
                    }
                }
            }
        }

        /// <summary>
        /// 鎻愬彇缃戦〉Byte
        /// </summary>
        /// <returns></returns>
        private byte[] GetByte()
        {
            byte[] ResponseByte = null;
            MemoryStream _stream = new MemoryStream();

            //GZIIP澶勭悊
            if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
            {
                //寮€濮嬭鍙栨祦骞惰缃紪鐮佹柟寮?
                _stream = GetMemoryStream(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
            }
            else
            {
                //寮€濮嬭鍙栨祦骞惰缃紪鐮佹柟寮?
                _stream = GetMemoryStream(response.GetResponseStream());
            }
            //鑾峰彇Byte
            ResponseByte = _stream.ToArray();
            _stream.Close();
            return ResponseByte;
        }

        /// <summary>
        /// 4.0浠ヤ笅.net鐗堟湰鍙栨暟鎹娇鐢?
        /// </summary>
        /// <param name="streamResponse">娴?/param>
        private MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            while (bytesRead > 0)
            {
                _stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return _stream;
        }

        #endregion GetData

        #region SetRequest

        /// <summary>
        /// 涓鸿姹傚噯澶囧弬鏁?
        /// </summary>
        ///<param name="item">鍙傛暟鍒楄〃</param>
        private void SetRequest(HttpItem item)
        {
            // 楠岃瘉璇佷功
            SetCer(item);
            //璁剧疆Header鍙傛暟
            if (item.Header != null && item.Header.Count > 0) foreach (string key in item.Header.AllKeys)
                {
                    request.Headers.Add(key, item.Header[key]);
                }
            // 璁剧疆浠ｇ悊
            SetProxy(item);
            if (item.ProtocolVersion != null) request.ProtocolVersion = item.ProtocolVersion;
            request.ServicePoint.Expect100Continue = item.Expect100Continue;
            //璇锋眰鏂瑰紡Get鎴栬€匬ost
            request.Method = item.Method;
            request.Timeout = item.Timeout;
            request.KeepAlive = item.KeepAlive;
            request.ReadWriteTimeout = item.ReadWriteTimeout;
            if (item.IfModifiedSince != null) request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
            //Accept
            request.Accept = item.Accept;
            //ContentType杩斿洖绫诲瀷
            request.ContentType = item.ContentType;
            //UserAgent瀹㈡埛绔殑璁块棶绫诲瀷锛屽寘鎷祻瑙堝櫒鐗堟湰鍜屾搷浣滅郴缁熶俊鎭?
            request.UserAgent = item.UserAgent;
            // 缂栫爜
            encoding = item.Encoding;
            //璁剧疆瀹夊叏鍑瘉
            request.Credentials = item.ICredentials;
            //璁剧疆Cookie
            SetCookie(item);
            //鏉ユ簮鍦板潃
            request.Referer = item.Referer;
            //鏄惁鎵ц璺宠浆鍔熻兘
            request.AllowAutoRedirect = item.Allowautoredirect;
            if (item.MaximumAutomaticRedirections > 0)
            {
                request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
            }
            //璁剧疆Post鏁版嵁
            SetPostData(item);
            //璁剧疆鏈€澶ц繛鎺?
            if (item.Connectionlimit > 0) request.ServicePoint.ConnectionLimit = item.Connectionlimit;
        }

        /// <summary>
        /// 璁剧疆璇佷功
        /// </summary>
        /// <param name="item"></param>
        private void SetCer(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.CerPath))
            {
                //杩欎竴鍙ヤ竴瀹氳鍐欏湪鍒涘缓杩炴帴鐨勫墠闈€備娇鐢ㄥ洖璋冪殑鏂规硶杩涜璇佷功楠岃瘉銆?
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //鍒濆鍖栧鍍忥紝骞惰缃姹傜殑URL鍦板潃
                request = (HttpWebRequest)WebRequest.Create(item.URL);
                SetCerList(item);
                //灏嗚瘉涔︽坊鍔犲埌璇锋眰閲?
                request.ClientCertificates.Add(new X509Certificate(item.CerPath));
            }
            else
            {
                //鍒濆鍖栧鍍忥紝骞惰缃姹傜殑URL鍦板潃
                request = (HttpWebRequest)WebRequest.Create(item.URL);
                SetCerList(item);
            }
        }

        /// <summary>
        /// 璁剧疆澶氫釜璇佷功
        /// </summary>
        /// <param name="item"></param>
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    request.ClientCertificates.Add(c);
                }
            }
        }

        /// <summary>
        /// 璁剧疆Cookie
        /// </summary>
        /// <param name="item">Http鍙傛暟</param>
        private void SetCookie(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.Cookie)) request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            //璁剧疆CookieCollection
            if (item.ResultCookieType == ResultCookieType.CookieCollection)
            {
                request.CookieContainer = new CookieContainer();
                if (item.CookieCollection != null && item.CookieCollection.Count > 0)
                    request.CookieContainer.Add(item.CookieCollection);
            }
        }

        /// <summary>
        /// 璁剧疆Post鏁版嵁
        /// </summary>
        /// <param name="item">Http鍙傛暟</param>
        private void SetPostData(HttpItem item)
        {
            //楠岃瘉鍦ㄥ緱鍒扮粨鏋滄椂鏄惁鏈変紶鍏ユ暟鎹?
            if (!request.Method.Trim().ToLower().Contains("get"))
            {
                if (item.PostEncoding != null)
                {
                    postencoding = item.PostEncoding;
                }
                byte[] buffer = null;
                //鍐欏叆Byte绫诲瀷
                if (item.PostDataType == PostDataType.Byte && item.PostdataByte != null && item.PostdataByte.Length > 0)
                {
                    //楠岃瘉鍦ㄥ緱鍒扮粨鏋滄椂鏄惁鏈変紶鍏ユ暟鎹?
                    buffer = item.PostdataByte;
                }//鍐欏叆鏂囦欢
                else if (item.PostDataType == PostDataType.FilePath && !string.IsNullOrEmpty(item.Postdata))
                {
                    StreamReader r = new StreamReader(item.Postdata, postencoding);
                    buffer = postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //鍐欏叆瀛楃涓?
                else if (!string.IsNullOrEmpty(item.Postdata))
                {
                    buffer = postencoding.GetBytes(item.Postdata);
                }
                if (buffer != null)
                {
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// 璁剧疆浠ｇ悊
        /// </summary>
        /// <param name="item">鍙傛暟瀵硅薄</param>
        private void SetProxy(HttpItem item)
        {
            bool isIeProxy = false;
            if (!string.IsNullOrEmpty(item.ProxyIp))
            {
                isIeProxy = item.ProxyIp.ToLower().Contains("ieproxy");
            }
            if (!string.IsNullOrEmpty(item.ProxyIp) && !isIeProxy)
            {
                //璁剧疆浠ｇ悊鏈嶅姟鍣?
                if (item.ProxyIp.Contains(":"))
                {
                    string[] plist = item.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    //寤鸿杩炴帴
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //缁欏綋鍓嶈姹傚璞?
                    request.Proxy = myProxy;
                }
                else
                {
                    WebProxy myProxy = new WebProxy(item.ProxyIp, false);
                    //寤鸿杩炴帴
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //缁欏綋鍓嶈姹傚璞?
                    request.Proxy = myProxy;
                }
            }
            else if (isIeProxy)
            {
                //璁剧疆涓篒E浠ｇ悊
            }
            else
            {
                request.Proxy = item.WebProxy;
            }
        }

        #endregion SetRequest

        #region private main

        /// <summary>
        /// 鍥炶皟楠岃瘉璇佷功闂
        /// </summary>
        /// <param name="sender">娴佸璞?/param>
        /// <param name="certificate">璇佷功</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }

        #endregion private main
    }

    /// <summary>
    /// Http璇锋眰鍙傝€冪被
    /// </summary>
    internal class HttpItem
    {
        private string _URL = string.Empty;

        /// <summary>
        /// 璇锋眰URL蹇呴』濉啓
        /// </summary>
        public string URL
        {
            get { return _URL; }
            set { _URL = value; }
        }

        private string _Method = "GET";

        /// <summary>
        /// 璇锋眰鏂瑰紡榛樿涓篏ET鏂瑰紡,褰撲负POST鏂瑰紡鏃跺繀椤昏缃甈ostdata鐨勫€?
        /// </summary>
        public string Method
        {
            get { return _Method; }
            set { _Method = value; }
        }

        private int _Timeout = 100000;

        /// <summary>
        /// 榛樿璇锋眰瓒呮椂鏃堕棿
        /// </summary>
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }

        private int _ReadWriteTimeout = 30000;

        /// <summary>
        /// 榛樿鍐欏叆Post鏁版嵁瓒呮椂闂?
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return _ReadWriteTimeout; }
            set { _ReadWriteTimeout = value; }
        }

        private Boolean _KeepAlive = true;

        /// <summary>
        ///  鑾峰彇鎴栬缃竴涓€硷紝璇ュ€兼寚绀烘槸鍚︿笌 Internet 璧勬簮寤虹珛鎸佷箙鎬ц繛鎺ラ粯璁や负true銆?
        /// </summary>
        public Boolean KeepAlive
        {
            get { return _KeepAlive; }
            set { _KeepAlive = value; }
        }

        private string _Accept = "text/html, application/xhtml+xml, */*";

        /// <summary>
        /// 璇锋眰鏍囧ご鍊?榛樿涓簍ext/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept
        {
            get { return _Accept; }
            set { _Accept = value; }
        }

        private string _ContentType = "text/html";

        /// <summary>
        /// 璇锋眰杩斿洖绫诲瀷榛樿 text/html
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }

        private string _UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";

        /// <summary>
        /// 瀹㈡埛绔闂俊鎭粯璁ozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
            set { _UserAgent = value; }
        }

        private Encoding _Encoding = null;

        /// <summary>
        /// 杩斿洖鏁版嵁缂栫爜榛樿涓篘Ull,鍙互鑷姩璇嗗埆,涓€鑸负utf-8,gbk,gb2312
        /// </summary>
        public Encoding Encoding
        {
            get { return _Encoding; }
            set { _Encoding = value; }
        }

        private PostDataType _PostDataType = PostDataType.String;

        /// <summary>
        /// Post鐨勬暟鎹被鍨?
        /// </summary>
        public PostDataType PostDataType
        {
            get { return _PostDataType; }
            set { _PostDataType = value; }
        }

        private string _Postdata = string.Empty;

        /// <summary>
        /// Post璇锋眰鏃惰鍙戦€佺殑瀛楃涓睵ost鏁版嵁
        /// </summary>
        public string Postdata
        {
            get { return _Postdata; }
            set { _Postdata = value; }
        }

        private byte[] _PostdataByte = null;

        /// <summary>
        /// Post璇锋眰鏃惰鍙戦€佺殑Byte绫诲瀷鐨凱ost鏁版嵁
        /// </summary>
        public byte[] PostdataByte
        {
            get { return _PostdataByte; }
            set { _PostdataByte = value; }
        }

        private WebProxy _WebProxy;

        /// <summary>
        /// 璁剧疆浠ｇ悊瀵硅薄锛屼笉鎯充娇鐢↖E榛樿閰嶇疆灏辫缃负Null锛岃€屼笖涓嶈璁剧疆ProxyIp
        /// </summary>
        public WebProxy WebProxy
        {
            get { return _WebProxy; }
            set { _WebProxy = value; }
        }

        private CookieCollection cookiecollection = null;

        /// <summary>
        /// Cookie瀵硅薄闆嗗悎
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return cookiecollection; }
            set { cookiecollection = value; }
        }

        private string _Cookie = string.Empty;

        /// <summary>
        /// 璇锋眰鏃剁殑Cookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }

        private string _Referer = string.Empty;

        /// <summary>
        /// 鏉ユ簮鍦板潃锛屼笂娆¤闂湴鍧€
        /// </summary>
        public string Referer
        {
            get { return _Referer; }
            set { _Referer = value; }
        }

        private string _CerPath = string.Empty;

        /// <summary>
        /// 璇佷功缁濆璺緞
        /// </summary>
        public string CerPath
        {
            get { return _CerPath; }
            set { _CerPath = value; }
        }

        private Boolean isToLower = false;

        /// <summary>
        /// 鏄惁璁剧疆涓哄叏鏂囧皬鍐欙紝榛樿涓轰笉杞寲
        /// </summary>
        public Boolean IsToLower
        {
            get { return isToLower; }
            set { isToLower = value; }
        }

        private Boolean allowautoredirect = false;

        /// <summary>
        /// 鏀寔璺宠浆椤甸潰锛屾煡璇㈢粨鏋滃皢鏄烦杞悗鐨勯〉闈紝榛樿鏄笉璺宠浆
        /// </summary>
        public Boolean Allowautoredirect
        {
            get { return allowautoredirect; }
            set { allowautoredirect = value; }
        }

        private int connectionlimit = 1024;

        /// <summary>
        /// 鏈€澶ц繛鎺ユ暟
        /// </summary>
        public int Connectionlimit
        {
            get { return connectionlimit; }
            set { connectionlimit = value; }
        }

        private string proxyusername = string.Empty;

        /// <summary>
        /// 浠ｇ悊Proxy 鏈嶅姟鍣ㄧ敤鎴峰悕
        /// </summary>
        public string ProxyUserName
        {
            get { return proxyusername; }
            set { proxyusername = value; }
        }

        private string proxypwd = string.Empty;

        /// <summary>
        /// 浠ｇ悊 鏈嶅姟鍣ㄥ瘑鐮?
        /// </summary>
        public string ProxyPwd
        {
            get { return proxypwd; }
            set { proxypwd = value; }
        }

        private string proxyip = string.Empty;

        /// <summary>
        /// 浠ｇ悊 鏈嶅姟IP ,濡傛灉瑕佷娇鐢↖E浠ｇ悊灏辫缃负ieproxy
        /// </summary>
        public string ProxyIp
        {
            get { return proxyip; }
            set { proxyip = value; }
        }

        private ResultType resulttype = ResultType.String;

        /// <summary>
        /// 璁剧疆杩斿洖绫诲瀷String鍜孊yte
        /// </summary>
        public ResultType ResultType
        {
            get { return resulttype; }
            set { resulttype = value; }
        }

        private WebHeaderCollection header = new WebHeaderCollection();

        /// <summary>
        /// header瀵硅薄
        /// </summary>
        public WebHeaderCollection Header
        {
            get { return header; }
            set { header = value; }
        }

        private Version _ProtocolVersion;

        /// <summary>
        //     鑾峰彇鎴栬缃敤浜庤姹傜殑 HTTP 鐗堟湰銆傝繑鍥炵粨鏋?鐢ㄤ簬璇锋眰鐨?HTTP 鐗堟湰銆傞粯璁や负 System.Net.HttpVersion.Version11銆?
        /// </summary>
        public Version ProtocolVersion
        {
            get { return _ProtocolVersion; }
            set { _ProtocolVersion = value; }
        }

        private Boolean _expect100continue = true;

        /// <summary>
        ///  鑾峰彇鎴栬缃竴涓?System.Boolean 鍊硷紝璇ュ€肩‘瀹氭槸鍚︿娇鐢?100-Continue 琛屼负銆傚鏋?POST 璇锋眰闇€瑕?100-Continue 鍝嶅簲锛屽垯涓?true锛涘惁鍒欎负 false銆傞粯璁ゅ€间负 true銆?
        /// </summary>
        public Boolean Expect100Continue
        {
            get { return _expect100continue; }
            set { _expect100continue = value; }
        }

        private X509CertificateCollection _ClentCertificates;

        /// <summary>
        /// 璁剧疆509璇佷功闆嗗悎
        /// </summary>
        public X509CertificateCollection ClentCertificates
        {
            get { return _ClentCertificates; }
            set { _ClentCertificates = value; }
        }

        private Encoding _PostEncoding;

        /// <summary>
        /// 璁剧疆鎴栬幏鍙朠ost鍙傛暟缂栫爜,榛樿鐨勪负Default缂栫爜
        /// </summary>
        public Encoding PostEncoding
        {
            get { return _PostEncoding; }
            set { _PostEncoding = value; }
        }

        private ResultCookieType _ResultCookieType = ResultCookieType.String;

        /// <summary>
        /// Cookie杩斿洖绫诲瀷,榛樿鐨勬槸鍙繑鍥炲瓧绗︿覆绫诲瀷
        /// </summary>
        public ResultCookieType ResultCookieType
        {
            get { return _ResultCookieType; }
            set { _ResultCookieType = value; }
        }

        private ICredentials _ICredentials = CredentialCache.DefaultCredentials;

        /// <summary>
        /// 鑾峰彇鎴栬缃姹傜殑韬唤楠岃瘉淇℃伅銆?
        /// </summary>
        public ICredentials ICredentials
        {
            get { return _ICredentials; }
            set { _ICredentials = value; }
        }

        /// <summary>
        /// 璁剧疆璇锋眰灏嗚窡闅忕殑閲嶅畾鍚戠殑鏈€澶ф暟鐩?
        /// </summary>
        private int _MaximumAutomaticRedirections;

        public int MaximumAutomaticRedirections
        {
            get { return _MaximumAutomaticRedirections; }
            set { _MaximumAutomaticRedirections = value; }
        }

        private DateTime? _IfModifiedSince = null;

        /// <summary>
        /// 鑾峰彇鍜岃缃甀fModifiedSince锛岄粯璁や负褰撳墠鏃ユ湡鍜屾椂闂?
        /// </summary>
        public DateTime? IfModifiedSince
        {
            get { return _IfModifiedSince; }
            set { _IfModifiedSince = value; }
        }
    }

    /// <summary>
    /// Http杩斿洖鍙傛暟绫?
    /// </summary>
    internal class HttpResult
    {
        private string _Cookie;

        /// <summary>
        /// Http璇锋眰杩斿洖鐨凜ookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }

        private CookieCollection _CookieCollection;

        /// <summary>
        /// Cookie瀵硅薄闆嗗悎
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return _CookieCollection; }
            set { _CookieCollection = value; }
        }

        private string _html = string.Empty;

        /// <summary>
        /// 杩斿洖鐨凷tring绫诲瀷鏁版嵁 鍙湁ResultType.String鏃舵墠杩斿洖鏁版嵁锛屽叾瀹冩儏鍐典负绌?
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        private byte[] _ResultByte;

        /// <summary>
        /// 杩斿洖鐨凚yte鏁扮粍 鍙湁ResultType.Byte鏃舵墠杩斿洖鏁版嵁锛屽叾瀹冩儏鍐典负绌?
        /// </summary>
        public byte[] ResultByte
        {
            get { return _ResultByte; }
            set { _ResultByte = value; }
        }

        private WebHeaderCollection _Header;

        /// <summary>
        /// header瀵硅薄
        /// </summary>
        public WebHeaderCollection Header
        {
            get { return _Header; }
            set { _Header = value; }
        }

        private string _StatusDescription;

        /// <summary>
        /// 杩斿洖鐘舵€佽鏄?
        /// </summary>
        public string StatusDescription
        {
            get { return _StatusDescription; }
            set { _StatusDescription = value; }
        }

        private HttpStatusCode _StatusCode;

        /// <summary>
        /// 杩斿洖鐘舵€佺爜,榛樿涓篛K
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return _StatusCode; }
            set { _StatusCode = value; }
        }
    }

    /// <summary>
    /// 杩斿洖绫诲瀷
    /// </summary>
    internal enum ResultType
    {
        /// <summary>
        /// 琛ㄧず鍙繑鍥炲瓧绗︿覆 鍙湁Html鏈夋暟鎹?
        /// </summary>
        String,

        /// <summary>
        /// 琛ㄧず杩斿洖瀛楃涓插拰瀛楄妭娴?ResultByte鍜孒tml閮芥湁鏁版嵁杩斿洖
        /// </summary>
        Byte
    }

    /// <summary>
    /// Post鐨勬暟鎹牸寮忛粯璁や负string
    /// </summary>
    internal enum PostDataType
    {
        /// <summary>
        /// 瀛楃涓茬被鍨嬶紝杩欐椂缂栫爜Encoding鍙笉璁剧疆
        /// </summary>
        String,

        /// <summary>
        /// Byte绫诲瀷锛岄渶瑕佽缃甈ostdataByte鍙傛暟鐨勫€肩紪鐮丒ncoding鍙缃负绌?
        /// </summary>
        Byte,

        /// <summary>
        /// 浼犳枃浠讹紝Postdata蹇呴』璁剧疆涓烘枃浠剁殑缁濆璺緞锛屽繀椤昏缃瓻ncoding鐨勫€?
        /// </summary>
        FilePath
    }

    /// <summary>
    /// Cookie杩斿洖绫诲瀷
    /// </summary>
    internal enum ResultCookieType
    {
        /// <summary>
        /// 鍙繑鍥炲瓧绗︿覆绫诲瀷鐨凜ookie
        /// </summary>
        String,

        /// <summary>
        /// CookieCollection鏍煎紡鐨凜ookie闆嗗悎鍚屾椂涔熻繑鍥濻tring绫诲瀷鐨刢ookie
        /// </summary>
        CookieCollection
    }
}