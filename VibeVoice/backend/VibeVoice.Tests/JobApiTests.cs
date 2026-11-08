using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VibeVoice.Application.Abstractions;

namespace VibeVoice.Tests;

/// <summary>"Prefer: respond-async" turns OCR / transcription into background jobs polled via /api/jobs/{id}.</summary>
public sealed class JobApiTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static CancellationToken Ct => CancellationToken.None;

    private static ApiFactory Factory(bool voiceUp, FakeVoiceClient? voice = null)
    {
        var factory = new ApiFactory();
        factory.Settings["Jobs:MaxAttempts"] = "1";
        factory.Settings["Jobs:RetryDelaySeconds"] = "0";
        if (voiceUp)
        {
            var fake = voice ?? new FakeVoiceClient();
            factory.Services = s =>
            {
                s.RemoveAll<IVoiceServiceClient>();
                s.AddSingleton<IVoiceServiceClient>(fake);
            };
        }
        return factory;
    }

    private static async Task<HttpClient> SignUpAsync(ApiFactory factory)
    {
        var client = factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"u{Guid.NewGuid():N}@test.local", password = "Password1", displayName = "Tester", preferredLanguage = "ar",
        }, Ct);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.GetProperty("token").GetString());
        return client;
    }

    private static HttpRequestMessage Async(HttpMethod method, string url, HttpContent? content = null)
    {
        var req = new HttpRequestMessage(method, url) { Content = content };
        req.Headers.Add("Prefer", "respond-async");
        return req;
    }

    /// <summary>Asserts 202 + Location, then polls the job until it leaves Queued/Running.</summary>
    private static async Task<JsonElement> AcceptedThenFinishedAsync(HttpClient client, HttpResponseMessage res)
    {
        Assert.Equal(HttpStatusCode.Accepted, res.StatusCode);
        Assert.Equal("respond-async", res.Headers.GetValues("Preference-Applied").Single());
        var job = await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        var location = res.Headers.Location!.ToString();
        Assert.Equal($"/api/jobs/{job.GetProperty("id").GetString()}", location);

        for (var i = 0; i < 100; i++)
        {
            job = await client.GetFromJsonAsync<JsonElement>(location, Json, Ct);
            var status = job.GetProperty("status").GetString();
            if (status is "Succeeded" or "Failed") return job;
            await Task.Delay(100, Ct);
        }
        throw new TimeoutException("job did not finish");
    }

    private static async Task<string> CreateNoteAsync(HttpClient client)
    {
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "Background", body = "" }, Ct);
        return (await created.Content.ReadFromJsonAsync<JsonElement>(Json, Ct)).GetProperty("id").GetString()!;
    }

    [Fact]
    public async Task Note_image_ocr_runs_in_the_background_and_saves_the_text()
    {
        using var factory = Factory(voiceUp: true);
        var client = await SignUpAsync(factory);
        var noteId = await CreateNoteAsync(client);

        using var form = new MultipartFormDataContent();
        var png = new ByteArrayContent(TestFiles.Png());
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(png, "files", "page.png");
        var images = await (await client.PostAsync($"/api/notes/{noteId}/images", form, Ct)).Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        var imageId = images[0].GetProperty("id").GetString();

        var res = await client.SendAsync(Async(HttpMethod.Post, $"/api/notes/{noteId}/images/{imageId}/ocr",
            JsonContent.Create(new { appendToBody = true, language = "ar" })), Ct);
        var job = await AcceptedThenFinishedAsync(client, res);

        Assert.Equal("Succeeded", job.GetProperty("status").GetString());
        Assert.Equal("ImageOcr", job.GetProperty("kind").GetString());
        Assert.Equal("قائمة التسوق\nCoffee and dates", job.GetProperty("result").GetProperty("ocrText").GetString());

        // Stored on the note like the synchronous call, including the appended paragraph.
        var note = await client.GetFromJsonAsync<JsonElement>($"/api/notes/{noteId}", Json, Ct);
        Assert.Equal("قائمة التسوق\nCoffee and dates", note.GetProperty("images")[0].GetProperty("ocrText").GetString());
        Assert.Contains("Coffee and dates", note.GetProperty("body").GetString());
    }

    [Fact]
    public async Task Note_audio_transcription_runs_in_the_background()
    {
        using var factory = Factory(voiceUp: true, new FakeVoiceClient { Transcript = "اشتر الخبز" });
        var client = await SignUpAsync(factory);
        var noteId = await CreateNoteAsync(client);

        using var form = new MultipartFormDataContent();
        var wav = new ByteArrayContent(TestFiles.Wav(1));
        wav.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        form.Add(wav, "file", "memo.wav");
        form.Add(new StringContent("Recorded"), "source");
        var clip = await (await client.PostAsync($"/api/notes/{noteId}/audio", form, Ct)).Content.ReadFromJsonAsync<JsonElement>(Json, Ct);

        var res = await client.SendAsync(Async(HttpMethod.Post,
            $"/api/notes/{noteId}/audio/{clip.GetProperty("id").GetString()}/transcribe", JsonContent.Create(new { appendToBody = false })), Ct);
        var job = await AcceptedThenFinishedAsync(client, res);

        Assert.Equal("Succeeded", job.GetProperty("status").GetString());
        Assert.Equal("اشتر الخبز", job.GetProperty("result").GetProperty("transcript").GetString());
    }

    [Fact]
    public async Task Studio_ocr_keeps_the_upload_only_until_the_job_finishes()
    {
        using var factory = Factory(voiceUp: true);
        var client = await SignUpAsync(factory);

        using var form = new MultipartFormDataContent();
        var png = new ByteArrayContent(TestFiles.Png());
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(png, "file", "receipt.png");
        form.Add(new StringContent("false"), "save");
        var job = await AcceptedThenFinishedAsync(client, await client.SendAsync(Async(HttpMethod.Post, "/api/voice/ocr", form), Ct));

        Assert.Equal("Succeeded", job.GetProperty("status").GetString());
        Assert.Equal("قائمة التسوق\nCoffee and dates", job.GetProperty("result").GetProperty("text").GetString());
        Assert.Equal(JsonValueKind.Null, job.GetProperty("result").GetProperty("image").ValueKind);

        var jobsFolder = Path.Combine(factory.Folder, "storage", "jobs");
        Assert.True(!Directory.Exists(jobsFolder) || Directory.EnumerateFiles(jobsFolder, "*", SearchOption.AllDirectories).Any() is false);
    }

    [Fact]
    public async Task Failed_job_reports_the_status_the_synchronous_call_would_have()
    {
        using var factory = Factory(voiceUp: false); // voice service on a closed port
        var client = await SignUpAsync(factory);

        using var form = new MultipartFormDataContent();
        var wav = new ByteArrayContent(TestFiles.Wav(1));
        wav.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        form.Add(wav, "file", "clip.wav");
        form.Add(new StringContent("false"), "save");
        var job = await AcceptedThenFinishedAsync(client, await client.SendAsync(Async(HttpMethod.Post, "/api/voice/transcribe", form), Ct));

        Assert.Equal("Failed", job.GetProperty("status").GetString());
        Assert.Equal(503, job.GetProperty("errorStatus").GetInt32());
        Assert.Equal("Voice service unavailable", job.GetProperty("errorTitle").GetString());
    }

    [Fact]
    public async Task Jobs_are_private_and_unknown_targets_are_rejected_up_front()
    {
        using var factory = Factory(voiceUp: true);
        var client = await SignUpAsync(factory);
        var noteId = await CreateNoteAsync(client);

        var missing = await client.SendAsync(Async(HttpMethod.Post, $"/api/notes/{noteId}/images/{Guid.NewGuid()}/ocr",
            JsonContent.Create(new { })), Ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        using var form = new MultipartFormDataContent();
        var png = new ByteArrayContent(TestFiles.Png());
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(png, "file", "a.png");
        var accepted = await client.SendAsync(Async(HttpMethod.Post, "/api/voice/ocr", form), Ct);
        var location = accepted.Headers.Location!.ToString();

        var other = await SignUpAsync(factory);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync(location, Ct)).StatusCode);
        Assert.Equal(0, (await other.GetFromJsonAsync<JsonElement>("/api/jobs", Json, Ct)).GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task Upload_with_ocr_saves_the_pictures_now_and_reads_them_in_background_jobs()
    {
        using var factory = Factory(voiceUp: true);
        var client = await SignUpAsync(factory);
        var noteId = await CreateNoteAsync(client);

        using var form = new MultipartFormDataContent();
        foreach (var name in new[] { "a.png", "b.png" })
        {
            var png = new ByteArrayContent(TestFiles.Png());
            png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(png, "files", name);
        }
        form.Add(new StringContent("true"), "ocr");
        var res = await client.SendAsync(Async(HttpMethod.Post, $"/api/notes/{noteId}/images", form), Ct);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var images = await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        Assert.Equal(2, images.GetArrayLength());
        var jobIds = res.Headers.GetValues("X-Job-Ids").Single().Split(',');
        Assert.Equal(2, jobIds.Length);

        foreach (var id in jobIds)
        {
            JsonElement job = default;
            for (var i = 0; i < 100; i++)
            {
                job = await client.GetFromJsonAsync<JsonElement>($"/api/jobs/{id}", Json, Ct);
                if (job.GetProperty("status").GetString() is "Succeeded" or "Failed") break;
                await Task.Delay(100, Ct);
            }
            Assert.Equal("Succeeded", job.GetProperty("status").GetString());
        }
        var note = await client.GetFromJsonAsync<JsonElement>($"/api/notes/{noteId}", Json, Ct);
        Assert.All(note.GetProperty("images").EnumerateArray(),
            img => Assert.Equal("قائمة التسوق\nCoffee and dates", img.GetProperty("ocrText").GetString()));
    }

    [Fact]
    public async Task Without_the_prefer_header_the_endpoints_stay_synchronous()
    {
        using var factory = Factory(voiceUp: true);
        var client = await SignUpAsync(factory);

        using var form = new MultipartFormDataContent();
        var png = new ByteArrayContent(TestFiles.Png());
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(png, "file", "a.png");
        form.Add(new StringContent("false"), "save");
        var res = await client.PostAsync("/api/voice/ocr", form, Ct);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("قائمة التسوق\nCoffee and dates",
            (await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct)).GetProperty("text").GetString());
    }

    private static async Task<(HttpClient Client, string Email)> SignUpWithEmailAsync(ApiFactory factory)
    {
        var email = $"u{Guid.NewGuid():N}@test.local";
        var client = factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Password1", displayName = "Member", preferredLanguage = "en" }, Ct);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.GetProperty("token").GetString());
        return (client, email);
    }

    private static MultipartFormDataContent StudioPicture()
    {
        var form = new MultipartFormDataContent();
        var png = new ByteArrayContent(TestFiles.Png());
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(png, "file", "receipt.png");
        form.Add(new StringContent("false"), "save");
        return form;
    }

    [Fact]
    public async Task Failed_studio_job_keeps_its_upload_and_can_be_retried_then_deleted()
    {
        var voice = new FakeVoiceClient { Available = false };
        using var factory = Factory(voiceUp: true, voice);
        var client = await SignUpAsync(factory);

        using var form = StudioPicture();
        var failed = await AcceptedThenFinishedAsync(client, await client.SendAsync(Async(HttpMethod.Post, "/api/voice/ocr", form), Ct));
        Assert.Equal("Failed", failed.GetProperty("status").GetString());
        Assert.True(failed.GetProperty("canRetry").GetBoolean());
        Assert.Equal("receipt.png", failed.GetProperty("payload").GetProperty("fileName").GetString());
        Assert.Equal(JsonValueKind.Null, failed.GetProperty("errorDetail").ValueKind); // admins only
        var id = failed.GetProperty("id").GetString();

        // Running jobs can't be deleted; finished ones can't be "retried" unless they failed.
        voice.Available = true;
        var retry = await client.PostAsync($"/api/jobs/{id}/retry", null, Ct);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        JsonElement job = default;
        for (var i = 0; i < 100; i++)
        {
            job = await client.GetFromJsonAsync<JsonElement>($"/api/jobs/{id}", Json, Ct);
            if (job.GetProperty("status").GetString() is "Succeeded" or "Failed") break;
            await Task.Delay(100, Ct);
        }
        Assert.Equal("Succeeded", job.GetProperty("status").GetString());
        Assert.Equal("قائمة التسوق\nCoffee and dates", job.GetProperty("result").GetProperty("text").GetString());
        Assert.True(job.GetProperty("durationSeconds").GetDouble() >= 0);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/jobs/{id}/retry", null, Ct)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/jobs/{id}", Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/jobs/{id}", Ct)).StatusCode);
    }

    [Fact]
    public async Task Jobs_page_filters_by_status_and_kind_and_counts_per_status()
    {
        var voice = new FakeVoiceClient();
        using var factory = Factory(voiceUp: true, voice);
        var client = await SignUpAsync(factory);

        using (var ok = StudioPicture())
            await AcceptedThenFinishedAsync(client, await client.SendAsync(Async(HttpMethod.Post, "/api/voice/ocr", ok), Ct));
        voice.Available = false;
        using (var bad = StudioPicture())
            await AcceptedThenFinishedAsync(client, await client.SendAsync(Async(HttpMethod.Post, "/api/voice/ocr", bad), Ct));

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/jobs/summary", Json, Ct);
        Assert.Equal(1, summary.GetProperty("succeeded").GetInt32());
        Assert.Equal(1, summary.GetProperty("failed").GetInt32());
        Assert.Equal(2, summary.GetProperty("total").GetInt32());

        var failed = await client.GetFromJsonAsync<JsonElement>("/api/jobs?status=failed", Json, Ct);
        Assert.Equal(1, failed.GetProperty("total").GetInt32());
        Assert.Equal(503, failed.GetProperty("items")[0].GetProperty("errorStatus").GetInt32());
        Assert.Equal(2, (await client.GetFromJsonAsync<JsonElement>("/api/jobs?kind=StudioOcr", Json, Ct)).GetProperty("total").GetInt32());
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/jobs?kind=AudioTranscribe", Json, Ct)).GetProperty("total").GetInt32());
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/jobs?status=active", Json, Ct)).GetProperty("total").GetInt32());

        var cleared = await client.DeleteAsync("/api/jobs/finished", Ct);
        Assert.Equal(2, (await cleared.Content.ReadFromJsonAsync<JsonElement>(Json, Ct)).GetProperty("deleted").GetInt32());
    }

    [Fact]
    public async Task Admins_see_every_users_jobs_with_the_technical_error_detail()
    {
        var voice = new FakeVoiceClient { Available = false };
        using var factory = Factory(voiceUp: true, voice);
        var (admin, _) = await SignUpWithEmailAsync(factory);   // first account becomes Admin
        var (member, memberEmail) = await SignUpWithEmailAsync(factory);

        using var form = StudioPicture();
        var job = await AcceptedThenFinishedAsync(member, await member.SendAsync(Async(HttpMethod.Post, "/api/voice/ocr", form), Ct));
        var id = job.GetProperty("id").GetString();

        Assert.Equal(HttpStatusCode.Forbidden, (await member.GetAsync("/api/admin/jobs", Ct)).StatusCode);

        var all = await admin.GetFromJsonAsync<JsonElement>("/api/admin/jobs?status=failed", Json, Ct);
        var row = all.GetProperty("items").EnumerateArray().Single(j => j.GetProperty("id").GetString() == id);
        Assert.Equal(memberEmail, row.GetProperty("userEmail").GetString());
        Assert.Contains("VoiceServiceUnavailableException", row.GetProperty("errorDetail").GetString());
        Assert.Equal(1, (await admin.GetFromJsonAsync<JsonElement>("/api/admin/jobs/summary", Json, Ct)).GetProperty("failed").GetInt32());

        // The admin's own list stays personal.
        Assert.Equal(0, (await admin.GetFromJsonAsync<JsonElement>("/api/jobs", Json, Ct)).GetProperty("total").GetInt32());

        voice.Available = true;
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/jobs/{id}/retry", null, Ct)).StatusCode);
    }

    private static async Task<JsonElement> TranscribeStudioClipAsync(HttpClient client)
    {
        using var form = new MultipartFormDataContent();
        var wav = new ByteArrayContent(TestFiles.Wav(1));
        wav.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
        form.Add(wav, "file", "recording-20261108.webm");
        form.Add(new StringContent("false"), "save");
        form.Add(new StringContent("Recorded"), "source");
        return await AcceptedThenFinishedAsync(client, await client.SendAsync(Async(HttpMethod.Post, "/api/voice/transcribe", form), Ct));
    }

    [Fact]
    public async Task A_recording_that_times_out_is_not_run_again_from_the_start()
    {
        var voice = new FakeVoiceClient();
        voice.TranscribeFailures.Enqueue(new VibeVoice.Application.Common.VoiceServiceUnavailableException("too slow", timedOut: true));
        using var factory = Factory(voiceUp: true, voice);
        factory.Settings["Jobs:MaxAttempts"] = "4";
        var client = await SignUpAsync(factory);

        var job = await TranscribeStudioClipAsync(client);

        Assert.Equal("Failed", job.GetProperty("status").GetString());
        Assert.Equal(504, job.GetProperty("errorStatus").GetInt32());
        Assert.Equal("Voice service timed out", job.GetProperty("errorTitle").GetString());
        Assert.Equal(1, job.GetProperty("attempts").GetInt32());
        Assert.Equal(1, voice.TranscribeCalls);
    }

    [Fact]
    public async Task A_recording_sent_while_the_voice_service_wakes_up_is_retried_and_transcribed()
    {
        var voice = new FakeVoiceClient { Transcript = "تسجيل من الهاتف" };
        voice.TranscribeFailures.Enqueue(new VibeVoice.Application.Common.VoiceServiceUnavailableException("asleep"));
        using var factory = Factory(voiceUp: true, voice);
        factory.Settings["Jobs:MaxAttempts"] = "3";
        var client = await SignUpAsync(factory);

        var job = await TranscribeStudioClipAsync(client);

        Assert.Equal("Succeeded", job.GetProperty("status").GetString());
        Assert.Equal(2, job.GetProperty("attempts").GetInt32());
        Assert.Equal("تسجيل من الهاتف", job.GetProperty("result").GetProperty("text").GetString());
    }
}
