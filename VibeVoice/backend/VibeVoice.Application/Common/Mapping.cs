using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Common;

public static class Mapping
{
    public static UserDto ToDto(this User u) => new(
        u.Id, u.Email, u.DisplayName, u.Role, u.PreferredLanguage, u.PreferredVoice, u.Theme, u.Palette, u.IsActive, u.CreatedAt);

    public static ImageDto ToDto(this ImageAttachment i) => new(
        i.Id, i.OwnerType, i.OwnerId, i.FilePath, i.ThumbnailPath, i.MimeType, i.Width, i.Height,
        i.SizeBytes, i.Caption, i.SortOrder, i.CreatedAt, i.OcrText, i.OcrLanguage);

    public static ImageWithOwnerDto ToDto(this ImageAttachment i, string? ownerTitle) => new(
        i.Id, i.OwnerType, i.OwnerId, i.FilePath, i.ThumbnailPath, i.MimeType, i.Width, i.Height,
        i.SizeBytes, i.Caption, i.SortOrder, i.CreatedAt, i.OcrText, i.OcrLanguage, ownerTitle);

    public static AudioDto ToDto(this AudioClip a) => new(
        a.Id, a.OwnerType, a.OwnerId, a.Source, a.MediaKind, a.FilePath, a.MimeType, a.DurationSeconds, a.SizeBytes,
        a.Transcript, a.TranscriptLanguage, TranscriptFormats.Deserialize(a.SegmentsJson), a.VoiceName, a.TtsEngine, a.Text, a.Source == AudioSource.Tts, a.CreatedAt);

    public static AudioWithOwnerDto ToDto(this AudioClip a, string? ownerTitle) => new(
        a.Id, a.OwnerType, a.OwnerId, a.Source, a.MediaKind, a.FilePath, a.MimeType, a.DurationSeconds, a.SizeBytes,
        a.Transcript, a.TranscriptLanguage, TranscriptFormats.Deserialize(a.SegmentsJson), a.VoiceName, a.TtsEngine, a.Text, a.Source == AudioSource.Tts, a.CreatedAt, ownerTitle);

    public static DateOnly ToDateOnly(this DateTime d) => DateOnly.FromDateTime(d);

    public static DateTime ToDateTime(this DateOnly d) => d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
}

/// <summary>Builds the normalized <c>SearchText</c> columns.</summary>
public static class SearchIndex
{
    public static string ForNote(Note n, IEnumerable<string> tags) =>
        TextNormalizer.Normalize($"{n.Title}\n{HtmlText.ToPlainText(n.Body)}\n{string.Join(' ', tags)}");

    public static string ForMemory(Memory m, IEnumerable<string> tags, IEnumerable<string> people) =>
        TextNormalizer.Normalize(
            $"{m.Title}\n{HtmlText.ToPlainText(m.Story)}\n{m.Location}\n{string.Join(' ', tags)}\n{string.Join(' ', people)}");

    public static string? ForImage(ImageAttachment i) =>
        i.OcrText is null && i.Caption is null ? null : TextNormalizer.Normalize($"{i.Caption}\n{i.OcrText}");

    public static string? ForAudio(AudioClip a) =>
        a.Transcript is null && a.Text is null ? null : TextNormalizer.Normalize($"{a.Transcript}\n{a.Text}");
}
