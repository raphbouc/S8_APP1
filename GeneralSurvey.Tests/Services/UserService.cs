using System.Text.Json;
using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;

namespace GeneralSurvey.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public void Constructor_WithNonExistingFile_CreatesEmptyFile()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            Assert.Empty(ReadUsers(filePath));
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Constructor_WithEmptyFile_CreatesEmptyUserList()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            File.WriteAllText(filePath, string.Empty);

            var service = new UserService(filePath);

            Assert.Null(service.GetById(1));
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Constructor_WithExistingUsers_LoadsUsers()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var users = new List<User>
            {
                new()
                {
                    Id = 1,
                    Username = "raphael",
                    Password = "password"
                }
            };

            File.WriteAllText(
                filePath,
                JsonSerializer.Serialize(users));

            var service = new UserService(filePath);

            var user = service.GetById(1);

            Assert.NotNull(user);
            Assert.Equal("raphael", user.Username);
            Assert.Equal("password", user.Password);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Register_WithNewUser_ReturnsTrueAndSavesUser()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            var result = service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            Assert.True(result);

            var users = ReadUsers(filePath);

            Assert.Single(users);
            Assert.Equal(1, users[0].Id);
            Assert.Equal("raphael", users[0].Username);
            Assert.Equal("password", users[0].Password);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Register_WithExistingUsername_ReturnsFalse()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            var result = service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "another-password"
                });

            Assert.False(result);
            Assert.Single(ReadUsers(filePath));
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Register_WithExistingUsers_AssignsNextId()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var users = new List<User>
            {
                new()
                {
                    Id = 3,
                    Username = "existing",
                    Password = "password"
                }
            };

            File.WriteAllText(
                filePath,
                JsonSerializer.Serialize(users));

            var service = new UserService(filePath);

            var result = service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            Assert.True(result);

            var registeredUser = service.GetById(4);

            Assert.NotNull(registeredUser);
            Assert.Equal("raphael", registeredUser.Username);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Authenticate_WithCorrectCredentials_ReturnsResponse()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            var response = service.Authenticate(
                new AuthentificationRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            Assert.NotNull(response);
            Assert.Equal(1, response.UserId);
            Assert.Equal("raphael", response.Username);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Authenticate_WithIncorrectCredentials_ReturnsNull()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            var response = service.Authenticate(
                new AuthentificationRequest
                {
                    Username = "raphael",
                    Password = "wrong-password"
                });

            Assert.Null(response);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void GetById_WithExistingId_ReturnsUser()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            var user = service.GetById(1);

            Assert.NotNull(user);
            Assert.Equal(1, user.Id);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void GetById_WithNonExistingId_ReturnsNull()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            var user = service.GetById(999);

            Assert.Null(user);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    private static string GetTemporaryFilePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");
    }

    private static List<User> ReadUsers(string filePath)
    {
        var content = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<List<User>>(content) ?? [];
    }

    private static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Authenticate_WithIncorrectUsername_ReturnsNull()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            var service = new UserService(filePath);

            service.Register(
                new RegisterRequest
                {
                    Username = "raphael",
                    Password = "password"
                });

            var response = service.Authenticate(
                new AuthentificationRequest
                {
                    Username = "wrong-username",
                    Password = "password"
                });

            Assert.Null(response);
        }
        finally
        {
            DeleteFile(filePath);
        }
    }

    [Fact]
    public void Constructor_WithNullJson_CreatesEmptyUserList()
    {
        var filePath = GetTemporaryFilePath();

        try
        {
            File.WriteAllText(filePath, "null");

            var service = new UserService(filePath);

            Assert.Null(service.GetById(1));
        }
        finally
        {
            DeleteFile(filePath);
        }
    }
}