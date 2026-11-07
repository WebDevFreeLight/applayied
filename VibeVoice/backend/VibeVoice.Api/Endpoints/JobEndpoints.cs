using VibeVoice.Application.Services;

namespace VibeVoice.Api.Endpoints;

/// <summary>
/// Background jobs (RFC 7240 "Prefer: respond-async"). OCR and transcription endpoints queue the work and
/// answer 202 Accepted + Location: /api/jobs/{id} when the client asks for it; without the header they keep
/// answering synchronously.
/// </summary>
public static class JobEndpoints
{
    public static bool WantsAsync(this HttpRequest request) =>
        request.Headers.TryGetValue("Prefer", out var values)
        && values.Any(v => v?.Contains("respond-async", StringComparison.OrdinalIgnoreCase) == true);

    public static IResult Accepted(HttpContext context, JobDto job)
    {
        context.Response.Headers["Preference-Applied"] = "respond-async";
        return Results.Accepted($"/api/jobs/{job.Id}", job);
    }

    public static void MapJobs(this WebApplication app)
    {
        var jobs = app.MapGroup("/api/jobs").WithTags("Jobs").RequireAuthorization();
        // ?active=true -> only queued/running jobs (e.g. to show progress after a page reload).
        jobs.MapGet("/", (bool? active, JobService s, CancellationToken ct) => s.ListAsync(active ?? false, ct));
        jobs.MapGet("/{id:guid}", (Guid id, JobService s, CancellationToken ct) => s.GetAsync(id, ct));
    }
}
