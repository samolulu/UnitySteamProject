using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MiniExcelLibs;
using System.Linq;
using System.Text;
 

public static class ExcelTool
{
    public static string ExcelPath
    {
        get
        {
            return $"{Application.dataPath}/GameRes/Config/Excels";;
        }
    }

    static StringBuilder sb = new();
    public static void CheckEmptyRow(string excelPath, string excelName, string sheetName )
    {
        string path = $"{excelPath}/{excelName}.xlsx";
        var rows = MiniExcel.Query(path, useHeaderRow:true, sheetName).ToList();
        var line = 1;
        int i_empty_start = -1;
        int i_empty_end = -1;
        List<int> emptyLines = new();
        sb.Clear();
        foreach (IDictionary<string,object> row in rows)
        {
            line ++;
            if(row.Values.All(d=>d==null)){
                if(i_empty_start == -1) i_empty_start = line;
                i_empty_end = line;
                emptyLines.Add(line);
            }else{
                if(i_empty_end != -1)
                {
                    if(i_empty_start == i_empty_end)
                        sb.Append($"{i_empty_start}；");
                    else
                        sb.Append($"{i_empty_start}->{i_empty_end}；");
                    i_empty_start = -1;
                    i_empty_end = -1;
                }
            }
               
        }
        if(i_empty_end != -1)
        {
            if(i_empty_start == i_empty_end)
                sb.Append($"{i_empty_start}；");
            else
                sb.Append($"{i_empty_start}->{i_empty_end}；");
        }
        if(emptyLines.Count > 0) Debug.LogError($"配置：{excelName}-{sheetName} 存在空行:{sb}");   // {string.Join(",",emptyLines)}
    }

    public static List<T> GetListOject<T>() where T : class, new()
    {
        //Debug.LogError($"typeof(T).Name {typeof(T).Name}");
        string tempName = typeof(T).Name;
        int index = tempName.IndexOf("Data");
        if (index > -1)
        {
            tempName = tempName.Substring(0, index);
        }
        string path = $"{ExcelPath}/{tempName}.xlsx";

        return MiniExcel.Query<T>(path, sheetName: tempName, excelType: ExcelType.XLSX).ToList();
    }


 
    public static List<T> GetListOject<T>(string excelName, string sheetName, string startCell = "A1") where T : class, new()
    {
        //System.Diagnostics.Stopwatch stopwatch = new();
        //stopwatch.Start();

        string path = $"{ExcelPath}/{excelName}.xlsx";

        try
        {
        #if UNITY_EDITOR
            //CheckEmptyRow(ExcelPath, excelName, sheetName); 不再运行时检查，定期手动执行检查  [MenuItem("Tool/配表/检查所有配置表中的空行")]
        #endif
            return MiniExcel.Query<T>(path, sheetName: sheetName, excelType: ExcelType.XLSX, startCell: startCell)
            .Where(row=>!row.IsDefault(true))//只取有效行
            .ToList();

            //stopwatch.Stop();
            //Debug.Log($"Excel({excelName}-{sheetName})加载消耗时间 : {stopwatch.ElapsedMilliseconds} 毫秒");
            //return list;
        }
        catch (Exception e)
        {
            Debug.LogError(excelName+  "++++++++++=" + sheetName);
            Debug.LogError($"Error:{e.Message} excelName:{excelName} sheetName:{sheetName} ");
            throw;
        }
    }
 
    /// <summary>
    /// 解析Excel表 得到字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="excelName"></param>
    /// <param name="sheetName"></param>
    /// <param name="keyName">字典的keyName</param>
    /// <returns></returns>
    public static Dictionary<TKey, T> GetDictionary<TKey, T>(string excelName, string sheetName, string startCell = "A1", string keyName = "id" ,bool isClearZero = false) where T : class, new()
    {
        List<T> listT = GetListOject<T>(excelName, sheetName, startCell);
        Dictionary<TKey, T> dic = null;
        if (listT != null && listT.Count > 0)
        {
            dic = new Dictionary<TKey, T>(listT.Count);
            object key;
            for (int i = 0; i < listT.Count; ++i)
            {
                var value = listT[i] ;
                if (value == null) continue;
 
                key = value.GetPropertyValue(keyName);
                if(key == null || key.GetType() != typeof(TKey) ){ 
                    Debug.LogError($"配置{excelName}-{sheetName} key的类型定义异常:{keyName},");
                    continue;
                }
                if (isClearZero && (int)key == 0) continue;
                dic[(TKey)key] = value;
            }
        }

        if (dic == null)
        {
            dic = new Dictionary<TKey, T>();
        }

        return dic;
    }

    public static Dictionary<int, T> GetDictionary<T>() where T : class, new()
    {
        List<T> listT = GetListOject<T>();
        Dictionary<int, T> dic = null;
        if (listT != null && listT.Count > 0)
        {
            dic = new Dictionary<int, T>(listT.Count);
            for (int i = 0; i < listT.Count; ++i)
            {
                dic[(int)listT[i].GetPropertyValue("id")] = listT[i];
            }
        }

        if (dic == null)
        {
            dic = new Dictionary<int, T>();
        }

        return dic;
    }


    public static Dictionary<int, T> GetDictionary<T>(List<T> listT) where T : class, new()
    {

        Dictionary<int, T> dic = null;
        if (listT != null && listT.Count > 0)
        {
            dic = new Dictionary<int, T>(listT.Count);
            for (int i = 0; i < listT.Count; ++i)
            {
                dic[(int)listT[i].GetPropertyValue("id")] = listT[i];
            }
        }

        if (dic == null)
        {
            dic = new Dictionary<int, T>();
        }

        return dic;
    }

    public static Dictionary<T2, T> GetDictionary<T2, T>(List<T> listT) where T : class, new()
    {

        Dictionary<T2, T> dic = null;
        if (listT != null && listT.Count > 0)
        {
            dic = new Dictionary<T2, T>(listT.Count);
            for (int i = 0; i < listT.Count; ++i)
            {
                dic[(T2)listT[i].GetPropertyValue("id")] = listT[i];
            }
        }

        if (dic == null)
        {
            dic = new Dictionary<T2, T>();
        }

        return dic;
    }
}
