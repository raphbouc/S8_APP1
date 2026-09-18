namespace GeneralSurvey.Api.Models;

public class Question
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public List<AnswerOption> Options { get; set; } = [];
}