using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;


public class Util
{
 
    public static string platformPath
    {
        get
        {
#if UNITY_EDITOR
            return "file:///";
#elif UNITY_IPHONE || UNITY_IOS
                return "file://";
#elif UNITY_ANDROID
                return "file:///";
#else
                return "file:///";
#endif
        }
    }

    /// <summary>
    /// 是否为ARM64(在32位进程中为4,在64位进程中为8)
    /// </summary>
    public static bool IsARM64()
    {
        bool arm64 = false;
        if (IntPtr.Size == 4)
        {
            arm64 = false;
        }
        else
        {
            arm64 = true;
        }
        return arm64;
    }

    /// <summary>
    /// 网络可用
    /// </summary>
    public static bool NetAvailable
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }

    /// <summary>
    /// 是否是无线
    /// </summary>
    public static bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }

    /// <summary>
    /// 世界坐标转屏幕坐标
    /// </summary>
    public static Vector2 WorldToScreenPoint(Vector3 worldPoint)
    {
        return RectTransformUtility.WorldToScreenPoint(Camera.main, worldPoint);
    }
}
