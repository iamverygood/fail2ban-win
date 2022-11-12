using System;
using System.Collections.Generic;
using System.Text;

namespace fail2ban_win.utils
{
    internal class IpUtils
    {
        // CIDR IP地址格式
        public static void CIDRIpRange(string sNetwork, out uint startIP, out uint endIP)
        {
            uint ip,        /* ip address */
                mask,       /* subnet mask */
                broadcast,  /* Broadcast address */
                network;    /* Network address */

            int bits;

            string[] elements = sNetwork.Split(new Char[] { '/' });

            ip = IP2Int(elements[0]);
            bits = Convert.ToInt32(elements[1]);

            mask = ~(0xffffffff >> bits);

            network = ip & mask;
            broadcast = network + ~mask;

            uint usableIps = (bits > 30) ? 0 : (broadcast - network - 1);

            if (usableIps <= 0)
            {
                startIP = endIP = 0;
            }
            else
            {
                startIP = network + 1;
                endIP = broadcast - 1;
            }
        }

        public static void IpRange(string sNetwork, out uint startIP, out uint endIP)
        {
            startIP = 0;
            endIP = 0;
            string[] elements = sNetwork.Split(new Char[] { '-' });
            if (elements == null || elements.Length != 2) return;
            startIP = IP2Int(elements[0]);
            endIP = IP2Int(elements[1]);
        }

        public static uint IP2Int(string IPNumber)
        {
            uint ip = 0;
            string[] elements = IPNumber.Split(new Char[] { '.' });
            if (elements.Length == 4)
            {
                ip = Convert.ToUInt32(elements[0]) << 24;
                ip += Convert.ToUInt32(elements[1]) << 16;
                ip += Convert.ToUInt32(elements[2]) << 8;
                ip += Convert.ToUInt32(elements[3]);
            }
            return ip;
        }

        // 判断IP是否在ipList中（多IP或范围使用英文逗号分隔）
        public static bool isIpIn(string ip, string ipList)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            if (string.IsNullOrEmpty(ipList)) return false;
            string[] elements = ipList.Split(new Char[] { ',' });
            if (elements == null || elements.Length <= 0) return false;
            for(int i = 0; i < elements.Length; i++)
            {
                elements[i] = elements[i].Trim();
            }
            return isIpIn(ip, ref elements);
        }

        public static bool isIpIn(string ip, ref string[] ipList)
        {
            uint ipInt = IP2Int(ip);
            foreach (string item in ipList)
            {
                uint startIP = 0;
                uint endIP = 0;
                if (item.Contains("/"))
                {
                    CIDRIpRange(item, out startIP, out endIP);
                }
                else if (item.Contains("-"))
                {
                    IpRange(item, out startIP, out endIP);
                } 
                else
                {
                    uint ipIntTmp = IP2Int(item);
                    startIP = ipIntTmp;
                    endIP = ipIntTmp;
                }
                if (ipInt >= startIP && ipInt <= endIP) return true;
            }
            return false;
        }

    }
}
