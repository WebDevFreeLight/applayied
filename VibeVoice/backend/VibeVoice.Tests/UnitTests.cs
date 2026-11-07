using VibeVoice.Application.Common;
using VibeVoice.Infrastructure.Persistence;

namespace VibeVoice.Tests;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("أفكار", "افكار")]                 // hamza on alef
    [InlineData("إسلام", "اسلام")]                 // hamza under alef
    [InlineData("آمال", "امال")]                   // madda
    [InlineData("مدرسة", "مدرسه")]                 // teh marbuta
    [InlineData("مستشفى", "مستشفي")]               // alef maqsura
    [InlineData("مُحَمَّد", "محمد")]                // harakat
    [InlineData("كـــتاب", "كتاب")]                 // tatweel
    [InlineData("٢٠٢٦", "2026")]                    // Arabic-Indic digits
    [InlineData("  Café   CRÈME ", "cafe creme")]  // Latin accents, case, whitespace
    public void Normalize_folds_variants(string input, string expected) =>
        Assert.Equal(expected, TextNormalizer.Normalize(input));

    [Fact]
    public void Normalize_handles_null_and_blank()
    {
        Assert.Equal("", TextNormalizer.Normalize(null));
        Assert.Equal("", TextNormalizer.Normalize("   "));
    }
}

public class HtmlTextTests
{
    [Fact]
    public void ToPlainText_strips_tags_and_keeps_paragraph_breaks()
    {
        var plain = HtmlText.ToPlainText("<h1>Title</h1><p>Hello <strong>world</strong> &amp; friends</p><ul><li>one</li><li>two</li></ul>");
        Assert.Equal("Title\nHello world & friends\none\ntwo", plain);
    }

    [Fact]
    public void Excerpt_cuts_on_a_word_boundary()
    {
        var excerpt = HtmlText.Excerpt("<p>" + string.Join(' ', Enumerable.Repeat("word", 100)) + "</p>", 50);
        Assert.True(excerpt.Length <= 51);
        Assert.EndsWith("…", excerpt);
        Assert.DoesNotContain("wor…", excerpt);
    }
}

public class FileSignatureTests
{
    [Fact]
    public void Detects_images_by_magic_bytes()
    {
        Assert.Equal("image/png", FileSignatures.DetectImage(TestFiles.Png())?.MimeType);
        Assert.Equal("image/jpeg", FileSignatures.DetectImage([0xFF, 0xD8, 0xFF, 0xE0])?.MimeType);
        Assert.Equal("image/gif", FileSignatures.DetectImage("GIF89a......"u8)?.MimeType);
        Assert.Equal("image/webp", FileSignatures.DetectImage("RIFF\0\0\0\0WEBPVP8 "u8)?.MimeType);
        Assert.Null(FileSignatures.DetectImage("<html>not an image"u8));
    }

    [Fact]
    public void Detects_audio_by_magic_bytes()
    {
        Assert.Equal("audio/wav", FileSignatures.DetectAudio(TestFiles.Wav(0.1))?.MimeType);
        Assert.Equal("audio/webm", FileSignatures.DetectAudio([0x1A, 0x45, 0xDF, 0xA3, 0x01])?.MimeType);
        Assert.Equal("audio/ogg", FileSignatures.DetectAudio("OggS\0\u0002"u8)?.MimeType);
        Assert.Equal("audio/mpeg", FileSignatures.DetectAudio("ID3\u0004\0"u8)?.MimeType);
        Assert.Equal("audio/mp4", FileSignatures.DetectAudio("\0\0\0 ftypM4A "u8)?.MimeType);
        Assert.Null(FileSignatures.DetectAudio("MZ\u0090\0 executable"u8));
    }

    [Fact]
    public void Reads_wav_duration()
    {
        Assert.Equal(2.5, WavInfo.TryGetDuration(new MemoryStream(TestFiles.Wav(2.5))));
        Assert.Null(WavInfo.TryGetDuration(new MemoryStream("not a wav"u8.ToArray())));
    }
}

public class DbProviderFactoryTests
{
    [Theory]
    [InlineData("Sqlite", SqlSugar.DbType.Sqlite)]
    [InlineData("PostgreSQL", SqlSugar.DbType.PostgreSQL)]
    [InlineData("postgres", SqlSugar.DbType.PostgreSQL)]
    [InlineData("SqlServer", SqlSugar.DbType.SqlServer)]
    [InlineData("MySql", SqlSugar.DbType.MySql)]
    [InlineData("Oracle", SqlSugar.DbType.Oracle)]
    public void Maps_provider_names(string provider, SqlSugar.DbType expected) =>
        Assert.Equal(expected, DbProviderFactory.ToDbType(provider));

    [Fact]
    public void Rejects_unknown_provider() =>
        Assert.Throws<InvalidOperationException>(() => DbProviderFactory.ToDbType("Access"));
}

public class MediaDetectionTests
{
    private static byte[] Ftyp(string brand) => [0, 0, 0, 0x20, .. "ftyp"u8, .. System.Text.Encoding.ASCII.GetBytes(brand), 0, 0, 2, 0];

    [Theory]
    [InlineData("isom", "clip.mp4", "video/mp4", "video/mp4", VibeVoice.Domain.MediaKind.Video)]
    [InlineData("qt  ", "IMG_0001.MOV", "video/quicktime", "video/quicktime", VibeVoice.Domain.MediaKind.Video)]
    [InlineData("M4A ", "song.m4a", "audio/x-m4a", "audio/mp4", VibeVoice.Domain.MediaKind.Audio)]
    [InlineData("iso5", "recording", "audio/mp4", "audio/mp4", VibeVoice.Domain.MediaKind.Audio)] // Safari MediaRecorder
    public void Mp4_brand_and_hints_pick_audio_or_video(string brand, string name, string declared, string mime, VibeVoice.Domain.MediaKind kind)
    {
        var media = FileSignatures.DetectMedia(Ftyp(brand), name, declared);
        Assert.NotNull(media);
        Assert.Equal(mime, media.File.MimeType);
        Assert.Equal(kind, media.Kind);
    }

    [Fact]
    public void Webm_follows_the_declared_type_and_matroska_is_video()
    {
        byte[] webm = [0x1A, 0x45, 0xDF, 0xA3, .. "B\u0082\u0084webm"u8];
        byte[] mkv = [0x1A, 0x45, 0xDF, 0xA3, .. "B\u0082\u0088matroska"u8];
        Assert.Equal(VibeVoice.Domain.MediaKind.Audio, FileSignatures.DetectMedia(webm, "rec.webm", "audio/webm;codecs=opus")!.Kind);
        Assert.Equal(VibeVoice.Domain.MediaKind.Video, FileSignatures.DetectMedia(webm, "screen.webm", "video/webm")!.Kind);
        Assert.Equal("video/x-matroska", FileSignatures.DetectMedia(mkv, "movie.mkv", null)!.File.MimeType);
    }

    [Fact]
    public void Renaming_a_non_media_file_to_mp4_is_rejected() =>
        Assert.Null(FileSignatures.DetectMedia("MZ\u0090\0 this is an exe"u8, "movie.mp4", "video/mp4"));
}

public class TranscriptFormatTests
{
    private static readonly VibeVoice.Application.Dtos.SegmentDto[] Segments =
    [
        new(0, 2.5, "Hello there", null),
        new(2.5, 3661.042, "مرحبا", "S1"),
    ];

    [Fact]
    public void Srt_has_numbered_cues_with_comma_millis()
    {
        var (srt, mime, ext) = TranscriptFormats.Render("srt", "Hello there مرحبا", Segments, null);
        Assert.Equal(("application/x-subrip", "srt"), (mime, ext));
        Assert.Equal("1\n00:00:00,000 --> 00:00:02,500\nHello there\n\n2\n00:00:02,500 --> 01:01:01,042\n[S1] مرحبا\n\n", srt);
    }

    [Fact]
    public void Vtt_starts_with_header_and_uses_dot_millis()
    {
        var (vtt, _, _) = TranscriptFormats.Render("vtt", "x", Segments, null);
        Assert.StartsWith("WEBVTT\n\n00:00:00.000 --> 00:00:02.500\nHello there", vtt);
    }

    [Fact]
    public void Missing_timings_produce_one_cue_spanning_the_clip()
    {
        var (srt, _, _) = TranscriptFormats.Render("srt", "Just text", [], 4.2);
        Assert.Equal("1\n00:00:00,000 --> 00:00:04,200\nJust text\n\n", srt);
    }

    [Fact]
    public void Segments_round_trip_through_json() =>
        Assert.Equal(Segments, TranscriptFormats.Deserialize(TranscriptFormats.Serialize(Segments)));
}

public class LocalNetworkTests
{
    [Theory]
    [InlineData("http://localhost:5173", true)]
    [InlineData("http://127.0.0.1:5173", true)]
    [InlineData("http://192.168.1.20:5173", true)]
    [InlineData("http://10.0.0.5:8080", true)]
    [InlineData("http://172.16.4.2:5173", true)]
    [InlineData("http://172.31.255.1", true)]
    [InlineData("http://172.32.0.1:5173", false)]   // just outside 172.16.0.0/12
    [InlineData("http://8.8.8.8:5173", false)]
    [InlineData("https://evil.example.com", false)]
    [InlineData("http://192.168.1.20.evil.com", false)] // a hostname, not an IP
    [InlineData("file:///C:/x.html", false)]
    [InlineData("null", false)]
    public void Only_loopback_and_private_ranges_are_local(string origin, bool expected) =>
        Assert.Equal(expected, VibeVoice.Api.Infrastructure.LocalNetwork.IsPrivateOrigin(origin));
}

public class PostgresUrlTests
{
    [Fact]
    public void Neon_style_url_becomes_an_npgsql_connection_string()
    {
        var cs = DbProviderFactory.FromPostgresUrl(
            "postgresql://vv_owner:p%40ss%3Aword@ep-cool-sun-123.eu-central-1.aws.neon.tech/vibevoice?sslmode=require");
        Assert.Contains("Host=ep-cool-sun-123.eu-central-1.aws.neon.tech", cs);
        Assert.Contains("Port=5432", cs);
        Assert.Contains("Database=vibevoice", cs);
        Assert.Contains("Username=vv_owner", cs);
        Assert.Contains("Password=p@ss:word", cs);   // URL-encoded characters decoded
        Assert.Contains("SSL Mode=Require", cs);
    }

    [Theory]
    [InlineData(" postgresql://u:pw@ep-x-pooler.c-8.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require\n")]
    [InlineData("'postgresql://u:pw@ep-x-pooler.c-8.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require'")]
    [InlineData("\"postgresql://u:pw@ep-x-pooler.c-8.us-east-2.aws.neon.tech/neondb?sslmode=require\"")]
    [InlineData("psql 'postgresql://u:pw@ep-x-pooler.c-8.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require'")]
    public void Pasted_extras_around_the_url_are_ignored(string pasted)
    {
        var cs = DbProviderFactory.FromPostgresUrl(pasted);
        Assert.StartsWith("Host=ep-x-pooler.c-8.us-east-2.aws.neon.tech;", cs);
        Assert.Contains("Password=pw", cs);
        Assert.Contains("SSL Mode=Require", cs);
    }

    [Fact]
    public void Garbled_url_fails_without_echoing_the_password()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            DbProviderFactory.FromPostgresUrl("DATABASE_URL=postgresql://u:secret-pw@host/db?sslmode=require"));
        Assert.DoesNotContain("secret-pw", ex.Message);
    }

    [Fact]
    public void Postgres_url_with_default_sqlite_provider_uses_postgresql()
    {
        var config = DbProviderFactory.CreateConfig(
            new DatabaseOptions { ConnectionString = " postgresql://u:pw@ep-x.neon.tech/neondb?sslmode=require" },
            Path.GetTempPath());
        Assert.Equal(SqlSugar.DbType.PostgreSQL, config.DbType);
        Assert.StartsWith("Host=ep-x.neon.tech;", config.ConnectionString);
    }

    [Fact]
    public void Key_value_strings_pass_through() =>
        Assert.Equal("Host=db;Database=x", DbProviderFactory.FromPostgresUrl("Host=db;Database=x"));
}
