using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ScreenRouter : MonoBehaviour
{
    [SerializeField] private ScreenId startScreen = ScreenId.Dice;

    private readonly Dictionary<ScreenId, ScreenView> _map = new();
    public ScreenId Current { get; private set; }
    private bool _hasCurrent = false;

    public event Action<ScreenId> OnChanged;

    void Awake()
    {
        foreach (var sv in GetComponentsInChildren<ScreenView>(true))
            if (!_map.ContainsKey(sv.Id)) _map.Add(sv.Id, sv);
    }

    void Start()
    {
        foreach (var sv in _map.Values) sv.Hide();  // ← true 인자 제거
        GoTo(startScreen);                           // 첫 전환은 강제 실행(_hasCurrent=false)
    }


    public void GoToByName(string id)
    {
        if (Enum.TryParse<ScreenId>(id, true, out var e)) GoTo(e);
    }

    public void GoTo(ScreenId id)
    {
        if (_hasCurrent && Current.Equals(id)) return;

        if (_hasCurrent && _map.TryGetValue(Current, out var prev)) prev.Hide();
        if (_map.TryGetValue(id, out var next)) next.Show();

        Current = id;
        _hasCurrent = true;
        OnChanged?.Invoke(Current);
    }
}
