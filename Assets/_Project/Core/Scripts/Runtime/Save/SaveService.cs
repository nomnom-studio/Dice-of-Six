// using을 추가합니다.
using System.IO;
using System.Threading.Tasks; // 비동기(async) 사용
using UnityEngine;
using Steamworks; // Steamworks.NET

public class SaveService
{
    private readonly SaveValidator _validator;
    private readonly SaveSchemaConverter _converter;

    // Steam Cloud와 연동할 저장 경로
    private readonly string _saveDirectory;
    private const string SAVE_FILE_NAME = "save_slot_{0}.json";
    private const string BACKUP_FILE_NAME = "save_slot_{0}.json.bak";
    private const string TEMP_FILE_NAME = "save_slot_{0}.json.tmp";

    public SaveService(SaveValidator validator, SaveSchemaConverter converter)
    {
        _validator = validator;
        _converter = converter;

        // 최종 저장 위치: .../AppData/LocalLow/NOMNOM Studio/Dice of Six/
        _saveDirectory = Path.Combine(Application.persistentDataPath, "NOMNOM_Studio", "DiceOfSix");

        // 폴더가 없으면 생성
        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }
    }

    // 슬롯별 파일 경로 반환
    private string GetFilePath(int slotId) => Path.Combine(_saveDirectory, string.Format(SAVE_FILE_NAME, slotId));
    private string GetBackupFilePath(int slotId) => Path.Combine(_saveDirectory, string.Format(BACKUP_FILE_NAME, slotId));
    private string GetTempFilePath(int slotId) => Path.Combine(_saveDirectory, string.Format(TEMP_FILE_NAME, slotId));

    // --- 게임 로드 ---
    public GameData LoadGame(int slotId)
    {
        string filePath = GetFilePath(slotId);
        string backupPath = GetBackupFilePath(slotId);
        string encryptedData = null;

        // 1. 메인 파일 로드 시도
        if (File.Exists(filePath))
        {
            try { encryptedData = File.ReadAllText(filePath); }
            catch (Exception e) { Debug.LogWarning($"메인 세이브({slotId}) 로드 실패: {e.Message}"); }
        }

        // 2. 메인 파일 실패 시 백업 파일 로드 시도
        if (string.IsNullOrEmpty(encryptedData) && File.Exists(backupPath))
        {
            try { encryptedData = File.ReadAllText(backupPath); }
            catch (Exception e) { Debug.LogWarning($"백업 세이브({slotId}) 로드 실패: {e.Message}"); }
        }

        // 3. 로드 완전 실패 (새 게임)
        if (string.IsNullOrEmpty(encryptedData))
        {
            Debug.Log($"세이브 파일({slotId}) 없음. 새 게임 시작.");
            return new GameData(); // 새 데이터 반환
        }

        try
        {
            // 4. 복호화
            string wrapperJson = _validator.Decrypt(encryptedData);
            SaveFileWrapper wrapper = JsonUtility.FromJson<SaveFileWrapper>(wrapperJson);

            // 5. 사용자(Steam ID) 검증
            if (!_validator.ValidateUser(wrapper.steamId))
            {
                Debug.LogError($"Steam ID 불일치!({slotId}). 다른 유저의 세이브 파일입니다. 로드 취소.");
                return new GameData(); // 새 데이터 반환
            }

            // 6. 무결성(체크섬) 검증
            if (!_validator.ValidateChecksum(wrapper.gameDataJson, wrapper.checksum))
            {
                Debug.LogError($"체크섬 불일치!({slotId}). 세이브 파일이 변조되었습니다. 로드 취소.");
                return new GameData(); // 새 데이터 반환
            }

            // 7. 실제 데이터 복원 및 마이그레이션
            GameData gameData = JsonUtility.FromJson<GameData>(wrapper.gameDataJson);
            return _converter.Migrate(gameData); // 최신 버전으로 변환
        }
        catch (Exception e)
        {
            Debug.LogError($"세이브 파일({slotId}) 처리 중 심각한 오류. 새 게임 시작. {e.Message}");
            return new GameData(); // 오류 시 새 데이터 반환
        }
    }

    // --- 게임 저장 (비동기) ---
    public async Task SaveGame(int slotId, GameData data)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam이 초기화되지 않아 저장을 건너뜁니다.");
            return;
        }

        string tempPath = GetTempFilePath(slotId);
        string filePath = GetFilePath(slotId);
        string backupPath = GetBackupFilePath(slotId);

        try
        {
            // 1. 데이터 준비 (메인 스레드)
            data.saveVersion = SaveSchemaConverter.CURRENT_VERSION;
            string gameDataJson = JsonUtility.ToJson(data);

            string checksum = _validator.GenerateChecksum(gameDataJson);
            ulong steamId = SteamUser.GetSteamID().m_SteamID;

            SaveFileWrapper wrapper = new SaveFileWrapper(gameDataJson, checksum, steamId);
            string wrapperJson = JsonUtility.ToJson(wrapper);
            string encryptedData = _validator.Encrypt(wrapperJson);

            // 2. 파일 쓰기 (별도 스레드)
            await Task.Run(() =>
            {
                // 원자적 저장 (Atomic Save)
                // 2-1. 임시 파일에 쓰기
                File.WriteAllText(tempPath, encryptedData);

                // 2-2. 기존 백업 삭제
                if (File.Exists(backupPath)) File.Delete(backupPath);

                // 2-3. 현재 파일을 백업으로 이름 변경
                if (File.Exists(filePath)) File.Move(filePath, backupPath);

                // 2-4. 임시 파일을 현재 파일로 이름 변경
                File.Move(tempPath, filePath);
            });

            Debug.Log($"게임 슬롯 {slotId} 저장 완료.");
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 슬롯 {slotId} 저장 실패: {e.Message}");
        }
    }

    // (선택) Import/Export 기능은 여기에 추가 구현...
}