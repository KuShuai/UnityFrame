using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T>: MonoBehaviour where T : MonoBehaviour,IMonoSingleton
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Util.CreateDonDestroyObj<T>(typeof(T).Name);
                instance.SingletonInit();
                Debug.LogWarning("Create MonoSingleton");
            }
            return instance;
        }
    }

    public virtual void Awake()
    {
        instance = this as T;    
    }
}
