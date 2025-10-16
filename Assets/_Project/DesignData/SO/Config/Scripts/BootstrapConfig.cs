using UnityEngine;

[CreateAssetMenu(menuName = "Config/Bootstrap Config", fileName = "BootstrapConfig")]
public sealed class BootstrapConfig : ScriptableObject
{
    [Header("Tick")]
    [Tooltip("�� ���� Tick ����(��). �ּ� 0.1")]
    [Min(0.1f)] public float tickIntervalSeconds = 5f;

    [Header("Dice")]
    [Tooltip("���� �ֻ��� ����(1~6)")]
    [Range(1, 6)] public int startingDiceCount = 1;

    [Tooltip("�ֻ��� �� �ּҰ�")]
    public int minFace = 1;

    [Tooltip("�ֻ��� �� �ִ밪(�ּҰ� �̻�)")]
    public int maxFace = 6;

    [Header("Limits (for design only)")]
    [Range(1, 6)] public int maxDiceCount = 6;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (tickIntervalSeconds < 0.1f) tickIntervalSeconds = 0.1f;
        if (startingDiceCount < 1) startingDiceCount = 1;
        if (startingDiceCount > maxDiceCount) startingDiceCount = maxDiceCount;
        if (minFace < 1) minFace = 1;
        if (maxFace < minFace) maxFace = minFace;
    }
#endif
}
