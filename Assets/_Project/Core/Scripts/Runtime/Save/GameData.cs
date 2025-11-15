[Serializable]
public class GameData
{
    public int saveVersion;
    public Dictionary<string, double> currencies;
    public Dictionary<string, int> upgradeLevels;

    // [추가] 이 플래그가 핵심입니다.
    public bool hasCompletedTutorial;

    public GameData()
    {
        // 새 게임을 위한 기본값 설정
        saveVersion = SaveSchemaConverter.CURRENT_VERSION;
        currencies = new Dictionary<string, double>();
        upgradeLevels = new Dictionary<string, int>();

        // 새로 생성된 데이터는 항상 튜토리얼을 완료하지 않은 상태입니다.
        hasCompletedTutorial = false;
    }
}