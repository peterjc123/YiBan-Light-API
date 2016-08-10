using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YiBan_Light_Lib;

namespace YiBan_Light_Lib
{
    /// <summary>
    /// 主界面
    /// </summary>
    public partial class SignController
    {
        private delegate void MyDelegate(string str);

        private readonly StreamWriter _writer;
        private ComboBox txtUserName = new ComboBox();
        private BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private TextBox txtPassWord = new TextBox();

        /// <summary>
        /// 构造函数
        /// </summary>
        public SignController()
        {
            InitializeComponent();
#if BackGround
            WindowState = FormWindowState.Minimized;
            Visible = false;
#endif
            _writer = File.AppendText("data.log");
            Form1_Load(null, null);
        }

        /// <summary>
        /// 初始化控件及其事件
        /// </summary>
        private void InitializeComponent()
        {
            txtUserName.SelectedIndexChanged += txtUserName_SelectedIndexChanged;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        /// <summary>
        /// 开始刷帖
        /// </summary>
        public void Start()
        {
            Button5_Click(null, null);
        }

        /// <summary>
        /// 载入用户数据
        /// </summary>
        public void Init()
        {
            SignInfo.Init(txtUserName);
        }

        /// <summary>
        /// 将用户名和密码设置为指定数据
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="pass">密码</param>
        public void Set(string user,string pass)
        {
            txtUserName.Text = user;
            txtPassWord.Text = pass;
        }

        #region UI线程相关

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="needConfirm">是否需要确认</param>
        private void DeleteUser(string needConfirm)
        {
            var sth = Convert.ToBoolean(needConfirm);
            if (txtUserName.Equals(""))
            {
                Console.WriteLine("无效用户名");
                return;
            }
            var index = txtUserName.Items.IndexOf(txtUserName.Text);
            if (index == -1) return;
            txtUserName.Items.RemoveAt(index);
            SignInfo.RemoveAt(index);
        }

        /// <summary>
        /// 用于在UI线程中修改状态条的字符
        /// </summary>
        /// <param name="str">要修改为的字符串</param>
        private void ChangeStatusText(string str)
        {
            lock (_writer)
            {
                Console.WriteLine(str);
                var task = new Task(() =>
                {
                    _writer.WriteLineAsync($"{DateTime.Now}\t{str}");
                    _writer.FlushAsync();
                });
                task.RunSynchronously();
            }
        }

        /// <summary>
        /// 刷帖预检查
        /// </summary>
        /// <returns>是否检查通过</returns>
        private bool PreCheck()
        {
            if (txtUserName.Text.Equals(""))
            {
                Console.WriteLine("无效用户名");
                return false;
            }
            if (!txtPassWord.Text.Equals("")) return true;
            Console.WriteLine("无效密码");
            return false;
        }

        /// <summary>
        /// 刷帖开始准备
        /// </summary>
        private void StartSigning()
        {
            SignInfo.StartSign(txtUserName, txtPassWord);
            backgroundWorker1.RunWorkerAsync();
        }

        /// <summary>
        /// button5单击函数
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void Button5_Click(object sender, EventArgs e)
        {
            if (!PreCheck()) return;
            StartSigning();
        }

        /// <summary>
        /// button1单击函数
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void button1_Click(object sender, EventArgs e)
        {
            DeleteUser("true");
        }

        /// <summary>
        /// 刷帖工作函数
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var i = 0;
            var t = false;
            var bw = sender as BackgroundWorker;
            while (SignInfo.Result)
            {
                if (bw != null && bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (!SignInfo.WaitToFill) backgroundWorker1.ReportProgress(++i, (t = new WebOperations().SignOnePerson(bw)));
                Thread.Sleep(1000);
            }
            e.Result = t;
        }

        /// <summary>
        /// 完成一个人的刷帖任务
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if ((bool)e.UserState)
            {
                SignInfo.ChangeDate();
                if (SignInfo.HasUserToSign())
                {
                    SignInfo.GetToNextUser(txtUserName, txtPassWord);
                }
                else
                    SignInfo.Result = false;
            }
            else
                SignInfo.Result = false;
        }

        /// <summary>
        /// 刷帖完成后要做的事
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result && !e.Cancelled)
            {
                ChangeStatusText("全部刷帖成功！");
                var i = 5;
                while (i > 0)
                {
                    Console.WriteLine(@"将在" + i--.ToString() + @"秒内关闭");
                    Thread.Sleep(1000);
                }
                Close();
            }
            else if (e.Cancelled)
                ChangeStatusText("刷帖过程被取消！");
            else
            {

            }
        }

        private void Close()
        {
            Form1_FormClosing();
            Environment.Exit(0);
        }

        /// <summary>
        /// 用户名框选择改变
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void txtUserName_SelectedIndexChanged(object sender, EventArgs e)
        {
            SignInfo.LoadPass(txtUserName.SelectedIndex, txtPassWord);
        }

        /// <summary>
        /// 程序开始加载
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            WebOperations.StatusTextChanged += WebOperations_StatusTextChanged;
#if BackGround
            Button5_Click(null, null);
#endif
        }

        /// <summary>
        /// 对StatusTextChanged事件做出反应
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void WebOperations_StatusTextChanged(object sender, EventArgs e)
        {
            if (sender != null)
                ChangeStatusText(sender.ToString());
            else
                DeleteUser("false");
        }

        /// <summary>
        /// 程序开始关闭
        /// </summary>
        /// <param name="sender">函数传递者</param>
        /// <param name="e">事件参数</param>
        private void Form1_FormClosing()
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
                while (backgroundWorker1.IsBusy)
                {
                    Thread.Sleep(1000);
                }
            }
            SignInfo.SaveData();
            _writer.Flush();
            _writer.Dispose();
        }

        #endregion


    }



}



