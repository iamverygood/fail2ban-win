using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace fail2ban_win.utils
{
    internal class FirewallUtils
    {


        public static string CmdRun(string cmd)
        {
            try
            {
                Console.WriteLine("CmdRun Start:" + cmd);
                Process myProcess = new Process();//创建进程对象
                myProcess.StartInfo.FileName = "cmd.exe";//设置打开cmd命令窗口
                myProcess.StartInfo.UseShellExecute = false;//不使用操作系统shell启动进程的值
                myProcess.StartInfo.RedirectStandardInput = true;//设置可以从标准输入流读取值
                myProcess.StartInfo.RedirectStandardOutput = true;//设置可以向标准输出流写入值
                myProcess.StartInfo.RedirectStandardError = true;//设置可以显示输入输出流中出现的错误
                myProcess.StartInfo.CreateNoWindow = true;//设置在新窗口中启动进程
                myProcess.Start();//启动进程
                myProcess.StandardInput.WriteLine(cmd);//传入要执行的命令
                myProcess.StandardInput.WriteLine("exit");
                myProcess.StandardInput.AutoFlush = true;
                string output = myProcess.StandardOutput.ReadToEnd();
                myProcess.WaitForExit();
                myProcess.Close();
                Console.WriteLine("CmdRun End:" + cmd);
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine("CmdRun Exception [" + cmd + "] " + e.Message);
                return string.Empty;
            }
        }

        public static string GetRuleDesc(string ruleName)
        {
            string keyStr = "描述:";
            string cmd = "netsh advfirewall firewall show rule name=" + ruleName + " verbose | findstr " + keyStr;
            string result = CmdRun(cmd);
            // Console.WriteLine(result);
            if (string.IsNullOrEmpty(result)) return null;
            string[] lines = result.Split('\n');
            foreach (string line in lines)
            {
                if (line.StartsWith(keyStr))
                {
                    return line.Substring(keyStr.Length).Trim();
                }
            }
            return null;
        }

        public static void AddIpDropRule(string ruleName, string remoteIp, string desc)
        {
            string cmd = "netsh advfirewall firewall add rule name=" + ruleName + " dir=in protocol=any action=block remoteip=" + remoteIp + " description=" + desc;
            CmdRun(cmd);
        }

        public static void DelRule(string ruleName)
        {
            string cmd = "netsh advfirewall firewall delete rule name=" + ruleName;
            CmdRun(cmd);
        }

        public static bool FindRule(string ruleName)
        {
            string cmd = "netsh advfirewall firewall show rule name=" + ruleName + " verbose";
            string result = CmdRun(cmd);
            if (string.IsNullOrEmpty(result) || result.Contains("没有与指定标准相匹配的规则。")) return false;
            return true;
        }

    }
}
