using VibeVoice.Domain;

namespace VibeVoice.Application.Dtos;

// ---------- Shared ----------

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);

public sealed record ToggleRequest(bool Value);

public sealed record NameCountDto(string Name, int Count);

// ---------- Auth & users ----------

public sealed record RegisterRequest(string Email, string Password, string DisplayName, string? PreferredLanguage);

public sealed record LoginRequest(string Email, string Password);

/// <summary>A user with their role name and effective permissions (what the UI may show).</summary>
public sealed record UserDto(
    Guid Id, string Email, string DisplayName, string Role, Guid RoleId, IReadOnlyList<string> Permissions,
    string PreferredLanguage, string? PreferredVoice, string Theme, string Palette, bool IsActive,
    DateTime CreatedAt, DateTime? LastLoginAt);

public sealed record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public sealed record UpdateProfileRequest(string DisplayName, string PreferredLanguage, string? PreferredVoice, string Theme, string? Palette = null);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record ChangeRoleRequest(Guid RoleId);

// ---------- Administration: users & roles ----------

public sealed record PermissionDto(string Key, string Group);

public sealed record RoleDto(
    Guid Id, string Name, string? Description, IReadOnlyList<string> Permissions, bool IsSystem,
    int UserCount, DateTime CreatedAt, DateTime UpdatedAt);

public sealed record SaveRoleRequest(string Name, string? Description, IReadOnlyList<string>? Permissions);

public sealed record CreateUserRequest(
    string Email, string DisplayName, string Password, Guid RoleId, string? PreferredLanguage, bool IsActive = true);

public sealed record UpdateUserRequest(string Email, string DisplayName, Guid RoleId, string PreferredLanguage, bool IsActive);

public sealed record ResetPasswordRequest(string NewPassword);

public sealed record UserQuery(int Page = 1, int PageSize = 20, string? Search = null, Guid? RoleId = null, bool? Active = null);

/// <summary>A row in the admin user list: the user plus what they store.</summary>
public sealed record AdminUserDto(UserDto User, int Notes, int Memories, int MediaFiles, long StorageBytes);

public sealed record AdminOverviewDto(
    int Users, int ActiveUsers, int Admins, int Roles, int Notes, int Memories, int AudioClips, int Images,
    long StorageBytes, VoiceStatusDto Voice, IReadOnlyList<UserDto> RecentUsers);

public sealed record SetActiveRequest(bool IsActive);

// ---------- Attachments ----------

public sealed record ImageDto(
    Guid Id, OwnerType OwnerType, Guid OwnerId, string FilePath, string ThumbnailPath, string MimeType,
    int Width, int Height, long SizeBytes, string? Caption, int SortOrder, DateTime CreatedAt,
    string? OcrText, string? OcrLanguage);

public sealed record ImageWithOwnerDto(
    Guid Id, OwnerType OwnerType, Guid OwnerId, string FilePath, string ThumbnailPath, string MimeType,
    int Width, int Height, long SizeBytes, string? Caption, int SortOrder, DateTime CreatedAt,
    string? OcrText, string? OcrLanguage, string? OwnerTitle);

public sealed record AudioDto(
    Guid Id, OwnerType OwnerType, Guid OwnerId, AudioSource Source, MediaKind MediaKind, string FilePath, string MimeType,
    double? DurationSeconds, long SizeBytes, string? Transcript, string? TranscriptLanguage, IReadOnlyList<SegmentDto> Segments,
    string? VoiceName, string? TtsEngine, string? Text, bool IsAiGenerated, DateTime CreatedAt);

public sealed record AudioWithOwnerDto(
    Guid Id, OwnerType OwnerType, Guid OwnerId, AudioSource Source, MediaKind MediaKind, string FilePath, string MimeType,
    double? DurationSeconds, long SizeBytes, string? Transcript, string? TranscriptLanguage, IReadOnlyList<SegmentDto> Segments,
    string? VoiceName, string? TtsEngine, string? Text, bool IsAiGenerated, DateTime CreatedAt, string? OwnerTitle);

public sealed record UpdateCaptionRequest(string? Caption);

public sealed record ReorderImagesRequest(IReadOnlyList<Guid> ImageIds);

public sealed record AttachImageRequest(OwnerType OwnerType, Guid OwnerId);

public sealed record SetCoverRequest(Guid? ImageId);

/// <summary>An uploaded file, decoupled from ASP.NET's IFormFile.</summary>
public sealed record UploadFile(string FileName, string ContentType, long Length, Func<Stream> OpenRead);

// ---------- Notes ----------

public sealed record SaveNoteRequest(string Title, string? Body, string? Color, IReadOnlyList<string>? Tags);

public sealed record NoteDto(
    Guid Id, string Title, string Body, string? Color, bool IsPinned, bool IsArchived, bool IsDeleted,
    IReadOnlyList<string> Tags, IReadOnlyList<ImageDto> Images, IReadOnlyList<AudioDto> Audio,
    DateTime CreatedAt, DateTime UpdatedAt, DateTime? DeletedAt);

public sealed record NoteListItemDto(
    Guid Id, string Title, string Excerpt, string? Color, bool IsPinned, bool IsArchived, bool IsDeleted,
    IReadOnlyList<string> Tags, int ImageCount, int AudioCount, string? ThumbnailPath,
    DateTime CreatedAt, DateTime UpdatedAt);

/// <summary>Query-string filters. Positional with defaults so minimal APIs treat every field as optional.</summary>
public sealed record NoteQuery(
    int Page = 1, int PageSize = 20, string? Search = null, string? Tag = null, DateTime? From = null, DateTime? To = null,
    string? Sort = null, bool IncludeArchived = false, bool ArchivedOnly = false, bool Deleted = false,
    bool? HasAudio = null, bool? HasImages = null);

// ---------- Memories ----------

public sealed record SaveMemoryRequest(
    string Title, string? Story, DateOnly MemoryDate, string? Location, Mood? Mood,
    IReadOnlyList<string>? Tags, IReadOnlyList<string>? People);

public sealed record MemoryDto(
    Guid Id, string Title, string Story, DateOnly MemoryDate, string? Location, Mood? Mood, Guid? CoverImageId,
    bool IsPinned, bool IsArchived, bool IsDeleted, IReadOnlyList<string> Tags, IReadOnlyList<string> People,
    IReadOnlyList<ImageDto> Images, IReadOnlyList<AudioDto> Audio, DateTime CreatedAt, DateTime UpdatedAt, DateTime? DeletedAt);

public sealed record MemoryListItemDto(
    Guid Id, string Title, string Excerpt, DateOnly MemoryDate, string? Location, Mood? Mood,
    bool IsPinned, bool IsArchived, bool IsDeleted, IReadOnlyList<string> Tags, IReadOnlyList<string> People,
    int ImageCount, int AudioCount, string? CoverThumbnailPath, DateTime CreatedAt, DateTime UpdatedAt);

public sealed record MemoryQuery(
    int Page = 1, int PageSize = 20, string? Search = null, string? Tag = null, string? Person = null, Mood? Mood = null,
    DateOnly? From = null, DateOnly? To = null, string? Sort = null, bool IncludeArchived = false, bool ArchivedOnly = false,
    bool Deleted = false, bool? HasAudio = null, bool? HasImages = null);

// ---------- Voice ----------

public sealed record TtsRequest(string? Text, string? Voice, string? Language);

public sealed record TranscribeRequest(bool AppendToBody, string? Language);

public sealed record VoiceDto(string Id, string Name, string Language, string? Gender, string Engine, bool IsFallback);

public sealed record VoiceStatusDto(bool Available, string? TtsEngine, string? AsrEngine, string? OcrEngine, string? Device, string? Message);

public sealed record OcrRequest(bool AppendToBody, string? Language);

public sealed record OcrLineDto(string Text, double Confidence);

public sealed record OcrResultDto(string Text, string? Language, IReadOnlyList<OcrLineDto> Lines, ImageDto? Image);

public sealed record SegmentDto(double Start, double End, string Text, string? Speaker);

public sealed record TranscriptionDto(string Text, string? Language, IReadOnlyList<SegmentDto> Segments, AudioDto? Audio);

// ---------- Search & dashboard ----------

public sealed record SearchResultDto(
    string Kind, Guid Id, Guid? OwnerId, string? OwnerKind, string Title, string Snippet, string MatchedIn, DateTime Date,
    string? ThumbnailPath = null);

public sealed record DashboardStatsDto(
    int Notes, int Memories, int AudioClips, int Images, int AiVoiceClips, int Transcripts, int Pinned, int Trash);

public sealed record ActivityDto(string Kind, string Action, Guid Id, Guid? OwnerId, string Title, DateTime At);

public sealed record DashboardDto(
    DashboardStatsDto Stats, IReadOnlyList<NoteListItemDto> RecentNotes, IReadOnlyList<MemoryListItemDto> RecentMemories,
    IReadOnlyList<MemoryListItemDto> OnThisDay, IReadOnlyList<ActivityDto> Activity);

public sealed record HealthDto(string Status, string Database, string VoiceService);
