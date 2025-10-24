using System;

[Serializable]
public class SaveFileWrapper
{
    // ��ȣȭ�� ���� ���� ������ (JSON ���ڿ�)
    public string gameDataJson;

    // ���Ἲ ������ üũ��
    public string checksum;

    // ����� ������ Steam ID
    public ulong steamId;

    public SaveFileWrapper(string json, string chksum, ulong id)
    {
        gameDataJson = json;
        checksum = chksum;
        steamId = id;
    }
}