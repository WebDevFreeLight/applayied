using SqlSugar;

namespace VibeVoice.Domain.Entities;

[SugarTable("audio_clips")]
[SugarIndex("ix_audio_owner", nameof(OwnerType), OrderByType.Asc, nameof(OwnerId), OrderByType.Asc)]
[SugarIndex("ix_audio_user_created", nameof(UserId), OrderByType.Asc, nameof(CreatedAt), OrderByType.Desc)]
public class AudioClip
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public OwnerType OwnerType { get; set; }

    /// <summary>Note/memory id, or the user id for Studio clips.</summary>
    public Guid OwnerId { get; set; }

    public AudioSource Source { get; set; }

    /// <summary>Path relative to the storage root, e.g. <c>audio/2026/11/{guid}.webm</c>.</summary>
    [SugarColumn(Length = 300)]
    public string FilePath { get; set; } = "";

    [SugarColumn(Length = 100)]
    public string MimeType { get; set; } = "";

    [SugarColumn(IsNullable = true)]
    public double? DurationSeconds { get; set; }

    public long SizeBytes { get; set; }

    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? Transcript { get; set; }

    [SugarColumn(Length = 10, IsNullable = true)]
    public string? TranscriptLanguage { get; set; }

    /// <summary>Normalized transcript (or TTS text) used by search.</summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? SearchText { get; set; }

    /// <summary>Voice preset used for TTS clips.</summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? VoiceName { get; set; }

    /// <summary>Engine that produced a TTS clip (vibevoice / edge / dev).</summary>
    [SugarColumn(Length = 30, IsNullable = true)]
    public string? TtsEngine { get; set; }

    /// <summary>The text a TTS clip was generated from.</summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? Text { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[SugarTable("image_attachments")]
[SugarIndex("ix_images_owner", nameof(OwnerType), OrderByType.Asc, nameof(OwnerId), OrderByType.Asc)]
[SugarIndex("ix_images_user_created", nameof(UserId), OrderByType.Asc, nameof(CreatedAt), OrderByType.Desc)]
public class ImageAttachment
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public OwnerType OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    [SugarColumn(Length = 300)]
    public string FilePath { get; set; } = "";

    [SugarColumn(Length = 300)]
    public string ThumbnailPath { get; set; } = "";

    [SugarColumn(Length = 50)]
    public string MimeType { get; set; } = "";

    public int Width { get; set; }

    public int Height { get; set; }

    public long SizeBytes { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Caption { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
