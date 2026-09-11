using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneralSurvey.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    private readonly ISurveyService _surveyService;

    public SurveysController(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    [HttpGet]
    public ActionResult<List<Survey>> GetSurveys()
    {
        return Ok(_surveyService.GetSurveys());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Survey> GetSurvey(int id)
    {
        var survey = _surveyService.GetSurvey(id);

        if (survey is null)
        {
            return NotFound();
        }

        return Ok(survey);
    }
}