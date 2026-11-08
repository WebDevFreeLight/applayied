using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Services;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Api.Infrastructure;

/// <summary>In-memory wake-up signal; the database row is the source of truth.</summary>
public sealed class JobQueue : IJobQueue
{
    private readonly Channel<Guid> channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions { SingleReader = true });

    public ChannelReader<Guid> Reader => channel.Reader;

    public void Enqueue(Guid jobId) => channel.Writer.TryWrite(jobId);
}

public sealed class JobOptions
{
    public const string Section = "Jobs";

    /// <summary>
    /// Longest a single OCR / transcription may run before it is failed. Long recordings on a slow CPU take
    /// several times their length; keep this above VoiceService:RequestTimeoutSeconds.
    /// </summary>
    public int TimeoutMinutes { get; set; } = 60;

    /// <summary>Tries when the voice service is asleep or restarting (free hosts) before the job fails.</summary>
    public int MaxAttempts { get; set; } = 4;

    public int RetryDelaySeconds { get; set; } = 20;

    /// <summary>Finished jobs older than this are deleted at start-up.</summary>
    public int KeepDays { get; set; } = 7;
}

/// <summary>
/// Runs queued OCR / transcription jobs one at a time (the voice service keeps a single model in memory
/// on small hosts, so parallel jobs would only fight over it). Jobs left queued or running by a previous
/// process are picked up again at start-up.
/// </summary>
public sealed class JobWorker(
    IServiceScopeFactory scopes,
    JobQueue queue,
    IOptions<JobOptions> options,
    IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> json,
    ILogger<JobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RecoverAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Could not recover background jobs at start-up");
        }

        await foreach (var id in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await RunOneAsync(id, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // shutting down: the job stays Running and is re-queued at the next start-up
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background job {JobId} crashed the worker loop", id);
            }
        }
    }

    private async Task RecoverAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IRepository<BackgroundJob>>();
        var cutoff = DateTime.UtcNow.AddDays(-options.Value.KeepDays);
        var expired = await jobs.ListAsync(j => (j.Status == JobStatus.Succeeded || j.Status == JobStatus.Failed) && j.CreatedAt < cutoff, ct);
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        foreach (var temp in expired.Select(j => j.TempFilePath).OfType<string>().Where(t => t.Length > 0))
        {
            try { await storage.DeleteAsync(temp, ct); }
            catch (Exception ex) { logger.LogWarning(ex, "Could not delete the upload {Path} of an expired job", temp); }
        }
        var purged = expired.Count == 0 ? 0
            : await jobs.DeleteAsync(j => (j.Status == JobStatus.Succeeded || j.Status == JobStatus.Failed) && j.CreatedAt < cutoff, ct);

        var pending = await jobs.ListAsync(j => j.Status == JobStatus.Queued || j.Status == JobStatus.Running,
            j => j.CreatedAt, false, 500, ct);
        foreach (var job in pending) queue.Enqueue(job.Id);
        if (pending.Count > 0 || purged > 0)
            logger.LogInformation("Background jobs: {Pending} re-queued, {Purged} old ones removed", pending.Count, purged);
    }

    private async Task RunOneAsync(Guid id, CancellationToken stoppingToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var jobs = sp.GetRequiredService<IRepository<BackgroundJob>>();
        var job = await jobs.GetByIdAsync(id, stoppingToken);
        if (job is null || job.Status is JobStatus.Succeeded or JobStatus.Failed) return;

        // Every service in this scope now sees the job's owner as the signed-in user.
        sp.GetRequiredService<HttpCurrentUser>().RunAs(job.UserId, job.IsAdmin);

        job.Status = JobStatus.Running;
        job.StartedAt = DateTime.UtcNow;
        job.Attempts++;
        await jobs.UpdateAsync(job, stoppingToken);
        logger.LogInformation("Background job {JobId} ({Kind}) started, attempt {Attempt}", job.Id, job.Kind, job.Attempts);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(options.Value.TimeoutMinutes));
        try
        {
            var result = await sp.GetRequiredService<JobRunner>().RunAsync(job, timeout.Token);
            job.ResultJson = JsonSerializer.Serialize(result, result.GetType(), json.Value.SerializerOptions);
            job.Status = JobStatus.Succeeded;
            job.ErrorStatus = null;
            job.ErrorTitle = null;
            job.Error = null;
            job.ErrorDetail = null;
        }
        catch (VoiceServiceUnavailableException ex) when (!ex.TimedOut && job.Attempts < options.Value.MaxAttempts && !stoppingToken.IsCancellationRequested)
        {
            // A sleeping free-tier voice service answers once it has woken up: try again shortly.
            job.Status = JobStatus.Queued;
            job.Error = ex.Message;
            job.ErrorDetail = Detail(ex);
            await jobs.UpdateAsync(job, CancellationToken.None);
            var delay = TimeSpan.FromSeconds(options.Value.RetryDelaySeconds * job.Attempts);
            logger.LogWarning("Background job {JobId}: voice service unavailable, retrying in {Delay}", job.Id, delay);
            _ = Task.Delay(delay, stoppingToken).ContinueWith(t => { if (!t.IsCanceled) queue.Enqueue(id); }, TaskScheduler.Default);
            return;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            (job.ErrorStatus, job.ErrorTitle, job.Error) = ex switch
            {
                AppException app => (app.StatusCode, app.Title, app.Message),
                OperationCanceledException => (504, "Timed out",
                    $"The job did not finish within {options.Value.TimeoutMinutes} minutes."),
                _ => (500, "Unexpected error", "Something went wrong. Please try again."),
            };
            job.ErrorDetail = Detail(ex);
            if (ex is not AppException) logger.LogError(ex, "Background job {JobId} ({Kind}) failed", job.Id, job.Kind);
            job.Status = JobStatus.Failed;
        }

        job.FinishedAt = DateTime.UtcNow;
        await jobs.UpdateAsync(job, CancellationToken.None);
        logger.LogInformation("Background job {JobId} ({Kind}) {Status}", job.Id, job.Kind, job.Status);

        // A failed studio job keeps its upload so it can be retried from the Jobs page.
        if (job.Status == JobStatus.Succeeded && job.TempFilePath is { Length: > 0 } temp)
        {
            try
            {
                await sp.GetRequiredService<IFileStorage>().DeleteAsync(temp, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not delete the upload of job {JobId}", job.Id);
            }
            job.TempFilePath = null;
            await jobs.UpdateAsync(job, CancellationToken.None);
        }
    }

    /// <summary>Exception chain for administrators: type and message of each level (max 2000 chars).</summary>
    private static string Detail(Exception ex)
    {
        var parts = new List<string>();
        for (var e = ex; e is not null && parts.Count < 4; e = e.InnerException ?? (e as VoiceServiceUnavailableException)?.Upstream)
            parts.Add($"{e.GetType().Name}: {e.Message}");
        var text = string.Join(" --> ", parts);
        return text.Length > 2000 ? text[..2000] : text;
    }
}
