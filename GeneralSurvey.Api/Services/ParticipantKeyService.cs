using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public class ParticipantKeyService : IParticipantKeyService
{
    private readonly string _filePath;
    private readonly List<ParticipantKey> _keys;

    public ParticipantKeyService(string filePath)
    {
        _filePath = filePath;

        if (!File.Exists(_filePath))
        {
            _keys = [];
            SaveKeys();
            return;
        }

        var content = File.ReadAllText(_filePath);

        _keys = string.IsNullOrWhiteSpace(content)
            ? []
            : JsonSerializer.Deserialize<List<ParticipantKey>>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [];
    }

    public bool IsValid(string key)
    {
        var keyHash = HashKey(key);

        return _keys.Any(
            participantKey =>
                participantKey.KeyHash == keyHash &&
                !participantKey.Used);
    }

    public bool Consume(string key)
    {
        var keyHash = HashKey(key);

        var participantKey = _keys.FirstOrDefault(
            participantKey =>
                participantKey.KeyHash == keyHash &&
                !participantKey.Used);

        if (participantKey is null)
        {
            return false;
        }

        participantKey.Used = true;

        SaveKeys();

        return true;
    }

    private static string HashKey(string key)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(key));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private void SaveKeys()
    {
        var json = JsonSerializer.Serialize(
            _keys,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        File.WriteAllText(_filePath, json);
    }
}