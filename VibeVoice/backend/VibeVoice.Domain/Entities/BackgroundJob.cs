using SqlSugar;

namespace VibeVoice.Domain.Entities;

public enum JobKind
{
    /// <summary>Text from a note/memory/library picture (POST .../images/{id}/ocr).</summary>
    ImageOcr = 1,
    /// <summary>Transcript of a note/memory clip (POST .../audio/{id}/transcribe).</summary>
    AudioTranscribe = 2,
    /// <summary>Voice studio picture → text (POST /api/voice/ocr); the upload waits in storage.</summary>
    StudioOcr = 3,
    /// <summary>Voice studio speech → text (POST /api/voice/transcribe); the upload waits in storage.</summary>
    StudioTranscribe = 4,
}

public enum JobStatus
{
    Queued = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4,
}

/// <summary>
/// One OCR / transcription request run in the background. Kept in the database so queued work
/// survives an API restart (free hosts sleep and restart often) and the result can be fetched later.
/// </summary>
[SugarTable("background_jobs")]
[SugarIndex("ix_jobs_user_created", nameof(UserId), OrderByType.Asc, nameof(CreatedAt), OrderByType.Desc)]
[SugarIndex("ix_jobs_status", nameof(Status), OrderByType.Asc)]
public class BackgroundJob
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The job runs as this user, exactly as the original request would have.</summary>
    public Guid UserId { get; set; }

    public bool IsAdmin { get; set; }

    public JobKind Kind { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Queued;

    /// <summary>Request parameters (ids, language, flags) as JSON.</summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? PayloadJson { get; set; }

    /// <summary>The DTO the synchronous endpoint would have returned, as JSON.</summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? ResultJson { get; set; }

    /// <summary>HTTP status the synchronous endpoint would have answered with (e.g. 404, 503).</summary>
    [SugarColumn(IsNullable = true)]
    public int? ErrorStatus { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? ErrorTitle { get; set; }

    [SugarColumn(Length = 1000, IsNullable = true)]
    public string? Error { get; set; }

    /// <summary>Studio uploads wait here until the job runs; deleted when it finishes.</summary>
    [SugarColumn(Length = 300, IsNullable = true)]
    public string? TempFilePath { get; set; }

    public int Attempts { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? FinishedAt { get; set; }
}
