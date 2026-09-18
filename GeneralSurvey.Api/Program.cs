using GeneralSurvey.Api.Middleware;
using GeneralSurvey.Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API Key authentication"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var surveyFilePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Data",
    "sondage.txt");

var responsesFilePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Database",
    "responses.json");

builder.Services.AddSingleton<ISurveyService>(
    serviceProvider =>
    {
        var parser = new SurveyParser();

        return new SurveyService(
            parser,
            surveyFilePath,
            responsesFilePath);
    });

var participantKeyFilePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Database",
    "participant-keys.json");

builder.Services.AddSingleton<IParticipantKeyService>(
    new ParticipantKeyService(participantKeyFilePath));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Program { }