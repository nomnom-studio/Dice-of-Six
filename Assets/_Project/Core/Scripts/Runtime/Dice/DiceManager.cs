using System;
using UnityEngine;

public sealed class DiceManager
{
    public int DiceCount { get; private set; } = 1; // 1~6
    public int MinFace { get; private set; } = 1;
    public int MaxFace { get; private set; } = 6;

    public event Action OnConfigChanged;

    private readonly System.Random _rng = new();

    public void SetDiceCount(int count)
    {
        int c = Mathf.Clamp(count, 1, 6);
        if (c == DiceCount) return;
        DiceCount = c; OnConfigChanged?.Invoke();
    }

    public void SetFaceRange(int min, int max)
    {
        min = Math.Max(1, min);
        max = Math.Max(min, max);
        if (min == MinFace && max == MaxFace) return;
        MinFace = min; MaxFace = max; OnConfigChanged?.Invoke();
    }

    /// <summary>
    /// ���Ҵ� ��. buffer ���̿� useCount(����� �ֻ��� ��)�� �����ϰ� ó��.
    /// �հ踦 long���� ��ȯ(�����÷ο� ����).
    /// </summary>
    public long RollAllNonAlloc(int[] buffer, int useCount)
    {
        if (buffer == null || buffer.Length == 0) return 0;
        int count = Math.Min(Mathf.Clamp(DiceCount, 1, 6), Math.Min(buffer.Length, Math.Max(1, useCount)));

        long sum = 0;
        for (int i = 0; i < count; i++)
        {
            int v = _rng.Next(MinFace, MaxFace + 1); // inclusive
            buffer[i] = v;
            sum += v;
        }
        return sum;
    }
}
