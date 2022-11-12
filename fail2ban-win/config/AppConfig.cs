using fail2ban_win.utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace fail2ban_win.config
{
    internal class AppConfig
    {

        private string m_iniConfigPath = $"{AppDomain.CurrentDomain.BaseDirectory}fail2ban-win.ini";
        private string m_iniConfigSection = "fail2ban-win";

        public string ignoreIp { get; set; }

        // 1年：1y，30分钟：30m，10小时：10h，1天：1d
        public string banTime { get; set; }

        public string findTime { get; set; }

        public long banTimeSecond { get; set; }

        public long findTimeSecond { get; set; }

        public int maxRetry { get; set; }

        public string firewallNamePrefix { get; set; }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void LoadConfig()
        {
            Logger.Debug("LoadConfig");
            try
            {
                //string iniConfigPath = Process.GetCurrentProcess().MainModule.FileName;
                // string iniConfigPath = AppDomain.CurrentDomain.BaseDirectory;
                ignoreIp = IniUtils.IniRead(m_iniConfigPath, m_iniConfigSection, "ignoreip", "127.0.0.1");
                banTime = IniUtils.IniRead(m_iniConfigPath, m_iniConfigSection, "bantime", "30m");
                findTime = IniUtils.IniRead(m_iniConfigPath, m_iniConfigSection, "findtime", "5m");
                firewallNamePrefix = IniUtils.IniRead(m_iniConfigPath, m_iniConfigSection, "firewallnameprefix", "fail2ban-win-");
                maxRetry = Convert.ToInt32(IniUtils.IniRead(m_iniConfigPath, m_iniConfigSection, "maxretry", "5"));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
            banTimeSecond = DateTimeUtils.GetSeconds(banTime);
            findTimeSecond = DateTimeUtils.GetSeconds(findTime);
        }

        public override string ToString()
        {
            return "[AppConfig]m_iniConfigPath=" + m_iniConfigPath + ",m_iniConfigSection=" + m_iniConfigSection + ",ignoreIp=" + ignoreIp + ",banTime=" + banTime + ",findTime=" + findTime + ",firewallNamePrefix=" + firewallNamePrefix + ",maxRetry=" + maxRetry;
        }

    }
}
