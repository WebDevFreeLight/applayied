using SqlSugar;
using VibeVoice.Api.Infrastructure;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Application.Services;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Api.Endpoints;

public static class AppEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");
        auth.MapPost("/register", (RegisterRequest body, AuthService s, CancellationToken ct) => s.RegisterAsync(body, ct))
            .Validate<RegisterRequest>().AllowAnonymous().RequireRateLimiting("auth");
        auth.MapPost("/login", (LoginRequest body, AuthService s, CancellationToken ct) => s.LoginAsync(body, ct))
            .Validate<LoginRequest>().AllowAnonymous().RequireRateLimiting("auth");
        auth.MapGet("/me", (AuthService s, CancellationToken ct) => s.MeAsync(ct)).RequireAuthorization();

        var users = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();
        users.MapPut("/me", (UpdateProfileRequest body, AuthService s, CancellationToken ct) => s.UpdateProfileAsync(body, ct))
            .Validate<UpdateProfileRequest>();
        users.MapPut("/me/password", async (ChangePasswordRequest body, AuthService s, CancellationToken ct) =>
        {
            await s.ChangePasswordAsync(body, ct);
            return Results.NoContent();
        }).Validate<ChangePasswordRequest>();

        // ---- Administration: users (view vs manage) ----
        users.MapGet("/", ([AsParameters] UserQuery query, UserAdminService s, CancellationToken ct) => s.ListAsync(query, ct))
            .RequirePermission(Permissions.UsersView);
        users.MapGet("/{id:guid}", (Guid id, UserAdminService s, CancellationToken ct) => s.GetAsync(id, ct))
            .RequirePermission(Permissions.UsersView);
        users.MapPost("/", async (CreateUserRequest body, UserAdminService s, CancellationToken ct) =>
        {
            var user = await s.CreateAsync(body, ct);
            return Results.Created($"/api/users/{user.Id}", user);
        }).Validate<CreateUserRequest>().RequirePermission(Permissions.UsersManage);
        users.MapPut("/{id:guid}", (Guid id, UpdateUserRequest body, UserAdminService s, CancellationToken ct) => s.UpdateAsync(id, body, ct))
            .Validate<UpdateUserRequest>().RequirePermission(Permissions.UsersManage);
        users.MapPut("/{id:guid}/role", (Guid id, ChangeRoleRequest body, UserAdminService s, CancellationToken ct) => s.SetRoleAsync(id, body.RoleId, ct))
            .RequirePermission(Permissions.UsersManage);
        users.MapPut("/{id:guid}/active", (Guid id, SetActiveRequest body, UserAdminService s, CancellationToken ct) => s.SetActiveAsync(id, body.IsActive, ct))
            .RequirePermission(Permissions.UsersManage);
        users.MapPut("/{id:guid}/password", async (Guid id, ResetPasswordRequest body, UserAdminService s, CancellationToken ct) =>
        {
            await s.ResetPasswordAsync(id, body.NewPassword, ct);
            return Results.NoContent();
        }).Validate<ResetPasswordRequest>().RequirePermission(Permissions.UsersManage);
        users.MapDelete("/{id:guid}", async (Guid id, UserAdminService s, CancellationToken ct) =>
        {
            await s.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequirePermission(Permissions.UsersManage);

        // ---- Administration: roles & permissions ----
        var roles = app.MapGroup("/api/roles").WithTags("Roles");
        roles.MapGet("/", (RoleService s, CancellationToken ct) => s.ListAsync(ct)).RequirePermission(Permissions.UsersView);
        roles.MapPost("/", async (SaveRoleRequest body, RoleService s, CancellationToken ct) =>
        {
            var role = await s.CreateAsync(body, ct);
            return Results.Created($"/api/roles/{role.Id}", role);
        }).Validate<SaveRoleRequest>().RequirePermission(Permissions.RolesManage);
        roles.MapPut("/{id:guid}", (Guid id, SaveRoleRequest body, RoleService s, CancellationToken ct) => s.UpdateAsync(id, body, ct))
            .Validate<SaveRoleRequest>().RequirePermission(Permissions.RolesManage);
        roles.MapDelete("/{id:guid}", async (Guid id, RoleService s, CancellationToken ct) =>
        {
            await s.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequirePermission(Permissions.RolesManage);

        app.MapGet("/api/permissions", () => RoleService.ListPermissions()).WithTags("Roles").RequireAuthorization();
        app.MapGet("/api/admin/overview", (UserAdminService s, CancellationToken ct) => s.OverviewAsync(ct))
            .WithTags("Admin").RequirePermission(Permissions.UsersView);
    }

    public static void MapLibrary(this IEndpointRouteBuilder app)
    {
        var images = app.MapGroup("/api/images").WithTags("Images").RequireAuthorization();
        images.MapGet("/", (int? page, int? pageSize, OwnerType? ownerType, LibraryService s, CancellationToken ct) =>
            s.ListImagesAsync(page ?? 1, pageSize ?? 30, ownerType, ct));
        images.MapPost("/", async (HttpRequest request, HttpResponse response, AttachmentService s, ILoggerFactory logs, CancellationToken ct) =>
        {
            var form = await request.ReadMultipartAsync(ct);
            var files = form.Files.GetFiles("files").Concat(form.Files.GetFiles("file")).Select(f => f.ToUpload()).ToList();
            var uploaded = await s.UploadImagesAsync(OwnerType.Library, Guid.Empty, files, ct);
            return await ContentEndpoints.OcrAfterUploadAsync(uploaded, form, response, s, logs, ct);
        }).Accepts<IFormFileCollection>("multipart/form-data").RequirePermission(Permissions.ImagesUpload);
        images.MapPost("/{id:guid}/ocr", async (Guid id, OcrRequest? body, HttpContext http, AttachmentService s, JobService jobs,
            CancellationToken ct) =>
        {
            var req = new OcrRequest(false, body?.Language);
            if (http.Request.WantsAsync())
                return JobEndpoints.Accepted(http, await jobs.EnqueueImageOcrAsync(id, null, null, req, ct));
            return Results.Ok(await s.ExtractTextAsync(id, req, ct));
        }).RequirePermission(Permissions.VoiceOcr);
        images.MapPost("/{id:guid}/attach", (Guid id, AttachImageRequest body, AttachmentService s, CancellationToken ct) =>
            s.AttachImageAsync(id, body.OwnerType, body.OwnerId, ct));
        images.MapDelete("/{id:guid}", async (Guid id, AttachmentService s, CancellationToken ct) =>
        {
            await s.DeleteImageAsync(id, ct);
            return Results.NoContent();
        });

        app.MapGet("/api/audio", (int? page, int? pageSize, AudioSource? source, OwnerType? ownerType, MediaKind? kind, string? search,
                LibraryService s, CancellationToken ct) =>
            s.ListAudioAsync(page ?? 1, pageSize ?? 30, source, ownerType, search, ct, kind)).WithTags("Audio").RequireAuthorization();

        app.MapGet("/api/audio/{id:guid}/transcript", async (Guid id, string? format, AttachmentService s, CancellationToken ct) =>
        {
            var (content, mime, fileName) = await s.ExportTranscriptAsync(id, format, ct);
            // UTF-8 BOM so Windows players / Notepad show Arabic correctly.
            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(content)).ToArray();
            return Results.File(bytes, $"{mime}; charset=utf-8", fileName);
        }).WithTags("Audio").RequireAuthorization();

        app.MapGet("/api/tags", (TaggingService s, ICurrentUser u, CancellationToken ct) => s.GetTagCountsAsync(u.UserId, ct))
            .WithTags("Tags").RequireAuthorization();
        app.MapGet("/api/people", (TaggingService s, ICurrentUser u, CancellationToken ct) => s.GetPeopleCountsAsync(u.UserId, ct))
            .WithTags("Tags").RequireAuthorization();

        app.MapGet("/api/search", (string? q, string? types, int? page, int? pageSize, SearchService s, CancellationToken ct) =>
            s.SearchAsync(q, types, page ?? 1, pageSize ?? 20, ct)).WithTags("Search").RequireAuthorization();

        app.MapGet("/api/dashboard", (DashboardService s, CancellationToken ct) => s.GetAsync(ct))
            .WithTags("Dashboard").RequireAuthorization();
    }

    public static void MapVoice(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/voices", async (HttpResponse response, VoiceStudioService s, CancellationToken ct) =>
        {
            try
            {
                return await s.VoicesAsync(ct);
            }
            catch (VoiceServiceUnavailableException)
            {
                response.Headers["X-Voice-Service"] = "down";
                return [];
            }
        }).WithTags("Voice").RequireAuthorization();

        var voice = app.MapGroup("/api/voice").WithTags("Voice").RequireAuthorization();
        voice.MapGet("/status", (VoiceStudioService s, CancellationToken ct) => s.StatusAsync(ct));
        voice.MapPost("/tts", (TtsRequest body, VoiceStudioService s, CancellationToken ct) => s.SpeakAsync(body, ct))
            .Validate<TtsRequest>().RequirePermission(Permissions.VoiceTts);
        voice.MapPost("/transcribe", async (HttpRequest request, VoiceStudioService s, JobService jobs, CancellationToken ct) =>
        {
            var form = await request.ReadMultipartAsync(ct);
            var file = form.Files.GetFile("file") ?? throw new BadRequestException("Attach the audio as the 'file' field.");
            var save = !bool.TryParse(form["save"], out var saveFlag) || saveFlag;
            var source = Enum.TryParse<AudioSource>(form["source"], true, out var parsed) ? parsed : AudioSource.Uploaded;
            string? language = string.IsNullOrWhiteSpace(form["language"]) ? null : form["language"].ToString();
            if (request.WantsAsync())
                return JobEndpoints.Accepted(request.HttpContext,
                    await jobs.EnqueueStudioAsync(JobKind.StudioTranscribe, file.ToUpload(), language, save, source, ct));
            return Results.Ok(await s.TranscribeAsync(file.ToUpload(), language, save, source, ct));
        }).Accepts<IFormFile>("multipart/form-data").RequirePermission(Permissions.VoiceTranscribe);
        voice.MapPost("/ocr", async (HttpRequest request, VoiceStudioService s, JobService jobs, CancellationToken ct) =>
        {
            var form = await request.ReadMultipartAsync(ct);
            var file = form.Files.GetFile("file") ?? throw new BadRequestException("Attach the picture as the 'file' field.");
            var save = !bool.TryParse(form["save"], out var saveFlag) || saveFlag;
            string? language = string.IsNullOrWhiteSpace(form["language"]) ? null : form["language"].ToString();
            if (request.WantsAsync())
                return JobEndpoints.Accepted(request.HttpContext,
                    await jobs.EnqueueStudioAsync(JobKind.StudioOcr, file.ToUpload(), language, save, AudioSource.Uploaded, ct));
            return Results.Ok(await s.RecognizeAsync(file.ToUpload(), language, save, ct));
        }).Accepts<IFormFile>("multipart/form-data").RequirePermission(Permissions.VoiceOcr);
        voice.MapGet("/history", (int? page, int? pageSize, LibraryService s, CancellationToken ct) =>
            s.StudioHistoryAsync(page ?? 1, pageSize ?? 20, ct));
        voice.MapDelete("/history/{id:guid}", async (Guid id, VoiceStudioService s, CancellationToken ct) =>
        {
            await s.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }

    public static void MapFilesAndHealth(this IEndpointRouteBuilder app)
    {
        // Streams a stored file after checking that it belongs to the caller. Range requests let <audio> seek.
        app.MapGet("/api/files/{**path}", async (string path, HttpResponse response, ICurrentUser user,
            IRepository<ImageAttachment> images, IRepository<AudioClip> audio, IFileStorage storage, CancellationToken ct) =>
        {
            var userId = user.UserId;
            string? mime = null;
            var img = await images.FirstOrDefaultAsync(i => i.UserId == userId && (i.FilePath == path || i.ThumbnailPath == path), ct);
            if (img is not null) mime = img.ThumbnailPath == path ? "image/webp" : img.MimeType;
            else
            {
                var clip = await audio.FirstOrDefaultAsync(a => a.UserId == userId && a.FilePath == path, ct);
                if (clip is not null) mime = clip.MimeType;
            }
            if (mime is null) return Results.NotFound();

            var stream = await storage.OpenReadAsync(path, ct);
            if (stream is null) return Results.NotFound();
            // Paths contain a random GUID and never change, so the browser may cache them.
            response.Headers.CacheControl = "private, max-age=86400, immutable";
            return Results.Stream(stream, mime, Path.GetFileName(path), enableRangeProcessing: true);
        }).WithTags("Files").RequireAuthorization();

        app.MapGet("/api/health", async (ISqlSugarClient db, IVoiceServiceClient voice, CancellationToken ct) =>
        {
            var database = "up";
            try { await db.Queryable<User>().Take(1).AnyAsync(ct); }
            catch { database = "down"; }
            var voiceUp = await voice.GetHealthAsync(ct) is not null;
            var status = database == "up" && voiceUp ? "Healthy" : "Degraded";
            var dto = new HealthDto(status, database, voiceUp ? "up" : "down");
            return database == "up" ? Results.Ok(dto) : Results.Json(dto, statusCode: 503);
        }).WithTags("Health").AllowAnonymous();
    }
}
