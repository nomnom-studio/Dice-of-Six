using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class GameInitializer : MonoBehaviour
{
    private static GameInitializer _instance; // 중복 가드

    [Header("Assign in Inspector")]
    [SerializeField] private GameTickLoop tickLoop;      // 씬의 GameTickLoop 컴포넌트 드래그
    [SerializeField] private BootstrapConfig config;     // Assets/_Project/DesignData/SO/Configs/BootstrapConfig.asset
    [SerializeField] private bool applyOnAwake = true;   // 부팅 시 1회 적용

    void Awake()
    {
        // 중복 가드 (Boot/Main 등 복수 씬 대비)
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);

        // 핵심 서비스 바인딩
        ServiceContainer.Bind(new ResourceCounter());
        ServiceContainer.Bind(new DiceManager());

        // Tick 루프 바인딩 (Inspector에서 반드시 할당)
        if (!tickLoop)
        {
            Debug.LogError("[GameInitializer] tickLoop is not assigned in Inspector.");
            return;
        }
        ServiceContainer.Bind(tickLoop);

        // 초기값 적용 (단일 소스: BootstrapConfig)
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
