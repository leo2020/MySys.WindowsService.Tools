using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Reflection;
using System.Threading;
using WindowsServiceCore;
using log4net;
using WindowsServiceCore.Utility;

namespace WindowsServiceTest
{
    public partial class ServicePlatform : ServiceBase
    {
        private static readonly ILog _log = LogHelper.GetInstance();

        //用哈希表存放任务项
        private Hashtable hashJobs;

        public ServicePlatform()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log.InfoFormat("Start Service：{0}",DateTime.Now.ToString());
            //启动服务
            this.runJobs();
        }

        protected override void OnStop()
        {
            //停止服务
            this.stopJobs();
            _log.InfoFormat("Stop Service：{0}", DateTime.Now.ToString());
        }

        #region 自定义方法

        private void runJobs()
        {
            try
            {
                //加载工作项
                if (this.hashJobs == null)
                {
                    hashJobs = new Hashtable();

                    //获取configSections节点
                    XmlNode configSections = ServiceTools.GetConfigSections();
                    foreach (XmlNode section in configSections)
                    {
                        //过滤注释节点（如section中还包含其它节点需过滤）
                        if (section.Name.ToLower() == "section")
                        {
                            //创建每个节点的配置对象
                            string sectionName = section.Attributes["name"].Value.Trim();
                            string sectionType = section.Attributes["type"].Value.Trim();

                            if (sectionName.Contains("Job"))
                            {
                                //程序集名称
                                string assemblyName = ServiceTools.GetAppSetting("WINDOWS_SERVICE_TEST"); //sectionType.Split(',')[1];
                                //完整类名
                                string classFullName = assemblyName + ".ServiceJobs." + sectionName + ".Config";

                                //创建配置对象
                                ServiceConfig config = (ServiceConfig)Assembly.Load(assemblyName).CreateInstance(classFullName);
                                //创建工作对象
                                ServiceJob job = (ServiceJob)Assembly.Load(config.Assembly.Split(',')[1]).CreateInstance(config.Assembly.Split(',')[0]);
                                job.ConfigObject = config;

                                //将工作对象加载进HashTable
                                this.hashJobs.Add(sectionName, job);
                            }
                        }
                    }
                }

                //执行工作项
                if (this.hashJobs.Keys.Count > 0)
                {
                    foreach (ServiceJob job in hashJobs.Values)
                    {
                        //插入一个新的请求到线程池
                        if (System.Threading.ThreadPool.QueueUserWorkItem(threadCallBack, job))
                        {
                            //方法成功排入队列
                        }
                        else
                        {
                            //失败
                        }
                    }
                }
            }
            catch (Exception error)
            {
                ServiceTools.WriteLog(ServiceTools.GetAppSetting("LOG_PATH") + "Error.txt", error.ToString(), true);
            }
        }

        private void stopJobs()
        {
            //停止
            if (this.hashJobs != null)
            {
                this.hashJobs.Clear();
            }
        }

        /// <summary>
        /// 线程池回调方法
        /// </summary>
        /// <param name="state"></param>
        private void threadCallBack(Object state)
        {
            while (true)
            {
                ((ServiceJob)state).StartJob();
                //休眠1秒
                Thread.Sleep(1000);
            }
        }

        #endregion
    }
}
