using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
 
public static partial class Extensions
{   
   	/// <summary>
    /// 搜索子物体组件-GameObject版
    /// </summary>
    public static T? Get<T>(this GameObject go, string subnode) where T : Component
    {
        if (go != null)
        {
            Transform sub = go.transform.Find(subnode);
			if (sub == null)
			{
				Debug.LogError("Node not exist");
				return null;
			}
			
			return sub.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 搜索子物体组件-Transform版
    /// </summary>
    public static T? Get<T>(this Transform go, string subnode) where T : Component
    {
        if (go != null)
        {
            Transform sub = go.Find(subnode);
			if (sub == null)
			{
				Debug.LogError("Node not exist");
				return null;
			}

			return sub.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// 搜索子物体组件-Component版
    /// </summary>
    public static T? Get<T>(this Component go, string subnode) where T : Component
    {
        return go.transform.Get<T>(subnode);
    }
 
    /// <summary>
	/// 获取或增加组件。
	/// </summary>
	/// <typeparam name="T">要获取或增加的组件。</typeparam>
	/// <param name="gameObject">目标对象。</param>
	/// <returns>获取或增加的组件。</returns>
    public static T? GetOrAddComponent<T>(this Transform transform) where T : Component
	{
		if (transform == null) return null;
		GameObject obj = transform.gameObject;
		return obj.GetOrAddComponent<T>();
	}
	
    /// <summary>
	/// 获取或增加组件。
	/// </summary>
	/// <typeparam name="T">要获取或增加的组件。</typeparam>
	/// <param name="gameObject">目标对象。</param>
	/// <returns>获取或增加的组件。</returns>
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		if (component == null)
		{
			component = gameObject.AddComponent<T>();
		}

		return component;
	}

    /// <summary>
    /// 获取 GameObject 是否在场景中。
    /// </summary>
    /// <param name="gameObject">目标对象。</param>
    /// <returns>GameObject 是否在场景中。</returns>
    /// <remarks>若返回 true，表明此 GameObject 是一个场景中的实例对象；若返回 false，表明此 GameObject 是一个 Prefab。</remarks>
    public static bool InScene(this GameObject gameObject)
    {
        return gameObject.scene.name != null;
    }
	
    private static readonly List<Transform> s_CachedTransforms = new List<Transform>();
	
    /// <summary>
	/// 递归设置游戏对象的层次。
	/// </summary>
	/// <param name="gameObject"><see cref="GameObject" /> 对象。</param>
	/// <param name="layer">目标层次的编号。</param>
	public static void SetLayerRecursively(this GameObject gameObject, int layer)
	{
		gameObject.GetComponentsInChildren(true, s_CachedTransforms);
		for (int i = 0; i < s_CachedTransforms.Count; i++)
		{
			s_CachedTransforms[i].gameObject.layer = layer;
		}

		s_CachedTransforms.Clear();
	}

    public static void DoLocalMovePosition(this Transform transform)
	{
		transform.DOComplete();
	}


    public static Tweener DoScaleYoyo(this Transform transform, float scale, float time, int loop = 1)
    {
       return transform.DOScale(scale, time).SetLoops(loop, LoopType.Yoyo).SetEase(Ease.InCirc);
    } 

    public static Coroutine DelayDoSomething(this MonoBehaviour monoBehaviour,float time, Action callback)
    {
       return monoBehaviour.StartCoroutine(_DelayDoSomething(time, callback));
    }
    public static IEnumerator _DelayDoSomething(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }

    
 
    /// <summary>
    /// 循环查找Childs
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static List<Transform>  FindChildsByName(this Transform transform, string name)
    {
        if (transform == null) return new();

        List<Transform> trans = new List<Transform>();

        int childCount = transform.childCount;
        if (childCount == 0) return new();

        for (int i = 0; i < childCount; ++i)
        {
            if (transform.GetChild(i).name == name)
            {
                trans.Add(transform.GetChild(i));
            }
        }
        return trans;
    }


    public static void RemoveAllChild(this Transform transform)
    {
        if (transform == null) return;
        transform.DestroyAllChild();
    }
    public static void DestroyAllChild(this Transform transform, bool immediate = false)
    {
        if (transform == null) return;

        int childCount = transform.childCount;
        if (childCount == 0) return;

        Transform? trans = null;
        for (int i = childCount-1; i >= 0; --i)
        {
            trans = transform.GetChild(i);
            if (trans != null)
            {
				if(immediate) GameObject.DestroyImmediate(trans.gameObject);
				else GameObject.Destroy(trans.gameObject);
				
                trans = null;
            }
        }
    }

    public static void SetChildActive(this Transform trans,bool active)
    {
        if (trans == null) return;

        int childCount = trans.childCount;
        if (childCount > 0)
        {
            for(int i = 0; i < childCount; ++i)
            {
                trans.GetChild(i).gameObject.SetActive(active);
            }
        }
    }

    public static int  GetActiveChildCount(this Transform trans)
    {
        if (trans == null) return 0;
        int activeCount = 0;
        int childCount = trans.childCount;
        if (childCount > 0)
        {
            for(int i = 0; i < childCount; ++i)
            {
                if(trans.GetChild(i).gameObject.activeSelf) activeCount++;
            }
        }
        return activeCount;
    }

    public static void SimplePool(this Transform trans, int count, Action<Transform, int> callback)
    {
        if(trans.childCount == 0 )
        {
            return;
        }
        if(count == 0 )
        {
            trans.SetChildActive(false);
            return;
        }
        Transform item = trans.GetChild(0);
        Transform temp;
        for (int i = 0; i < count; ++i)
        {
            if (trans.childCount < i + 1)
            {   
                temp = Transform.Instantiate(item, trans, false);
            }
            else
            {
                temp = trans.GetChild(i);
            }


            temp.gameObject.SetActive(true);
            callback?.Invoke(temp, i);
        }

        for (int i = count; i < trans.childCount; ++i)
        {
            trans.GetChild(i).gameObject.SetActive(false);
        }
    }
   
    public static void SimplePool_Add(this Transform trans, int count, int sibling, Action<Transform, int> callback)
    {
        if(trans.childCount == 0 )
        {
            return;
        }
        if(count == 0 )
        {
            return;
        }
        Transform item = trans.GetChild(0);

        for (int i = 0; i < count; ++i)
        {
            Transform? temp = null; 
            for (int c = 1; c < trans.childCount; c++)
            {
                var child = trans.GetChild(c);
                if(child.gameObject.activeSelf == false) 
                {
                    temp = child;
                    break;
                }
            }
            if(temp == null)temp = Transform.Instantiate(item, trans, false);
            temp.SetSiblingIndex(sibling);
            temp.gameObject.SetActive(true);
            callback?.Invoke(temp, i);
        }
    }

    public static List<GameObject> SimplePoolObjList(this Transform trans, int count)
    {

        List<GameObject> objList = new List<GameObject>();
        Transform item = trans.GetChild(0);
        Transform temp;
        for (int i = 0; i < count; ++i)
        {
            if (trans.childCount < i + 1)
            {
                temp = Transform.Instantiate(item, trans, false);
            }
            else
            {
                temp = trans.GetChild(i);
            }
            objList.Add(temp.gameObject);
            temp.gameObject.SetActive(true);
        }
        for (int i = count; i < trans.childCount; ++i)
        {
            trans.GetChild(i).gameObject.SetActive(false);
        }
        return objList;
    }
    
    public static IEnumerator SimplePoolYield(this Transform trans, int count, Action<Transform, int> callback,Func<IEnumerator> yieldFunc)
    {
        trans.SetChildActive(false);
        if(count == 0)
        {
            yield break;
        }
        Transform item = trans.GetChild(0);
        Transform temp;
        for (int i = 0; i < count; ++i)
        {
            if (trans.childCount < i + 1)
            {   
                temp = Transform.Instantiate(item, trans, false);
            }
            else
            {
                temp = trans.GetChild(i);
            }


            temp.gameObject.SetActive(true);
            callback?.Invoke(temp, i);

            yield return yieldFunc;
        }

        
    }

    /// <summary>
    /// 设置UI置灰状态
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="grayState">是否置灰</param>
    public static void SetUIGray(this Transform trans, bool grayState)
    {
		Material? mat_gray = null;// UIManager.Instance.uiGrayMat;
        if(null == mat_gray)
        {
            Debug.LogError("UI置灰失败，材质丢失！");
            return;
        }
        var graphics = trans.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics)
        {
            if(grayState)
            {
                if(graphic.material == null || graphic.material.name == "Default UI Material") graphic.material = mat_gray;
            }else{
                if(graphic.material == mat_gray)graphic.material = graphic.defaultMaterial;
            }
            
        }
    }

}
