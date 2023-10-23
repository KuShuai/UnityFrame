using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : class, ISingleton
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Activator.CreateInstance<T>();
                instance.SingletonInit();
                Debug.LogWarning("Create Singleton");
            }
            return instance;
        }
    }

    static Singleton()
    {
        Application.quitting += () =>
        {
            instance.SingletonDestory();
            instance = null;
            Debug.Log("Singleton    " + typeof(T) + "   Destory");
        };
    }
}
