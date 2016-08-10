using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YiBan_Light_Lib
{
    /// <summary>
    /// 对网络和签到的操作类
    /// </summary>
    public class WebOperations
    {
        /// <summary>
        /// 自定义事件委托
        /// </summary>
        /// <param name="sender">参数传递</param>
        /// <param name="e">无效参数</param>
        public delegate void MyEventHandler(object sender, EventArgs e);
        /// <summary>
        /// 自定义事件
        /// </summary>
        public static event MyEventHandler StatusTextChanged;
        /// <summary>
        /// 用于保持会话状态的cookieContainer
        /// </summary>
        private CookieContainer _cookieContainer;
        private readonly News _mynews = new News();
        private static int _areaid = 69036;
        private string _puid = "";

        /// <summary>
        /// 完成对网站HTTP的GET操作
        /// </summary>
        /// <param name="url">请求的网址</param>
        /// <param name="needread">是否需要加载源代码</param>
        /// <param name="xmlhttp">是否为ajax</param>
        /// <returns>网页的源代码</returns>
        private string Get(string url, bool needread, bool xmlhttp)
        {
            var retrytimes = 0;
        reget:
            var req = (HttpWebRequest)WebRequest.Create(url);
            System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();

            handler.AllowAutoRedirect = true;

            req.Accept = "text/html, application/xhtml+xml, */*";
            req.Headers["Referer"] = "http://www.yiban.cn/eclass/topic?id=" + _areaid.ToString();
            req.Headers["Host"] = "www.yiban.cn";
            //req.Timeout = 50000;
            req.Method = "GET";
            req.CookieContainer = _cookieContainer;
            if (xmlhttp)
                req.Headers["X-Requested-With"] = "XMLHttpRequest";
            //req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            string str;
            try
            {
                var task = req.GetResponseAsync();
                var response = (HttpWebResponse)task.Result;
                if (needread)
                {
                    var stream = response.GetResponseStream();
                    if (stream == null) throw new WebException();
                    using (var sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        str = sr.ReadToEnd();
                    }
                }
                else
                    str = "";
                response.Dispose();
            }
            catch (WebException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                StatusTextChanged?.Invoke("网络连接失败，将进行第" + (++retrytimes).ToString() + "次重试", null);
                Thread.Sleep(10000);
                goto reget;
            }
            return str;
        }

        /// <summary>
        /// 获取当前用户的PUID
        /// </summary>
        /// <returns>当前用户的PUID</returns>
        private string GetPuid()
        {
            _areaid = SignInfo.Areaid;
            var htmlstr = Get("http://www.yiban.cn/ajax/group/getJoined", true, true);
            var jo = (JObject)JsonConvert.DeserializeObject(htmlstr);
            var jp = jo.Children<JProperty>().Last();
            var ja = (JArray)jp.Value;
            foreach (var t in from t in ja let id = (int)t["oldClassId"] where id == _areaid select t)
            {
                return t["puid"].ToString();
            }
            htmlstr = Get("http://www.yiban.cn/ajax/group/getMyManageForVote", true, true);
            jo = (JObject)JsonConvert.DeserializeObject(htmlstr);
            jp = jo.Children<JProperty>().Last();
            ja = (JArray)jp.Value;
            foreach (var t in from t in ja let id = (int)t["oldClassId"] where id == _areaid select t)
            {
                return t["puid"].ToString();
            }
            return "";

        }

        /// <summary>
        /// 完成对易班登陆操作
        /// </summary>
        /// <param name="usr">用户名</param>
        /// <param name="pas">密码</param>
        /// <returns>登陆的结果</returns>
        private bool Login(string usr, string pas)
        {
            _cookieContainer = new CookieContainer();
            
            usr = System.Net.WebUtility.UrlEncode(usr);
            pas = System.Net.WebUtility.UrlEncode(pas);

            const string url = "http://www.yiban.cn/login/doLoginAjax";
            var formdata = "account=" + usr + "&password=" + pas;
            Post(url, formdata, true);

            var result = Get("http://www.yiban.cn/ajax/my/getLogin", true, true);
            if (result.Contains("\"isLogin\":false")) //登陆失败
            {
                StatusTextChanged?.Invoke("用户名或密码错误", null);
                return false;
            }
            StatusTextChanged?.Invoke("登陆成功", null);
            return true;
        }

        /// <summary>
        /// 完成对网站HTTP的POST操作
        /// </summary>
        /// <param name="url">请求的网址</param>
        /// <param name="formdata">表单数据</param>
        /// <param name="xmlhttp">是否为ajax</param>
        /// <returns>网页的源代码</returns>
        private void Post(string url, string formdata, bool xmlhttp)
        {
            var retrytimes = 0;
        retry:
            var req = (HttpWebRequest)WebRequest.Create(url);
            System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();

            handler.AllowAutoRedirect = true;
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            //req.Timeout = 50000;
            req.Method = "POST";
            req.Headers["Referer"] = "http://www.yiban.cn/bbs/publish?area=" + _areaid.ToString();
            req.Headers["Host"] = "www.yiban.cn";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = _cookieContainer;
            //req.Headers["KeepAlive"] = true;
            if (xmlhttp)
                req.Headers["X-Requested-With"] =  "XMLHttpRequest";
            var byte1 = Encoding.GetEncoding("UTF-8").GetBytes(formdata);
            req.Headers["ContentLength"] = byte1.Length + "";
            try
            {
                var reqTask = req.GetRequestStreamAsync();
                using (var reqStream = reqTask.Result)
                { 
                    reqStream.Write(byte1, 0, byte1.Length);
                }
                var result = req.GetResponseAsync().Result;
            }
            catch (WebException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                StatusTextChanged?.Invoke("网络连接失败，将进行" + (++retrytimes).ToString() + "重试中", null);
                Thread.Sleep(10000);
                goto retry;
            }
        }

        private bool WriteArticle()
        {
            var news = _mynews.Get();
            //StringBuilder sb = new StringBuilder();
            var urlEncode = System.Net.WebUtility.UrlEncode(news);
            if (urlEncode == null) return true;
            var title = urlEncode.ToUpper();
            //sb.Append("puid=").Append(puid).Append("&title=").Append(news).Append("&content=<p>");
            var content = title + "+%E8%BD%AC%E8%BD%BD%E8%87%AA%E6%96%B0%E6%B5%AA%E6%96%B0%E9%97%BB";
            content = content.Replace("+", "%26nbsp%3B");
            //sb.Append(news).Append("</p>&article_id=&isNotice=false&pubArea=").Append(areaid);
            var formdata =
                $"puid={_puid}&title={title}&content=<p>{content}</p>&&article_id=&isNotice=false&pubArea={_areaid}";
            //var formdata = sb.ToString();
            Post("http://www.yiban.cn/forum/article/addAjax", formdata, true);
            return true;
        }

        /// <summary>
        /// 完成单人刷帖
        /// </summary>
        /// <param name="bw">后台工作者</param>
        /// <returns>刷帖的结果</returns>
        public bool SignOnePerson(BackgroundWorker bw)
        {
            if (SignInfo.HaveSigned())
            {
                StatusTextChanged?.Invoke(SignInfo.User + "的工作已完成，跳过", null);
                return true;
            }
            if (!Login(SignInfo.User, SignInfo.Pass)) //登陆失败
            {
                StatusTextChanged?.Invoke(null, null);
                return false;
            }
            Thread.Sleep(1000);
            if (bw.CancellationPending) return false;
            var i = 0;
            do
            {
                if (bw.CancellationPending) return false;
                if (i++ != 0)
                {
                    StatusTextChanged?.Invoke("刷帖冷却中", null);
                    Thread.Sleep(15000);
                    if (bw.CancellationPending)
                    {
                        StatusTextChanged?.Invoke("用户请求中断", null);
                        return false;
                    }
                }
                StatusTextChanged?.Invoke("正在为" + SignInfo.User + "刷第" + i.ToString() + "贴，共" + SignInfo.Num.ToString() + "条", null);
                _puid = GetPuid();
                if (_puid.Equals(""))
                {
                    StatusTextChanged?.Invoke("PUID获取失败，跳过该用户", null);
                    return true;
                }
                Thread.Sleep(1000);
                if (bw.CancellationPending)
                {
                    StatusTextChanged?.Invoke("用户请求中断", null);
                    return false;
                }
                if (!WriteArticle())
                {
                    StatusTextChanged?.Invoke("发布帖子失败，跳过该用户", null);
                    return true;
                }
                if (bw.CancellationPending)
                {
                    StatusTextChanged?.Invoke("用户请求中断", null);
                    return false;
                }
                Thread.Sleep(1000);
            }
            while (i != SignInfo.Num);
            return true;
        }
    }
}
