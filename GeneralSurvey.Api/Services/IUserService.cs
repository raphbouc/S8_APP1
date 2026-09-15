using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public interface IUserService
{
    AuthentificationResponse? Authenticate(
        AuthentificationRequest model);

    bool Register(RegisterRequest model);

    User? GetById(int id);
}