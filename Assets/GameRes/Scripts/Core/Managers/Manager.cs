using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager<T> : MonoSingleton<T> where T : MonoBehaviour
{
    public static List<Manager<T>> allMangers = new List<Manager<T>>();

    public virtual void Init() { }

    void Awake()
    {
        allMangers.Add(this);
    }
    
    void OnDestroy()
    {
        allMangers.Remove(this);
    }
}
