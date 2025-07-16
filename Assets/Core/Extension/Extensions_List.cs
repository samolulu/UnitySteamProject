using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static partial class Extensions
{
    /// <summary>
    /// 移除列表第一个元素并返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T Shift<T>(this List<T> list)
    {
        var element = list[0];
        list.RemoveAt(0);
        return element;
    }

    /// <summary>
    /// 获取最后一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetLast<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }

    /// <summary>
    /// 移除列表最后一个元素并返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T Pop<T>(this List<T> list)
    {
        var element = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return element;
    }

    /// <summary>
    /// 从0位置插入一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Unshift<T>(this List<T> list, T item)
    {
        list.Insert(0, item);
    }

    /// <summary>
    /// 简单的升序排序
    /// </summary>
    /// <param name="list"></param>
    public static void SortUp(this List<int> list)
    {
        list.Sort((x, y) => x >= y ? 1 : -1);
    }

    /// <summary>
    /// 简单的降序排序
    /// </summary>
    /// <param name="list"></param>
    public static void SortDown(this List<int> list)
    {
        list.Sort((x, y) => x > y ? -1 : 1);
    }

    /// <summary>
    /// 清理列表
    /// </summary>
    /// <param name="list"></param>
    public static void ClearObjList(this List<GameObject> list)
    {
        for (int k = 0; k < list.Count; k++)
        {
            if (list[k] != null)
            {
                GameObject.Destroy(list[k]);
            }
        }
        list.Clear();
    }
	   
    /// <summary>
    /// 合并两个列表，并且剔除相同的元素
    /// </summary>
    /// <param name="list"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static List<T> Merge<T>(this List<T> list, List<T> list2)
    {
        if (list == null || list2 == null) return new List<T>();
        foreach (var data in list2)
        {
            if (!list.Contains(data))
            {
                list.Add(data);
            }
        }
        return list;
    }

    /// <summary>
    /// 获得两个列表的交集
    /// </summary>
    /// <param name="list"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static List<T> Intersect<T>(this List<T> list, List<T> list2)
    {
        List<T> newList = new List<T>();
        if (list == null || list2 == null) return newList;
        foreach (var data in list2)
        {
            if (list.Contains(data))
            {
                newList.Add(data);
            }
        }
        return newList;
    }

    /// <summary>
    /// 获得两个列表的差集
    /// </summary>
    /// <param name="list"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static List<T> Diff<T>(this List<T> list, List<T> list2)
    {
        List<T> newList = new List<T>();
        if (list == null || list2 == null) return newList;
        foreach (var data in list2)
        {
            if (!list.Contains(data))
            {
                newList.Add(data);
            }
        }
        return newList;
    }

 


}
