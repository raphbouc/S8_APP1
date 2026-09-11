using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public interface ISurveyService
{
    List<Survey> GetSurveys();

    Survey? GetSurvey(int id);
}