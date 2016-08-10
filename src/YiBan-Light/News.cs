using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YiBan_Light_Lib
{
    /// <summary>
    /// 新闻类
    /// </summary>
    internal class News
    {
        private string _url;
        int _curr;
        private readonly List<string> _list;
        private readonly Random _rnd;

        readonly string[] _xmlurl = {"http://rss.sina.com.cn/tech/rollnews.xml",
                       "http://rss.sina.com.cn/roll/sports/hot_roll.xml",
                       "http://rss.sina.com.cn/news/allnews/sports.xml",
                       "http://rss.sina.com.cn/sports/global/focus.xml",
                       "http://rss.sina.com.cn/sports/global/italy.xml",
                       "http://rss.sina.com.cn/sports/global/england.xml",
                       "http://rss.sina.com.cn/sports/global/spain.xml",
                       "http://rss.sina.com.cn/sports/global/germanyfrance.xml",
                       "http://rss.sina.com.cn/sports/global/others.xml",
                       "http://rss.sina.com.cn/sports/china/focus.xml",
                       "http://rss.sina.com.cn/sports/china/team.xml",
                       "http://rss.sina.com.cn/news/society/wonder15.xml",
                       "http://rss.sina.com.cn/sports/basketball/focus.xml",
                       "http://rss.sina.com.cn/news/allnews/games.xml",
                       "http://rss.sina.com.cn/sports/basketball/nba.xml",
                       "http://rss.sina.com.cn/sports/basketball/cba.xml",
                       "http://rss.sina.com.cn/sports/others/focus.xml",
                       "http://rss.sina.com.cn/sports/global/lottery.xml",
                       "http://rss.sina.com.cn/sports/global/golf.xml",
                       "http://rss.sina.com.cn/sports/global/golf.xml",
                       "http://rss.sina.com.cn/sports/global/f1.xml",
                       "http://rss.sina.com.cn/sports/global/chess.xml",
                       "http://rss.sina.com.cn/blog/index/cul.xml",
                       "http://rss.sina.com.cn/blog/index/exc.xml",
                       "http://rss.sina.com.cn/blog/index/feel.xml",
                       "http://rss.sina.com.cn/blog/index/ent.xml",
                       "http://rss.sina.com.cn/blog/index/enjoy.xml",
                       "http://rss.sina.com.cn/blog/index/other.xml",
                       "http://rss.sina.com.cn/blog/index/stocks.xml",
                       "http://rss.sina.com.cn/news/marquee/ddt.xml",
                       "http://rss.sina.com.cn/news/china/focus15.xml",
                       "http://rss.sina.com.cn/news/world/focus15.xml",
                       "http://rss.sina.com.cn/news/society/focus15.xml",
                       "http://rss.sina.com.cn/news/china/hktaiwan15.xml",
                       "http://rss.sina.com.cn/news/society/law15.xml",
                       "http://rss.sina.com.cn/news/society/misc15.xml",
                       "http://rss.sina.com.cn/news/society/feeling15.xml"};

        /// <summary>
        /// 构造函数
        /// </summary>
        public News()
        {
            _curr = 0;
            _list = new List<string>();
            _rnd = new Random();
        }

        /// <summary>
        /// 获取新闻
        /// </summary>
        /// <returns>返回列表中当前可用的新闻</returns>
        public string Get()
        {
            //TODO:Confirm this function won't crash
            var success = false;
            while (!success || _curr == _list.Count)
            {
                success = false;
                try
                {
                    if (_curr == _list.Count)
                        LoadNewUri();
                    success = true;
                }
                catch (Exception e)
                {
                    //若加载失败
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }

                while (_curr < _list.Count && _list[_curr].Length < 5)
                    _curr++;
            }
            return _list[_curr++];
        }

        /// <summary>
        /// 加载新的新闻网站，获取更多新闻
        /// </summary>
        private void LoadNewUri()
        {
            var ran = _rnd.Next(0, _xmlurl.Length);
            _url = _xmlurl[ran];
            LoadXmLtoCache();
            LoadItem();
        }

        /// <summary>
        /// 读取RSS文件并存放在本地以供操作
        /// </summary>
        private void LoadXmLtoCache()
        {
            var xmldocument = new XmlDocument();
            var client = new HttpClient();
            var task = client.GetStreamAsync(_url);
            var inStream = task.Result;
            xmldocument.Load(inStream);
            var outStream = new FileStream(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "~doc.xml", FileMode.Create);
            xmldocument.Save(outStream);
            outStream.Dispose();
        }

        /// <summary>
        /// 从本地文件中进行操作，读取RSS中内容的标题及作者
        /// </summary>
        private void LoadItem()
        {
            var xmlDocument = new XmlDocument();
            var outStream = new FileStream(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "~doc.xml", FileMode.Open);
            xmlDocument.Load(outStream);
            var mynodelist = xmlDocument.SelectNodes("/rss/channel/item");
            int i;
            for (i = 0; i <= mynodelist?.Count - 1; i++)
                _list.Add(mynodelist[i]["title"]?.InnerText.Trim());
        }
    }

}
