using fail2ban_win.utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Principal;
using System.Text;

namespace fail2ban_win.db
{
    internal class Fail2banWinDb
    {
        private SQLiteHelper db = null;
        private string sql = null;
        private readonly Object m_lock = new object();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Fail2banWinDb() {
            InitDb();
        }

        public void InitDb()
        {

            Logger.Info("InitDb Start");

            //db = new SQLiteHelper("Data Source=:memory:");
            // 内存共享数据库
            //db = new SQLiteHelper("Data Source=InMemorySample;Mode=Memory;Cache=Shared");
            //db = new SQLiteHelper($"data source={AppDomain.CurrentDomain.BaseDirectory}fail2ban-win.db");

            db = new SQLiteHelper($"data source={AppDomain.CurrentDomain.BaseDirectory}fail2ban-win.db");

            Logger.Info(db.GetSQLiteVersion());

            sql = @"CREATE TABLE IF NOT EXISTS EventRecord (
                    ID INTEGER PRIMARY KEY,
                    EventId INTEGER,
                    Account TEXT,
                    Ip TEXT,
                    Crtm INTEGER
                  );";
            db.ExecuteNonQuery(sql);

            sql = @"CREATE INDEX IF NOT EXISTS idx_EventRecord_EventId ON EventRecord (EventId);";
            db.ExecuteNonQuery(sql);

            sql = @"CREATE INDEX IF NOT EXISTS idx_EventRecord_Ip ON EventRecord (Ip);";
            db.ExecuteNonQuery(sql);

            sql = @"CREATE INDEX IF NOT EXISTS idx_EventRecord_Crtm ON EventRecord (Crtm);";
            db.ExecuteNonQuery(sql);

            sql = @"CREATE TABLE IF NOT EXISTS BanRecord (
                    ID INTEGER PRIMARY KEY,
                    Ip TEXT,
                    BTime INTEGER,
                    ETime INTEGER
                  );";
            db.ExecuteNonQuery(sql);

            sql = @"CREATE UNIQUE INDEX IF NOT EXISTS idx_BanRecord_Ip ON BanRecord (Ip);";
            db.ExecuteNonQuery(sql);

            sql = @"CREATE INDEX IF NOT EXISTS idx_BanRecord_ETime ON BanRecord (ETime);";
            db.ExecuteNonQuery(sql);

            Logger.Info("InitDb End");

        }

        public int FindEventRecordCount(string eventId, string ip, long findTime)
        {
            long time = DateTimeUtils.CurrentTimeSeconds() - findTime;
            lock (m_lock)
            {
                string queryString = "SELECT COUNT(*) FROM EventRecord WHERE EventId = " + eventId + " AND Ip = '" + ip + "' AND Crtm >= " + time;
                // Logger.Info(queryString);
                string result = db.ExecuteQueryForString(queryString);
                if (string.IsNullOrEmpty(result)) return -1;
                try
                {
                    return Convert.ToInt32(result);
                }
                catch
                {
                    return -1;
                }
            }
        }

        public int FindBanRecordCount(string ip)
        {
            lock (m_lock)
            {
                string queryString = "SELECT COUNT(*) FROM BanRecord WHERE Ip = '" + ip + "'";
                // Logger.Info(queryString);
                string result = db.ExecuteQueryForString(queryString);
                if (string.IsNullOrEmpty(result)) return -1;
                try
                {
                    return Convert.ToInt32(result);
                }
                catch
                {
                    return -1;
                }
            }
        }


        public List<String> FindAllExpiredBanRecord()
        {
            long time = DateTimeUtils.CurrentTimeSeconds();
            List<string> resultList = new List<string>();
            string queryString = "SELECT Ip FROM BanRecord WHERE ETime < " + time + " limit 1000";
            lock (m_lock)
            {
                SQLiteDataReader reader = db.ExecuteQuery(queryString);
                while (reader.Read())
                {
                    resultList.Add(reader.GetString(reader.GetOrdinal("Ip")));
                }
            }
            return resultList;
        }

        public void InsertEventRecord(string eventId, string account, string ip)
        {
            long id = DateTimeUtils.GetTimeId();
            long crtm = DateTimeUtils.CurrentTimeSeconds();
            lock (m_lock)
            {
                db.InsertValues("EventRecord", new string[] { id.ToString(), eventId, account, ip, crtm.ToString() });
            }
        }

        public void DeleteEventRecordHistory(long findTime)
        {
            long crtm = DateTimeUtils.CurrentTimeSeconds() - findTime - 60;
            lock (m_lock)
            {
                db.DeleteValuesAND("EventRecord", new string[] { "Crtm" }, new string[] { crtm.ToString() }, new string[] { "<" });
            }
        }

        public void InsertBanRecord(string ip, long banTime)
        {
            long id = DateTimeUtils.GetTimeId();
            long btime = DateTimeUtils.CurrentTimeSeconds();
            long etime = btime + banTime;
            lock (m_lock)
            {
                db.InsertValues("BanRecord", new string[] { id.ToString(), ip, btime.ToString(), etime.ToString() });
            }
        }

        public void UpdateBanRecordEtime(string ip, long banTime)
        {
            long etime = DateTimeUtils.CurrentTimeSeconds() + banTime;
            lock (m_lock)
            {
                db.UpdateValues("BanRecord", new string[] { "Etime" }, new string[] { etime.ToString() }, "Ip", ip);
            }
        }

        public void DeleteBanRecord(string ip)
        {
            lock (m_lock)
            {
                db.DeleteValuesAND("BanRecord", new string[] { "Ip" }, new string[] { ip }, new string[] { "=" });
            }
        }

        public void PrintEventRecord(int count = 0)
        {
            try
            {
                lock (m_lock)
                {
                    SQLiteDataReader reader = db.ReadFullTable("EventRecord");
                    while (reader.Read())
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("ID=" + reader.GetDecimal(reader.GetOrdinal("ID")));
                        sb.Append(",EventId=" + reader.GetDecimal(reader.GetOrdinal("EventId")));
                        sb.Append(",Account=" + reader.GetString(reader.GetOrdinal("Account")));
                        sb.Append(",Ip=" + reader.GetString(reader.GetOrdinal("Ip")));
                        sb.Append(",Crtm=" + reader.GetDecimal(reader.GetOrdinal("Crtm")));
                        Logger.Debug(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public void PrintBanRecord(int count = 0)
        {
            try
            {
                lock (m_lock)
                {
                    SQLiteDataReader reader = db.ReadFullTable("BanRecord");
                    while (reader.Read())
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("ID=" + reader.GetDecimal(reader.GetOrdinal("ID")));
                        sb.Append("Ip=" + reader.GetString(reader.GetOrdinal("Ip")));
                        sb.Append("BTime=" + reader.GetDecimal(reader.GetOrdinal("BTime")));
                        sb.Append("ETime=" + reader.GetDecimal(reader.GetOrdinal("ETime")));
                        Logger.Debug(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

    }
}
