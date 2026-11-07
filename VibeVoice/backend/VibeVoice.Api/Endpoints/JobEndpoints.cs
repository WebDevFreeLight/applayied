using VibeVoice.Api.Infrastructure;
using VibeVoice.Application.Services;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

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
        // ---- The signed-in user's own jobs ----
        var jobs = app.MapGroup("/api/jobs").WithTags("Jobs").RequireAuthorization();
        // ?active=true keeps the plain "my running jobs" array; otherwise a page filtered by status/kind.
        jobs.MapGet("/", async (bool? active, string? status, JobKind? kind, int? page, int? pageSize, JobService s, CancellationToken ct) =>
            active == true
                ? Results.Ok(await s.ListAsync(true, ct))
                : Results.Ok(await s.QueryAsync(new JobQuery(status, kind, null, page ?? 1, pageSize ?? 20), ct)));
        jobs.MapGet("/summary", (JobService s, CancellationToken ct) => s.SummaryAsync(ct));
        jobs.MapGet("/{id:guid}", (Guid id, JobService s, CancellationToken ct) => s.GetAsync(id, ct));
        jobs.MapPost("/{id:guid}/retry", (Guid id, JobService s, CancellationToken ct) => s.RetryAsync(id, ct));
        jobs.MapDelete("/{id:guid}", async (Guid id, JobService s, CancellationToken ct) =>
        {
            await s.DeleteAsync(id, ct);
            return Results.NoContent();
        });
        jobs.MapDelete("/finished", async (JobService s, CancellationToken ct) =>
            Results.Ok(new { deleted = await s.ClearFinishedAsync(ct) }));

        // ---- Administrators: every user's jobs, with the technical error detail ----
        var admin = app.MapGroup("/api/admin/jobs").WithTags("Jobs").RequireAuthorization();
        admin.MapGet("/", (string? status, JobKind? kind, Guid? userId, int? page, int? pageSize, JobService s, CancellationToken ct) =>
            s.QueryAsync(new JobQuery(status, kind, userId, page ?? 1, pageSize ?? 20), ct, admin: true))
            .RequirePermission(Permissions.UsersView);
        admin.MapGet("/summary", (Guid? userId, JobService s, CancellationToken ct) => s.SummaryAsync(ct, admin: true, userId))
            .RequirePermission(Permissions.UsersView);
        admin.MapGet("/{id:guid}", (Guid id, JobService s, CancellationToken ct) => s.GetAsync(id, ct, admin: true))
            .RequirePermission(Permissions.UsersView);
        admin.MapPost("/{id:guid}/retry", (Guid id, JobService s, CancellationToken ct) => s.RetryAsync(id, ct, admin: true))
            .RequirePermission(Permissions.UsersManage);
        admin.MapDelete("/{id:guid}", async (Guid id, JobService s, CancellationToken ct) =>
        {
            await s.DeleteAsync(id, ct, admin: true);
            return Results.NoContent();
        }).RequirePermission(Permissions.UsersManage);
    }
}
