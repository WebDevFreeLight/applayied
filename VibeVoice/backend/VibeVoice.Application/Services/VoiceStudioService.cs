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
    ICurrentUser currentUser,
    Microsoft.Extensions.Options.IOptions<UploadOptions> limits)
{
    public async Task<VoiceStatusDto> StatusAsync(CancellationToken ct)
    {
        var health = await voice.GetHealthAsync(ct);
        return health is null
            ? new VoiceStatusDto(false, null, null, null, null, "The voice service is not reachable. Text, images and recordings still work.")
            : new VoiceStatusDto(true, health.TtsEngine, health.AsrEngine, health.OcrEngine is null or "none" ? null : health.OcrEngine, health.Device, null);
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
            AttachmentService.ApplyTranscript(clip, result, language);
            await audio.UpdateAsync(clip, ct);
        }
        return new TranscriptionDto(result.Text.Trim(), result.Language ?? language, result.Segments, clip?.ToDto());
    }

    /// <summary>
    /// Picture → text. With <paramref name="save"/> the picture is kept in the Images library (and its text is
    /// stored and searchable); otherwise it is only validated and sent to the OCR engine.
    /// </summary>
    public async Task<OcrResultDto> RecognizeAsync(UploadFile file, string? language, bool save, CancellationToken ct)
    {
        if (save)
        {
            var stored = (await attachments.UploadImagesAsync(OwnerType.Library, Guid.Empty, [file], ct)).Single();
            var (image, ocr) = await attachments.ExtractTextWithLinesAsync(stored.Id, new OcrRequest(false, language), ct);
            return new OcrResultDto(image.OcrText ?? "", image.OcrLanguage, ocr.Lines, image);
        }

        if (file.Length == 0) throw new BadRequestException("The image is empty.");
        if (file.Length > limits.Value.MaxImageBytes)
            throw new PayloadTooLargeException($"Images must be smaller than {limits.Value.MaxImageBytes / 1024 / 1024} MB.");
        byte[] header;
        await using (var probe = file.OpenRead()) header = await FileSignatures.ReadHeaderAsync(probe, ct);
        var detected = FileSignatures.DetectImage(header)
            ?? throw new BadRequestException($"'{file.FileName}' is not a supported image (jpeg, png, webp, gif).");

        await using var stream = file.OpenRead();
        var result = await voice.RecognizeTextAsync(stream, file.FileName, detected.MimeType, language, ct);
        return new OcrResultDto(result.Text.Trim(), result.Language ?? language, result.Lines, null);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct) =>
        attachments.DeleteAudioAsync(id, ct, OwnerType.Studio, currentUser.UserId);
}
