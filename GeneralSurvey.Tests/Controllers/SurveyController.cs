using GeneralSurvey.Api.Controllers;
using GeneralSurvey.Api.Models;
using GeneralSurvey.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneralSurvey.Tests.Controllers;

public class SurveysControllerTests
{
    private static SurveysController CreateController(
        FakeSurveyService? surveyService = null,
        FakeParticipantKeyService? participantKeyService = null)
    {
        surveyService ??= new FakeSurveyService();
        participantKeyService ??= new FakeParticipantKeyService();

        return new SurveysController(
            surveyService,
            participantKeyService);
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

    [Fact]
    public async Task RespondToSurvey_WithoutParticipantKey_ReturnsUnauthorized()
    {
        var controller = CreateController();

        var result = await controller.RespondToSurvey(
            1,
            null,
            CreateValidResponse());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RespondToSurvey_WithEmptyParticipantKey_ReturnsUnauthorized()
    {
        var controller = CreateController();

        var result = await controller.RespondToSurvey(
            1,
            "",
            CreateValidResponse());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RespondToSurvey_WithWhitespaceParticipantKey_ReturnsUnauthorized()
    {
        var controller = CreateController();

        var result = await controller.RespondToSurvey(
            1,
            "   ",
            CreateValidResponse());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RespondToSurvey_WithInvalidParticipantKey_ReturnsUnauthorized()
    {
        var participantKeyService = new FakeParticipantKeyService
        {
            ConsumeResult = false
        };

        var controller = CreateController(
            participantKeyService: participantKeyService);

        var result = await controller.RespondToSurvey(
            1,
            "invalid-key",
            CreateValidResponse());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RespondToSurvey_WithDifferentSurveyId_ReturnsBadRequest()
    {
        var controller = CreateController();

        var response = CreateValidResponse();
        response.SurveyId = 2;

        var result = await controller.RespondToSurvey(
            1,
            "valid-key",
            response);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task RespondToSurvey_WithInvalidResponse_ReturnsBadRequest()
    {
        var surveyService = new FakeSurveyService
        {
            RespondToSurveyResult = false
        };

        var controller = CreateController(
            surveyService: surveyService);

        var result = await controller.RespondToSurvey(
            1,
            "valid-key",
            CreateValidResponse());

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task RespondToSurvey_WithValidResponse_ReturnsCreated()
    {
        var participantKeyService = new FakeParticipantKeyService();

        var controller = CreateController(
            participantKeyService: participantKeyService);

        var result = await controller.RespondToSurvey(
            1,
            "valid-key",
            CreateValidResponse());

        Assert.IsType<CreatedResult>(result);
        Assert.True(participantKeyService.ConsumeCalled);
    }

    private static SurveyResponse CreateValidResponse()
    {
        return new SurveyResponse
        {
            SurveyId = 1,
            Answers =
            [
                new Answer
                {
                    QuestionId = 1,
                    AnswerValue = "a"
                }
            ]
        };
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

        public bool RespondToSurveyResult { get; set; } = true;

        public List<Survey> GetSurveys()
        {
            return _surveys;
        }

        public Survey? GetSurvey(int id)
        {
            return _surveys.FirstOrDefault(
                survey => survey.Id == id);
        }

        public List<SurveyResponse> GetAllAnswersBySurveyId(int id)
        {
            return [];
        }

        public bool RespondToSurvey(SurveyResponse surveyResponse)
        {
            return RespondToSurveyResult;
        }
    }

    private sealed class FakeParticipantKeyService
        : IParticipantKeyService
    {
        public bool ConsumeResult { get; set; } = true;

        public bool ConsumeCalled { get; private set; }

        public bool IsValid(string key)
        {
            return ConsumeResult;
        }

        public Task<bool> ConsumeAsync(string key)
        {
            ConsumeCalled = true;
            return Task.FromResult(ConsumeResult);
        }
    }
}