using System;

public sealed class ResourceCounter
{
    public double Total { get; private set; } = 0;
    public event Action<double> OnChanged;

    // ��������: ���� ����, NaN ����, Infinity ĸ��
    public void Add(long amount)
    {
        if (amount <= 0) return;

        double next = Total + amount;      // long��double�� ����(���е��� �ս� ����)
        if (double.IsNaN(next)) return;

        if (double.IsInfinity(next))
        {
            Total = double.MaxValue;       // ����Ʈ ĸ
            OnChanged?.Invoke(Total);
            return;
        }

        Total = next;
        OnChanged?.Invoke(Total);
    }
}
