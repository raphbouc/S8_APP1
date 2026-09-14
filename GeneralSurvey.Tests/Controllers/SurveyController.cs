using GeneralSurvey.Api.Controllers;
using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneralSurvey.Tests.Controllers;

public class SurveysControllerTests
{
    private static SurveysController CreateController()
    {
        var service = new FakeSurveyService();

        return new SurveysController(service);
    }

    [Fact]
    public void GetSurveys_ReturnsOkWithSurveys()
    {
        var controller = CreateController();

        var result = controller.GetSurveys();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var surveys = Assert.IsType<List<Survey>>(okResult.Value);

        Assert.Equal(2, surveys.Count);
    }

    [Fact]
    public void GetSurvey_WithExistingId_ReturnsOkWithSurvey()
    {
        var controller = CreateController();

        var result = controller.GetSurvey(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var survey = Assert.IsType<Survey>(okResult.Value);

        Assert.Equal(1, survey.Id);
        Assert.Equal("Sondage 1", survey.Title);
    }

    [Fact]
    public void GetSurvey_WithNonExistingId_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.GetSurvey(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private sealed class FakeSurveyService : ISurveyService
    {
        private readonly List<Survey> _surveys =
        [
            new Survey
            {
                Id = 1,
                Title = "Sondage 1"
            },
            new Survey
            {
                Id = 2,
                Title = "Sondage 2"
            }
        ];

        public List<Survey> GetSurveys()
        {
            return _surveys;
        }

        public Survey? GetSurvey(int id)
        {
            return _surveys.FirstOrDefault(survey => survey.Id == id);
        }
    }
}