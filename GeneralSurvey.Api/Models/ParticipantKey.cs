namespace GeneralSurvey.Api.Models;

public class ParticipantKey
{
    public string KeyHash { get; set; } = string.Empty;
    public bool Used { get; set; }
}