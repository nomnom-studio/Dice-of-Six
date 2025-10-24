// using�� �߰��մϴ�.
using System.IO;
using System.Threading.Tasks; // �񵿱�(async) ���
using UnityEngine;
using Steamworks; // Steamworks.NET

public class SaveService
{
    private readonly SaveValidator _validator;
    private readonly SaveSchemaConverter _converter;

    // Steam Cloud�� ������ ���� ���
    private readonly string _saveDirectory;
    private const string SAVE_FILE_NAME = "save_slot_{0}.json";
    private const string BACKUP_FILE_NAME = "save_slot_{0}.json.bak";
    private const string TEMP_FILE_NAME = "save_slot_{0}.json.tmp";

    public SaveService(SaveValidator validator, SaveSchemaConverter converter)
    {
        _validator = validator;
        _converter = converter;

        // ���� ���� ��ġ: .../AppData/LocalLow/NOMNOM Studio/Dice of Six/
        _saveDirectory = Path.Combine(Application.persistentDataPath, "NOMNOM_Studio", "DiceOfSix");

        // ������ ������ ����
        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }
    }

    // ���Ժ� ���� ��� ��ȯ
    private string GetFilePath(int slotId) => Path.Combine(_saveDirectory, string.Format(SAVE_FILE_NAME, slotId));
    private string GetBackupFilePath(int slotId) => Path.Combine(_saveDirectory, string.Format(BACKUP_FILE_NAME, slotId));
    private string GetTempFilePath(int slotId) => Path.Combine(_saveDirectory, string.Format(TEMP_FILE_NAME, slotId));

    // --- ���� �ε� ---
    public GameData LoadGame(int slotId)
    {
        string filePath = GetFilePath(slotId);
        string backupPath = GetBackupFilePath(slotId);
        string encryptedData = null;

        // 1. ���� ���� �ε� �õ�
        if (File.Exists(filePath))
        {
            try { encryptedData = File.ReadAllText(filePath); }
            catch (Exception e) { Debug.LogWarning($"���� ���̺�({slotId}) �ε� ����: {e.Message}"); }
        }

        // 2. ���� ���� ���� �� ��� ���� �ε� �õ�
        if (string.IsNullOrEmpty(encryptedData) && File.Exists(backupPath))
        {
            try { encryptedData = File.ReadAllText(backupPath); }
            catch (Exception e) { Debug.LogWarning($"��� ���̺�({slotId}) �ε� ����: {e.Message}"); }
        }

        // 3. �ε� ���� ���� (�� ����)
        if (string.IsNullOrEmpty(encryptedData))
        {
            Debug.Log($"���̺� ����({slotId}) ����. �� ���� ����.");
            return new GameData(); // �� ������ ��ȯ
        }

        try
        {
            // 4. ��ȣȭ
            string wrapperJson = _validator.Decrypt(encryptedData);
            SaveFileWrapper wrapper = JsonUtility.FromJson<SaveFileWrapper>(wrapperJson);

            // 5. �����(Steam ID) ����
            if (!_validator.ValidateUser(wrapper.steamId))
            {
                Debug.LogError($"Steam ID ����ġ!({slotId}). �ٸ� ������ ���̺� �����Դϴ�. �ε� ���.");
                return new GameData(); // �� ������ ��ȯ
            }

            // 6. ���Ἲ(üũ��) ����
            if (!_validator.ValidateChecksum(wrapper.gameDataJson, wrapper.checksum))
            {
                Debug.LogError($"üũ�� ����ġ!({slotId}). ���̺� ������ �����Ǿ����ϴ�. �ε� ���.");
                return new GameData(); // �� ������ ��ȯ
            }

            // 7. ���� ������ ���� �� ���̱׷��̼�
            GameData gameData = JsonUtility.FromJson<GameData>(wrapper.gameDataJson);
            return _converter.Migrate(gameData); // �ֽ� �������� ��ȯ
        }
        catch (Exception e)
        {
            Debug.LogError($"���̺� ����({slotId}) ó�� �� �ɰ��� ����. �� ���� ����. {e.Message}");
            return new GameData(); // ���� �� �� ������ ��ȯ
        }
    }

    // --- ���� ���� (�񵿱�) ---
    public async Task SaveGame(int slotId, GameData data)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam�� �ʱ�ȭ���� �ʾ� ������ �ǳʶݴϴ�.");
            return;
        }

        string tempPath = GetTempFilePath(slotId);
        string filePath = GetFilePath(slotId);
        string backupPath = GetBackupFilePath(slotId);

        try
        {
            // 1. ������ �غ� (���� ������)
            data.saveVersion = SaveSchemaConverter.CURRENT_VERSION;
            string gameDataJson = JsonUtility.ToJson(data);

            string checksum = _validator.GenerateChecksum(gameDataJson);
            ulong steamId = SteamUser.GetSteamID().m_SteamID;

            SaveFileWrapper wrapper = new SaveFileWrapper(gameDataJson, checksum, steamId);
            string wrapperJson = JsonUtility.ToJson(wrapper);
            string encryptedData = _validator.Encrypt(wrapperJson);

            // 2. ���� ���� (���� ������)
            await Task.Run(() =>
            {
                // ������ ���� (Atomic Save)
                // 2-1. �ӽ� ���Ͽ� ����
                File.WriteAllText(tempPath, encryptedData);

                // 2-2. ���� ��� ����
                if (File.Exists(backupPath)) File.Delete(backupPath);

                // 2-3. ���� ������ ������� �̸� ����
                if (File.Exists(filePath)) File.Move(filePath, backupPath);

                // 2-4. �ӽ� ������ ���� ���Ϸ� �̸� ����
                File.Move(tempPath, filePath);
            });

            Debug.Log($"���� ���� {slotId} ���� �Ϸ�.");
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� {slotId} ���� ����: {e.Message}");
        }
    }

    // (����) Import/Export ����� ���⿡ �߰� ����...
}