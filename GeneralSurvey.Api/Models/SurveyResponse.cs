namespace GeneralSurvey.Api.Models;

public class SurveyResponse
{
    public int SurveyId { get; set; }
    public int UserId { get; set; }
    public List<Answer> Answers { get; set; } = [];
}