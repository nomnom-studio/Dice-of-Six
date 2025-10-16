using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Roll(=Tick)�� �߻��� �������� ���� �ֱ� ����� �ٽ� �����ϴ� ����.
/// Tick ���� ������ �ϵ�, �� Tick�� �� ���� �����.
/// </summary>
public sealed class GameTickLoop : MonoBehaviour
{
    private double _intervalSeconds;
    private int _version;
    private bool _configured;

    public bool IsConfigured => _configured;

    public double IntervalSeconds
    {
        get => _intervalSeconds;
        set
        {
            double v = Math.Max(0.1, value);
            if (!_configured) _configured = true;
            if (Math.Abs(v - _intervalSeconds) > double.Epsilon)
            {
                _intervalSeconds = v;
                _version++;
            }
            EnsureRunning();
        }
    }

    public event Action Tick;

    private Coroutine _co;
    private bool _running;

    void OnEnable() => EnsureRunning();

    void OnDisable()
    {
        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
            _running = false;
        }
    }

    void EnsureRunning()
    {
        if (_configured && !_running && isActiveAndEnabled)
        {
            _co = StartCoroutine(TickRoutine());
            _running = true;
        }
    }

    IEnumerator TickRoutine()
    {
        double nextTickTime = Time.realtimeSinceStartupAsDouble + _intervalSeconds;

        while (true)
        {
            int snap = _version;

            // ��ٸ�
            while (Time.realtimeSinceStartupAsDouble < nextTickTime)
            {
                if (snap != _version)
                {
                    nextTickTime = Time.realtimeSinceStartupAsDouble + _intervalSeconds;
                    break;
                }
                yield return null;
            }

            if (snap != _version) continue;

            // Tick �߻�
            Tick?.Invoke();

            // �߿�: ���⼭ ���� �� ���� �������� ���� �ֱ� ����
            nextTickTime = Time.realtimeSinceStartupAsDouble + _intervalSeconds;
        }
    }
}
