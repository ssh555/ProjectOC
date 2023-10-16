using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Utility
{
    public static class TLogger
    {
        public static System.DateTime BJTime => System.TimeZoneInfo.ConvertTime(System.DateTime.UtcNow, System.TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
    }
}

