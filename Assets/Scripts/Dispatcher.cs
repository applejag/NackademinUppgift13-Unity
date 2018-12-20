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

    public static Action Wrap(Action action)
    {
        return delegate { Invoke(action); };
    }

    public static Action<T1> Wrap<T1>(Action<T1> action)
    {
        return delegate (T1 param1) { Invoke(action, param1); };
    }

    public static Action<T1, T2> Wrap<T1, T2>(Action<T1, T2> action)
    {
        return delegate (T1 param1, T2 param2) { Invoke(action, param1, param2); };
    }

    public static Action<T1, T2, T3> Wrap<T1, T2, T3>(Action<T1, T2, T3> action)
    {
        return delegate (T1 param1, T2 param2, T3 param3) { Invoke(action, param1, param2, param3); };
    }

    public static void Invoke(Action action)
    {
        _singleton.actions.Add(action);
    }

    public static void Invoke<T>(Action<T> action, T param1)
    {
        _singleton.actions.Add(() => { action(param1); });
    }

    public static void Invoke<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
    {
        _singleton.actions.Add(() => { action(param1, param2); });
    }

    public static void Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        _singleton.actions.Add(() => { action(param1, param2, param3); });
    }

    public static void Invoke<T>(Func<T> action)
    {
        _singleton.actions.Add(() => { action(); });
    }

    public static void Invoke<T1, T>(Func<T1, T> action, T1 param1)
    {
        _singleton.actions.Add(() => { action(param1); });
    }

    public static void Invoke<T1, T2, T3, T>(Func<T1, T2, T3, T> action, T1 param1, T2 param2, T3 param3)
    {
        _singleton.actions.Add(() => { action(param1, param2, param3); });
    }
}
