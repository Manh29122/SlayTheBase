using System;
using UnityEngine;
using UnityEngine.Events;

public class OnLoadFuntion : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UnityEvent onLoad;
    void Awake()
    {
        //second commit
        onLoad?.Invoke();
    }
}
