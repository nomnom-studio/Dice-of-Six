using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int saveVersion;

    // [수정안 1] 재화:
    // "gold", "gem", "soul_essence" 등 재화의 고유 ID(string)를 Key로,
    // 해당 재화의 보유량(double)을 Value로 갖는 Dictionary입니다.
    public Dictionary<string, double> currencies;

    // [수정안 2] 업그레이드 레벨:
    // "dice_power_1", "gold_gain_mult" 등 업그레이드 ID(string)를 Key로,
    // 해당 업그레이드의 레벨(int)을 Value로 갖는 Dictionary입니다.
    public Dictionary<string, int> upgradeLevels;

    // (예시) 해금된 기능 목록
    // public List<string> unlockedFeatures;

    public GameData()
    {
        // 새 게임을 위한 기본값 설정
        saveVersion = SaveSchemaConverter.CURRENT_VERSION;

        currencies = new Dictionary<string, double>();
        upgradeLevels = new Dictionary<string, int>();

        // (선택) 새 게임 시작 시 기본 재화를 여기서 초기화할 수 있습니다.
        // currencies["gold"] = 0; 
    }
}