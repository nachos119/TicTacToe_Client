using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazySingleton<T> where T : class
{
    private static readonly Lazy<T> instance = new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T);

    public static T Instance
    {
        get { return instance.Value; }
    }

}
