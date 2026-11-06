using System.Buffers.Binary;

namespace VibeVoice.Application.Common;

/// <summary>Reads the duration of a PCM WAV file by walking its RIFF chunks.</summary>
public static class WavInfo
{
    public static double? TryGetDuration(Stream stream)
    {
        try
        {
            Span<byte> header = stackalloc byte[12];
            if (stream.Read(header) != 12) return null;
            if (header[..4].SequenceCompareTo("RIFF"u8) != 0 || header[8..12].SequenceCompareTo("WAVE"u8) != 0) return null;

            uint byteRate = 0;
            Span<byte> chunk = stackalloc byte[8];
            while (stream.Read(chunk) == 8)
            {
                var id = chunk[..4];
                var size = BinaryPrimitives.ReadUInt32LittleEndian(chunk[4..]);
                if (id.SequenceCompareTo("fmt "u8) == 0)
                {
                    Span<byte> fmt = stackalloc byte[16];
                    if (size < 16 || stream.Read(fmt) != 16) return null;
                    byteRate = BinaryPrimitives.ReadUInt32LittleEndian(fmt[8..12]);
                    stream.Seek(size - 16 + (size % 2), SeekOrigin.Current);
                }
                else if (id.SequenceCompareTo("data"u8) == 0)
                {
                    // Streaming writers sometimes leave 0 / 0xFFFFFFFF; fall back to the remaining length.
                    long dataSize = size is 0 or uint.MaxValue ? stream.Length - stream.Position : size;
                    return byteRate == 0 ? null : Math.Round((double)dataSize / byteRate, 2);
                }
                else
                {
                    stream.Seek(size + (size % 2), SeekOrigin.Current);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException)
        {
            // Not a well-formed WAV; duration stays unknown.
        }
        return null;
    }
}
