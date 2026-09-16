namespace GeneralSurvey.Api.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-API-Key";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsSwaggerRequest(context))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(
                ApiKeyHeaderName,
                out var providedApiKey))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsync(
                "Clé API manquante.");

            return;
        }

        var apiKey = _configuration["ApiKey"];

        if (string.IsNullOrEmpty(apiKey) ||
            providedApiKey != apiKey)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsync(
                "Clé API invalide.");

            return;
        }

        await _next(context);
    }

    private static bool IsSwaggerRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/swagger");
    }
}