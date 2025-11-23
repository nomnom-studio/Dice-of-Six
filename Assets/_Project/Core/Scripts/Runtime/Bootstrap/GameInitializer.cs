public class GameInitializer : MonoBehaviour // 또는 GameManager
{
    public static SaveService SaveService { get; private set; }
    public static GameData CurrentGameData { get; private set; }
    public static int CurrentSlotId = 0;

    void Awake()
    {
        // 1. 서비스들을 생성합니다.
        SaveValidator validator = new SaveValidator();
        SaveSchemaConverter converter = new SaveSchemaConverter();
        SaveService = new SaveService(validator, converter);

        // 2. 0번 슬롯의 데이터를 로드합니다.
        //    - 세이브 파일이 없으면, SaveService가 알아서 hasCompletedTutorial=false인
        //      'new GameData()'를 반환해 줄 것입니다.
        CurrentGameData = SaveService.LoadGame(CurrentSlotId);

        // 3. [핵심] 튜토리얼 플래그를 검사하여 로직을 분기합니다.
        if (!CurrentGameData.hasCompletedTutorial)
        {
            // --- A. 새 사용자 또는 튜토리얼 미완료자 ---
            InitializeNewGameValues(); // (선택) 새 유저용 특별 지급 아이템 등
            StartTutorial();           // 튜토리얼 씬 또는 UI를 활성화
        }
        else
        {
            // --- B. 기존 사용자 ---
            StartGame(); // 메인 게임 씬으로 바로 진입
        }
    }

    private void InitializeNewGameValues()
    {
        // GameData() 생성자만으로는 부족한,
        // '새 게임 시작 시'에만 특별히 줘야 하는 동적 값이 있다면 여기서 설정합니다.
        // 예: CurrentGameData.currencies["gold"] = 100; // 웰컴 골드
        Debug.Log("새 사용자를 위한 게임을 초기화합니다.");
    }

    private void StartTutorial()
    {
        Debug.Log("튜토리얼을 시작합니다.");
        // 예: SceneManager.LoadScene("TutorialScene");
    }

    private void StartGame()
    {
        Debug.Log("기존 사용자의 메인 게임을 시작합니다.");
        // 예: SceneManager.LoadScene("MainGameScene");
    }
}