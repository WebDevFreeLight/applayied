using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VibeVoice.Application;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Infrastructure;
using VibeVoice.Infrastructure.Persistence;

namespace VibeVoice.Tests;

/// <summary>Real services over a throw-away SQLite file and storage folder.</summary>
public sealed class TestHost : IDisposable
{
    private readonly ServiceProvider _root;
    public string Folder { get; } = Path.Combine(Path.GetTempPath(), "vibevoice-tests", Guid.NewGuid().ToString("N"));
    public FakeCurrentUser User { get; } = new();
    public FakeVoiceClient Voice { get; } = new();

    public TestHost()
    {
        Directory.CreateDirectory(Folder);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Provider"] = "Sqlite",
            ["Database:ConnectionString"] = $"Data Source={Path.Combine(Folder, "test.db")}",
            ["Storage:RootPath"] = Path.Combine(Folder, "storage"),
            ["Jwt:Key"] = "test-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz",
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddApplication();
        services.AddInfrastructure(config, Folder);
        services.AddSingleton<ICurrentUser>(User);
        services.AddSingleton<IVoiceServiceClient>(Voice);
        _root = services.BuildServiceProvider();
        _root.GetRequiredService<DatabaseInitializer>().Initialize();
    }

    public AsyncServiceScope Scope() => _root.CreateAsyncScope();

    public T Get<T>(AsyncServiceScope scope) where T : notnull => scope.ServiceProvider.GetRequiredService<T>();

    public void Dispose()
    {
        _root.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { Directory.Delete(Folder, recursive: true); } catch (IOException) { /* best effort */ }
    }
}

public sealed class FakeCurrentUser : ICurrentUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public bool IsAdmin { get; set; }
}

/// <summary>Stands in for the Python service: returns a tiny valid WAV and a canned transcript.</summary>
public sealed class FakeVoiceClient : IVoiceServiceClient
{
    public bool Available { get; set; } = true;
    public string Transcript { get; set; } = "مرحبا من التسجيل";
    public List<string> SpokenTexts { get; } = [];

    public Task<VoiceHealth?> GetHealthAsync(CancellationToken ct) =>
        Task.FromResult(Available ? new VoiceHealth("dev", "none", "cpu", true, "dev") : null);

    public Task<IReadOnlyList<VoiceDto>> GetVoicesAsync(CancellationToken ct) =>
        Available ? Task.FromResult<IReadOnlyList<VoiceDto>>([new VoiceDto("dev-en", "Dev", "en", null, "dev", false)])
                  : throw new VoiceServiceUnavailableException("down");

    public Task<TtsResult> SynthesizeAsync(string text, string? voice, string? language, CancellationToken ct)
    {
        if (!Available) throw new VoiceServiceUnavailableException("down");
        SpokenTexts.Add(text);
        return Task.FromResult(new TtsResult(TestFiles.Wav(seconds: 1), "audio/wav", voice ?? "dev-en", "dev", language ?? "en", null));
    }

    public string OcrText { get; set; } = "قائمة التسوق\nCoffee and dates";

    public Task<OcrResult> RecognizeTextAsync(Stream image, string fileName, string contentType, string? language, CancellationToken ct)
    {
        if (!Available) throw new VoiceServiceUnavailableException("down");
        var lines = OcrText.Split('\n').Select(l => new OcrLineDto(l, 0.97)).ToList();
        return Task.FromResult(new OcrResult(OcrText, "ar", lines));
    }

    /// <summary>Thrown by the next transcriptions, one per call, before falling back to normal behaviour.</summary>
    public Queue<Exception> TranscribeFailures { get; } = new();

    public int TranscribeCalls { get; private set; }

    public Task<AsrResult> TranscribeAsync(Stream audio, string fileName, string contentType, string? language, CancellationToken ct)
    {
        TranscribeCalls++;
        if (TranscribeFailures.TryDequeue(out var failure)) throw failure;
        if (!Available) throw new VoiceServiceUnavailableException("down");
        return Task.FromResult(new AsrResult(Transcript, "ar", [new SegmentDto(0, 1, Transcript, null)], "fake"));
    }
}

public static class TestFiles
{
    /// <summary>16 kHz mono 16-bit silence.</summary>
    public static byte[] Wav(double seconds)
    {
        const int rate = 16000;
        var dataSize = (int)(rate * seconds) * 2;
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write("RIFF"u8); w.Write(36 + dataSize); w.Write("WAVE"u8);
        w.Write("fmt "u8); w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(rate); w.Write(rate * 2); w.Write((short)2); w.Write((short)16);
        w.Write("data"u8); w.Write(dataSize); w.Write(new byte[dataSize]);
        w.Flush();
        return ms.ToArray();
    }

    public static byte[] Png(int width = 64, int height = 48)
    {
        using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height);
        using var ms = new MemoryStream();
        SixLabors.ImageSharp.ImageExtensions.SaveAsPng(image, ms);
        return ms.ToArray();
    }

    public static UploadFile Upload(byte[] bytes, string name, string contentType) =>
        new(name, contentType, bytes.Length, () => new MemoryStream(bytes));
}
