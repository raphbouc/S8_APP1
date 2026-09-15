using System.Text.Json;
using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public class UserService : IUserService
{
    private readonly string _filePath;
    private readonly List<User> _users;

    public UserService(string filePath)
    {
        _filePath = filePath;

        if (!File.Exists(_filePath))
        {
            _users = [];
            SaveUsers();
            return;
        }

        var content = File.ReadAllText(_filePath);

        _users = string.IsNullOrWhiteSpace(content)
            ? []
            : JsonSerializer.Deserialize<List<User>>(content) ?? [];
    }

    public AuthentificationResponse? Authenticate(
        AuthentificationRequest model)
    {
        var user = _users.FirstOrDefault(
            user => user.Username == model.Username &&
                    user.Password == model.Password);

        if (user is null)
        {
            return null;
        }

        return new AuthentificationResponse
        {
            UserId = user.Id,
            Username = user.Username
        };
    }

    public bool Register(RegisterRequest model)
    {
        if (_users.Any(user => user.Username == model.Username))
        {
            return false;
        }

        var user = new User
        {
            Id = _users.Count == 0
                ? 1
                : _users.Max(user => user.Id) + 1,
            Username = model.Username,
            Password = model.Password
        };

        _users.Add(user);

        SaveUsers();

        return true;
    }

    public User? GetById(int id)
    {
        return _users.FirstOrDefault(user => user.Id == id);
    }

    private void SaveUsers()
    {
        var json = JsonSerializer.Serialize(
            _users,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_filePath, json);
    }
}