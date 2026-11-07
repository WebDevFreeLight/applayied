using System.Globalization;
using VibeVoice.Api.Infrastructure;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Application.Services;
using VibeVoice.Domain;

namespace VibeVoice.Api.Endpoints;

public static class ContentEndpoints
{
    public static void MapNotes(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/notes").WithTags("Notes").RequirePermission(Permissions.Notes);

        g.MapGet("/", ([AsParameters] NoteQuery query, NoteService s, CancellationToken ct) => s.ListAsync(query, ct));
        g.MapGet("/{id:guid}", (Guid id, NoteService s, CancellationToken ct) => s.GetAsync(id, ct));
        g.MapPost("/", async (SaveNoteRequest body, NoteService s, CancellationToken ct) =>
        {
            var note = await s.CreateAsync(body, ct);
            return Results.Created($"/api/notes/{note.Id}", note);
        }).Validate<SaveNoteRequest>();
        g.MapPut("/{id:guid}", (Guid id, SaveNoteRequest body, NoteService s, CancellationToken ct) => s.UpdateAsync(id, body, ct))
            .Validate<SaveNoteRequest>();
        g.MapPatch("/{id:guid}/pin", (Guid id, ToggleRequest body, NoteService s, CancellationToken ct) => s.SetPinnedAsync(id, body.Value, ct));
        g.MapPatch("/{id:guid}/archive", (Guid id, ToggleRequest body, NoteService s, CancellationToken ct) => s.SetArchivedAsync(id, body.Value, ct));
        g.MapDelete("/{id:guid}", async (Guid id, NoteService s, CancellationToken ct) =>
        {
            await s.SoftDeleteAsync(id, ct);
            return Results.NoContent();
        });
        g.MapPost("/{id:guid}/restore", (Guid id, NoteService s, CancellationToken ct) => s.RestoreAsync(id, ct));
        g.MapDelete("/{id:guid}/permanent", async (Guid id, NoteService s, CancellationToken ct) =>
        {
            await s.DeletePermanentlyAsync(id, ct);
            return Results.NoContent();
        });

        g.MapAttachments(OwnerType.Note);
    }

    public static void MapMemories(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/memories").WithTags("Memories").RequirePermission(Permissions.Memories);

        g.MapGet("/", ([AsParameters] MemoryQuery query, MemoryService s, CancellationToken ct) => s.ListAsync(query, ct));
        g.MapGet("/{id:guid}", (Guid id, MemoryService s, CancellationToken ct) => s.GetAsync(id, ct));
        g.MapPost("/", async (SaveMemoryRequest body, MemoryService s, CancellationToken ct) =>
        {
            var memory = await s.CreateAsync(body, ct);
            return Results.Created($"/api/memories/{memory.Id}", memory);
        }).Validate<SaveMemoryRequest>();
        g.MapPut("/{id:guid}", (Guid id, SaveMemoryRequest body, MemoryService s, CancellationToken ct) => s.UpdateAsync(id, body, ct))
            .Validate<SaveMemoryRequest>();
        g.MapPut("/{id:guid}/cover", (Guid id, SetCoverRequest body, MemoryService s, CancellationToken ct) => s.SetCoverAsync(id, body.ImageId, ct));
        g.MapPatch("/{id:guid}/pin", (Guid id, ToggleRequest body, MemoryService s, CancellationToken ct) => s.SetPinnedAsync(id, body.Value, ct));
        g.MapPatch("/{id:guid}/archive", (Guid id, ToggleRequest body, MemoryService s, CancellationToken ct) => s.SetArchivedAsync(id, body.Value, ct));
        g.MapDelete("/{id:guid}", async (Guid id, MemoryService s, CancellationToken ct) =>
        {
            await s.SoftDeleteAsync(id, ct);
            return Results.NoContent();
        });
        g.MapPost("/{id:guid}/restore", (Guid id, MemoryService s, CancellationToken ct) => s.RestoreAsync(id, ct));
        g.MapDelete("/{id:guid}/permanent", async (Guid id, MemoryService s, CancellationToken ct) =>
        {
            await s.DeletePermanentlyAsync(id, ct);
            return Results.NoContent();
        });

        g.MapAttachments(OwnerType.Memory);
    }

    /// <summary>Image, audio, TTS and transcription endpoints shared by notes and memories.</summary>
    private static void MapAttachments(this RouteGroupBuilder g, OwnerType type)
    {
        g.MapPost("/{id:guid}/images", async (Guid id, HttpRequest request, HttpResponse response, AttachmentService s,
            ILoggerFactory logs, CancellationToken ct) =>
        {
            var form = await request.ReadMultipartAsync(ct);
            var files = form.Files.GetFiles("files").Concat(form.Files.GetFiles("file")).Select(f => f.ToUpload()).ToList();
            var uploaded = await s.UploadImagesAsync(type, id, files, ct);
            return await OcrAfterUploadAsync(uploaded, form, response, s, logs, ct);
        }).Accepts<IFormFileCollection>("multipart/form-data").RequirePermission(Permissions.ImagesUpload);

        g.MapPost("/{id:guid}/images/{imageId:guid}/ocr", async (Guid id, Guid imageId, OcrRequest? body, AttachmentService s, CancellationToken ct) =>
        {
            await s.GetOwnerAsync(type, id, ct);
            return await s.ExtractTextAsync(imageId, body ?? new OcrRequest(false, null), ct, type, id);
        }).RequirePermission(Permissions.VoiceOcr);

        g.MapPatch("/{id:guid}/images/{imageId:guid}", (Guid id, Guid imageId, UpdateCaptionRequest body, AttachmentService s, CancellationToken ct) =>
            s.UpdateCaptionAsync(type, id, imageId, body.Caption, ct));

        g.MapDelete("/{id:guid}/images/{imageId:guid}", async (Guid id, Guid imageId, AttachmentService s, CancellationToken ct) =>
        {
            await s.GetOwnerAsync(type, id, ct);
            await s.DeleteImageAsync(imageId, ct, type, id);
            return Results.NoContent();
        });

        g.MapPut("/{id:guid}/images/order", (Guid id, ReorderImagesRequest body, AttachmentService s, CancellationToken ct) =>
            s.ReorderImagesAsync(type, id, body.ImageIds, ct));

        // Audio or video. With transcribe=true the clip is transcribed right away; if that fails the upload
        // still succeeds and the reason is returned in X-Transcription-Error.
        g.MapPost("/{id:guid}/audio", async (Guid id, HttpRequest request, HttpResponse response, AttachmentService s,
            ILoggerFactory logs, CancellationToken ct) =>
        {
            var form = await request.ReadMultipartAsync(ct);
            var file = form.Files.GetFile("file") ?? throw new BadRequestException("Attach the audio or video as the 'file' field.");
            var source = Enum.TryParse<AudioSource>(form["source"], true, out var parsed) ? parsed : AudioSource.Uploaded;
            double? duration = double.TryParse(form["durationSeconds"], NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;
            var clip = await s.UploadAudioAsync(type, id, file.ToUpload(), source, duration, ct);

            if (!bool.TryParse(form["transcribe"], out var transcribe) || !transcribe) return clip;
            string? language = string.IsNullOrWhiteSpace(form["language"]) ? null : form["language"].ToString();
            try
            {
                return await s.TranscribeAsync(type, id, clip.Id, new TranscribeRequest(false, language), ct);
            }
            catch (AppException ex)
            {
                logs.CreateLogger("Upload").LogWarning("Auto-transcription of {ClipId} failed: {Message}", clip.Id, ex.Message);
                // Header values must be ASCII; keep it short and readable.
                response.Headers["X-Transcription-Error"] = new string(ex.Message.Where(c => c is >= ' ' and <= '~').Take(300).ToArray());
                return clip;
            }
        }).Accepts<IFormFile>("multipart/form-data").RequirePermission(Permissions.AudioUpload);

        g.MapDelete("/{id:guid}/audio/{audioId:guid}", async (Guid id, Guid audioId, AttachmentService s, CancellationToken ct) =>
        {
            await s.GetOwnerAsync(type, id, ct);
            await s.DeleteAudioAsync(audioId, ct, type, id);
            return Results.NoContent();
        });

        g.MapPost("/{id:guid}/tts", (Guid id, TtsRequest body, AttachmentService s, CancellationToken ct) =>
            s.GenerateTtsAsync(type, id, body, ct)).Validate<TtsRequest>().RequirePermission(Permissions.VoiceTts);

        g.MapPost("/{id:guid}/audio/{audioId:guid}/transcribe",
            (Guid id, Guid audioId, TranscribeRequest? body, AttachmentService s, CancellationToken ct) =>
                s.TranscribeAsync(type, id, audioId, body ?? new TranscribeRequest(false, null), ct))
            .RequirePermission(Permissions.VoiceTranscribe);
    }

    /// <summary>
    /// With form field ocr=true, reads the text in each freshly uploaded picture. OCR problems never fail the
    /// upload: the images are returned as saved and the first error is reported in X-Ocr-Error.
    /// </summary>
    internal static async Task<List<ImageDto>> OcrAfterUploadAsync(List<ImageDto> uploaded, IFormCollection form, HttpResponse response,
        AttachmentService s, ILoggerFactory logs, CancellationToken ct)
    {
        if (!bool.TryParse(form["ocr"], out var ocr) || !ocr) return uploaded;
        string? language = string.IsNullOrWhiteSpace(form["language"]) ? null : form["language"].ToString();
        var result = new List<ImageDto>(uploaded.Count);
        foreach (var image in uploaded)
        {
            try
            {
                result.Add(await s.ExtractTextAsync(image.Id, new OcrRequest(false, language), ct));
            }
            catch (AppException ex)
            {
                logs.CreateLogger("Upload").LogWarning("OCR of image {ImageId} failed: {Message}", image.Id, ex.Message);
                if (!response.Headers.ContainsKey("X-Ocr-Error"))
                    response.Headers["X-Ocr-Error"] = new string(ex.Message.Where(c => c is >= ' ' and <= '~').Take(300).ToArray());
                result.Add(image);
            }
        }
        return result;
    }
}