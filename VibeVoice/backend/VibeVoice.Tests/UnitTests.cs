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
