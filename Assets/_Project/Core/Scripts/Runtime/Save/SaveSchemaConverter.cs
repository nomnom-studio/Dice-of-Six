using UnityEngine;

public class SaveSchemaConverter // �Ǵ� SaveMigration
{
    // ���� ������ �ֽ� ���̺� ����
    public const int CURRENT_VERSION = 1;

    // ���̱׷��̼� ����
    public GameData Migrate(GameData data)
    {
        // ����� ������ �ֽ� �������� ������, �ֽ��� �� ������ ���������� ���׷��̵�
        while (data.saveVersion < CURRENT_VERSION)
        {
            switch (data.saveVersion)
            {
                // ����: 1���� -> 2���� ���׷��̵� ����
                // case 1:
                //     data = MigrateV1toV2(data);
                //     break;

                // ����: 2���� -> 3���� ���׷��̵� ����
                // case 2:
                //     data = MigrateV2toV3(data);
                //     break;

                default:
                    Debug.LogError($"�� �� ���� ���̺� ����({data.saveVersion}) ���̱׷��̼� ����.");
                    return null; // ��ȯ ����
            }
        }
        return data;
    }

    // // ����: 1 -> 2 ���� ��ȯ �Լ�
    // private GameData MigrateV1toV2(GameData v1Data)
    // {
    //     // v2���� ���� ���� 'gem'�̶�� ��ȭ�� �߰�
    //     // v1Data�� ������� v2Data ��ü�� �����ϰ� ���� ����...
    //     // v2Data.gem = 0; // �⺻�� �Ҵ�
    //     // v2Data.saveVersion = 2;
    //     // return v2Data;
    //     
    //     // �ӽ÷� ������ �ø�
    //     v1Data.saveVersion = 2;
    //     Debug.Log("���̺� ������ 2�������� ���̱׷��̼��߽��ϴ�.");
    //     return v1Data;
    // }
}