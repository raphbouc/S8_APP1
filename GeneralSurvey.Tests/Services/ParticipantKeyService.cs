namespace GeneralSurvey.Tests.Services;

using GeneralSurvey.Api.Services;

public class ParticipantKeyServiceTests
{
    [Fact]
    public void Constructor_WithNonExistingFile_CreatesEmptyFile()
    {
        var filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");

        var service = new ParticipantKeyService(filePath);

        Assert.True(File.Exists(filePath));
        Assert.False(service.IsValid("8F4K92LM"));

        File.Delete(filePath);
    }

    [Fact]
    public void Constructor_WithEmptyFile_CreatesEmptyKeyList()
    {
        var filePath = Path.GetTempFileName();

        File.WriteAllText(filePath, string.Empty);

        var service = new ParticipantKeyService(filePath);

        Assert.False(service.IsValid("8F4K92LM"));

        File.Delete(filePath);
    }

    [Fact]
    public void Constructor_WithExistingKeys_LoadsKeys()
    {
        var filePath = Path.GetTempFileName();

        File.WriteAllText(
            filePath,
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              },
              {
                "keyHash": "ee4ae6c9037c726147b11c50045a601cbdc190976182b2bcf011b1def8fa8846",
                "used": true
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        Assert.True(service.IsValid("8F4K92LM"));
        Assert.False(service.IsValid("3D7P51XZ"));

        File.Delete(filePath);
    }

    [Fact]
    public void IsValid_WithExistingUnusedKey_ReturnsTrue()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        var result = service.IsValid("8F4K92LM");

        Assert.True(result);

        File.Delete(filePath);
    }

    [Fact]
    public void IsValid_WithNonExistingKey_ReturnsFalse()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        var result = service.IsValid("UNKNOWN");

        Assert.False(result);

        File.Delete(filePath);
    }

    [Fact]
    public void IsValid_WithUsedKey_ReturnsFalse()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": true
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        var result = service.IsValid("8F4K92LM");

        Assert.False(result);

        File.Delete(filePath);
    }

    [Fact]
    public async Task Consume_WithValidKey_ReturnsTrue()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        var result = await service.ConsumeAsync("8F4K92LM");

        Assert.True(result);
        Assert.False(service.IsValid("8F4K92LM"));

        File.Delete(filePath);
    }

    [Fact]
    public async Task Consume_WithNonExistingKey_ReturnsFalse()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        var result = await service.ConsumeAsync("UNKNOWN");

        Assert.False(result);

        File.Delete(filePath);
    }

    [Fact]
    public async Task Consume_WithAlreadyUsedKey_ReturnsFalse()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": true
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        var result = await service.ConsumeAsync("8F4K92LM");

        Assert.False(result);

        File.Delete(filePath);
    }

    [Fact]
    public async Task Consume_SavesKeyAsUsed()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        await service.ConsumeAsync("8F4K92LM");

        var content = await File.ReadAllTextAsync(filePath);

        Assert.Contains(
            "\"used\": true",
            content);

        File.Delete(filePath);
    }

    [Fact]
    public void Constructor_WithNullJson_CreatesEmptyKeyList()
    {
        var filePath = Path.GetTempFileName();

        File.WriteAllText(filePath, "null");

        var service = new ParticipantKeyService(filePath);

        Assert.False(service.IsValid("8F4K92LM"));

        File.Delete(filePath);
    }

    [Fact]
    public async Task Consume_DoesNotStorePlainTextKey()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        await service.ConsumeAsync("8F4K92LM");

        var content = await File.ReadAllTextAsync(filePath);

        Assert.DoesNotContain("8F4K92LM", content);
        Assert.Contains(
            "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
            content);
        Assert.Contains(
            "\"used\": true",
            content);

        File.Delete(filePath);
    }

    [Fact]
    public void IsValid_WithCorrectPlainTextKey_ReturnsTrue()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        Assert.True(service.IsValid("8F4K92LM"));

        File.Delete(filePath);
    }

    [Fact]
    public async Task ConsumeAsync_ConcurrentRequests_OnlyOneSucceeds()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        const int threads = 10;
        var results = new bool[threads];

        var tasks = Enumerable.Range(0, threads)
            .Select(i => Task.Run(async () =>
            {
                results[i] = await service.ConsumeAsync("8F4K92LM");
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(1, results.Count(r => r));
        Assert.Equal(threads - 1, results.Count(r => !r));

        File.Delete(filePath);
    }

    [Fact]
    public async Task ConsumeAsync_AfterSuccessfulConsume_KeyIsInvalid()
    {
        var filePath = CreateKeyFile(
            """
            [
              {
                "keyHash": "c7578d896a8c40e7485c3988fe533866d6bcb42b758665b7f6685a3fcce0c733",
                "used": false
              }
            ]
            """);

        var service = new ParticipantKeyService(filePath);

        var firstConsume = await service.ConsumeAsync("8F4K92LM");
        var secondConsume = await service.ConsumeAsync("8F4K92LM");
        var isValidAfter = service.IsValid("8F4K92LM");
        Assert.True(firstConsume);
        Assert.False(secondConsume);     // double-vote blocked
        Assert.False(isValidAfter);

        File.Delete(filePath);
    }

    private static string CreateKeyFile(string content)
    {
        var filePath = Path.GetTempFileName();

        File.WriteAllText(filePath, content);

        return filePath;
    }
}