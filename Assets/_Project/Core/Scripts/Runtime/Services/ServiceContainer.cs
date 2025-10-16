using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>간단한 전역 DI 컨테이너 (타입 → 인스턴스 매핑).</summary>
public static class ServiceContainer
{
    static readonly Dictionary<Type, object> _map = new();

    public static void Bind<T>(T instance, bool overwrite = true)
    {
        var key = typeof(T);
        if (instance == null)
        {
            Debug.LogError($"[ServiceContainer] Bind null attempted for {key.Name}");
            return;
        }
        if (_map.ContainsKey(key))
        {
            if (!overwrite)
            {
                Debug.LogWarning($"[ServiceContainer] {key.Name} already bound. Skipped.");
                return;
            }
            _map[key] = instance;
        }
        else
        {
            _map.Add(key, instance);
        }
    }

    public static T Get<T>() where T : class
    {
        var key = typeof(T);
        if (_map.TryGetValue(key, out var obj)) return obj as T;
        Debug.LogError($"[ServiceContainer] Service not found: {key.Name}");
        return null;
    }

    public static bool TryGet<T>(out T value) where T : class
    {
        var key = typeof(T);
        if (_map.TryGetValue(key, out var obj) && obj is T cast)
        {
            value = cast; return true;
        }
        value = null; return false;
    }

    public static bool IsBound<T>() => _map.ContainsKey(typeof(T));
    public static void Unbind<T>() => _map.Remove(typeof(T));
    public static void Clear() => _map.Clear(); // 테스트용
}
