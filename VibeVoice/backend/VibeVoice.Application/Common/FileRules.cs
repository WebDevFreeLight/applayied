using VibeVoice.Domain;

namespace VibeVoice.Application.Common;

public sealed class UploadOptions
{
    public const string Section = "Uploads";
    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxAudioBytes { get; set; } = 50 * 1024 * 1024;
    public long MaxVideoBytes { get; set; } = 500 * 1024 * 1024;
    public int MaxFilesPerRequest { get; set; } = 20;
}

/// <summary>Account rules for internet-facing installs.</summary>
public sealed class AuthOptions
{
    public const string Section = "Auth";

    /// <summary>
    /// When set, only this email becomes Admin on registration (instead of "whoever registers first"),
    /// so a stranger who finds a fresh public install can't claim it.
    /// </summary>
    public string? BootstrapAdminEmail { get; set; }
}

public sealed record DetectedFile(string MimeType, string Extension);

public sealed record DetectedMedia(DetectedFile File, MediaKind Kind);

/// <summary>
/// Identifies uploads by their magic bytes. The client-supplied Content-Type and file name are
/// never trusted on their own: a renamed .exe must not be stored as "photo.jpg".
/// </summary>
public static class FileSignatures
{
    /// <summary>Enough bytes to see the EBML DocType ("webm"/"matroska") and the MP4 ftyp brand.</summary>
    public const int HeaderLength = 64;

    public static DetectedFile? DetectImage(ReadOnlySpan<byte> h)
    {
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return new("image/jpeg", "jpg");
        if (h.Length >= 8 && h[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return new("image/png", "png");
        if (h.Length >= 6 && (Ascii(h, 0, "GIF87a") || Ascii(h, 0, "GIF89a"))) return new("image/gif", "gif");
        if (h.Length >= 12 && Ascii(h, 0, "RIFF") && Ascii(h, 8, "WEBP")) return new("image/webp", "webp");
        return null;
    }

    public static DetectedFile? DetectAudio(ReadOnlySpan<byte> h) =>
        DetectMedia(h, null, null) is { Kind: MediaKind.Audio } m ? m.File : null;

    /// <summary>
    /// Identifies audio and video uploads. MP4 and WebM/Matroska containers can hold either, so the
    /// container brand decides when it can and the declared type / extension is only a tie-breaker —
    /// the bytes still have to be a real media container.
    /// </summary>
    public static DetectedMedia? DetectMedia(ReadOnlySpan<byte> h, string? fileName, string? declaredType)
    {
        var ext = Path.GetExtension(fileName ?? "").TrimStart('.').ToLowerInvariant();
        var wantsVideo = declaredType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) == true
                         || ext is "mp4" or "m4v" or "mov" or "mkv" or "avi" or "3gp";
        var wantsAudio = declaredType?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) == true
                         || ext is "m4a" or "aac" or "weba";

        static DetectedMedia Audio(string mime, string ext) => new(new(mime, ext), MediaKind.Audio);
        static DetectedMedia Video(string mime, string ext) => new(new(mime, ext), MediaKind.Video);

        if (h.Length >= 12 && Ascii(h, 0, "RIFF") && Ascii(h, 8, "WAVE")) return Audio("audio/wav", "wav");
        if (h.Length >= 12 && Ascii(h, 0, "RIFF") && Ascii(h, 8, "AVI ")) return Video("video/x-msvideo", "avi");
        if (h.Length >= 4 && Ascii(h, 0, "OggS")) return Audio("audio/ogg", "ogg");
        if (h.Length >= 3 && Ascii(h, 0, "ID3")) return Audio("audio/mpeg", "mp3");
        if (h.Length >= 2 && h[0] == 0xFF && (h[1] & 0xE0) == 0xE0) return Audio("audio/mpeg", "mp3"); // MPEG frame sync

        if (h.Length >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3)
        {
            // EBML header: the DocType says "webm" or "matroska" within the first bytes.
            var matroska = Contains(h, "matroska");
            if (matroska) return Video("video/x-matroska", "mkv");
            return wantsVideo && !wantsAudio ? Video("video/webm", "webm") : Audio("audio/webm", "webm");
        }

        if (h.Length >= 12 && Ascii(h, 4, "ftyp"))
        {
            var brand = System.Text.Encoding.ASCII.GetString(h[8..12]);
            if (brand is "M4A " or "M4B " or "M4P ") return Audio("audio/mp4", "m4a");
            if (brand == "qt  ") return Video("video/quicktime", "mov");
            if (brand.StartsWith("3g")) return Video("video/3gpp", "3gp");
            // Generic brands (isom, mp42, iso5, avc1, …): Safari records audio-only MP4 with these too.
            return wantsAudio && !wantsVideo ? Audio("audio/mp4", "m4a") : Video("video/mp4", "mp4");
        }
        return null;
    }

    private static bool Contains(ReadOnlySpan<byte> h, string text) =>
        h.IndexOf(System.Text.Encoding.ASCII.GetBytes(text)) >= 0;

    private static bool Ascii(ReadOnlySpan<byte> h, int offset, string text)
    {
        if (h.Length < offset + text.Length) return false;
        for (var i = 0; i < text.Length; i++)
            if (h[offset + i] != (byte)text[i]) return false;
        return true;
    }

    public static async Task<byte[]> ReadHeaderAsync(Stream stream, CancellationToken ct)
    {
        var buffer = new byte[HeaderLength];
        var read = 0;
        while (read < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(read), ct);
            if (n == 0) break;
            read += n;
        }
        return buffer[..read];
    }
}
