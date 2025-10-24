// using�� �߰��մϴ�.
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Steamworks; // Steamworks.NET ����Ʈ

public class SaveValidator
{
    // �߿�: �� Ű���� NOMNOM Studio���� ������ ������ �����ؾ� �մϴ�.
    // 32����Ʈ (256��Ʈ) Ű
    private readonly byte[] _aesKey = Encoding.UTF8.GetBytes("NOMNOMStudioDiceOfSixSecretKey123");
    // 16����Ʈ (128��Ʈ) IV
    private readonly byte[] _aesIV = Encoding.UTF8.GetBytes("NOMNOMStudioIV1234");

    // ������ ��ȣȭ
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

    // ������ ��ȣȭ
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

    // üũ�� ���� (SHA256)
    public string GenerateChecksum(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2")); // 16������ ��ȯ
            }
            return builder.ToString();
        }
    }

    // üũ�� ����
    public bool ValidateChecksum(string jsonData, string expectedChecksum)
    {
        string newChecksum = GenerateChecksum(jsonData);
        return newChecksum == expectedChecksum;
    }

    // ����� ���� (�ٸ� ���� ���̺� ����)
    public bool ValidateUser(ulong savedSteamId)
    {
        if (!SteamManager.Initialized)
        {
            UnityEngine.Debug.LogError("Steam�� �ʱ�ȭ���� �ʾҽ��ϴ�. ����� ���� ����.");
            return false; // Steam�� ���� �ȵǸ� �ε� ����
        }

        ulong currentUserId = SteamUser.GetSteamID().m_SteamID;
        return savedSteamId == currentUserId;
    }
}