using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>The standalone "VibeVoice Studio": text→speech and speech→text that isn't tied to a note.</summary>
public sealed class VoiceStudioService(
    IVoiceServiceClient voice,
    AttachmentService attachments,
    IRepository<AudioClip> audio,
    IFileStorage storage,
    ICurrentUser currentUser)
{
    public async Task<VoiceStatusDto> StatusAsync(CancellationToken ct)
    {
        var health = await voice.GetHealthAsync(ct);
        return health is null
            ? new VoiceStatusDto(false, null, null, null, "The voice service is not reachable. Text, images and recordings still work.")
            : new VoiceStatusDto(true, health.TtsEngine, health.AsrEngine, health.Device, null);
    }

    public Task<IReadOnlyList<VoiceDto>> VoicesAsync(CancellationToken ct) => voice.GetVoicesAsync(ct);

    public Task<AudioDto> SpeakAsync(TtsRequest request, CancellationToken ct) =>
        attachments.GenerateTtsAsync(OwnerType.Studio, currentUser.UserId, request, ct);

    /// <summary>Transcribes an uploaded/recorded file; when <paramref name="save"/> is true the clip is kept in the studio history.</summary>
    public async Task<TranscriptionDto> TranscribeAsync(UploadFile file, string? language, bool save, AudioSource source, CancellationToken ct)
    {
        AudioClip? clip = null;
        AsrResult result;
        if (save)
        {
            var dto = await attachments.UploadAudioAsync(OwnerType.Studio, currentUser.UserId, file, source, null, ct);
            clip = await audio.GetByIdAsync(dto.Id, ct);
            await using var stored = await storage.OpenReadAsync(dto.FilePath, ct) ?? throw new NotFoundException("Audio file", dto.FilePath);
            result = await voice.TranscribeAsync(stored, Path.GetFileName(dto.FilePath), dto.MimeType, language, ct);
        }
        else
        {
            await using var stream = file.OpenRead();
            result = await voice.TranscribeAsync(stream, file.FileName, file.ContentType, language, ct);
        }

        if (clip is not null)
        {
            clip.Transcript = result.Text.Trim();
            clip.TranscriptLanguage = result.Language ?? language;
            clip.SearchText = SearchIndex.ForAudio(clip);
            await audio.UpdateAsync(clip, ct);
        }
        return new TranscriptionDto(result.Text.Trim(), result.Language ?? language, result.Segments, clip?.ToDto());
    }

    public Task DeleteAsync(Guid id, CancellationToken ct) =>
        attachments.DeleteAudioAsync(id, ct, OwnerType.Studio, currentUser.UserId);
}
