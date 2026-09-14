using GeneralSurvey.Api.Services;

namespace GeneralSurvey.Tests.Services;

public class SurveyServiceTests
{
    private static string GetSurveyFilePath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "sondage.txt");
    }

    private static SurveyService CreateService()
    {
        var parser = new SurveyParser();
        var filePath = GetSurveyFilePath();

        return new SurveyService(parser, filePath);
    }

    [Fact]
    public void GetSurveys_ReturnsAllSurveys()
    {
        var service = CreateService();

        var surveys = service.GetSurveys();

        Assert.Equal(2, surveys.Count);
    }

    [Fact]
    public void GetSurvey_WithExistingId_ReturnsSurvey()
    {
        var service = CreateService();

        var survey = service.GetSurvey(1);

        Assert.NotNull(survey);
        Assert.Equal(1, survey.Id);
        Assert.Equal("Sondage 1", survey.Title);
    }

    [Fact]
    public void GetSurvey_WithNonExistingId_ReturnsNull()
    {
        var service = CreateService();

        var survey = service.GetSurvey(999);

        Assert.Null(survey);
    }

    [Fact]
    public void GetSurvey_One_ReturnsExpectedQuestions()
    {
        var service = CreateService();

        var survey = service.GetSurvey(1);

        Assert.NotNull(survey);

        Assert.Equal(4, survey.Questions.Count);

        Assert.Equal(
            "À quelle tranche d'âge appartenez-vous?",
            survey.Questions[0].Text);

        Assert.Equal(4, survey.Questions[0].Options.Count);

        Assert.Equal("a", survey.Questions[0].Options[0].Code);
        Assert.Equal("0-25 ans", survey.Questions[0].Options[0].Text);

        Assert.Equal("b", survey.Questions[0].Options[1].Code);
        Assert.Equal("25-50 ans", survey.Questions[0].Options[1].Text);

        Assert.Equal("c", survey.Questions[0].Options[2].Code);
        Assert.Equal("50-75 ans", survey.Questions[0].Options[2].Text);

        Assert.Equal("d", survey.Questions[0].Options[3].Code);
        Assert.Equal("75 ans et plus", survey.Questions[0].Options[3].Text);
    }

    [Fact]
    public void GetSurvey_Two_ReturnsExpectedQuestions()
    {
        var service = CreateService();

        var survey = service.GetSurvey(2);

        Assert.NotNull(survey);

        Assert.Equal(4, survey.Questions.Count);

        Assert.Equal(
            "Combien de tasses de café buvez-vous chaque jour?",
            survey.Questions[2].Text);

        Assert.Equal(4, survey.Questions[2].Options.Count);

        Assert.Equal(
            "Je ne bois pas de café",
            survey.Questions[2].Options[0].Text);
    }

    [Fact]
    public void Constructor_WithNonExistingFile_ThrowsFileNotFoundException()
    {
        var parser = new SurveyParser();

        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "fichier-inexistant.txt");

        var exception = Assert.Throws<FileNotFoundException>(
            () => new SurveyService(parser, filePath));

        Assert.Equal(
            "Le fichier de sondages est introuvable.",
            exception.Message);

        Assert.Equal(filePath, exception.FileName);
}
}