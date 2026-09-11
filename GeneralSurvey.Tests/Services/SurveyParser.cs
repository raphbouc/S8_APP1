using GeneralSurvey.Api.Services;

namespace GeneralSurvey.Tests.Services;

public class SurveyParserTests
{
    [Fact]
    public void Parse_ValidSurvey_ReturnsSurvey()
    {
        const string content = """
            Sondage 1:
            1. Quel âge avez-vous? a:18-25 ans, b:26-50 ans
            """;

        var parser = new SurveyParser();

        var surveys = parser.Parse(content);

        Assert.Single(surveys);
        Assert.Equal(1, surveys[0].Id);
        Assert.Equal("Sondage 1", surveys[0].Title);
    }

    [Fact]
    public void Parse_ValidSurvey_ParsesQuestions()
    {
        const string content = """
            Sondage 1:
            1. Quel âge avez-vous? a:18-25 ans, b:26-50 ans
            2. Aimez-vous le café? a:Oui, b:Non
            """;

        var parser = new SurveyParser();

        var surveys = parser.Parse(content);

        Assert.Equal(2, surveys[0].Questions.Count);
        Assert.Equal("Quel âge avez-vous?", surveys[0].Questions[0].Text);
        Assert.Equal("Aimez-vous le café?", surveys[0].Questions[1].Text);
    }

    [Fact]
    public void Parse_ValidSurvey_ParsesOptions()
    {
        const string content = """
            Sondage 1:
            1. Quel âge avez-vous? a:18-25 ans, b:26-50 ans
            """;

        var parser = new SurveyParser();

        var surveys = parser.Parse(content);

        var options = surveys[0].Questions[0].Options;

        Assert.Equal(2, options.Count);
        Assert.Equal("a", options[0].Code);
        Assert.Equal("18-25 ans", options[0].Text);
        Assert.Equal("b", options[1].Code);
        Assert.Equal("26-50 ans", options[1].Text);
    }

    [Fact]
    public void Parse_NullContent_ThrowsException()
    {
        var parser = new SurveyParser();

        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
    }

    [Fact]
    public void Parse_QuestionBeforeSurvey_ThrowsException()
    {
        const string content = """
            1. Quel âge avez-vous? a:18-25 ans, b:26-50 ans
            """;

        var parser = new SurveyParser();

        Assert.Throws<FormatException>(() => parser.Parse(content));
    }

    [Fact]
    public void Parse_InvalidSurveyId_ThrowsException()
    {
        const string content = """
            Sondage ABC:
            """;

        var parser = new SurveyParser();

        Assert.Throws<FormatException>(() => parser.Parse(content));
    }

    [Fact]
    public void Parse_InvalidQuestion_ThrowsException()
    {
        const string content = """
            Sondage 1:
            Question invalide
            """;

        var parser = new SurveyParser();

        Assert.Throws<FormatException>(() => parser.Parse(content));
    }

    [Fact]
    public void Parse_InvalidOption_ThrowsException()
    {
        const string content = """
            Sondage 1:
            1. Quel âge avez-vous? a:18-25 ans, réponse invalide
            """;

        var parser = new SurveyParser();

        Assert.Throws<FormatException>(() => parser.Parse(content));
    }
}