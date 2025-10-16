using UnityEngine;

[CreateAssetMenu(menuName = "Config/Bootstrap Config", fileName = "BootstrapConfig")]
public sealed class BootstrapConfig : ScriptableObject
{
    [Header("Tick")]
    [Tooltip("한 번의 Tick 간격(초). 최소 0.1")]
    [Min(0.1f)] public float tickIntervalSeconds = 5f;

    [Header("Dice")]
    [Tooltip("시작 주사위 개수(1~6)")]
    [Range(1, 6)] public int startingDiceCount = 1;

    [Tooltip("주사위 눈 최소값")]
    public int minFace = 1;

    [Tooltip("주사위 눈 최대값(최소값 이상)")]
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
