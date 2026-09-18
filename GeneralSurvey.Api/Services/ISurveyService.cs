using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public interface ISurveyService
{
    List<Survey> GetSurveys();
    Survey? GetSurvey(int id);
    List<SurveyResponse> GetAllAnswersBySurveyId(int id);
    bool IsValidResponse(SurveyResponse surveyResponse);
    bool RespondToSurvey(SurveyResponse surveyResponse);
}