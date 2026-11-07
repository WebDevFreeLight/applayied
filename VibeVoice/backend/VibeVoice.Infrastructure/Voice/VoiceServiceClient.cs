using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;

namespace VibeVoice.Infrastructure.Voice;

public sealed class VoiceServiceOptions
{
    public const string Section = "VoiceService";

    public string BaseUrl { get; set; } = "http://localhost:8001";

    /// <summary>TTS and long-video transcription on CPU can be slow; this bounds a single attempt.</summary>
    public int RequestTimeoutSeconds { get; set; } = 900;

    public int HealthTimeoutSeconds { get; set; } = 3;

    /// <summary>Shared secret sent as X-Api-Key when the voice service is reachable from the internet.</summary>
    public string? ApiKey { get; set; }
}

/// <summary>Typed HttpClient for the Python voice service (snake_case JSON). Resilience is configured in DI.</summary>
public sealed class VoiceServiceClient(HttpClient http, VoiceServiceOptions options, ILogger<VoiceServiceClient> logger) : IVoiceServiceClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<VoiceHealth?> GetHealthAsync(CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(options.HealthTimeoutSeconds));
        try
        {
            var h = await http.GetFromJsonAsync<HealthPayload>("health", Json, cts.Token);
            return h is null ? null : new VoiceHealth(h.TtsEngine, h.AsrEngine, h.Device, h.ModelLoaded, h.ArabicTtsEngine, h.OcrEngine);
        }
        catch (Exception ex) when (IsUnavailable(ex, ct))
        {
            logger.LogDebug(ex, "Voice service health check failed");
            return null;
        }
    }

    public async Task<IReadOnlyList<VoiceDto>> GetVoicesAsync(CancellationToken ct)
    {
        var voices = await SendAsync(c => http.GetAsync("voices", c), ct, async r => await r.Content.ReadFromJsonAsync<List<VoicePayload>>(Json, ct));
        return (voices ?? []).Select(v => new VoiceDto(v.Id, v.Name, v.Language, v.Gender, v.Engine, v.IsFallback)).ToList();
    }

    public Task<TtsResult> SynthesizeAsync(string text, string? voice, string? language, CancellationToken ct) =>
        SendAsync(
            c => http.PostAsJsonAsync("tts", new TtsPayload(text, voice, language, "wav"), Json, c),
            ct,
            async r =>
            {
                var bytes = await r.Content.ReadAsByteArrayAsync(ct);
                if (bytes.Length == 0) throw new VoiceServiceUnavailableException("The voice service returned empty audio.");
                return new TtsResult(
                    bytes,
                    r.Content.Headers.ContentType?.MediaType ?? "audio/wav",
                    Header(r, "X-Voice"),
                    Header(r, "X-Engine"),
                    Header(r, "X-Language"),
                    double.TryParse(Header(r, "X-Duration-Seconds"), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null);
            });

    public async Task<AsrResult> TranscribeAsync(Stream audio, string fileName, string contentType, string? language, CancellationToken ct)
    {
        // Videos can be hundreds of MB, so stream instead of copying into memory. StreamContent rewinds a
        // seekable stream when the resilience handler retries; non-seekable input is buffered as a fallback.
        var source = audio;
        if (!audio.CanSeek)
        {
            var copy = new MemoryStream();
            await audio.CopyToAsync(copy, ct);
            copy.Position = 0;
            source = copy;
        }

        return await SendAsync(
            c =>
            {
                var form = new MultipartFormDataContent();
                var file = new StreamContent(source);
                file.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);
                form.Add(file, "file", fileName);
                if (!string.IsNullOrWhiteSpace(language)) form.Add(new StringContent(language), "language");
                return http.PostAsync("transcribe", form, c);
            },
            ct,
            async r =>
            {
                var p = await r.Content.ReadFromJsonAsync<TranscribePayload>(Json, ct)
                        ?? throw new VoiceServiceUnavailableException("The voice service returned an empty transcription.");
                var segments = (p.Segments ?? []).Select(s => new SegmentDto(s.Start, s.End, s.Text ?? "", s.Speaker)).ToList();
                return new AsrResult(p.Text ?? "", p.Language, segments, p.Engine);
            });
    }

    public async Task<OcrResult> RecognizeTextAsync(Stream image, string fileName, string contentType, string? language, CancellationToken ct)
    {
        // Images are capped at a few MB, so a byte copy keeps retries simple.
        using var buffer = new MemoryStream();
        await image.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();

        return await SendAsync(
            c =>
            {
                var form = new MultipartFormDataContent();
                var file = new ByteArrayContent(bytes);
                file.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);
                form.Add(file, "file", fileName);
                if (!string.IsNullOrWhiteSpace(language)) form.Add(new StringContent(language), "language");
                return http.PostAsync("ocr", form, c);
            },
            ct,
            async r =>
            {
                var p = await r.Content.ReadFromJsonAsync<OcrPayload>(Json, ct)
                        ?? throw new VoiceServiceUnavailableException("The voice service returned an empty OCR result.");
                var lines = (p.Lines ?? []).Where(l => !string.IsNullOrWhiteSpace(l.Text))
                    .Select(l => new OcrLineDto(l.Text!, Math.Round(l.Confidence, 3))).ToList();
                return new OcrResult(p.Text ?? "", p.Language, lines);
            });
    }

    /// <summary>
    /// Sends a request and maps failures: unreachable/timeouts/5xx → 503, voice-service validation (4xx) → 400.
    /// </summary>
    private async Task<T> SendAsync<T>(Func<CancellationToken, Task<HttpResponseMessage>> send, CancellationToken ct, Func<HttpResponseMessage, Task<T>> read)
    {
        HttpResponseMessage response;
        try
        {
            response = await send(ct);
        }
        catch (Exception ex) when (IsUnavailable(ex, ct))
        {
            logger.LogWarning(ex, "Voice service request failed");
            throw new VoiceServiceUnavailableException("The voice service is not reachable. Start it with `uvicorn app.main:app --port 8001` in voice-service/.", ex);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode) return await read(response);

            var detail = await ReadDetailAsync(response, ct);
            logger.LogWarning("Voice service responded {Status}: {Detail}", (int)response.StatusCode, detail);
            if ((int)response.StatusCode is >= 400 and < 500 && response.StatusCode != HttpStatusCode.RequestTimeout)
                throw new BadRequestException(detail ?? "The voice service rejected the request.");
            throw new VoiceServiceUnavailableException(detail ?? $"The voice service failed ({(int)response.StatusCode}).");
        }
    }

    private static bool IsUnavailable(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException or BrokenCircuitException or Polly.Timeout.TimeoutRejectedException
        || (ex is TaskCanceledException or OperationCanceledException && !ct.IsCancellationRequested);

    private static async Task<string?> ReadDetailAsync(HttpResponseMessage r, CancellationToken ct)
    {
        try
        {
            var body = await r.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body)) return null;
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("detail", out var d))
                return d.ValueKind == JsonValueKind.String ? d.GetString() : d.ToString();
            return body.Length > 300 ? body[..300] : body;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? Header(HttpResponseMessage r, string name) =>
        r.Headers.TryGetValues(name, out var v) ? v.FirstOrDefault() : null;

    private sealed record HealthPayload(string? Status, string? TtsEngine, string? AsrEngine, string? Device, bool ModelLoaded, string? ArabicTtsEngine, string? OcrEngine);
    private sealed record OcrLinePayload(string? Text, double Confidence);
    private sealed record OcrPayload(string? Text, string? Language, List<OcrLinePayload>? Lines);
    private sealed record VoicePayload(string Id, string Name, string Language, string? Gender, string Engine, bool IsFallback);
    private sealed record TtsPayload(string Text, string? Voice, string? Language, string Format);
    private sealed record SegmentPayload(double Start, double End, string? Text, string? Speaker);
    private sealed record TranscribePayload(string? Text, string? Language, List<SegmentPayload>? Segments, string? Engine);
}
