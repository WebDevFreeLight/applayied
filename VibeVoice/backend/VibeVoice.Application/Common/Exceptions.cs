namespace VibeVoice.Application.Common;

/// <summary>Base for errors that map to a specific HTTP status in the API's exception handler.</summary>
public abstract class AppException(string title, string message) : Exception(message)
{
    public string Title { get; } = title;
    public abstract int StatusCode { get; }
}

public sealed class NotFoundException(string entity, object id)
    : AppException("Not found", $"{entity} '{id}' was not found.")
{
    public override int StatusCode => 404;
}

public sealed class BadRequestException(string message, IDictionary<string, string[]>? errors = null)
    : AppException("Bad request", message)
{
    public IDictionary<string, string[]>? Errors { get; } = errors;
    public override int StatusCode => 400;
}

public sealed class ConflictException(string message) : AppException("Conflict", message)
{
    public override int StatusCode => 409;
}

public sealed class UnauthorizedAppException(string message) : AppException("Unauthorized", message)
{
    public override int StatusCode => 401;
}

public sealed class ForbiddenException(string message) : AppException("Forbidden", message)
{
    public override int StatusCode => 403;
}

/// <summary>The Python voice service is down or returned an error; the rest of the API keeps working.</summary>
public sealed class VoiceServiceUnavailableException(string message, Exception? inner = null, bool timedOut = false)
    : AppException(timedOut ? "Voice service timed out" : "Voice service unavailable", message)
{
    public Exception? Upstream { get; } = inner;

    /// <summary>
    /// The service was reachable but did not finish in time (a long recording on a slow CPU). Unlike "not
    /// reachable" (asleep / restarting), running it again from the start would only take as long again.
    /// </summary>
    public bool TimedOut { get; } = timedOut;

    public override int StatusCode => timedOut ? 504 : 503;
}

public sealed class PayloadTooLargeException(string message) : AppException("Payload too large", message)
{
    public override int StatusCode => 413;
}
