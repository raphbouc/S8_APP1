using GeneralSurvey.Api.Middleware;
using GeneralSurvey.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var userFilePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Database",
    "users.json");

builder.Services.AddSingleton<IUserService>(
    new UserService(userFilePath));

var surveyFilePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Data",
    "sondage.txt");

builder.Services.AddSingleton<ISurveyService>(serviceProvider =>
{
    var parser = new SurveyParser();

    return new SurveyService(parser, surveyFilePath);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();