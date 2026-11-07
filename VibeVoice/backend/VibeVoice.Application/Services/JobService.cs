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

public sealed record JobDto(
    Guid Id, JobKind Kind, JobStatus Status, DateTime CreatedAt, DateTime? StartedAt, DateTime? FinishedAt,
    int Attempts, int? ErrorStatus, string? ErrorTitle, string? Error, JsonElement? Result);

/// <summary>
/// Queues OCR / transcription work instead of doing it inside the HTTP request. The caller gets a
/// <see cref="JobDto"/> right away and polls GET /api/jobs/{id}; the API's worker runs the job as the
/// same user with <see cref="JobRunner"/> and stores exactly what the synchronous endpoint returns.
/// </summary>
public sealed class JobService(
    IRepository<BackgroundJob> jobs,
    IRepository<ImageAttachment> images,
    IRepository<AudioClip> audio,
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

    public async Task<JobDto> GetAsync(Guid id, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var job = await jobs.FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId, ct)
            ?? throw new NotFoundException("Job", id);
        return ToDto(job);
    }

    /// <summary>The user's most recent jobs; <paramref name="activeOnly"/> keeps queued and running ones.</summary>
    public async Task<List<JobDto>> ListAsync(bool activeOnly, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var list = activeOnly
            ? await jobs.ListAsync(j => j.UserId == userId && (j.Status == JobStatus.Queued || j.Status == JobStatus.Running),
                j => j.CreatedAt, true, 50, ct)
            : await jobs.ListAsync(j => j.UserId == userId, j => j.CreatedAt, true, 50, ct);
        return list.Select(ToDto).ToList();
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

    public static JobDto ToDto(BackgroundJob job)
    {
        JsonElement? result = null;
        if (!string.IsNullOrEmpty(job.ResultJson))
        {
            using var doc = JsonDocument.Parse(job.ResultJson);
            result = doc.RootElement.Clone();
        }
        return new JobDto(job.Id, job.Kind, job.Status, job.CreatedAt, job.StartedAt, job.FinishedAt,
            job.Attempts, job.ErrorStatus, job.ErrorTitle, job.Error, result);
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
