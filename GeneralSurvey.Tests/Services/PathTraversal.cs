using GeneralSurvey.Api.Services;

namespace GeneralSurvey.Tests.Services;

public class PathTraversalTests
{
    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("../secrets.json")]
    [InlineData("subfolder/../../etc/shadow")]
    public void ParticipantKeyService_WithRelativeTraversalPath_ThrowsUnauthorized(
        string maliciousPath)
    {
        Assert.Throws<UnauthorizedAccessException>(
            () => new ParticipantKeyService(maliciousPath));
    }

    [Theory]
    [InlineData("..\\..\\Windows\\system.ini")]
    [InlineData("..\\secrets.json")]
    public void ParticipantKeyService_WithWindowsTraversalPath_ThrowsUnauthorized(
        string maliciousPath)
    {
        Assert.Throws<UnauthorizedAccessException>(
            () => new ParticipantKeyService(maliciousPath));
    }

    [Fact]
    public void ParticipantKeyService_WithNullBytePath_ThrowsArgumentException()
    {
        var maliciousPath = "keys.json\0../../etc/passwd";

        Assert.Throws<ArgumentException>(
            () => new ParticipantKeyService(maliciousPath));
    }

    [Fact]
    public void ParticipantKeyService_WithValidPath_DoesNotThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        try
        {
            var svc = new ParticipantKeyService(path);
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("../surveys.txt")]
    [InlineData("data/../../../etc/shadow")]
    public void SurveyService_WithRelativeTraversalSurveyPath_ThrowsUnauthorized(
        string maliciousSurveyPath)
    {
        var parser = new SurveyParser();
        var responsesPath = Path.Combine(
            Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        Assert.Throws<UnauthorizedAccessException>(
            () => new SurveyService(parser, maliciousSurveyPath, responsesPath));
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("../responses.json")]
    public void SurveyService_WithRelativeTraversalResponsesPath_ThrowsUnauthorized(
        string maliciousResponsesPath)
    {
        var surveyPath = Path.Combine(
            Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
        File.WriteAllText(surveyPath, string.Empty);

        var parser = new SurveyParser();

        try
        {
            Assert.Throws<UnauthorizedAccessException>(
                () => new SurveyService(parser, surveyPath, maliciousResponsesPath));
        }
        finally
        {
            File.Delete(surveyPath);
        }
    }

    [Fact]
    public void SurveyService_WithNullByteSurveyPath_ThrowsArgumentException()
    {
        var maliciousPath = "survey.txt\0../../etc/passwd";
        var parser = new SurveyParser();
        var responsesPath = Path.Combine(
            Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        Assert.Throws<ArgumentException>(
            () => new SurveyService(parser, maliciousPath, responsesPath));
    }

    [Fact]
    public void SurveyService_WithNullByteResponsesPath_ThrowsArgumentException()
    {
        var surveyPath = Path.Combine(
            Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
        File.WriteAllText(surveyPath, string.Empty);

        var maliciousResponsesPath = "responses.json\0../../etc/passwd";
        var parser = new SurveyParser();

        try
        {
            Assert.Throws<ArgumentException>(
                () => new SurveyService(parser, surveyPath, maliciousResponsesPath));
        }
        finally
        {
            File.Delete(surveyPath);
        }
    }
    [Fact]
    public void SurveyService_WithBackTraversalSurveyPath_ThrowsUnauthorized()
    {
        // Covers the Contains("/..")  branch – path has /.. but not ../
        var parser = new SurveyParser();
        var responsesPath = Path.Combine(
            Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        Assert.Throws<UnauthorizedAccessException>(
            () => new SurveyService(parser, "/tmp/foo/..", responsesPath));
    }

    [Fact]
    public void SurveyService_WithStandaloneDoubleDotSurveyPath_ThrowsUnauthorized()
    {
        // Covers the normalised == ".." branch – exact two-dot path
        var parser = new SurveyParser();
        var responsesPath = Path.Combine(
            Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        Assert.Throws<UnauthorizedAccessException>(
            () => new SurveyService(parser, "..", responsesPath));
    }
}
