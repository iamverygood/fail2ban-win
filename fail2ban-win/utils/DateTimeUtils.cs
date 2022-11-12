using System;
using System.Collections.Generic;
using System.Text;

namespace fail2ban_win.utils
{
    internal class DateTimeUtils
    {

        private static int sn = 0;
        private static DateTime dtFrom = new DateTime(1970, 1, 1, 8, 0, 0, 0);

        private static readonly Object m_lock_sn = new object();

        public static double GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow.AddHours(8) - dtFrom;
            return ts.TotalMilliseconds;
        }

        // 获取当前毫秒时间戳
        public static long CurrentTimeSeconds()
        {
            long currentTicks = DateTime.Now.Ticks;
            return (currentTicks - dtFrom.Ticks) / 10000 / 1000;
        }

        // 获取当前毫秒时间戳
        public static long CurrentTimeMillis()
        {
            long currentTicks = DateTime.Now.Ticks;
            return (currentTicks - dtFrom.Ticks) / 10000;
        }

        // 获取当前毫秒时间戳
        public static long CurrentTimeMillisForDb()
        {
            return DateTime.Now.Ticks;
        }

        public static long GetTimeId()
        {
            lock (m_lock_sn)
            {
                string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                sn++;
                if (sn >= 100) sn = 0;
                return long.Parse(time) * 100L + sn;
            }
        }

        public static long GetMillis(string time)
        {
            return GetSeconds(time) * 1000L;
        }

        // 1年：1y，30分钟：30m，10小时：10h，1天：1d
        public static long GetSeconds(string time)
        {
            if (string.IsNullOrEmpty(time)) return 0;
            time = time.ToLower();
            long seconds = 0;
            try
            {
                if (time.EndsWith("y"))
                {
                    long value = Convert.ToInt32(time.Substring(0, time.Length - 1));
                    seconds = 60L * 60 * 24 * 365 * value;
                }
                else if (time.EndsWith("d"))
                {
                    long value = Convert.ToInt32(time.Substring(0, time.Length - 1));
                    seconds = 60L * 60 * 24 * value;
                }
                else if (time.EndsWith("d"))
                {
                    long value = Convert.ToInt32(time.Substring(0, time.Length - 1));
                    seconds = 60L * 60 * 24 * value;
                }
                else if (time.EndsWith("h"))
                {
                    long value = Convert.ToInt32(time.Substring(0, time.Length - 1));
                    seconds = 60L * 60 * value;
                }
                else if (time.EndsWith("m"))
                {
                    long value = Convert.ToInt32(time.Substring(0, time.Length - 1));
                    seconds = 60L * value;
                }
                else
                {
                    seconds = Convert.ToInt32(time);
                }
            }
            catch
            {
                seconds = 0;
            }
            return seconds;
        }

        public static string GetTimeString(long time)
        {
            long sl = time;
            string dw = "ms";
            long year = 1000L * 60 * 60 * 24 * 30 * 12;
            long month = 1000L * 60 * 60 * 24 * 30;
            long day = 1000L * 60 * 60 * 24;
            long hour = 1000L * 60 * 60;
            long minute = 1000L * 60;
            long second = 1000L;
            if (time >= year)
            {
                sl = (long)time / year;
                dw = "y";
            }
            else if (time >= month)
            {
                sl = (long)time / month;
                dw = "m";
            }
            else if (time >= day)
            {
                sl = (long)time / day;
                dw = "d";
            }
            else if (time >= hour)
            {
                sl = (long)time / hour;
                dw = "h";
            }
            else if (time >= minute)
            {
                sl = (long)time / minute;
                dw = "min";
            }
            else if (time >= second)
            {
                sl = (long)time / second;
                dw = "s";
            }
            return sl + dw;
        }

    }
}
