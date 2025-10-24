using UnityEngine;

public class SaveSchemaConverter // 또는 SaveMigration
{
    // 현재 게임의 최신 세이브 버전
    public const int CURRENT_VERSION = 1;

    // 마이그레이션 실행
    public GameData Migrate(GameData data)
    {
        // 저장된 버전이 최신 버전보다 낮으면, 최신이 될 때까지 순차적으로 업그레이드
        while (data.saveVersion < CURRENT_VERSION)
        {
            switch (data.saveVersion)
            {
                // 예시: 1버전 -> 2버전 업그레이드 로직
                // case 1:
                //     data = MigrateV1toV2(data);
                //     break;

                // 예시: 2버전 -> 3버전 업그레이드 로직
                // case 2:
                //     data = MigrateV2toV3(data);
                //     break;

                default:
                    Debug.LogError($"알 수 없는 세이브 버전({data.saveVersion}) 마이그레이션 실패.");
                    return null; // 변환 실패
            }
        }
        return data;
    }

    // // 예시: 1 -> 2 버전 변환 함수
    // private GameData MigrateV1toV2(GameData v1Data)
    // {
    //     // v2에서 새로 생긴 'gem'이라는 재화를 추가
    //     // v1Data를 기반으로 v2Data 객체를 생성하고 값을 이전...
    //     // v2Data.gem = 0; // 기본값 할당
    //     // v2Data.saveVersion = 2;
    //     // return v2Data;
    //     
    //     // 임시로 버전만 올림
    //     v1Data.saveVersion = 2;
    //     Debug.Log("세이브 파일을 2버전으로 마이그레이션했습니다.");
    //     return v1Data;
    // }
}