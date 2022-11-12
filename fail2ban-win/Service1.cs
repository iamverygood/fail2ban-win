using fail2ban_win.config;
using fail2ban_win.db;
using fail2ban_win.utils;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Runtime.Remoting.Messaging;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

// https://learn.microsoft.com/zh-cn/dotnet/api/system.diagnostics.eventlog.enableraisingevents?view=netframework-2.0
// https://blog.csdn.net/u012563853/article/details/124788963
// https://blog.csdn.net/xuyongbeijing2008/article/details/122863786
// 安装与卸载服务（.net工具箱）
// installutil.exe /i ail2ban-win.exe
// installutil.exe /u ail2ban-win.exe

namespace fail2ban_win
{
    public partial class Service1 : ServiceBase
    {

        private bool m_thread_run = false;

        // 服务线程
        private Thread m_threadService = null;

        private static AppConfig m_appConfig = new AppConfig();

        private static Fail2banWinDb m_fail2banWinDb = new Fail2banWinDb();

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("OnStart Begin");

            m_appConfig.LoadConfig();
            Logger.Info(m_appConfig.ToString());

            m_thread_run = true;
            m_threadService = new Thread(ThreadService);
            m_threadService.IsBackground = true;
            m_threadService.Name = "m_threadService";
            m_threadService.Priority = ThreadPriority.Normal;
            m_threadService.Start();

            Logger.Info("OnStart End");
        }

        protected override void OnStop()
        {
            Logger.Info("OnStop Begin");

            m_thread_run = false;
            Thread.Sleep(1000);
            if (m_threadService != null && m_threadService.IsAlive) m_threadService.Abort();

            Logger.Info("OnStop End");

        }

        private void ThreadService()
        {
            // "Application"应用程序, "Security"安全, "System"系统
            // EventLog myEventLog = new EventLog("Security");
            try
            {
                EventLog myEventLog = new EventLog();
                myEventLog.Log = "Security";
                myEventLog.EntryWritten += new EntryWrittenEventHandler(MyOnEntryWritten);
                myEventLog.EnableRaisingEvents = true;
            } 
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            long count = 0;
            while (m_thread_run)
            {
                try
                {
                    count++;
                    if (count % 600 == 0)
                    {
                        m_appConfig.LoadConfig();
                        m_fail2banWinDb.DeleteEventRecordHistory(m_appConfig.findTimeSecond);
                        // m_fail2banWinDb.PrintEventRecord();
                        // 定时移除过期的防火墙规则
                        RemoveExpiredBanRecord();
                    }
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
            }
        }

        public static void RemoveExpiredBanRecord()
        {
            List<string> resultList = m_fail2banWinDb.FindAllExpiredBanRecord();
            if (resultList == null || resultList.Count <= 0) return;
            foreach (string ip in resultList)
            {
                string ruleName = "\"" + m_appConfig.firewallNamePrefix + "[" + ip + "]\"";
                FirewallUtils.DelRule(ruleName);
                m_fail2banWinDb.DeleteBanRecord(ip);
                Logger.Info("UnBanIp:" + ip);
            }
        }

        public static void MyOnEntryWritten(Object source, EntryWrittenEventArgs e)
        {
            if (e == null || e.Entry == null) return;
            string instanceId = e.Entry.InstanceId.ToString();
            if (instanceId == "4625") // 4625 登陆失败；
            {
                processLoginFail4625(e.Entry);
            } 
        }

        private static void processLoginFail4625(EventLogEntry entry)
        {
            Logger.Debug("-----------------------------------------[EVENT-BEGIN]-----------------------------------------");
            string account;
            string ip;
            if (!getLoginFail4625Info(entry, out account, out ip)) return;
            Logger.Info("[LoginError] ip=" + ip + ", account" + account);
            m_fail2banWinDb.InsertEventRecord("4625", account, ip);
            if (IpUtils.isIpIn(ip, m_appConfig.ignoreIp)) return;
            // m_fail2banWinDb.PrintEventRecord();
            int errorCount = m_fail2banWinDb.FindEventRecordCount("4625", ip, m_appConfig.findTimeSecond);
            if (errorCount >= m_appConfig.maxRetry)
            {
                if (m_fail2banWinDb.FindBanRecordCount(ip) > 0)
                    m_fail2banWinDb.UpdateBanRecordEtime(ip, m_appConfig.banTimeSecond);
                else
                    m_fail2banWinDb.InsertBanRecord(ip, m_appConfig.banTimeSecond);
                m_fail2banWinDb.PrintBanRecord();
                string ruleName = "\"" + m_appConfig.firewallNamePrefix + "[" + ip + "]\"";
                string ruleDesc = "\"Expired:" + DateTime.Now.AddSeconds(m_appConfig.banTimeSecond).ToString("yyyy-MM-dd HH:mm:ss") + "\"";
                FirewallUtils.DelRule(ruleName);
                FirewallUtils.AddIpDropRule(ruleName, ip, ruleDesc);
                Logger.Info("BanIp:" + ip);
            }
            Logger.Debug("-----------------------------------------[EVENT-ENDED]-----------------------------------------");
        }

        private static bool getLoginFail4625Info(EventLogEntry entry, out string account, out string ip)
        {
            Logger.Debug("instanceId:4625");
            Logger.Debug("eventCreateTime:" + entry.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss"));
            Logger.Debug("MachineName:" + entry.MachineName);
            Logger.Debug("UserName:" + entry.UserName);
            if (entry.ReplacementStrings != null && entry.ReplacementStrings.Length > 19)
            {
                account = entry.ReplacementStrings[5];
                ip = entry.ReplacementStrings[19];
                Logger.Debug("account:" + account);
                Logger.Debug("ip:" + ip);
                return true;
            } 
            else
            {
                account = null;
                ip = null;
                return false;
            }
        }

    }
}
