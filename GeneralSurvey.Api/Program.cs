using GeneralSurvey.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

app.MapControllers();

app.Run();