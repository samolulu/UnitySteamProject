

using UnityEngine;

/// <summary>
/// MonoBehaviour 单例类
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                if (instance == null)
                {
                    GameObject go = GameObject.Find("GameManager");
                    if (go == null)
                    {
                        go = new GameObject("GameManager");
                    }

                    instance = go.AddComponent<T>();
                }

                if (instance == null)
                {
                    Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
                }
            }
            return instance;
        }
    }

 
}
