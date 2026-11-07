namespace VibeVoice.Domain;

/// <summary>
/// Feature permissions granted through roles. Keys are stable strings (stored in roles and sent to the
/// frontend); add new ones at the end of <see cref="All"/>.
/// </summary>
public static class Permissions
{
    public const string Notes = "notes";
    public const string Memories = "memories";
    public const string AudioUpload = "audio.upload";
    public const string ImagesUpload = "images.upload";
    public const string VoiceTts = "voice.tts";
    public const string VoiceTranscribe = "voice.transcribe";
    public const string VoiceOcr = "voice.ocr";
    public const string UsersView = "users.view";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";

    public sealed record Definition(string Key, string Group);

    public static readonly IReadOnlyList<Definition> All =
    [
        new(Notes, "content"),
        new(Memories, "content"),
        new(AudioUpload, "media"),
        new(ImagesUpload, "media"),
        new(VoiceTts, "ai"),
        new(VoiceTranscribe, "ai"),
        new(VoiceOcr, "ai"),
        new(UsersView, "admin"),
        new(UsersManage, "admin"),
        new(RolesManage, "admin"),
    ];

    public static readonly IReadOnlySet<string> Keys = All.Select(p => p.Key).ToHashSet();

    /// <summary>Everything a regular member can do (no administration).</summary>
    public static readonly IReadOnlyList<string> Member =
        [Notes, Memories, AudioUpload, ImagesUpload, VoiceTts, VoiceTranscribe, VoiceOcr];
}
