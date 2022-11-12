﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fail2ban_win.utils
{
    internal class IniUtils
    {
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);


        /// 写入INI的方法
        public static void IniWrite(string section, string key, string value, string path)
        {
            // section=配置节点名称，key=键名，value=返回键值，path=路径
            WritePrivateProfileString(section, key, value, path);
        }

        //读取INI的方法
        public static string IniRead(string path, string section, string key, string defaultValue)
        {
            // 每次从ini中读取多少字节
            StringBuilder temp = new StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, defaultValue, temp, 255, path);
            return temp.ToString();

        }

        //删除一个INI文件
        public static void IniDelete(string FilePath)
        {
            File.Delete(FilePath);
        }
    }
}
