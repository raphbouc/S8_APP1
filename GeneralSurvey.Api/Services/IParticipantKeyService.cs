using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public interface IParticipantKeyService
{
    bool IsValid(string key);
    bool Consume(string key);
}