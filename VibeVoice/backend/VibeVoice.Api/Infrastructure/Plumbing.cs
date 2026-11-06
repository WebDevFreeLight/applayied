using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;

namespace VibeVoice.Api.Infrastructure;

/// <summary>Reads the signed-in user from the JWT claims.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;
            var sub = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : throw new UnauthorizedAppException("Not signed in.");
        }
    }

    public bool IsAdmin => accessor.HttpContext?.User.IsInRole("Admin") == true;
}

/// <summary>Maps exceptions to RFC 7807 ProblemDetails responses.</summary>
public sealed class AppExceptionHandler(IProblemDetailsService problems, ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem;
        switch (exception)
        {
            case BadRequestException { Errors: not null } bad:
                problem = new ValidationProblemDetails(bad.Errors) { Title = bad.Title, Detail = bad.Message, Status = 400 };
                break;
            case AppException app:
                problem = new ProblemDetails { Title = app.Title, Detail = app.Message, Status = app.StatusCode };
                if (app is VoiceServiceUnavailableException) logger.LogWarning("Voice service unavailable: {Message}", app.Message);
                break;
            case BadHttpRequestException bad:
                problem = new ProblemDetails { Title = "Bad request", Detail = bad.Message, Status = bad.StatusCode };
                break;
            case OperationCanceledException when context.RequestAborted.IsCancellationRequested:
                context.Response.StatusCode = 499; // client closed the request
                return true;
            default:
                logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                problem = new ProblemDetails { Title = "Unexpected error", Detail = "Something went wrong. Please try again.", Status = 500 };
                break;
        }

        context.Response.StatusCode = problem.Status ?? 500;
        return await problems.TryWriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem, Exception = exception });
    }
}

/// <summary>Runs the FluentValidation validator for the first argument of type <typeparamref name="T"/>.</summary>
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        var arg = context.Arguments.OfType<T>().FirstOrDefault();
        if (validator is not null && arg is not null)
        {
            var result = await validator.ValidateAsync(arg, context.HttpContext.RequestAborted);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary(), title: "Validation failed");
        }
        return await next(context);
    }
}

public static class EndpointExtensions
{
    public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder builder) where T : class =>
        builder.AddEndpointFilter<ValidationFilter<T>>().ProducesValidationProblem();
}

/// <summary>
/// SQLite (and some other providers) return <see cref="DateTimeKind.Unspecified"/>. Every stored timestamp is UTC,
/// so always emit ISO-8601 with a trailing "Z" and normalize incoming values to UTC.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        return value.Kind switch
        {
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value,
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
    }
}

public static class FormFiles
{
    public static UploadFile ToUpload(this IFormFile f) => new(f.FileName, f.ContentType, f.Length, f.OpenReadStream);

    public static async Task<IFormCollection> ReadMultipartAsync(this HttpRequest request, CancellationToken ct)
    {
        if (!request.HasFormContentType) throw new BadRequestException("Expected a multipart/form-data request.");
        return await request.ReadFormAsync(ct);
    }
}
