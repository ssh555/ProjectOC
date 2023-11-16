using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Utility
{
    public static class OSTime
    {
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public static System.DateTime BJTime => System.TimeZoneInfo.ConvertTime(System.DateTime.UtcNow, System.TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));

        /// <summary>
        /// ��ǰϵͳʱ��
        /// </summary>
        public static System.DateTime OSCurTime => System.DateTime.Now;

        public static string OSCurTimeWithmMM => OSCurTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

        /// <summary>
        /// ��ǰϵͳʱ����1970-01-01-00:00:00����������(��������)ʱ��
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
        /// ��ǰϵͳʱ����1970-01-01-00:00:00:000����������(������)ʱ�� -> ��������ֵƴ���� OSCurSeconedTime ����
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

