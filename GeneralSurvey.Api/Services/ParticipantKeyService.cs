using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public class ParticipantKeyService : IParticipantKeyService
{
    private readonly string _filePath;
    private readonly List<ParticipantKey> _keys;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public ParticipantKeyService(string filePath)
    {
        ValidatePath(filePath);

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
    private static void ValidatePath(string filePath)
    {
        if (filePath.Contains('\0', StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Le chemin contient des caractères invalides (null byte).",
                nameof(filePath));
        }

        var normalised = filePath.Replace('\\', '/');
        if (normalised.Contains("../", StringComparison.Ordinal) ||
            normalised.Contains("/..", StringComparison.Ordinal) ||
            normalised == "..")
        {
            throw new UnauthorizedAccessException(
                "Tentative de traversée de répertoire détectée dans le chemin.");
        }
    }

    public bool IsValid(string key)
    {
        var keyHash = HashKey(key);

        return _keys.Any(
            participantKey =>
                participantKey.KeyHash == keyHash &&
                !participantKey.Used);
    }

    public async Task<bool> ConsumeAsync(string key)
    {
        var keyHash = HashKey(key);
        var semaphore = _locks.GetOrAdd(keyHash, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var participantKey = _keys.FirstOrDefault(
                participantKey =>
                    participantKey.KeyHash == keyHash &&
                    !participantKey.Used);

            if (participantKey is null)
            {
                return false;
            }

            participantKey.Used = true;
            await SaveKeysAsync();
            return true;
        }
        finally
        {
            semaphore.Release();
        }
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
    private async Task SaveKeysAsync()
    {
        var json = JsonSerializer.Serialize(
            _keys,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await File.WriteAllTextAsync(_filePath, json);
    }
}