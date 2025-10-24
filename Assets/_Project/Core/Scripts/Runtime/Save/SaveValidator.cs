// using을 추가합니다.
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Steamworks; // Steamworks.NET 임포트

public class SaveValidator
{
    // 중요: 이 키들은 NOMNOM Studio에서 고유한 값으로 변경해야 합니다.
    // 32바이트 (256비트) 키
    private readonly byte[] _aesKey = Encoding.UTF8.GetBytes("NOMNOMStudioDiceOfSixSecretKey123");
    // 16바이트 (128비트) IV
    private readonly byte[] _aesIV = Encoding.UTF8.GetBytes("NOMNOMStudioIV1234");

    // 데이터 암호화
    public string Encrypt(string text)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _aesKey;
            aesAlg.IV = _aesIV;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }
                }
                return System.Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    // 데이터 복호화
    public string Decrypt(string cipherText)
    {
        byte[] buffer = System.Convert.FromBase64String(cipherText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _aesKey;
            aesAlg.IV = _aesIV;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(buffer))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    // 체크섬 생성 (SHA256)
    public string GenerateChecksum(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2")); // 16진수로 변환
            }
            return builder.ToString();
        }
    }

    // 체크섬 검증
    public bool ValidateChecksum(string jsonData, string expectedChecksum)
    {
        string newChecksum = GenerateChecksum(jsonData);
        return newChecksum == expectedChecksum;
    }

    // 사용자 검증 (다른 유저 세이브 차단)
    public bool ValidateUser(ulong savedSteamId)
    {
        if (!SteamManager.Initialized)
        {
            UnityEngine.Debug.LogError("Steam이 초기화되지 않았습니다. 사용자 검증 실패.");
            return false; // Steam이 실행 안되면 로드 실패
        }

        ulong currentUserId = SteamUser.GetSteamID().m_SteamID;
        return savedSteamId == currentUserId;
    }
}