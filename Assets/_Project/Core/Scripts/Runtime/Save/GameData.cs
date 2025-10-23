using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int saveVersion;

    // [������ 1] ��ȭ:
    // "gold", "gem", "soul_essence" �� ��ȭ�� ���� ID(string)�� Key��,
    // �ش� ��ȭ�� ������(double)�� Value�� ���� Dictionary�Դϴ�.
    public Dictionary<string, double> currencies;

    // [������ 2] ���׷��̵� ����:
    // "dice_power_1", "gold_gain_mult" �� ���׷��̵� ID(string)�� Key��,
    // �ش� ���׷��̵��� ����(int)�� Value�� ���� Dictionary�Դϴ�.
    public Dictionary<string, int> upgradeLevels;

    // (����) �رݵ� ��� ���
    // public List<string> unlockedFeatures;

    public GameData()
    {
        // �� ������ ���� �⺻�� ����
        saveVersion = SaveSchemaConverter.CURRENT_VERSION;

        currencies = new Dictionary<string, double>();
        upgradeLevels = new Dictionary<string, int>();

        // (����) �� ���� ���� �� �⺻ ��ȭ�� ���⼭ �ʱ�ȭ�� �� �ֽ��ϴ�.
        // currencies["gold"] = 0; 
    }
}