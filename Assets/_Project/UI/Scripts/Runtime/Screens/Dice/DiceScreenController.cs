// Assets/_Project/UI/Scripts/Runtime/Screens/Dice/DiceScreenController.cs
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class DiceScreenController : MonoBehaviour
{
    private const int MaxDice = 6;

    [Header("References")]
    [SerializeField] private Transform diceRoot; // Dice Row (HorizontalLayoutGroup)
    [SerializeField] private DieView diePrefab;  // Die.prefab (프로젝트 자산)

    private DiceManager _dice;
    private ResourceCounter _res;
    private GameTickLoop _tick;

    private readonly List<DieView> _views = new(6);
    private int _activeCount;
    private bool _ready;
    private readonly int[] _facesBuf = new int[MaxDice];

    private volatile int _rollGate = 0; // 0=닫힘, 1=열림

    void Awake()
    {
        _dice = ServiceContainer.Get<DiceManager>();
        _res = ServiceContainer.Get<ResourceCounter>();
        _tick = ServiceContainer.Get<GameTickLoop>();

        if (_tick != null) _tick.Tick += OnTick;                // ← null 체크
        if (_dice != null) _dice.OnConfigChanged += SyncDiceViews;

        SyncDiceViews();
        SetAllReady(false);
        _rollGate = 0;
    }

    void OnDestroy()
    {
        if (_tick != null) _tick.Tick -= OnTick;               // ← null 체크
        if (_dice != null) _dice.OnConfigChanged -= SyncDiceViews;

        for (int i = 0; i < _views.Count; i++)
            if (_views[i] != null) _views[i].OnClicked = null;
    }

    private void OnTick()
    {
        SetAllReady(true);
        Interlocked.Exchange(ref _rollGate, 1);
    }

    private void OnDieClicked(DieView _)
    {
        if (Interlocked.Exchange(ref _rollGate, 0) != 1) return;

        int used = Mathf.Min(_activeCount, MaxDice);
        long sum = _dice.RollAllNonAlloc(_facesBuf, used);
        _res.Add(sum);

        for (int i = 0; i < used; i++)
            _views[i].SetValue(_facesBuf[i]);

        SetAllReady(false);
    }

    private void SyncDiceViews()
    {
        if (diceRoot == null || diePrefab == null)
        {
            Debug.LogError("[DiceScreen] Missing refs: diceRoot or diePrefab");
            return;
        }

        int want = Mathf.Clamp(_dice != null ? _dice.DiceCount : 1, 1, MaxDice);

        while (_views.Count < want)
        {
            var v = Instantiate(diePrefab, diceRoot);
            v.OnClicked = OnDieClicked;
            _views.Add(v);
        }

        for (int i = 0; i < _views.Count; i++)
        {
            bool active = i < want;
            var go = _views[i].gameObject;
            if (go.activeSelf != active) go.SetActive(active);
        }

        _activeCount = want;
    }

    private void SetAllReady(bool ready)
    {
        if (_ready == ready) return;
        _ready = ready;
        for (int i = 0; i < _activeCount; i++)
            _views[i].SetRollReady(ready);
    }
}
