using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class FunTool
{
 
    /// <summary>
    /// 随机概率
    /// </summary>
    public static bool Prob(int p)
    {
        return UnityEngine.Random.Range(0, 100) < p;
    }
    public static bool ProbLong(long p)
    {
        return UnityEngine.Random.Range(0, 100) < p;
    }
 

    [System.Serializable]
    public class RandomConfig
    {
        public RandomConfig(int v, int w)
        {
            value = v;
            weight = w;
        }

        //返回值（不一定是int，float、枚举、类、结构体都可以）
        public int value;
        //随机权重
        public int weight;
    }


    public static RandomConfig GetRandomValue(List<RandomConfig> randomConfigList)
    {
        //累加结算总权重
        int totalWeight = randomConfigList.Aggregate(0, (all, next) => all += next.weight);

        //在0~total范围内随机
        int cursor = 0;
        int random = UnityEngine.Random.Range(0, totalWeight);
        foreach (var item in randomConfigList)
        {
            //累加当前权重
            cursor += item.weight;
            //判断随机数
            if (cursor > random)
            {
                return item;
            }
        }
        return null;
    }

    //从字典中获取n个随机元素
    public static Dictionary<TKey, TValue> UnorderedDictionary<TKey, TValue>(Dictionary<TKey, TValue> dic, int count = 0)
    {
        if(count <= 0 || count >= dic.Count) return dic;
        Dictionary<TKey, TValue> randomDic = new Dictionary<TKey, TValue>();
        List<TKey> list = new(dic.Keys);
        System.Random random = new();
        list = list.Select(v => new{ran = random.Next(), value = v}).OrderBy(x => x.ran).Select(x =>x.value).ToList();
 
        for (int i = 0; i < count; i++)
        {
            randomDic.Add(list[i], dic[list[i]] );
 
        }
        return randomDic;
    }

    /// <summary>
    /// 字典内容打乱装进List
    /// </summary>
    public static List<TValue> UnorderedDicToList<TValue>(Dictionary<int, TValue> dic, int count = 0)
    {
        if(count <= 0 || count >= dic.Count) return dic.Values.ToList();
        System.Random random = new();
        var que = dic.Values.Select(v => new{ran = random.Next(), value = v}).OrderBy(x => x.ran).Select(x =>x.value);
        List<TValue> list = new();

        list.AddRange(que.Take(count));

        return list;
    }
 
 
    /// <summary>
    /// 解析权重配置获得随机值
    /// 配置格式：value_weight|value_weight|value_weight
    /// </summary>
    /// <returns></returns>
    public static int WeightValueInt(string conf)
    {
        var data = conf.ToIntList2();
        var weight = 0;
        foreach (var item in data)
        {
            if(item.Count != 2)
            {
                Debug.LogError($"权重配置解析异常:{conf}");
                return 0;
            }
            weight += item[1];
        }
        int point = UnityEngine.Random.Range(0, weight);
        weight = 0;
        foreach (var item in data)
        {
            weight += item[1];
            if(weight > point) return item[0];
        }
        Debug.LogError($"权重配置解析异常:{conf}");
        return 0;
    }

     
    /// <summary>
    /// 随机获取N个满足条件的集合元素
    /// </summary>
    public static List<T> GetRandomItemListInCollection<T>(IEnumerable<T> collection, int num, Func<T, bool> checkFunc = null)
    {
        List<T> result = new(collection);
        System.Random random = new();
        
        result = result.Where(d =>  checkFunc?.Invoke(d)??true ).
        Select(v => new{ran = random.Next(), value = v}).OrderBy(x => x.ran).Select(x =>x.value).ToList();
        if(num < 0 ) num = result.Count;
        result = result.GetRange(0, Math.Min(result.Count, num));
 
        return result;
    }

    /// <summary>
    /// 根据排序规则获取前N个满足条件的集合元素
    /// </summary>
    public static List<T> GetSortItemListInCollection<T>(IEnumerable<T> collection, int num, Func<T, bool> checkFunc = null,  Func<T, int> sortFunc = null)
    {
        List<T> result = new(collection);
 
        result = result.Where(d =>  checkFunc?.Invoke(d)??true ).
        OrderByDescending(d => sortFunc?.Invoke(d) ?? 0).Take(num).ToList();
 
        return result;
    }

    /// <summary>
    /// 获取集合中的随机一个满足条件的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="checkFunc"></param>
    /// <returns></returns>
    public static T GetRandomItemInCollection<T>(IEnumerable<T> collection, Func<T, bool> checkFunc = null)
    {
        System.Random random = new();
        T item = collection.Where(d =>  checkFunc?.Invoke(d)??true ).
        Select(v => new{ran = random.Next(), value = v}).OrderBy(x => x.ran).Select(x =>x.value).FirstOrDefault();
        return item;
    }

    

}
 