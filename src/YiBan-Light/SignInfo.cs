using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace YiBan_Light_Lib
{
    /// <summary>
    /// 共享变量的类
    /// </summary>
    public static class SignInfo
    {
        private static List<string> _user;
        private static List<string> _pass;
        private static List<DateTime> _date;
        private static List<int> _areaid;
        private const int AreaId = 69036;

        /// <summary>
        /// 刷帖结果
        /// </summary>
        public static bool Result;
        /// <summary>
        /// 是否等待填入下一用户的信息
        /// </summary>
        public static bool WaitToFill;
        /// <summary>
        /// 当前用户的位置
        /// </summary>
        private static int _curr;

        /// <summary>
        /// 更新刷帖信息
        /// </summary>
        public static void Init(ComboBox usr)
        {
            LoadData();
            RefreshControl(usr);
        }

        /// <summary>
        /// 每人刷帖数
        /// </summary>
        public static int Num { get; private set; }

        /// <summary>
        /// 当前用户的用户名
        /// </summary>
        public static string User => _user[_curr];

        /// <summary>
        /// 当前用户的密码
        /// </summary>
        public static string Pass => _pass[_curr];

        /// <summary>
        /// 当前用户的区域号
        /// </summary>
        public static int Areaid => _areaid[_curr];

        /// <summary>
        /// 判断是否还有用户需要刷帖
        /// </summary>
        /// <returns>是否还有用户需要刷帖</returns>
        public static bool HasUserToSign()
        {
            return _curr != _user.Count - 1;
        }

        /// <summary>
        /// 切换至下一个用户
        /// </summary>
        /// <param name="usr">用户名控件</param>
        /// <param name="pas">密码控件</param>
        public static void GetToNextUser(ComboBox usr, TextBox pas)
        {
            _curr++;
            usr.Text = _user[_curr];
            pas.Text = _pass[_curr];
        }

        /// <summary>
        /// 开始刷帖的准备工作
        /// </summary>
        /// <param name="usr">用户名控件</param>
        /// <param name="pas">密码控件</param>
        public static void StartSign(ComboBox usr, TextBox pas)
        {
            if (!_user.Contains(usr.Text))
            {
                _user.Add(usr.Text);
                usr.Items.Add(usr.Text);
                _pass.Add(pas.Text);
                _date.Add(DateTime.Now.AddDays(-1));
                _areaid.Add(AreaId);
            }
            _curr = usr.Items.IndexOf(usr.Text);
            Result = true;
            WaitToFill = false;
        }

        /// <summary>
        /// 切换刷帖日期
        /// </summary>
        public static void ChangeDate()
        {
            _date[_curr] = DateTime.Now;
        }

        /// <summary>
        /// 判断是否已经刷过
        /// </summary>
        /// <returns>判断是否已经刷过</returns>
        public static bool HaveSigned()
        {
            return _date[_curr].Date == DateTime.Now.Date;
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        /// <param name="usr">用户名控件</param>
        private static void RefreshControl(ComboBox usr)
        {
            //var usr = Form1.ActiveForm.Controls["txtUserName"] as ComboBox;
            foreach (var item in _user)
                usr.Items.Add(item);
            if (usr.Items.Count != 0) usr.SelectedIndex = 0;
        }

        /// <summary>
        /// 加载当前用户的密码
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="pas">密码框控件</param>
        public static void LoadPass(int index, TextBox pas)
        {
            //var pas = Form1.ActiveForm.Controls["txtPassWord"] as TextBox;
            pas.Text = _pass[index];
        }

        /// <summary>
        /// 保存用户及程序数据
        /// </summary>
        public static void SaveData()
        {
            var filename = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "//data.xml";
            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                var doc = new XmlDocument();
                var dec = doc.CreateXmlDeclaration("1.0", "GB2312", null);
                doc.AppendChild(dec);
                //创建一个根节点（一级）
                var root = doc.CreateElement("info");
                using (var wrapper = new Simple3Des("YIBAN---API"))
                {
                    for (var i = 0; i < _user.Count; i++)
                    {
                        var node = doc.CreateElement("user");
                        var username = doc.CreateElement("username");
                        username.InnerText = _user[i];
                        var password = doc.CreateElement("password");
                        password.InnerText = wrapper.EncryptData(_pass[i]);
                        var datetime = doc.CreateElement("date");
                        datetime.InnerText = _date[i].Ticks.ToString();
                        var areaid = doc.CreateElement("areaid");
                        areaid.InnerText = _areaid[i].ToString();
                        node.AppendChild(username);
                        node.AppendChild(password);
                        node.AppendChild(datetime);
                        node.AppendChild(areaid);
                        root.AppendChild(node);
                    }
                }
                var mynode = doc.CreateElement("app");
                var signnum = doc.CreateElement("SignNumber");
                signnum.InnerText = Num.ToString();
                mynode.AppendChild(signnum);
                root.AppendChild(mynode);
                doc.AppendChild(root);
                doc.Save(stream);
                stream.Flush();
                stream.Dispose();
            }
        }

        /// <summary>
        /// 从本地文件中进行操作，读取RSS中内容的标题及作者
        /// </summary>
        private static void LoadData()
        {
            _user = new List<string>();
            _pass = new List<string>();
            _date = new List<DateTime>();
            _areaid = new List<int>();
            Num = 8;
            if (!File.Exists(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "//data.xml")) return;
            var xmlDocument = new XmlDocument();
            var fileStream = new FileStream(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "//data.xml", FileMode.Open);
            xmlDocument.Load(fileStream);
            var mynodelist = xmlDocument.SelectNodes("/info/user");
            if (mynodelist != null)
            {
                using (var wrapper = new Simple3Des("YIBAN---API"))
                {
                    for (var i = 0; i <= mynodelist.Count - 1; i++)
                    {
                        _user.Add(mynodelist[i]["username"]?.InnerText);
                        _pass.Add(wrapper.DecryptData(mynodelist[i]["password"]?.InnerText));
                        _date.Add(mynodelist[i].SelectSingleNode("date") != null
                            ? new DateTime(Convert.ToInt64(mynodelist[i]["date"]?.InnerText))
                            : DateTime.Now.AddDays(-1));
                        _areaid.Add(mynodelist[i].SelectSingleNode("areaid") != null
                            ? Convert.ToInt32(mynodelist[i]["areaid"]?.InnerText)
                            : AreaId);
                    }
                }
            }
            mynodelist = xmlDocument.SelectNodes("/info/app");
            if (mynodelist != null)
            {
                Num = Convert.ToInt32(mynodelist[0]["SignNumber"]?.InnerText);
            }
            fileStream.Dispose();
        }

        /// <summary>
        /// 删除某个用户
        /// </summary>
        /// <param name="index">该用户的索引</param>
        public static void RemoveAt(int index)
        {
            if (index >= _user.Count) return;
            _user.RemoveAt(index);
            _pass.RemoveAt(index);
        }
    }
}
