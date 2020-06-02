
using CefSharp;
using CefSharp.WinForms;
using HtmlAgilityPack;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kuaidi
{
    public partial class Framain : Form
    {
        /// <summary>
        /// 快递编号
        /// </summary>
        public static string LogsNum = "";
        //定义一个委托
        public delegate void searchDel(string num);
        /// <summary>
        /// 提供给外部方法调用内部方法的委托变量
        /// </summary>
        public static searchDel framainDel = null;
        /// <summary>
        /// 浏览器对象
        /// </summary>
        ChromiumWebBrowser webCom = null;
        public Framain()
        {
            InitializeComponent();
            framainDel = SearchBaidu;//委托的实际方法
        }

        private void Framain_Load(object sender, EventArgs e)
        {
            panel1.Controls.Remove(webCom);
            webCom = new ChromiumWebBrowser("https://www.baidu.com/");
            webCom.Dock = DockStyle.Fill;
            panel1.Controls.Add(webCom);
            webCom.FrameLoadEnd += webCom_FrameLoadEnd;//给浏览器对象绑定 每次加载完成触发的事件
            Task task = new Task(StartTask);//绑定定时查询任务
            task.Start();
        }
        /// <summary>
        /// 执行浏览器查询
        /// </summary>
        /// <param name="kudinum"></param>
        private void SearchBaidu(string kudinum)
        {
            LogsNum = kudinum;
            webCom.Load($"https://www.baidu.com/s?ie=UTF-8&wd={kudinum}");
        }
        /// <summary>
        /// The scheduler调度器
        /// </summary>
        static IScheduler _scheduler = null;
        /// <summary>
        /// Starts the task.
        /// </summary>
        private static async void StartTask()
        {
            ISchedulerFactory sf = new StdSchedulerFactory();
            if (_scheduler == null)
            {
                _scheduler = await sf.GetScheduler();
            }
            await _scheduler.Start();

            #region 任务
            //创建任务对象
            IJobDetail job1 = JobBuilder.Create<TaskAutoReptileJob>().WithIdentity("job1", "group1").Build();
            //创建触发器
            ITrigger trigger1 = TriggerBuilder.Create().WithIdentity("trigger1", "group1").StartNow().WithCronSchedule("0 * * * * ?").Build();
            #endregion
            await _scheduler.ScheduleJob(job1, trigger1);
        }
        /// <summary>
        /// 每次浏览器  加载页面完成触发 在这方法完成快递信息查询并自定义入库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void webCom_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            string html = string.Empty;
            List<LogsView> logsViews = new List<LogsView>();
            html = await e.Frame.GetSourceAsync();
            try
            {
                HtmlAgilityPack.HtmlDocument docComp = new HtmlAgilityPack.HtmlDocument();
                docComp.LoadHtml(html);//获取页面的dom对象
                var codehtml = docComp.DocumentNode.SelectNodes(
                    "//ul[@class='op_express_delivery_timeline_box']/li//div[@class='op_express_delivery_timeline_info']");
                if (codehtml != null)
                {
                    foreach (HtmlNode node in codehtml)
                    {
                        string cont = node.InnerHtml;
                        int brindex = cont.IndexOf("<br>");
                        if (brindex <= 0)
                        {
                            continue;
                        }
                        string time = cont.Substring(0, brindex);
                        string txt = cont.Substring(brindex + 4);
                        logsViews.Add(new LogsView
                        {
                            LogsCont = txt,
                            LogsTime = Convert.ToDateTime(time)
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                //更新业务快递信息
            }

        }

        /// <summary>
        /// 测试方法  富文本框中输入快递号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            LogsNum = this.richTextBox1.Text;
            SearchBaidu(LogsNum);
        }
    }
}
