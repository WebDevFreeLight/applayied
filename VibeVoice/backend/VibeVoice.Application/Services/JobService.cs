using System.Text.Json;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>Wakes the background worker when a job is queued (implemented in the API host).</summary>
public interface IJobQueue
{
    void Enqueue(Guid jobId);
}

/// <summary>What a queued job needs to run later. Only the fields its <see cref="JobKind"/> uses are set.</summary>
public sealed record JobPayload(
    Guid? TargetId = null,
    OwnerType? OwnerType = null,
    Guid? OwnerId = null,
    bool AppendToBody = false,
    string? Language = null,
    bool Save = true,
    AudioSource Source = AudioSource.Uploaded,
    string? FileName = null,
    string? ContentType = null);

/// <summary>
/// A background job with everything the Jobs page shows. <see cref="UserEmail"/>, <see cref="UserName"/> and
/// <see cref="ErrorDetail"/> are only filled in for the administrators' view.
/// </summary>
public sealed record JobDto(
    Guid Id, JobKind Kind, JobStatus Status, DateTime CreatedAt, DateTime? StartedAt, DateTime? FinishedAt,
    int Attempts, int? ErrorStatus, string? ErrorTitle, string? Error, JsonElement? Result,
    JobPayload? Payload = null, double? DurationSeconds = null, bool CanRetry = false, bool CanDelete = false,
    Guid? UserId = null, string? UserEmail = null, string? UserName = null, string? ErrorDetail = null);

/// <summary>Filters for the Jobs page. <c>Status</c> also accepts "active" (queued + running).</summary>
public sealed record JobQuery(string? Status = null, JobKind? Kind = null, Guid? UserId = null, int Page = 1, int PageSize = 20);

public sealed record JobSummaryDto(int Queued, int Running, int Succeeded, int Failed, int Total);

/// <summary>
/// Queues OCR / transcription work instead of doing it inside the HTTP request. The caller gets a
/// <see cref="JobDto"/> right away and polls GET /api/jobs/{id}; the API's worker runs the job as the
/// same user with <see cref="JobRunner"/> and stores exactly what the synchronous endpoint returns.
/// </summary>
public sealed class JobService(
    IRepository<BackgroundJob> jobs,
    IRepository<ImageAttachment> images,
    IRepository<AudioClip> audio,
    IRepository<User> users,
    IFileStorage storage,
    IJobQueue queue,
    ICurrentUser currentUser)
{
    internal static readonly JsonSerializerOptions PayloadJson = new(JsonSerializerDefaults.Web);

    /// <summary>Picture → text for a stored image (note/memory when <paramref name="ownerType"/> is set, else any of the user's).</summary>
    public async Task<JobDto> EnqueueImageOcrAsync(Guid imageId, OwnerType? ownerType, Guid? ownerId, OcrRequest request, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var exists = await images.AnyAsync(i => i.Id == imageId && i.UserId == userId, ct);
        if (!exists) throw new NotFoundException("Image", imageId);
        return await QueueAsync(JobKind.ImageOcr,
            new JobPayload(imageId, ownerType, ownerId, request.AppendToBody, request.Language), null, ct);
    }

    public async Task<JobDto> EnqueueTranscribeAsync(OwnerType ownerType, Guid ownerId, Guid audioId, TranscribeRequest request, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var exists = await audio.AnyAsync(a => a.Id == audioId && a.UserId == userId && a.OwnerType == ownerType && a.OwnerId == ownerId, ct);
        if (!exists) throw new NotFoundException("Audio clip", audioId);
        return await QueueAsync(JobKind.AudioTranscribe,
            new JobPayload(audioId, ownerType, ownerId, request.AppendToBody, request.Language), null, ct);
    }

    /// <summary>Voice studio upload (picture or audio): the file waits in storage until the job runs.</summary>
    public async Task<JobDto> EnqueueStudioAsync(JobKind kind, UploadFile file, string? language, bool save, AudioSource source, CancellationToken ct)
    {
        if (kind is not (JobKind.StudioOcr or JobKind.StudioTranscribe))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (file.Length == 0) throw new BadRequestException("The uploaded file is empty.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || ext.Length > 10) ext = ".bin";
        string path;
        await using (var stream = file.OpenRead())
            path = await storage.SaveAsync(stream, "jobs", ext.ToLowerInvariant(), ct);

        try
        {
            return await QueueAsync(kind,
                new JobPayload(Language: language, Save: save, Source: source, FileName: file.FileName, ContentType: file.ContentType),
                path, ct);
        }
        catch
        {
            await storage.DeleteAsync(path, CancellationToken.None);
            throw;
        }
    }

    // ---------- Reading ----------

    /// <summary>One job: the owner's own, or anyone's for the administrators' view (<paramref name="admin"/>).</summary>
    public async Task<JobDto> GetAsync(Guid id, CancellationToken ct, bool admin = false)
    {
        var job = await FindAsync(id, admin, ct);
        return admin ? (await WithUsersAsync([job], ct)).Single() : ToDto(job);
    }

    /// <summary>Paged, newest first. Members see their own jobs; <paramref name="admin"/> sees everybody's.</summary>
    public async Task<PagedResult<JobDto>> QueryAsync(JobQuery q, CancellationToken ct, bool admin = false)
    {
        var page = Math.Max(1, q.Page);
        var size = Math.Clamp(q.PageSize, 1, 100);
        var (items, total) = await jobs.PageAsync(Filter(q, admin), j => j.CreatedAt, true, page, size, ct);
        var dtos = admin ? await WithUsersAsync(items, ct) : items.Select(j => ToDto(j)).ToList();
        return new PagedResult<JobDto>(dtos, page, size, total);
    }

    /// <summary>Counts per status for the summary tiles (same scope as <see cref="QueryAsync"/>).</summary>
    public async Task<JobSummaryDto> SummaryAsync(CancellationToken ct, bool admin = false, Guid? userId = null)
    {
        Guid? scope = admin ? userId : currentUser.UserId;
        async Task<int> Count(JobStatus status) =>
            await jobs.CountAsync(j => (scope == null || j.UserId == scope) && j.Status == status, ct);
        var queued = await Count(JobStatus.Queued);
        var running = await Count(JobStatus.Running);
        var succeeded = await Count(JobStatus.Succeeded);
        var failed = await Count(JobStatus.Failed);
        return new JobSummaryDto(queued, running, succeeded, failed, queued + running + succeeded + failed);
    }

    /// <summary>Kept for clients that poll "my active jobs" (GET /api/jobs?active=true).</summary>
    public async Task<List<JobDto>> ListAsync(bool activeOnly, CancellationToken ct) =>
        (await QueryAsync(new JobQuery(activeOnly ? "active" : null, PageSize: 50), ct)).Items.ToList();

    // ---------- Managing ----------

    /// <summary>Runs a failed job again (as its owner). Studio uploads are kept after a failure for this.</summary>
    public async Task<JobDto> RetryAsync(Guid id, CancellationToken ct, bool admin = false)
    {
        var job = await FindAsync(id, admin, ct);
        if (job.Status != JobStatus.Failed)
            throw new ConflictException("Only failed jobs can be retried.");
        if (job.Kind is JobKind.StudioOcr or JobKind.StudioTranscribe && string.IsNullOrEmpty(job.TempFilePath))
            throw new ConflictException("The uploaded file of this job is gone; upload it again.");

        job.Status = JobStatus.Queued;
        job.Attempts = 0;
        job.ErrorStatus = null;
        job.ErrorTitle = null;
        job.Error = null;
        job.ErrorDetail = null;
        job.ResultJson = null;
        job.StartedAt = null;
        job.FinishedAt = null;
        await jobs.UpdateAsync(job, ct);
        queue.Enqueue(job.Id);
        return admin ? (await WithUsersAsync([job], ct)).Single() : ToDto(job);
    }

    /// <summary>Removes a finished job (and a studio upload still waiting for a retry).</summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct, bool admin = false)
    {
        var job = await FindAsync(id, admin, ct);
        if (job.Status is JobStatus.Queued or JobStatus.Running)
            throw new ConflictException("A job that is still queued or running can't be deleted.");
        await RemoveAsync(job, ct);
    }

    /// <summary>Deletes all of the user's finished jobs; returns how many.</summary>
    public async Task<int> ClearFinishedAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var done = await jobs.ListAsync(j => j.UserId == userId && (j.Status == JobStatus.Succeeded || j.Status == JobStatus.Failed), ct);
        foreach (var job in done) await RemoveAsync(job, ct);
        return done.Count;
    }

    private async Task RemoveAsync(BackgroundJob job, CancellationToken ct)
    {
        var id = job.Id;
        await jobs.DeleteAsync(j => j.Id == id, ct);
        if (!string.IsNullOrEmpty(job.TempFilePath))
            await storage.DeleteAsync(job.TempFilePath, CancellationToken.None);
    }

    private async Task<BackgroundJob> FindAsync(Guid id, bool admin, CancellationToken ct)
    {
        Guid? userId = admin ? null : currentUser.UserId;
        return await jobs.FirstOrDefaultAsync(j => j.Id == id && (userId == null || j.UserId == userId), ct)
            ?? throw new NotFoundException("Job", id);
    }

    private System.Linq.Expressions.Expression<Func<BackgroundJob, bool>> Filter(JobQuery q, bool admin)
    {
        Guid? user = admin ? q.UserId : currentUser.UserId;
        var kind = q.Kind;
        var status = (q.Status ?? "").Trim();
        var active = status.Equals("active", StringComparison.OrdinalIgnoreCase);
        JobStatus? exact = !active && Enum.TryParse<JobStatus>(status, true, out var parsed) ? parsed : null;
        return j => (user == null || j.UserId == user)
                    && (kind == null || j.Kind == kind)
                    && (!active || j.Status == JobStatus.Queued || j.Status == JobStatus.Running)
                    && (exact == null || j.Status == exact);
    }

    private async Task<List<JobDto>> WithUsersAsync(IReadOnlyCollection<BackgroundJob> list, CancellationToken ct)
    {
        var ids = list.Select(j => j.UserId).Distinct().ToList();
        var owners = (await users.ListAsync(u => ids.Contains(u.Id), ct)).ToDictionary(u => u.Id);
        return list.Select(j => ToDto(j, owners.GetValueOrDefault(j.UserId), adminView: true)).ToList();
    }

    private async Task<JobDto> QueueAsync(JobKind kind, JobPayload payload, string? tempFile, CancellationToken ct)
    {
        var job = new BackgroundJob
        {
            UserId = currentUser.UserId,
            IsAdmin = currentUser.IsAdmin,
            Kind = kind,
            PayloadJson = JsonSerializer.Serialize(payload, PayloadJson),
            TempFilePath = tempFile,
        };
        await jobs.InsertAsync(job, ct);
        queue.Enqueue(job.Id);
        return ToDto(job);
    }

    public static JobDto ToDto(BackgroundJob job, User? owner = null, bool adminView = false)
    {
        JsonElement? result = null;
        if (!string.IsNullOrEmpty(job.ResultJson))
        {
            using var doc = JsonDocument.Parse(job.ResultJson);
            result = doc.RootElement.Clone();
        }
        JobPayload? payload = null;
        try
        {
            if (!string.IsNullOrEmpty(job.PayloadJson)) payload = JsonSerializer.Deserialize<JobPayload>(job.PayloadJson, PayloadJson);
        }
        catch (JsonException)
        {
            // An unreadable payload shouldn't hide the job.
        }

        double? duration = job is { StartedAt: { } started, FinishedAt: { } finishedAt }
            ? Math.Round((finishedAt - started).TotalSeconds, 2)
            : null;
        var finished = job.Status is JobStatus.Succeeded or JobStatus.Failed;
        var canRetry = job.Status == JobStatus.Failed
                       && (job.Kind is JobKind.ImageOcr or JobKind.AudioTranscribe || !string.IsNullOrEmpty(job.TempFilePath));
        return new JobDto(job.Id, job.Kind, job.Status, job.CreatedAt, job.StartedAt, job.FinishedAt,
            job.Attempts, job.ErrorStatus, job.ErrorTitle, job.Error, result,
            payload, duration, canRetry, finished,
            adminView ? job.UserId : null, adminView ? owner?.Email : null, adminView ? owner?.DisplayName : null,
            adminView ? job.ErrorDetail : null);
    }
}

/// <summary>Runs one job inside a scope whose <see cref="ICurrentUser"/> is the job's owner.</summary>
public sealed class JobRunner(AttachmentService attachments, VoiceStudioService studio, IFileStorage storage)
{
    /// <summary>Returns the DTO the synchronous endpoint returns (serialized by the host).</summary>
    public async Task<object> RunAsync(BackgroundJob job, CancellationToken ct)
    {
        var p = JsonSerializer.Deserialize<JobPayload>(job.PayloadJson ?? "{}", JobService.PayloadJson) ?? new JobPayload();
        switch (job.Kind)
        {
            case JobKind.ImageOcr:
                return await attachments.ExtractTextAsync(Required(p.TargetId), new OcrRequest(p.AppendToBody, p.Language), ct,
                    p.OwnerType, p.OwnerId);

            case JobKind.AudioTranscribe:
                return await attachments.TranscribeAsync(Required(p.OwnerType), Required(p.OwnerId), Required(p.TargetId),
                    new TranscribeRequest(p.AppendToBody, p.Language), ct);

            case JobKind.StudioOcr:
                return await studio.RecognizeAsync(await LoadUploadAsync(job, p, ct), p.Language, p.Save, ct);

            case JobKind.StudioTranscribe:
                return await studio.TranscribeAsync(await LoadUploadAsync(job, p, ct), p.Language, p.Save, p.Source, ct);

            default:
                throw new BadRequestException($"Unknown job kind '{job.Kind}'.");
        }
    }

    private async Task<UploadFile> LoadUploadAsync(BackgroundJob job, JobPayload p, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(job.TempFilePath)) throw new NotFoundException("Upload for job", job.Id);
        byte[] bytes;
        await using (var stream = await storage.OpenReadAsync(job.TempFilePath, ct)
            ?? throw new NotFoundException("Upload for job", job.Id))
        using (var buffer = new MemoryStream())
        {
            await stream.CopyToAsync(buffer, ct);
            bytes = buffer.ToArray();
        }
        // The studio services open the file more than once (signature probe, then the real read).
        return new UploadFile(p.FileName ?? Path.GetFileName(job.TempFilePath), p.ContentType ?? "application/octet-stream",
            bytes.Length, () => new MemoryStream(bytes, writable: false));
    }

    private static T Required<T>(T? value) where T : struct =>
        value ?? throw new BadRequestException("The job is missing a required parameter.");
}
