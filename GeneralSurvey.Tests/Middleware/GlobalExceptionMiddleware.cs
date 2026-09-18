using System.Text.Json;
using GeneralSurvey.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace GeneralSurvey.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{

    [Fact]
    public async Task InvokeAsync_WhenNextSucceeds_DoesNotWriteBody()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_Returns500WithProblemDetails()
    {
        RequestDelegate next = _ => throw new InvalidOperationException(
            "This is an internal error with a stack trace – must not leak.");

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);

        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string body;
        using (var reader = new StreamReader(context.Response.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        Assert.False(string.IsNullOrWhiteSpace(body));

        var problem = JsonSerializer.Deserialize<ProblemDetails>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_DoesNotLeakExceptionMessage()
    {
        const string sensitiveMessage = "Connection string: Server=prod-db;Password=secret123";

        RequestDelegate next = _ => throw new InvalidOperationException(sensitiveMessage);

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string body;
        using (var reader = new StreamReader(context.Response.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        Assert.DoesNotContain(sensitiveMessage, body, StringComparison.Ordinal);
        Assert.Equal(500, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        var middleware = new GlobalExceptionMiddleware(
            _ => Task.CompletedTask,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => middleware.InvokeAsync(null!));
    }
}
