using System;

public sealed class ResourceCounter
{
    public double Total { get; private set; } = 0;
    public event Action<double> OnChanged;

    // 안전가드: 음수 무시, NaN 방지, Infinity 캡쳐
    public void Add(long amount)
    {
        if (amount <= 0) return;

        double next = Total + amount;      // long→double은 안전(정밀도만 손실 가능)
        if (double.IsNaN(next)) return;

        if (double.IsInfinity(next))
        {
            Total = double.MaxValue;       // 소프트 캡
            OnChanged?.Invoke(Total);
            return;
        }

        Total = next;
        OnChanged?.Invoke(Total);
    }
}
