using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; // 기존 using
using UnityEngine.ResourceManagement.AsyncOperations; // 기존 using
using UnityEngine.ResourceManagement.ResourceLocations; // 기존 using
using System.Threading.Tasks; // [신규 추가] OnApplicationQuit에서 비동기 저장을 기다리기 위해 필요

/// <summary>
/// 게임의 부트스트래핑을 담당하는 핵심 초기화 스크립트입니다.
/// 모든 서비스를 등록하고, 세이브 데이터를 로드하며, 메인 씬을 시작합니다.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private BootstrapConfig _bootstrapConfig;

    private List<AsyncOperationHandle> _handles;
    private ServiceContainer _serviceContainer;

    // --- [신규 추가] 세이브 데이터 및 서비스 전역 접근 ---
    // 다른 모든 스크립트(예: UI, 강화 버튼)에서 이 static 속성을 통해
    // 현재 게임 데이터와 저장 기능에 접근할 수 있습니다.

    /// <summary>
    /// 게임의 모든 저장/로드 기능을 담당하는 서비스입니다.
    /// </summary>
    public static SaveService SaveService { get; private set; }

    /// <summary>
    /// 현재 플레이어의 모든 진행 상황 데이터입니다. (재화, 업그레이드 등)
    /// </summary>
    public static GameData CurrentGameData { get; private set; }

    /// <summary>
    /// 현재 플레이 중인 세이브 슬롯 번호입니다. (기본 0번)
    /// </summary>
    public static int CurrentSlotId { get; private set; } = 0;

    // --- [신규 추가] 자동 저장 및 상태 플래그 ---
    private float _autoSaveTimer = 0f; // 자동 저장 타이머
    private const float AUTO_SAVE_INTERVAL = 300f; // 자동 저장 주기 (예: 300초 = 5분)
    private bool _isInitialized = false; // 모든 초기화가 완료되었는지 여부
    private bool _isQuitting = false; // 현재 게임이 종료되는 중인지 여부

    private void Awake()
    {
        _handles = new List<AsyncOperationHandle>();
        _serviceContainer = new ServiceContainer();

        // 부트스트래퍼는 게임이 실행되는 동안 절대 파괴되면 안 됩니다.
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 비동기 초기화 시작
        InitializeAsync().Forget();
    }

    /// <summary>
    /// 게임 시작 시 필요한 모든 비동기 로직을 순차적으로 실행합니다.
    /// </summary>
    private async UniTaskVoid InitializeAsync()
    {
        // 1. 어드레서블 초기화 (기존 로직)
        await InitializeAddressablesAsync();

        // 2. 모든 핵심 서비스 등록 (SaveService 포함)
        RegisterServices();

        // 3. [신규] 서비스 등록 직후, 디스크에서 세이브 파일 로드
        LoadSaveData();

        // 4. [신규] 로드한 데이터를 기반으로 "새 유저" / "기존 유저" 분기
        if (!CurrentGameData.hasCompletedTutorial)
        {
            // --- A. 새 사용자 (또는 튜토리얼을 끝내지 못한 사용자) ---
            Debug.Log("[GameInitializer] 새 사용자를 감지했습니다. 튜토리얼을 준비합니다.");
            InitializeNewGameValues(); // 새 유저에게만 특별히 지급할 아이템 등이 있다면 여기서 처리
        }
        else
        {
            // --- B. 기존 사용자 ---
            Debug.Log("[GameInitializer] 기존 사용자를 감지했습니다. 메인 게임을 시작합니다.");
            // (확장) 여기에 오프라인 보상 계산 로직을 넣을 수 있습니다.
            // 예: _serviceContainer.Get<OfflineProgressService>().CalculateOfflineProgress(CurrentGameData);
        }

        // 5. 디자인 데이터 로드 (기존 로직)
        // 튜토리얼 필요 여부와 관계없이 게임 데이터는 로드해야 합니다.
        LoadDesignData();

        // 6. 메인 게임 씬 로드 (기존 로직)
        // 씬이 로드된 후, 씬 내부의 'UIManager' 같은 스크립트가
        // GameInitializer.CurrentGameData.hasCompletedTutorial 값을 다시 확인해서
        // 튜토리얼 UI를 띄울지, 메인 UI를 띄울지 최종 결정합니다.
        await LoadGameSceneAsync();

        // 7. [신규] 모든 초기화 완료!
        Debug.Log("[GameInitializer] 모든 초기화가 완료되었습니다.");
        _isInitialized = true; // 이제부터 자동 저장을 활성화합니다.
    }

    private async UniTask InitializeAddressablesAsync()
    {
        var handle = Addressables.InitializeAsync();
        _handles.Add(handle);
        await handle.Task;
    }

    /// <summary>
    /// 게임에 필요한 모든 서비스를 생성하고 ServiceContainer에 등록합니다.
    /// </summary>
    private void RegisterServices()
    {
        // --- 기존 서비스 등록 ---
        _serviceContainer.Register<LogService>(new LogService());
        _serviceContainer.Register<EventBus>(new EventBus());
        _serviceContainer.Register<GameInputService>(new GameInputService());
        _serviceContainer.Register<GameTickLoop>(new GameTickLoop());
        _serviceContainer.Register<ScreenRouter>(new ScreenRouter());
        _serviceContainer.Register<DiceManager>(new DiceManager());
        _serviceContainer.Register<NumericAdapter>(new NumericAdapter());
        _serviceContainer.Register<OfflineProgressService>(new OfflineProgressService());
        _serviceContainer.Register<ResourceCounter>(new ResourceCounter());

        // --- [신규 추가] 세이브 시스템 서비스 등록 ---
        // 1. 보안/검증 모듈 생성
        SaveValidator validator = new SaveValidator();
        // 2. 버전 변환 모듈 생성
        SaveSchemaConverter converter = new SaveSchemaConverter();
        // 3. 두 모듈을 주입하여 메인 서비스 생성
        SaveService = new SaveService(validator, converter);

        // 4. ServiceContainer에도 등록 (다른 서비스들이 DI를 통해 접근할 수 있도록)
        _serviceContainer.Register<SaveService>(SaveService);

        Debug.Log("[GameInitializer] SaveService가 성공적으로 등록되었습니다.");

        // --- 기존 서비스 등록 ---
        _serviceContainer.Register<TimeService>(new TimeService());
    }

    /// <summary>
    /// [신규] SaveService를 사용해 세이브 데이터를 동기적으로 로드합니다.
    /// </summary>
    private void LoadSaveData()
    {
        Debug.Log($"[GameInitializer] 세이브 슬롯 {CurrentSlotId} 로드를 시도합니다...");

        // LoadGame은 파일이 없거나, 변조되었거나, 다른 유저의 파일일 경우
        // 알아서 new GameData() (hasCompletedTutorial = false)를 반환합니다.
        CurrentGameData = SaveService.LoadGame(CurrentSlotId);

        Debug.Log("[GameInitializer] 데이터 로드 완료.");
    }

    /// <summary>
    /// [신규] 새 게임을 시작하는 유저를 위한 초기값 설정
    /// </summary>
    private void InitializeNewGameValues()
    {
        // GameData() 생성자에서 대부분의 값(재화 0, 튜토리얼 false)은 이미 설정되었습니다.
        // 만약 '새 유저 환영 보상' (예: 골드 100개 지급) 같은 것을 주고 싶다면
        // 이 함수에서 CurrentGameData의 값을 수정하면 됩니다.

        // 예시: CurrentGameData.currencies["gold"] = 100;
        // 예시: CurrentGameData.upgradeLevels["dice_power"] = 1;
        Debug.Log("[GameInitializer] 새 사용자 기본값 설정 완료.");
    }

    private void LoadDesignData()
    {
        foreach (var location in _bootstrapConfig.DesignDataLocations)
        {
            var handle = Addressables.LoadAssetAsync<ScriptableObject>(location);
            _handles.Add(handle);
        }
    }

    private async UniTask LoadGameSceneAsync()
    {
        var handle = Addressables.LoadSceneAsync(_bootstrapConfig.GameSceneLocation,
            UnityEngine.SceneManagement.LoadSceneMode.Single);
        _handles.Add(handle);
        await handle.Task;
    }

    // --- [신규 추가] 자동 저장 로직 ---
    private void Update()
    {
        // 초기화가 완료되기 전이거나, 게임이 종료되는 중이면 아무것도 하지 않습니다.
        if (!_isInitialized || _isQuitting) return;

        _autoSaveTimer += Time.deltaTime;

        // 자동 저장 주기가 되면
        if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            _autoSaveTimer = 0f; // 타이머 초기화
            Debug.Log($"[GameInitializer] 자동 저장 실행... (주기: {AUTO_SAVE_INTERVAL}초)");

            // SaveGame은 비동기(Task) 함수입니다.
            // Update에서 결과를 기다릴 필요가 없으므로(await 안함), 
            // _ (discard) 처리를 통해 백그라운드에서 조용히 저장하도록 합니다.
            _ = SaveService.SaveGame(CurrentSlotId, CurrentGameData);
        }
    }

    // --- [신규 추가] 애플리케이션 종료 시 최종 저장 ---
    private async void OnApplicationQuit()
    {
        _isQuitting = true; // Update()의 자동 저장이 중복 실행되지 않도록 막습니다.

        // (안전장치) 혹시 초기화가 끝나기도 전에 게임이 꺼지는 경우
        if (SaveService == null || CurrentGameData == null)
        {
            Debug.LogWarning("[GameInitializer] 초기화 전 종료... 저장을 건너뜁니다.");
            return;
        }

        Debug.Log("[GameInitializer] 애플리케이션 종료 감지. 최종 저장 시도...");

        // 게임이 완전히 종료되기 전에, 저장이 완료되는 것을 보장하기 위해
        // 여기서는 await를 사용해 저장이 끝날 때까지 기다립니다.
        await SaveService.SaveGame(CurrentSlotId, CurrentGameData);

        Debug.Log("[GameInitializer] 최종 저장 완료. 게임을 종료합니다.");
    }

    private void OnDestroy()
    {
        // 기존 핸들 해제 로직
        foreach (var handle in _handles)
        {
            Addressables.Release(handle);
        }
    }
}