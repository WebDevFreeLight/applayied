using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VibeVoice.Tests;

/// <summary>Boots the real API (SQLite temp file) with the voice service pointed at a closed port.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public string Folder { get; } = Path.Combine(Path.GetTempPath(), "vibevoice-api-tests", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(Folder);
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:ConnectionString", $"Data Source={Path.Combine(Folder, "api.db")}");
        builder.UseSetting("Database:Seed", "false");
        builder.UseSetting("Storage:RootPath", Path.Combine(Folder, "storage"));
        builder.UseSetting("Jwt:Key", "integration-test-key-0123456789-abcdefghijklmnopqrstuvwxyz");
        builder.UseSetting("VoiceService:BaseUrl", "http://127.0.0.1:9");
        builder.UseSetting("VoiceService:RequestTimeoutSeconds", "2");
        builder.UseSetting("VoiceService:HealthTimeoutSeconds", "1");
        builder.UseSetting("RateLimiting:AuthPermitsPerMinute", "1000");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { Directory.Delete(Folder, true); } catch (IOException) { }
    }
}

public sealed class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private CancellationToken Ct => CancellationToken.None;

    private async Task<(HttpClient Client, JsonElement User)> SignUpAsync(string? email = null)
    {
        var client = factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = email ?? $"u{Guid.NewGuid():N}@test.local",
            password = "Password1",
            displayName = "Tester",
            preferredLanguage = "ar",
        }, Ct);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.GetProperty("token").GetString());
        return (client, body.GetProperty("user"));
    }

    [Fact]
    public async Task Protected_endpoints_require_a_token()
    {
        var res = await factory.CreateClient().GetAsync("/api/notes", Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Register_validates_input_with_camelCase_problem_details()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/auth/register", new { email = "bad", password = "x", displayName = "" }, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var problem = await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("email", out _));
        Assert.True(errors.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Note_round_trip_with_enums_as_strings_and_utc_dates()
    {
        var (client, user) = await SignUpAsync();
        Assert.Equal("ar", user.GetProperty("preferredLanguage").GetString());

        var created = await client.PostAsJsonAsync("/api/notes", new { title = "ملاحظة", body = "<p>نص</p>", tags = new[] { "عمل" } }, Ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var note = await created.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        var id = note.GetProperty("id").GetString();
        Assert.EndsWith("Z", note.GetProperty("updatedAt").GetString());

        var list = await client.GetFromJsonAsync<JsonElement>("/api/notes?tag=عمل", Json, Ct);
        Assert.Equal(1, list.GetProperty("total").GetInt32());

        var pinned = await client.PatchAsJsonAsync($"/api/notes/{id}/pin", new { value = true }, Ct);
        Assert.True((await pinned.Content.ReadFromJsonAsync<JsonElement>(Json, Ct)).GetProperty("isPinned").GetBoolean());

        // Another user cannot read it.
        var (other, _) = await SignUpAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/notes/{id}", Ct)).StatusCode);
    }

    [Fact]
    public async Task Uploaded_audio_is_served_with_range_support_and_query_token()
    {
        var (client, _) = await SignUpAsync();
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "Voice memo", body = "" }, Ct);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>(Json, Ct)).GetProperty("id").GetString();

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(TestFiles.Wav(1));
        file.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        form.Add(file, "file", "memo.wav");
        form.Add(new StringContent("Recorded"), "source");
        var upload = await client.PostAsync($"/api/notes/{id}/audio", form, Ct);
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        var clip = await upload.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        Assert.Equal("Recorded", clip.GetProperty("source").GetString());
        Assert.Equal(1.0, clip.GetProperty("durationSeconds").GetDouble());

        var token = client.DefaultRequestHeaders.Authorization!.Parameter;
        var anonymous = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/files/{clip.GetProperty("filePath").GetString()}?access_token={token}");
        request.Headers.Range = new RangeHeaderValue(0, 99);
        var ranged = await anonymous.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.PartialContent, ranged.StatusCode);
        Assert.Equal(100, (await ranged.Content.ReadAsByteArrayAsync(Ct)).Length);
    }

    [Fact]
    public async Task Tts_returns_503_when_the_voice_service_is_down_but_the_rest_still_works()
    {
        var (client, _) = await SignUpAsync();
        var created = await client.PostAsJsonAsync("/api/notes", new { title = "Read me", body = "<p>text</p>" }, Ct);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>(Json, Ct)).GetProperty("id").GetString();

        var tts = await client.PostAsJsonAsync($"/api/notes/{id}/tts", new { voice = "en-Carter_man" }, Ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, tts.StatusCode);
        var problem = await tts.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        Assert.Equal("Voice service unavailable", problem.GetProperty("title").GetString());

        var voices = await client.GetAsync("/api/voices", Ct);
        Assert.Equal(HttpStatusCode.OK, voices.StatusCode);
        Assert.Equal("down", voices.Headers.GetValues("X-Voice-Service").Single());

        var health = await client.GetFromJsonAsync<JsonElement>("/api/health", Json, Ct);
        Assert.Equal("Degraded", health.GetProperty("status").GetString());
        Assert.Equal("up", health.GetProperty("database").GetString());
    }

    [Fact]
    public async Task Image_upload_with_ocr_still_succeeds_when_the_voice_service_is_down()
    {
        var (client, _) = await SignUpAsync();
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(TestFiles.Png());
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "files", "scan.png");
        form.Add(new StringContent("true"), "ocr");

        var res = await client.PostAsync("/api/images", form, Ct);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.True(res.Headers.Contains("X-Ocr-Error"));
        var images = await res.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        Assert.Equal(JsonValueKind.Null, images[0].GetProperty("ocrText").ValueKind);
    }

    [Fact]
    public async Task Only_admins_can_manage_users()
    {
        // The factory's database is shared by this class, so the first-user-is-admin rule is tested in isolation.
        await using var fresh = new ApiFactory();
        var first = fresh.CreateClient();
        var reg = await first.PostAsJsonAsync("/api/auth/register", new { email = "admin@x.test", password = "Password1", displayName = "A" }, Ct);
        var admin = await reg.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        Assert.Equal("Admin", admin.GetProperty("user").GetProperty("role").GetString());
        first.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.GetProperty("token").GetString());

        var second = fresh.CreateClient();
        var reg2 = await second.PostAsJsonAsync("/api/auth/register", new { email = "user@x.test", password = "Password1", displayName = "B" }, Ct);
        var user = await reg2.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        Assert.Equal("User", user.GetProperty("user").GetProperty("role").GetString());
        second.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.GetProperty("token").GetString());

        Assert.Equal(HttpStatusCode.Forbidden, (await second.GetAsync("/api/users", Ct)).StatusCode);
        var users = await first.GetFromJsonAsync<JsonElement>("/api/users", Json, Ct);
        Assert.Equal(2, users.GetArrayLength());

        // The last admin cannot demote themselves.
        var demote = await first.PutAsJsonAsync($"/api/users/{admin.GetProperty("user").GetProperty("id").GetString()}/role", new { role = "User" }, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, demote.StatusCode);
    }
}
