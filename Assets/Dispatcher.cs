using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : MonoBehaviour
{
    private static Dispatcher _singleton;

    private readonly ConcurrentBag<Action> actions = new ConcurrentBag<Action>();

    private void OnEnable()
    {
        _singleton = this;
    }

    private void Update()
    {
        if (actions.IsEmpty) return;

        while (actions.TryTake(out Action action))
        {
            action();
        }
    }

    public static void Invoke(Action action)
    {
        _singleton.actions.Add(action);
    }
}
