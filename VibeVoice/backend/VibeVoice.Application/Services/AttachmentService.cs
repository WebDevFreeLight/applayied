using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>
/// Images and audio clips for any owner: notes, memories, the image library and the voice studio.
/// Files are written first, then rows are inserted in a transaction; if the insert fails the files are removed.
/// </summary>
public sealed class AttachmentService(
    IRepository<ImageAttachment> images,
    IRepository<AudioClip> audio,
    IRepository<Note> notes,
    IRepository<Memory> memories,
    TaggingService tagging,
    IFileStorage storage,
    IImageProcessor imageProcessor,
    IVoiceServiceClient voice,
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IOptions<UploadOptions> uploadOptions,
    ILogger<AttachmentService> logger)
{
    public const int MaxTtsChars = 5000;
    private readonly UploadOptions _limits = uploadOptions.Value;
    private Guid UserId => currentUser.UserId;

    // ---------- owners ----------

    /// <summary>Loads the note/memory and verifies it belongs to the current user (404 otherwise).</summary>
    public async Task<IOwnedContent> GetOwnerAsync(OwnerType type, Guid ownerId, CancellationToken ct) => type switch
    {
        OwnerType.Note => await notes.FirstOrDefaultAsync(n => n.Id == ownerId && n.UserId == UserId, ct)
                          ?? throw new NotFoundException("Note", ownerId),
        OwnerType.Memory => await memories.FirstOrDefaultAsync(m => m.Id == ownerId && m.UserId == UserId, ct)
                            ?? throw new NotFoundException("Memory", ownerId),
        _ => throw new BadRequestException($"Owner type '{type}' cannot be used here."),
    };

    private async Task TouchOwnerAsync(IOwnedContent owner, CancellationToken ct)
    {
        owner.UpdatedAt = DateTime.UtcNow;
        switch (owner)
        {
            case Note n: await notes.UpdateAsync(n, ct); break;
            case Memory m: await memories.UpdateAsync(m, ct); break;
        }
    }

    public async Task<List<ImageDto>> GetImagesAsync(OwnerType type, Guid ownerId, CancellationToken ct) =>
        (await images.ListAsync(i => i.OwnerType == type && i.OwnerId == ownerId && i.UserId == UserId, ct))
            .OrderBy(i => i.SortOrder).ThenBy(i => i.CreatedAt).Select(i => i.ToDto()).ToList();

    public async Task<List<AudioDto>> GetAudioAsync(OwnerType type, Guid ownerId, CancellationToken ct) =>
        (await audio.ListAsync(a => a.OwnerType == type && a.OwnerId == ownerId && a.UserId == UserId, ct))
            .OrderBy(a => a.CreatedAt).Select(a => a.ToDto()).ToList();

    /// <summary>
    /// Removes every attachment row of an owner (used by permanent delete) and returns the stored
    /// file paths, so the caller can delete the files only after its transaction has committed.
    /// </summary>
    public async Task<List<string>> DeleteAllRowsForOwnerAsync(OwnerType type, Guid ownerId, CancellationToken ct)
    {
        var imgs = await images.ListAsync(i => i.OwnerType == type && i.OwnerId == ownerId, ct);
        var clips = await audio.ListAsync(a => a.OwnerType == type && a.OwnerId == ownerId, ct);
        await images.DeleteAsync(i => i.OwnerType == type && i.OwnerId == ownerId, ct);
        await audio.DeleteAsync(a => a.OwnerType == type && a.OwnerId == ownerId, ct);
        return imgs.SelectMany(i => new[] { i.FilePath, i.ThumbnailPath }).Concat(clips.Select(c => c.FilePath)).ToList();
    }

    // ---------- images ----------

    public async Task<List<ImageDto>> UploadImagesAsync(OwnerType type, Guid ownerId, IReadOnlyList<UploadFile> files, CancellationToken ct)
    {
        IOwnedContent? owner = type == OwnerType.Library ? null : await GetOwnerAsync(type, ownerId, ct);
        if (files.Count == 0) throw new BadRequestException("No files were uploaded.");
        if (files.Count > _limits.MaxFilesPerRequest)
            throw new BadRequestException($"At most {_limits.MaxFilesPerRequest} files can be uploaded at once.");

        var existing = await images.ListAsync(i => i.OwnerType == type && i.OwnerId == ownerId, ct);
        var nextOrder = existing.Count == 0 ? 0 : existing.Max(i => i.SortOrder) + 1;
        var saved = new List<string>();
        var rows = new List<ImageAttachment>();

        try
        {
            foreach (var file in files)
            {
                if (file.Length > _limits.MaxImageBytes)
                    throw new PayloadTooLargeException($"'{file.FileName}' is larger than {_limits.MaxImageBytes / 1024 / 1024} MB.");

                await using var buffer = await BufferAsync(file, ct);
                var header = await FileSignatures.ReadHeaderAsync(buffer, ct);
                if (FileSignatures.DetectImage(header) is null)
                    throw new BadRequestException($"'{file.FileName}' is not a supported image (jpeg, png, webp, gif).");
                buffer.Position = 0;

                await using var processed = await ProcessImageAsync(buffer, file.FileName, ct);
                var path = await storage.SaveAsync(processed.Image, "images", processed.Extension, ct);
                saved.Add(path);
                var thumb = await storage.SaveAsync(processed.Thumbnail, "thumbs", "webp", ct);
                saved.Add(thumb);

                rows.Add(new ImageAttachment
                {
                    UserId = UserId,
                    OwnerType = type,
                    OwnerId = ownerId,
                    FilePath = path,
                    ThumbnailPath = thumb,
                    MimeType = processed.MimeType,
                    Width = processed.Width,
                    Height = processed.Height,
                    SizeBytes = processed.Image.Length,
                    SortOrder = nextOrder++,
                });
            }

            await uow.ExecuteInTransactionAsync(async () =>
            {
                await images.InsertRangeAsync(rows, ct);
                if (owner is not null) await TouchOwnerAsync(owner, ct);
            }, ct);
        }
        catch
        {
            await DeleteFilesQuietlyAsync(saved);
            throw;
        }

        return rows.Select(r => r.ToDto()).ToList();
    }

    private async Task<ProcessedImage> ProcessImageAsync(Stream input, string fileName, CancellationToken ct)
    {
        try
        {
            return await imageProcessor.ProcessAsync(input, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Image {FileName} could not be decoded", fileName);
            throw new BadRequestException($"'{fileName}' could not be read as an image.");
        }
    }

    private async Task<ImageAttachment> GetImageAsync(Guid imageId, CancellationToken ct) =>
        await images.FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == UserId, ct)
        ?? throw new NotFoundException("Image", imageId);

    public async Task<ImageDto> UpdateCaptionAsync(OwnerType type, Guid ownerId, Guid imageId, string? caption, CancellationToken ct)
    {
        var img = await GetImageAsync(imageId, ct);
        if (img.OwnerType != type || img.OwnerId != ownerId) throw new NotFoundException("Image", imageId);
        img.Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim()[..Math.Min(caption.Trim().Length, 500)];
        await images.UpdateAsync(img, ct);
        return img.ToDto();
    }

    public async Task<List<ImageDto>> ReorderImagesAsync(OwnerType type, Guid ownerId, IReadOnlyList<Guid> orderedIds, CancellationToken ct)
    {
        var owner = await GetOwnerAsync(type, ownerId, ct);
        var current = await images.ListAsync(i => i.OwnerType == type && i.OwnerId == ownerId, ct);
        if (orderedIds.Count != current.Count || orderedIds.Except(current.Select(i => i.Id)).Any())
            throw new BadRequestException("imageIds must contain every image of this item exactly once.");

        var position = orderedIds.Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => x.idx);
        foreach (var img in current) img.SortOrder = position[img.Id];
        await uow.ExecuteInTransactionAsync(async () =>
        {
            await images.UpdateRangeAsync(current, ct);
            await TouchOwnerAsync(owner, ct);
        }, ct);
        return current.OrderBy(i => i.SortOrder).Select(i => i.ToDto()).ToList();
    }

    public async Task DeleteImageAsync(Guid imageId, CancellationToken ct, OwnerType? expectedType = null, Guid? expectedOwner = null)
    {
        var img = await GetImageAsync(imageId, ct);
        if (expectedType is not null && (img.OwnerType != expectedType || img.OwnerId != expectedOwner))
            throw new NotFoundException("Image", imageId);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            await images.DeleteAsync(i => i.Id == img.Id, ct);
            if (img.OwnerType == OwnerType.Memory)
            {
                var memory = await memories.GetByIdAsync(img.OwnerId, ct);
                if (memory?.CoverImageId == img.Id)
                {
                    memory.CoverImageId = null;
                    await memories.UpdateAsync(memory, ct);
                }
            }
        }, ct);
        await DeleteFilesQuietlyAsync([img.FilePath, img.ThumbnailPath]);
    }

    /// <summary>Moves an image (e.g. from the library) onto a note or memory.</summary>
    public async Task<ImageDto> AttachImageAsync(Guid imageId, OwnerType type, Guid ownerId, CancellationToken ct)
    {
        var img = await GetImageAsync(imageId, ct);
        var owner = await GetOwnerAsync(type, ownerId, ct);
        var siblings = await images.ListAsync(i => i.OwnerType == type && i.OwnerId == ownerId, ct);

        if (img.OwnerType == OwnerType.Memory)
        {
            var oldMemory = await memories.GetByIdAsync(img.OwnerId, ct);
            if (oldMemory?.CoverImageId == img.Id)
            {
                oldMemory.CoverImageId = null;
                await memories.UpdateAsync(oldMemory, ct);
            }
        }

        img.OwnerType = type;
        img.OwnerId = ownerId;
        img.SortOrder = siblings.Count == 0 ? 0 : siblings.Max(i => i.SortOrder) + 1;
        await uow.ExecuteInTransactionAsync(async () =>
        {
            await images.UpdateAsync(img, ct);
            await TouchOwnerAsync(owner, ct);
        }, ct);
        return img.ToDto();
    }

    // ---------- audio ----------

    public async Task<AudioDto> UploadAudioAsync(OwnerType type, Guid ownerId, UploadFile file, AudioSource source, double? durationSeconds, CancellationToken ct)
    {
        IOwnedContent? owner = type == OwnerType.Studio ? null : await GetOwnerAsync(type, ownerId, ct);
        var (clip, _) = await SaveAudioFileAsync(type, type == OwnerType.Studio ? UserId : ownerId, file, source, durationSeconds, ct);

        try
        {
            await uow.ExecuteInTransactionAsync(async () =>
            {
                await audio.InsertAsync(clip, ct);
                if (owner is not null) await TouchOwnerAsync(owner, ct);
            }, ct);
        }
        catch
        {
            await DeleteFilesQuietlyAsync([clip.FilePath]);
            throw;
        }
        return clip.ToDto();
    }

    /// <summary>Validates and stores an audio upload; returns the (not yet inserted) row and the detected type.</summary>
    internal async Task<(AudioClip Clip, DetectedFile Detected)> SaveAudioFileAsync(
        OwnerType type, Guid ownerId, UploadFile file, AudioSource source, double? durationSeconds, CancellationToken ct)
    {
        if (source == AudioSource.Tts) throw new BadRequestException("Use the TTS endpoint to create AI voice clips.");
        if (file.Length == 0) throw new BadRequestException("The audio file is empty.");
        if (file.Length > _limits.MaxAudioBytes)
            throw new PayloadTooLargeException($"Audio files must be smaller than {_limits.MaxAudioBytes / 1024 / 1024} MB.");

        await using var buffer = await BufferAsync(file, ct);
        var header = await FileSignatures.ReadHeaderAsync(buffer, ct);
        var detected = FileSignatures.DetectAudio(header)
            ?? throw new BadRequestException($"'{file.FileName}' is not a supported audio file (webm, ogg, wav, mp3, m4a).");
        buffer.Position = 0;

        durationSeconds ??= detected.Extension == "wav" ? WavInfo.TryGetDuration(buffer) : null;
        buffer.Position = 0;

        var path = await storage.SaveAsync(buffer, "audio", detected.Extension, ct);
        var clip = new AudioClip
        {
            UserId = UserId,
            OwnerType = type,
            OwnerId = ownerId,
            Source = source,
            FilePath = path,
            MimeType = detected.MimeType,
            DurationSeconds = durationSeconds is > 0 and < 60 * 60 * 24 ? Math.Round(durationSeconds.Value, 2) : null,
            SizeBytes = buffer.Length,
        };
        return (clip, detected);
    }

    private async Task<AudioClip> GetClipAsync(Guid audioId, CancellationToken ct) =>
        await audio.FirstOrDefaultAsync(a => a.Id == audioId && a.UserId == UserId, ct)
        ?? throw new NotFoundException("Audio clip", audioId);

    public async Task DeleteAudioAsync(Guid audioId, CancellationToken ct, OwnerType? expectedType = null, Guid? expectedOwner = null)
    {
        var clip = await GetClipAsync(audioId, ct);
        if (expectedType is not null && (clip.OwnerType != expectedType || clip.OwnerId != expectedOwner))
            throw new NotFoundException("Audio clip", audioId);
        await audio.DeleteAsync(a => a.Id == clip.Id, ct);
        await DeleteFilesQuietlyAsync([clip.FilePath]);
    }

    /// <summary>
    /// Generates speech with the voice service and stores it as an AI-generated clip.
    /// For notes/memories the text defaults to the item's title + plain-text body.
    /// </summary>
    public async Task<AudioDto> GenerateTtsAsync(OwnerType type, Guid ownerId, TtsRequest request, CancellationToken ct)
    {
        IOwnedContent? owner = type == OwnerType.Studio ? null : await GetOwnerAsync(type, ownerId, ct);
        var text = request.Text;
        if (string.IsNullOrWhiteSpace(text) && owner is not null)
        {
            var body = owner switch { Note n => n.Body, Memory m => m.Story, _ => "" };
            text = $"{owner.Title}.\n{HtmlText.ToPlainText(body)}";
        }
        text = HtmlText.ToPlainText(text).Trim();
        if (text.Length == 0 || text == ".") throw new BadRequestException("There is no text to read aloud.");
        if (text.Length > MaxTtsChars) throw new BadRequestException($"Text is too long for speech generation (max {MaxTtsChars} characters).");

        var result = await voice.SynthesizeAsync(text, request.Voice, request.Language, ct);
        var ext = result.ContentType.Contains("mpeg") ? "mp3" : "wav";
        await using var ms = new MemoryStream(result.Audio, writable: false);
        var path = await storage.SaveAsync(ms, "tts", ext, ct);

        var clip = new AudioClip
        {
            UserId = UserId,
            OwnerType = type,
            OwnerId = type == OwnerType.Studio ? UserId : ownerId,
            Source = AudioSource.Tts,
            FilePath = path,
            MimeType = ext == "mp3" ? "audio/mpeg" : "audio/wav",
            SizeBytes = result.Audio.Length,
            DurationSeconds = result.DurationSeconds ?? (ext == "wav" ? WavInfo.TryGetDuration(new MemoryStream(result.Audio)) : null),
            VoiceName = result.Voice ?? request.Voice,
            TtsEngine = result.Engine,
            Text = text,
            TranscriptLanguage = result.Language ?? request.Language,
        };
        clip.SearchText = SearchIndex.ForAudio(clip);

        try
        {
            await uow.ExecuteInTransactionAsync(async () =>
            {
                await audio.InsertAsync(clip, ct);
                if (owner is not null) await TouchOwnerAsync(owner, ct);
            }, ct);
        }
        catch
        {
            await DeleteFilesQuietlyAsync([path]);
            throw;
        }
        return clip.ToDto();
    }

    /// <summary>Speech-to-text for a stored clip; optionally appends the transcript to the owner's body.</summary>
    public async Task<AudioDto> TranscribeAsync(OwnerType type, Guid ownerId, Guid audioId, TranscribeRequest request, CancellationToken ct)
    {
        var owner = await GetOwnerAsync(type, ownerId, ct);
        var clip = await GetClipAsync(audioId, ct);
        if (clip.OwnerType != type || clip.OwnerId != ownerId) throw new NotFoundException("Audio clip", audioId);

        await using var stream = await storage.OpenReadAsync(clip.FilePath, ct)
            ?? throw new NotFoundException("Audio file", clip.FilePath);
        var result = await voice.TranscribeAsync(stream, Path.GetFileName(clip.FilePath), clip.MimeType, request.Language, ct);

        clip.Transcript = result.Text.Trim();
        clip.TranscriptLanguage = result.Language ?? request.Language;
        clip.SearchText = SearchIndex.ForAudio(clip);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            await audio.UpdateAsync(clip, ct);
            if (request.AppendToBody && clip.Transcript.Length > 0)
                await AppendTranscriptAsync(owner, clip.Transcript, ct);
            else
                await TouchOwnerAsync(owner, ct);
        }, ct);
        return clip.ToDto();
    }

    private async Task AppendTranscriptAsync(IOwnedContent owner, string transcript, CancellationToken ct)
    {
        var paragraph = $"<p dir=\"auto\">{WebUtility.HtmlEncode(transcript)}</p>";
        owner.UpdatedAt = DateTime.UtcNow;
        switch (owner)
        {
            case Note n:
                n.Body = string.IsNullOrWhiteSpace(n.Body) ? paragraph : n.Body + paragraph;
                var noteTags = await tagging.GetNoteTagsAsync([n.Id], ct);
                n.SearchText = SearchIndex.ForNote(n, noteTags.GetValueOrDefault(n.Id) ?? []);
                await notes.UpdateAsync(n, ct);
                break;
            case Memory m:
                m.Story = string.IsNullOrWhiteSpace(m.Story) ? paragraph : m.Story + paragraph;
                var memTags = await tagging.GetMemoryTagsAsync([m.Id], ct);
                var memPeople = await tagging.GetMemoryPeopleAsync([m.Id], ct);
                m.SearchText = SearchIndex.ForMemory(m, memTags.GetValueOrDefault(m.Id) ?? [], memPeople.GetValueOrDefault(m.Id) ?? []);
                await memories.UpdateAsync(m, ct);
                break;
        }
    }

    // ---------- helpers ----------

    /// <summary>Copies the upload into memory so it can be sniffed, measured and re-read (sizes are capped above).</summary>
    private static async Task<MemoryStream> BufferAsync(UploadFile file, CancellationToken ct)
    {
        var ms = new MemoryStream((int)Math.Min(file.Length, int.MaxValue));
        await using (var src = file.OpenRead()) await src.CopyToAsync(ms, ct);
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteFilesQuietlyAsync(IEnumerable<string> paths)
    {
        foreach (var p in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            try { await storage.DeleteAsync(p, CancellationToken.None); }
            catch (Exception ex) { logger.LogWarning(ex, "Could not delete stored file {Path}", p); }
        }
    }
}
