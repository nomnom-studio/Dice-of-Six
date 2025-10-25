using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; // ���� using
using UnityEngine.ResourceManagement.AsyncOperations; // ���� using
using UnityEngine.ResourceManagement.ResourceLocations; // ���� using
using System.Threading.Tasks; // [�ű� �߰�] OnApplicationQuit���� �񵿱� ������ ��ٸ��� ���� �ʿ�

/// <summary>
/// ������ ��Ʈ��Ʈ������ ����ϴ� �ٽ� �ʱ�ȭ ��ũ��Ʈ�Դϴ�.
/// ��� ���񽺸� ����ϰ�, ���̺� �����͸� �ε��ϸ�, ���� ���� �����մϴ�.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private BootstrapConfig _bootstrapConfig;

    private List<AsyncOperationHandle> _handles;
    private ServiceContainer _serviceContainer;

    // --- [�ű� �߰�] ���̺� ������ �� ���� ���� ���� ---
    // �ٸ� ��� ��ũ��Ʈ(��: UI, ��ȭ ��ư)���� �� static �Ӽ��� ����
    // ���� ���� �����Ϳ� ���� ��ɿ� ������ �� �ֽ��ϴ�.

    /// <summary>
    /// ������ ��� ����/�ε� ����� ����ϴ� �����Դϴ�.
    /// </summary>
    public static SaveService SaveService { get; private set; }

    /// <summary>
    /// ���� �÷��̾��� ��� ���� ��Ȳ �������Դϴ�. (��ȭ, ���׷��̵� ��)
    /// </summary>
    public static GameData CurrentGameData { get; private set; }

    /// <summary>
    /// ���� �÷��� ���� ���̺� ���� ��ȣ�Դϴ�. (�⺻ 0��)
    /// </summary>
    public static int CurrentSlotId { get; private set; } = 0;

    // --- [�ű� �߰�] �ڵ� ���� �� ���� �÷��� ---
    private float _autoSaveTimer = 0f; // �ڵ� ���� Ÿ�̸�
    private const float AUTO_SAVE_INTERVAL = 300f; // �ڵ� ���� �ֱ� (��: 300�� = 5��)
    private bool _isInitialized = false; // ��� �ʱ�ȭ�� �Ϸ�Ǿ����� ����
    private bool _isQuitting = false; // ���� ������ ����Ǵ� ������ ����

    private void Awake()
    {
        _handles = new List<AsyncOperationHandle>();
        _serviceContainer = new ServiceContainer();

        // ��Ʈ��Ʈ���۴� ������ ����Ǵ� ���� ���� �ı��Ǹ� �� �˴ϴ�.
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // �񵿱� �ʱ�ȭ ����
        InitializeAsync().Forget();
    }

    /// <summary>
    /// ���� ���� �� �ʿ��� ��� �񵿱� ������ ���������� �����մϴ�.
    /// </summary>
    private async UniTaskVoid InitializeAsync()
    {
        // 1. ��巹���� �ʱ�ȭ (���� ����)
        await InitializeAddressablesAsync();

        // 2. ��� �ٽ� ���� ��� (SaveService ����)
        RegisterServices();

        // 3. [�ű�] ���� ��� ����, ��ũ���� ���̺� ���� �ε�
        LoadSaveData();

        // 4. [�ű�] �ε��� �����͸� ������� "�� ����" / "���� ����" �б�
        if (!CurrentGameData.hasCompletedTutorial)
        {
            // --- A. �� ����� (�Ǵ� Ʃ�丮���� ������ ���� �����) ---
            Debug.Log("[GameInitializer] �� ����ڸ� �����߽��ϴ�. Ʃ�丮���� �غ��մϴ�.");
            InitializeNewGameValues(); // �� �������Ը� Ư���� ������ ������ ���� �ִٸ� ���⼭ ó��
        }
        else
        {
            // --- B. ���� ����� ---
            Debug.Log("[GameInitializer] ���� ����ڸ� �����߽��ϴ�. ���� ������ �����մϴ�.");
            // (Ȯ��) ���⿡ �������� ���� ��� ������ ���� �� �ֽ��ϴ�.
            // ��: _serviceContainer.Get<OfflineProgressService>().CalculateOfflineProgress(CurrentGameData);
        }

        // 5. ������ ������ �ε� (���� ����)
        // Ʃ�丮�� �ʿ� ���ο� ������� ���� �����ʹ� �ε��ؾ� �մϴ�.
        LoadDesignData();

        // 6. ���� ���� �� �ε� (���� ����)
        // ���� �ε�� ��, �� ������ 'UIManager' ���� ��ũ��Ʈ��
        // GameInitializer.CurrentGameData.hasCompletedTutorial ���� �ٽ� Ȯ���ؼ�
        // Ʃ�丮�� UI�� �����, ���� UI�� ����� ���� �����մϴ�.
        await LoadGameSceneAsync();

        // 7. [�ű�] ��� �ʱ�ȭ �Ϸ�!
        Debug.Log("[GameInitializer] ��� �ʱ�ȭ�� �Ϸ�Ǿ����ϴ�.");
        _isInitialized = true; // �������� �ڵ� ������ Ȱ��ȭ�մϴ�.
    }

    private async UniTask InitializeAddressablesAsync()
    {
        var handle = Addressables.InitializeAsync();
        _handles.Add(handle);
        await handle.Task;
    }

    /// <summary>
    /// ���ӿ� �ʿ��� ��� ���񽺸� �����ϰ� ServiceContainer�� ����մϴ�.
    /// </summary>
    private void RegisterServices()
    {
        // --- ���� ���� ��� ---
        _serviceContainer.Register<LogService>(new LogService());
        _serviceContainer.Register<EventBus>(new EventBus());
        _serviceContainer.Register<GameInputService>(new GameInputService());
        _serviceContainer.Register<GameTickLoop>(new GameTickLoop());
        _serviceContainer.Register<ScreenRouter>(new ScreenRouter());
        _serviceContainer.Register<DiceManager>(new DiceManager());
        _serviceContainer.Register<NumericAdapter>(new NumericAdapter());
        _serviceContainer.Register<OfflineProgressService>(new OfflineProgressService());
        _serviceContainer.Register<ResourceCounter>(new ResourceCounter());

        // --- [�ű� �߰�] ���̺� �ý��� ���� ��� ---
        // 1. ����/���� ��� ����
        SaveValidator validator = new SaveValidator();
        // 2. ���� ��ȯ ��� ����
        SaveSchemaConverter converter = new SaveSchemaConverter();
        // 3. �� ����� �����Ͽ� ���� ���� ����
        SaveService = new SaveService(validator, converter);

        // 4. ServiceContainer���� ��� (�ٸ� ���񽺵��� DI�� ���� ������ �� �ֵ���)
        _serviceContainer.Register<SaveService>(SaveService);

        Debug.Log("[GameInitializer] SaveService�� ���������� ��ϵǾ����ϴ�.");

        // --- ���� ���� ��� ---
        _serviceContainer.Register<TimeService>(new TimeService());
    }

    /// <summary>
    /// [�ű�] SaveService�� ����� ���̺� �����͸� ���������� �ε��մϴ�.
    /// </summary>
    private void LoadSaveData()
    {
        Debug.Log($"[GameInitializer] ���̺� ���� {CurrentSlotId} �ε带 �õ��մϴ�...");

        // LoadGame�� ������ ���ų�, �����Ǿ��ų�, �ٸ� ������ ������ ���
        // �˾Ƽ� new GameData() (hasCompletedTutorial = false)�� ��ȯ�մϴ�.
        CurrentGameData = SaveService.LoadGame(CurrentSlotId);

        Debug.Log("[GameInitializer] ������ �ε� �Ϸ�.");
    }

    /// <summary>
    /// [�ű�] �� ������ �����ϴ� ������ ���� �ʱⰪ ����
    /// </summary>
    private void InitializeNewGameValues()
    {
        // GameData() �����ڿ��� ��κ��� ��(��ȭ 0, Ʃ�丮�� false)�� �̹� �����Ǿ����ϴ�.
        // ���� '�� ���� ȯ�� ����' (��: ��� 100�� ����) ���� ���� �ְ� �ʹٸ�
        // �� �Լ����� CurrentGameData�� ���� �����ϸ� �˴ϴ�.

        // ����: CurrentGameData.currencies["gold"] = 100;
        // ����: CurrentGameData.upgradeLevels["dice_power"] = 1;
        Debug.Log("[GameInitializer] �� ����� �⺻�� ���� �Ϸ�.");
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

    // --- [�ű� �߰�] �ڵ� ���� ���� ---
    private void Update()
    {
        // �ʱ�ȭ�� �Ϸ�Ǳ� ���̰ų�, ������ ����Ǵ� ���̸� �ƹ��͵� ���� �ʽ��ϴ�.
        if (!_isInitialized || _isQuitting) return;

        _autoSaveTimer += Time.deltaTime;

        // �ڵ� ���� �ֱⰡ �Ǹ�
        if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            _autoSaveTimer = 0f; // Ÿ�̸� �ʱ�ȭ
            Debug.Log($"[GameInitializer] �ڵ� ���� ����... (�ֱ�: {AUTO_SAVE_INTERVAL}��)");

            // SaveGame�� �񵿱�(Task) �Լ��Դϴ�.
            // Update���� ����� ��ٸ� �ʿ䰡 �����Ƿ�(await ����), 
            // _ (discard) ó���� ���� ��׶��忡�� ������ �����ϵ��� �մϴ�.
            _ = SaveService.SaveGame(CurrentSlotId, CurrentGameData);
        }
    }

    // --- [�ű� �߰�] ���ø����̼� ���� �� ���� ���� ---
    private async void OnApplicationQuit()
    {
        _isQuitting = true; // Update()�� �ڵ� ������ �ߺ� ������� �ʵ��� �����ϴ�.

        // (������ġ) Ȥ�� �ʱ�ȭ�� �����⵵ ���� ������ ������ ���
        if (SaveService == null || CurrentGameData == null)
        {
            Debug.LogWarning("[GameInitializer] �ʱ�ȭ �� ����... ������ �ǳʶݴϴ�.");
            return;
        }

        Debug.Log("[GameInitializer] ���ø����̼� ���� ����. ���� ���� �õ�...");

        // ������ ������ ����Ǳ� ����, ������ �Ϸ�Ǵ� ���� �����ϱ� ����
        // ���⼭�� await�� ����� ������ ���� ������ ��ٸ��ϴ�.
        await SaveService.SaveGame(CurrentSlotId, CurrentGameData);

        Debug.Log("[GameInitializer] ���� ���� �Ϸ�. ������ �����մϴ�.");
    }

    private void OnDestroy()
    {
        // ���� �ڵ� ���� ����
        foreach (var handle in _handles)
        {
            Addressables.Release(handle);
        }
    }
}