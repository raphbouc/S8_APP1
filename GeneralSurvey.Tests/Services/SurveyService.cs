namespace GeneralSurvey.Tests.Services;

using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;

public class SurveyServiceTests
{
    private static string GetSurveyFilePath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "sondage.txt");
    }

    private static string CreateResponsesFile()
    {
        var filePath = Path.GetTempFileName();

        File.WriteAllText(filePath, "[]");

        return filePath;
    }

    private static SurveyService CreateService()
    {
        var parser = new SurveyParser();
        var surveyFilePath = GetSurveyFilePath();
        var responsesFilePath = CreateResponsesFile();

        return new SurveyService(
            parser,
            surveyFilePath,
            responsesFilePath);
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

        var surveyFilePath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "fichier-inexistant.txt");

        var responsesFilePath = CreateResponsesFile();

        var exception = Assert.Throws<FileNotFoundException>(
            () => new SurveyService(
                parser,
                surveyFilePath,
                responsesFilePath));

        Assert.Equal(
            "Le fichier de sondages est introuvable.",
            exception.Message);

        Assert.Equal(
            surveyFilePath,
            exception.FileName);
    }

    [Fact]
    public void RespondToSurvey_WithValidResponse_ReturnsTrue()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 2,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 3,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 4,
                    AnswerValue = "a"
                }
            ]
        };

        var result = service.RespondToSurvey(response);

        Assert.True(result);
    }

    [Fact]
    public void RespondToSurvey_WithValidResponse_SavesResponse()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 2,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 3,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 4,
                    AnswerValue = "a"
                }
            ]
        };

        service.RespondToSurvey(response);

        var responses = service.GetAllAnswersBySurveyId(1);

        Assert.Single(responses);
        Assert.Equal(1, responses[0].SurveyId);
        Assert.Equal(4, responses[0].Answers.Count);
    }

    [Fact]
    public void GetAllAnswersBySurveyId_WithNoResponses_ReturnsEmptyList()
    {
        var service = CreateService();

        var responses = service.GetAllAnswersBySurveyId(1);

        Assert.Empty(responses);
    }

    [Fact]
    public void GetAllAnswersBySurveyId_WithDifferentSurvey_ReturnsOnlyMatchingResponses()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 2,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 3,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 4,
                    AnswerValue = "a"
                }
            ]
        };

        service.RespondToSurvey(response);

        var responses = service.GetAllAnswersBySurveyId(2);

        Assert.Empty(responses);
    }

    [Fact]
    public void RespondToSurvey_WithNonExistingSurvey_ReturnsFalse()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 999,
            Answers = []
        };

        var result = service.RespondToSurvey(response);

        Assert.False(result);
    }

    [Fact]
    public void RespondToSurvey_WithWrongNumberOfAnswers_ReturnsFalse()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "a"
                }
            ]
        };

        var result = service.RespondToSurvey(response);

        Assert.False(result);
    }

    [Fact]
    public void RespondToSurvey_WithDuplicateQuestion_ReturnsFalse()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "b"
                },
                new Answer
                {
                    QuestionId = 3,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 4,
                    AnswerValue = "a"
                }
            ]
        };

        var result = service.RespondToSurvey(response);

        Assert.False(result);
    }

    [Fact]
    public void RespondToSurvey_WithNonExistingQuestion_ReturnsFalse()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 2,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 3,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 999,
                    AnswerValue = "a"
                }
            ]
        };

        var result = service.RespondToSurvey(response);

        Assert.False(result);
    }

    [Fact]
    public void RespondToSurvey_WithNonExistingOption_ReturnsFalse()
    {
        var service = CreateService();

        var response = new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "z"
                },
                new Answer
                {
                    QuestionId = 2,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 3,
                    AnswerValue = "a"
                },
                new Answer
                {
                    QuestionId = 4,
                    AnswerValue = "a"
                }
            ]
        };

        var result = service.RespondToSurvey(response);

        Assert.False(result);
    }

    [Fact]
    public void Constructor_WithNonExistingResponsesFile_CreatesFile()
    {
        var parser = new SurveyParser();
        var surveyFilePath = GetSurveyFilePath();

        var responsesFilePath = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid() + ".json");

        try
        {
            _ = new SurveyService(
                parser,
                surveyFilePath,
                responsesFilePath);

            Assert.True(File.Exists(responsesFilePath));
            Assert.Equal("[]", File.ReadAllText(responsesFilePath));
        }
        finally
        {
            if (File.Exists(responsesFilePath))
            {
                File.Delete(responsesFilePath);
            }
        }
    }

    [Fact]
    public void GetAllAnswersBySurveyId_WithEmptyResponsesFile_ReturnsEmptyList()
    {
        var parser = new SurveyParser();
        var surveyFilePath = GetSurveyFilePath();
        var responsesFilePath = Path.GetTempFileName();

        try
        {
            File.WriteAllText(responsesFilePath, "");

            var service = new SurveyService(
                parser,
                surveyFilePath,
                responsesFilePath);

            var responses = service.GetAllAnswersBySurveyId(1);

            Assert.Empty(responses);
        }
        finally
        {
            if (File.Exists(responsesFilePath))
            {
                File.Delete(responsesFilePath);
            }
        }
    }

    [Fact]
    public void GetAllAnswersBySurveyId_WithNullJson_ReturnsEmptyList()
    {
        var parser = new SurveyParser();
        var surveyFilePath = GetSurveyFilePath();
        var responsesFilePath = Path.GetTempFileName();

        try
        {
            File.WriteAllText(responsesFilePath, "null");

            var service = new SurveyService(
                parser,
                surveyFilePath,
                responsesFilePath);

            var responses = service.GetAllAnswersBySurveyId(1);

            Assert.Empty(responses);
        }
        finally
        {
            if (File.Exists(responsesFilePath))
            {
                File.Delete(responsesFilePath);
            }
        }
    }
    [Fact]
    public void Constructor_WithNullByteSurveyPath_ThrowsArgumentException()
    {
        var parser = new SurveyParser();
        var maliciousPath = "sondage.txt\0../../etc/passwd";
        var responsesFilePath = CreateResponsesFile();

        Assert.Throws<ArgumentException>(
            () => new SurveyService(parser, maliciousPath, responsesFilePath));
    }

    [Fact]
    public void Constructor_WithNullByteResponsesPath_ThrowsArgumentException()
    {
        var parser = new SurveyParser();
        var surveyFilePath = GetSurveyFilePath();
        var maliciousPath = "responses.json\0../../etc/passwd";

        Assert.Throws<ArgumentException>(
            () => new SurveyService(parser, surveyFilePath, maliciousPath));
    }

    [Fact]
    public void Constructor_WithStandaloneDoubleDotSurveyPath_ThrowsUnauthorized()
    {
        var parser = new SurveyParser();
        var responsesFilePath = CreateResponsesFile();

        Assert.Throws<UnauthorizedAccessException>(
            () => new SurveyService(parser, "..", responsesFilePath));
    }
}