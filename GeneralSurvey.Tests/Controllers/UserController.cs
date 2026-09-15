using GeneralSurvey.Api.Controllers;
using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneralSurvey.Tests.Controllers;

public class UserControllerTests
{
    [Fact]
    public void Register_WithNewUser_ReturnsCreated()
    {
        var service = new FakeUserService
        {
            RegisterResult = true
        };

        var controller = new UsersController(service);

        var result = controller.Register(
            new RegisterRequest
            {
                Username = "raphael",
                Password = "password"
            });

        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public void Register_WithExistingUser_ReturnsConflict()
    {
        var service = new FakeUserService
        {
            RegisterResult = false
        };

        var controller = new UsersController(service);

        var result = controller.Register(
            new RegisterRequest
            {
                Username = "raphael",
                Password = "password"
            });

        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public void Authenticate_WithValidCredentials_ReturnsOk()
    {
        var service = new FakeUserService
        {
            AuthenticationResult = new AuthentificationResponse
            {
                UserId = 1,
                Username = "raphael"
            }
        };

        var controller = new UsersController(service);

        var result = controller.Authenticate(
            new AuthentificationRequest
            {
                Username = "raphael",
                Password = "password"
            });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var response =
            Assert.IsType<AuthentificationResponse>(okResult.Value);

        Assert.Equal(1, response.UserId);
        Assert.Equal("raphael", response.Username);
    }

    [Fact]
    public void Authenticate_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var service = new FakeUserService
        {
            AuthenticationResult = null
        };

        var controller = new UsersController(service);

        var result = controller.Authenticate(
            new AuthentificationRequest
            {
                Username = "raphael",
                Password = "wrong-password"
            });

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public void GetById_WithExistingUser_ReturnsUserWithoutPassword()
    {
        var service = new FakeUserService
        {
            UserResult = new User
            {
                Id = 1,
                Username = "raphael",
                Password = "secret"
            }
        };

        var controller = new UsersController(service);

        var result = controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var response =
            Assert.IsType<UserResponse>(okResult.Value);

        Assert.Equal(1, response.Id);
        Assert.Equal("raphael", response.Username);
    }

    [Fact]
    public void GetById_WithNonExistingUser_ReturnsNotFound()
    {
        var service = new FakeUserService
        {
            UserResult = null
        };

        var controller = new UsersController(service);

        var result = controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private class FakeUserService : IUserService
    {
        public bool RegisterResult { get; set; }

        public AuthentificationResponse? AuthenticationResult { get; set; }

        public User? UserResult { get; set; }

        public AuthentificationResponse? Authenticate(
            AuthentificationRequest model)
        {
            return AuthenticationResult;
        }

        public bool Register(RegisterRequest model)
        {
            return RegisterResult;
        }

        public User? GetById(int id)
        {
            return UserResult;
        }
    }
}