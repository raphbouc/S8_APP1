using GeneralSurvey.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GeneralSurvey.Tests.Middleware;

public class ApiKeyMiddlewareTests
{
    private const string ValidApiKey = "test-api-key";

    private static ApiKeyMiddleware CreateMiddleware(
        RequestDelegate next,
        string? configuredApiKey = ValidApiKey)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ApiKey"] = configuredApiKey
                })
            .Build();

        return new ApiKeyMiddleware(next, configuration);
    }

    [Fact]
    public async Task InvokeAsync_WithoutApiKey_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();

        var middleware = CreateMiddleware(
            _ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidApiKey_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();

        context.Request.Headers["X-API-Key"] = "wrong-key";

        var middleware = CreateMiddleware(
            _ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithValidApiKey_CallsNext()
    {
        var context = new DefaultHttpContext();

        context.Request.Headers["X-API-Key"] = ValidApiKey;

        var nextCalled = false;

        var middleware = CreateMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithoutConfiguredApiKey_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();

        context.Request.Headers["X-API-Key"] = ValidApiKey;

        var middleware = CreateMiddleware(
            _ => Task.CompletedTask,
            configuredApiKey: null);

        await middleware.InvokeAsync(context);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithSwaggerRequest_CallsNext()
    {
        var context = new DefaultHttpContext();

        context.Request.Path = "/swagger";

        var nextCalled = false;

        var middleware = CreateMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }
}