using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public class SurveyService : ISurveyService
{
    private readonly List<Survey> _surveys;

    public SurveyService(SurveyParser parser, string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Le fichier de sondages est introuvable.",
                filePath);
        }

        var content = File.ReadAllText(filePath);
        _surveys = parser.Parse(content);
    }

    public List<Survey> GetSurveys()
    {
        return _surveys;
    }

    public Survey? GetSurvey(int id)
    {
        return _surveys.FirstOrDefault(survey => survey.Id == id);
    }
}