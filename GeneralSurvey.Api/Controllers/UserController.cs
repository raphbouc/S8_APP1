using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneralSurvey.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public ActionResult Register(RegisterRequest model)
    {
        var registered = _userService.Register(model);

        if (!registered)
        {
            return Conflict();
        }

        return Created();
    }

    [HttpPost("authenticate")]
    public ActionResult<AuthentificationResponse> Authenticate(
        AuthentificationRequest model)
    {
        var response = _userService.Authenticate(model);

        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public ActionResult<UserResponse> GetById(int id)
    {
        var user = _userService.GetById(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username
        });
    }
}