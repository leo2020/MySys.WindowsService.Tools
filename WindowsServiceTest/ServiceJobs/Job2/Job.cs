using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsServiceCore;
using System.IO;
using log4net;
using WindowsServiceCore.Utility;

namespace WindowsServiceTest.ServiceJobs.Job2
{
    public class Job : ServiceJob
    {
        private static readonly ILog _log = LogHelper.GetInstance();

        /// <summary>
        /// 任务开始
        /// </summary>
        protected override void Start()
        {
            try
            {
                _log.InfoFormat("WindowsServiceTest.ServiceJobs.Job2：{0} Start。",DateTime.Now.ToString());

                //运行中
                this.mIsRunning = true;
                //执行工作项
                this.executeLogic();

                _log.InfoFormat("WindowsServiceTest.ServiceJobs.Job2：{0} End。", DateTime.Now.ToString());
            }
            catch (Exception error)
            {
                //异常日志
                ServiceTools.WriteLog(ServiceTools.GetAppSetting("LOG_PATH") + @"Job2\" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", error.ToString(), true);
                //发生异常时停止服务程序
                ServiceTools.WindowsServiceStop("ServiceTest");
            }
            finally
            {
                //空闲
                this.mIsRunning = false;
            }
        }

        /// <summary>
        /// 任务停止
        /// </summary>
        protected override void Stop()
        {
            this.mIsRunning = false;
        }

        /// <summary>
        /// 执行逻辑
        /// </summary>
        private void executeLogic()
        {
            //写入演示文本
            using (StreamWriter sw = new StreamWriter(System.Windows.Forms.Application.StartupPath.ToString() + @"\job2.txt", true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine("");
                sw.Close();
            }
        }
    }
}
