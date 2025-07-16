using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeUtil
{
    /// <summary>
    /// 一天的秒数
    /// </summary>
    public const long SecondsPerDay = 86400;
    public const long SecondsPerHour = 3600;
    public const long SecondsPerMinute = 60;
    public const long MillisecondPerSecond = 1000;
 

    /// <summary>
    /// 一天的毫秒数
    /// </summary>
    public const long MillisecondPerDay = SecondsPerDay * MillisecondPerSecond;
 
	static private DateTime mTime1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
 
    public static long GetTime()
    {
        TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - mTime1970.Ticks);
        return (long)ts.TotalMilliseconds;
    }
	
    public static long Now()
	{
		return (DateTime.UtcNow.Ticks - mTime1970.Ticks) / 10000;
	}

    public static long NowSec()
    {
        return (DateTime.UtcNow.Ticks - mTime1970.Ticks) / 10000000;
    }
}
