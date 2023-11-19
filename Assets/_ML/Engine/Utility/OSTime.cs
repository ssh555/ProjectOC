using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Utility
{
    public static class OSTime
    {
        /// <summary>
        /// 北京时间
        /// </summary>
        public static System.DateTime BJTime => System.TimeZoneInfo.ConvertTime(System.DateTime.UtcNow, System.TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));

        /// <summary>
        /// 当前系统时间
        /// </summary>
        public static System.DateTime OSCurTime => System.DateTime.Now;

        public static string OSCurTimeWithmMM => OSCurTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

        /// <summary>
        /// 当前系统时间自1970-01-01-00:00:00以来的秒数(不带毫秒)时间
        /// </summary>
        public static System.Int64 OSCurSeconedTime
        {
            get
            {
                System.TimeSpan timeSpan = OSCurTime - new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                return System.Convert.ToInt64(timeSpan.TotalSeconds);
            }
        }

        /// <summary>
        /// 当前系统时间自1970-01-01-00:00:00:000以来的秒数(带毫秒)时间 -> 将毫秒数值拼接在 OSCurSeconedTime 后面
        /// </summary>
        public static System.Int64 OSCurMilliSeconedTime
        {
            get
            {
                System.TimeSpan timeSpan = OSCurTime - new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                return System.Convert.ToInt64(timeSpan.TotalMilliseconds);
            }
        }
    }
}

