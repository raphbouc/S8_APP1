using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneralSurvey.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    private readonly ISurveyService _surveyService;
    private readonly IParticipantKeyService _participantKeyService;

    public SurveysController(
        ISurveyService surveyService,
        IParticipantKeyService participantKeyService)
    {
        _surveyService = surveyService;
        _participantKeyService = participantKeyService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Survey>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<List<Survey>> GetSurveys()
    {
        return Ok(_surveyService.GetSurveys());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Survey), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Survey> GetSurvey(int id)
    {
        var survey = _surveyService.GetSurvey(id);

        if (survey is null)
        {
            return NotFound();
        }

        return Ok(survey);
    }

    [HttpPost("{id:int}/responses")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RespondToSurvey(
        int id,
        [FromHeader(Name = "X-Participant-Key")] string? participantKey,
        SurveyResponse response)
    {
        if (string.IsNullOrWhiteSpace(participantKey))
        {
            return Unauthorized();
        }

        if (id != response.SurveyId || !_surveyService.IsValidResponse(response))
        {
            return BadRequest();
        }

        if (!await _participantKeyService.ConsumeAsync(participantKey))
        {
            return Unauthorized();
        }

        _surveyService.RespondToSurvey(response);

        return Created();
    }
}