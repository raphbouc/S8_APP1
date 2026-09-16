using System.Text.Json;
using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public class SurveyService : ISurveyService
{
    private readonly List<Survey> _surveys;
    private readonly string _responsesFilePath;

    public SurveyService(
        SurveyParser parser,
        string surveyFilePath,
        string responsesFilePath)
    {
        ValidatePath(surveyFilePath);
        ValidatePath(responsesFilePath);

        if (!File.Exists(surveyFilePath))
        {
            throw new FileNotFoundException(
                "Le fichier de sondages est introuvable.",
                surveyFilePath);
        }

        var content = File.ReadAllText(surveyFilePath);
        _surveys = parser.Parse(content);

        _responsesFilePath = responsesFilePath;

        if (!File.Exists(_responsesFilePath))
        {
            SaveResponses([]);
        }
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

    public List<Survey> GetSurveys()
    {
        return _surveys;
    }

    public Survey? GetSurvey(int id)
    {
        return _surveys.FirstOrDefault(
            survey => survey.Id == id);
    }

    public List<SurveyResponse> GetAllAnswersBySurveyId(int id)
    {
        var responses = LoadResponses();

        return responses
            .Where(response => response.SurveyId == id)
            .ToList();
    }

    public bool RespondToSurvey(SurveyResponse surveyResponse)
    {
        var survey = GetSurvey(surveyResponse.SurveyId);

        if (survey is null)
        {
            return false;
        }

        if (!AreAnswersValid(survey, surveyResponse))
        {
            return false;
        }

        var responses = LoadResponses();

        responses.Add(surveyResponse);

        SaveResponses(responses);

        return true;
    }

    private List<SurveyResponse> LoadResponses()
    {
        var content = File.ReadAllText(_responsesFilePath);

        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<SurveyResponse>>(content)
            ?? [];
    }

    private void SaveResponses(List<SurveyResponse> responses)
    {
        var json = JsonSerializer.Serialize(
            responses,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_responsesFilePath, json);
    }

    private static bool AreAnswersValid(
        Survey survey,
        SurveyResponse response)
    {
        if (response.Answers.Count != survey.Questions.Count)
        {
            return false;
        }

        if (response.Answers
            .Select(answer => answer.QuestionId)
            .Distinct()
            .Count() != survey.Questions.Count)
        {
            return false;
        }

        foreach (var answer in response.Answers)
        {
            var question = survey.Questions.FirstOrDefault(
                question => question.Id == answer.QuestionId);

            if (question is null)
            {
                return false;
            }

            var optionExists = question.Options.Any(
                option => option.Code == answer.AnswerValue);

            if (!optionExists)
            {
                return false;
            }
        }

        return true;
    }
}