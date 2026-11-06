using System.Globalization;
using System.Text;
using System.Text.Json;
using VibeVoice.Application.Dtos;

namespace VibeVoice.Application.Common;

/// <summary>Stores transcript segments and renders them as plain text, SubRip (.srt) or WebVTT (.vtt).</summary>
public static class TranscriptFormats
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string? Serialize(IReadOnlyList<SegmentDto> segments) =>
        segments.Count == 0 ? null : JsonSerializer.Serialize(segments, Json);

    public static IReadOnlyList<SegmentDto> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<SegmentDto>>(json, Json) ?? []; }
        catch (JsonException) { return []; }
    }

    /// <summary>
    /// Returns (content, mime type, file extension). When the engine gave no timings, SRT/VTT get a single
    /// cue spanning the clip so the file is still valid.
    /// </summary>
    public static (string Content, string MimeType, string Extension) Render(
        string format, string transcript, IReadOnlyList<SegmentDto> segments, double? durationSeconds)
    {
        var cues = segments.Where(s => !string.IsNullOrWhiteSpace(s.Text)).ToList();
        if (cues.Count == 0 && transcript.Length > 0)
            cues = [new SegmentDto(0, durationSeconds is > 0 ? durationSeconds.Value : 5, transcript, null)];

        return format.ToLowerInvariant() switch
        {
            "srt" => (Srt(cues), "application/x-subrip", "srt"),
            "vtt" => (Vtt(cues), "text/vtt", "vtt"),
            _ => (transcript.Trim() + "\n", "text/plain", "txt"),
        };
    }

    private static string Srt(IReadOnlyList<SegmentDto> cues)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < cues.Count; i++)
        {
            sb.Append(i + 1).Append('\n')
              .Append(Time(cues[i].Start, ',')).Append(" --> ").Append(Time(EndOf(cues[i]), ',')).Append('\n')
              .Append(Line(cues[i])).Append("\n\n");
        }
        return sb.ToString();
    }

    private static string Vtt(IReadOnlyList<SegmentDto> cues)
    {
        var sb = new StringBuilder("WEBVTT\n\n");
        foreach (var cue in cues)
        {
            sb.Append(Time(cue.Start, '.')).Append(" --> ").Append(Time(EndOf(cue), '.')).Append('\n')
              .Append(Line(cue)).Append("\n\n");
        }
        return sb.ToString();
    }

    /// <summary>Some engines emit zero-length cues; players skip those, so give them at least a second.</summary>
    private static double EndOf(SegmentDto s) => s.End > s.Start ? s.End : s.Start + 1;

    private static string Line(SegmentDto s) =>
        (string.IsNullOrWhiteSpace(s.Speaker) ? "" : $"[{s.Speaker}] ") + s.Text.Trim().ReplaceLineEndings(" ");

    /// <summary>hh:mm:ss,mmm (SRT uses a comma, VTT a dot).</summary>
    private static string Time(double seconds, char millisSeparator)
    {
        var t = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return string.Create(CultureInfo.InvariantCulture,
            $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}{millisSeparator}{t.Milliseconds:000}");
    }
}
