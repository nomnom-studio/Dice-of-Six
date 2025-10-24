using System;

[Serializable]
public class SaveFileWrapper
{
    // 암호화된 실제 게임 데이터 (JSON 문자열)
    public string gameDataJson;

    // 무결성 검증용 체크섬
    public string checksum;

    // 사용자 검증용 Steam ID
    public ulong steamId;

    public SaveFileWrapper(string json, string chksum, ulong id)
    {
        gameDataJson = json;
        checksum = chksum;
        steamId = id;
    }
}