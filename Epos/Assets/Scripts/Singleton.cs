using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;
    
    public static T Instance
    {
        get
        {
            if (_instance == null)
                Create();

            return _instance;
        }
    }

    private static void Create()
    {
        if (!Application.isPlaying)
            return;
        
        var obj = FindAnyObjectByType<T>();
        if (obj == null)
        {
            var gameObj = new GameObject(typeof(T).Name);
            _instance = gameObj.AddComponent<T>();
        }
        else
        {
            _instance = obj;
        }
                
        _instance.GetComponent<Singleton<T>>()?.Initialize();
    }
    
    public static bool Validate()
    {
        return _instance != null;
    }

    protected virtual void Awake()
    {
        if (_instance == null)
            Create();  
    }

    protected abstract void Initialize();
}