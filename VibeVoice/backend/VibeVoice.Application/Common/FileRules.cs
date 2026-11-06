namespace VibeVoice.Application.Common;

public sealed class UploadOptions
{
    public const string Section = "Uploads";
    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxAudioBytes { get; set; } = 50 * 1024 * 1024;
    public int MaxFilesPerRequest { get; set; } = 20;
}

public sealed record DetectedFile(string MimeType, string Extension);

/// <summary>
/// Identifies uploads by their magic bytes. The client-supplied Content-Type and file name are
/// never trusted on their own: a renamed .exe must not be stored as "photo.jpg".
/// </summary>
public static class FileSignatures
{
    public const int HeaderLength = 16;

    public static DetectedFile? DetectImage(ReadOnlySpan<byte> h)
    {
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return new("image/jpeg", "jpg");
        if (h.Length >= 8 && h[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return new("image/png", "png");
        if (h.Length >= 6 && (Ascii(h, 0, "GIF87a") || Ascii(h, 0, "GIF89a"))) return new("image/gif", "gif");
        if (h.Length >= 12 && Ascii(h, 0, "RIFF") && Ascii(h, 8, "WEBP")) return new("image/webp", "webp");
        return null;
    }

    public static DetectedFile? DetectAudio(ReadOnlySpan<byte> h)
    {
        if (h.Length >= 12 && Ascii(h, 0, "RIFF") && Ascii(h, 8, "WAVE")) return new("audio/wav", "wav");
        if (h.Length >= 4 && Ascii(h, 0, "OggS")) return new("audio/ogg", "ogg");
        if (h.Length >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3) return new("audio/webm", "webm");
        if (h.Length >= 3 && Ascii(h, 0, "ID3")) return new("audio/mpeg", "mp3");
        if (h.Length >= 2 && h[0] == 0xFF && (h[1] & 0xE0) == 0xE0) return new("audio/mpeg", "mp3"); // MPEG frame sync
        // MP4 container: Safari's MediaRecorder output and uploaded .m4a files.
        if (h.Length >= 8 && Ascii(h, 4, "ftyp")) return new("audio/mp4", "m4a");
        return null;
    }

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
