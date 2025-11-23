public class TutorialManager : MonoBehaviour
{
    public void OnTutorialFinished()
    {
        // 1. 현재 게임 데이터의 플래그를 true로 변경합니다.
        GameInitializer.CurrentGameData.hasCompletedTutorial = true;

        // 2. [중요] 이 상태를 즉시 저장합니다.
        //    (플레이어가 튜토리얼 직후 게임을 종료해도 다시 보지 않도록)
        _ = GameInitializer.SaveService.SaveGame(
            GameInitializer.CurrentSlotId,
            GameInitializer.CurrentGameData
        );

        // 3. 메인 게임으로 보냅니다.
        // 예: SceneManager.LoadScene("MainGameScene");
    }
}