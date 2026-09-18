namespace GeneralSurvey.Api.Models;

public class Answer
{
    public int QuestionId { get; set; }
    public string AnswerValue { get; set; } = string.Empty;
}