namespace GeneralSurvey.Api.Services;

public interface IParticipantKeyService
{
    bool IsValid(string key);
    Task<bool> ConsumeAsync(string key);
}