using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class GameInitializer : MonoBehaviour
{
    private static GameInitializer _instance; // �ߺ� ����

    [Header("Assign in Inspector")]
    [SerializeField] private GameTickLoop tickLoop;      // ���� GameTickLoop ������Ʈ �巡��
    [SerializeField] private BootstrapConfig config;     // Assets/_Project/DesignData/SO/Configs/BootstrapConfig.asset
    [SerializeField] private bool applyOnAwake = true;   // ���� �� 1ȸ ����

    void Awake()
    {
        // �ߺ� ���� (Boot/Main �� ���� �� ���)
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);

        // �ٽ� ���� ���ε�
        ServiceContainer.Bind(new ResourceCounter());
        ServiceContainer.Bind(new DiceManager());

        // Tick ���� ���ε� (Inspector���� �ݵ�� �Ҵ�)
        if (!tickLoop)
        {
            Debug.LogError("[GameInitializer] tickLoop is not assigned in Inspector.");
            return;
        }
        ServiceContainer.Bind(tickLoop);

        // �ʱⰪ ���� (���� �ҽ�: BootstrapConfig)
        if (applyOnAwake)
        {
            if (config != null)
            {
                ApplyConfig(config);
            }
            else
            {
                Debug.LogWarning("[GameInitializer] BootstrapConfig is null. Using defaults.");
            }
        }
    }

    private void ApplyConfig(BootstrapConfig cfg)
    {
        var dice = ServiceContainer.Get<DiceManager>();
        if (dice != null)
        {
            dice.SetDiceCount(cfg.startingDiceCount);
            dice.SetFaceRange(cfg.minFace, cfg.maxFace);
        }

        tickLoop.IntervalSeconds = cfg.tickIntervalSeconds;
        // Debug.Log($"[Init] Tick interval applied: {tickLoop.IntervalSeconds}s, Dice: {cfg.startingDiceCount} ({cfg.minFace}-{cfg.maxFace})");
    }
}
