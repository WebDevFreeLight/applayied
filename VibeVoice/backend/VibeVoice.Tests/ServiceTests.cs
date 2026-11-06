using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Application.Services;
using VibeVoice.Domain;

namespace VibeVoice.Tests;

public sealed class ServiceTests : IDisposable
{
    private readonly TestHost _host = new();
    private CancellationToken Ct => CancellationToken.None;

    public void Dispose() => _host.Dispose();

    [Fact]
    public async Task Notes_are_listed_pinned_first_and_filtered_by_tag_and_search()
    {
        await using var scope = _host.Scope();
        var notes = _host.Get<NoteService>(scope);

        var a = await notes.CreateAsync(new SaveNoteRequest("Alpha", "<p>first</p>", null, ["Work"]), Ct);
        var b = await notes.CreateAsync(new SaveNoteRequest("أفكار جديدة", "<p>مشروع</p>", null, ["أفكار"]), Ct);
        await notes.CreateAsync(new SaveNoteRequest("Gamma", "", null, null), Ct);
        await notes.SetPinnedAsync(a.Id, true, Ct);

        var all = await notes.ListAsync(new NoteQuery(), Ct);
        Assert.Equal(3, all.Total);
        Assert.Equal(a.Id, all.Items[0].Id);

        var byTag = await notes.ListAsync(new NoteQuery(Tag: "work"), Ct); // case-insensitive
        Assert.Equal([a.Id], byTag.Items.Select(n => n.Id));

        var arabic = await notes.ListAsync(new NoteQuery(Search: "افكار"), Ct); // no hamza in the query
        Assert.Equal([b.Id], arabic.Items.Select(n => n.Id));
    }

    [Fact]
    public async Task Users_only_see_their_own_notes()
    {
        await using var scope = _host.Scope();
        var notes = _host.Get<NoteService>(scope);
        var mine = await notes.CreateAsync(new SaveNoteRequest("Mine", "", null, null), Ct);

        _host.User.UserId = Guid.NewGuid();
        Assert.Equal(0, (await notes.ListAsync(new NoteQuery(), Ct)).Total);
        await Assert.ThrowsAsync<NotFoundException>(() => notes.GetAsync(mine.Id, Ct));
    }

    [Fact]
    public async Task Soft_delete_moves_to_trash_and_restore_brings_it_back()
    {
        await using var scope = _host.Scope();
        var notes = _host.Get<NoteService>(scope);
        var note = await notes.CreateAsync(new SaveNoteRequest("Temp", "", null, null), Ct);

        await notes.SoftDeleteAsync(note.Id, Ct);
        Assert.Equal(0, (await notes.ListAsync(new NoteQuery(), Ct)).Total);
        Assert.Equal(1, (await notes.ListAsync(new NoteQuery(Deleted: true), Ct)).Total);
        await Assert.ThrowsAsync<ConflictException>(() => notes.UpdateAsync(note.Id, new SaveNoteRequest("x", "", null, null), Ct));

        await notes.RestoreAsync(note.Id, Ct);
        Assert.Equal(1, (await notes.ListAsync(new NoteQuery(), Ct)).Total);
    }

    [Fact]
    public async Task Images_are_stored_with_thumbnails_and_removed_with_the_note()
    {
        await using var scope = _host.Scope();
        var notes = _host.Get<NoteService>(scope);
        var attachments = _host.Get<AttachmentService>(scope);
        var storage = _host.Get<IFileStorage>(scope);
        var note = await notes.CreateAsync(new SaveNoteRequest("Photos", "", null, null), Ct);

        var images = await attachments.UploadImagesAsync(OwnerType.Note, note.Id,
            [TestFiles.Upload(TestFiles.Png(64, 48), "a.png", "image/png"), TestFiles.Upload(TestFiles.Png(10, 10), "b.png", "image/png")], Ct);
        Assert.Equal(2, images.Count);
        Assert.Equal((64, 48), (images[0].Width, images[0].Height));
        Assert.StartsWith("thumbs/", images[0].ThumbnailPath);

        var reordered = await attachments.ReorderImagesAsync(OwnerType.Note, note.Id, [images[1].Id, images[0].Id], Ct);
        Assert.Equal(images[1].Id, reordered[0].Id);

        await notes.DeletePermanentlyAsync(note.Id, Ct);
        Assert.Null(await storage.OpenReadAsync(images[0].FilePath, Ct));
    }

    [Fact]
    public async Task Rejects_files_whose_bytes_do_not_match_an_allowed_type()
    {
        await using var scope = _host.Scope();
        var notes = _host.Get<NoteService>(scope);
        var attachments = _host.Get<AttachmentService>(scope);
        var note = await notes.CreateAsync(new SaveNoteRequest("x", "", null, null), Ct);

        await Assert.ThrowsAsync<BadRequestException>(() => attachments.UploadImagesAsync(OwnerType.Note, note.Id,
            [TestFiles.Upload("<script>alert(1)</script>"u8.ToArray(), "evil.png", "image/png")], Ct));
        await Assert.ThrowsAsync<BadRequestException>(() => attachments.UploadAudioAsync(OwnerType.Note, note.Id,
            TestFiles.Upload("MZ not audio"u8.ToArray(), "x.mp3", "audio/mpeg"), AudioSource.Uploaded, null, Ct));
    }

    [Fact]
    public async Task Tts_defaults_to_note_text_and_transcription_appends_to_body()
    {
        await using var scope = _host.Scope();
        var notes = _host.Get<NoteService>(scope);
        var attachments = _host.Get<AttachmentService>(scope);
        var note = await notes.CreateAsync(new SaveNoteRequest("Hello", "<p>World</p>", null, null), Ct);

        var clip = await attachments.GenerateTtsAsync(OwnerType.Note, note.Id, new TtsRequest(null, "dev-en", null), Ct);
        Assert.True(clip.IsAiGenerated);
        Assert.Equal(1.0, clip.DurationSeconds);
        Assert.Equal("Hello.\nWorld", _host.Voice.SpokenTexts.Single());

        var recorded = await attachments.UploadAudioAsync(OwnerType.Note, note.Id,
            TestFiles.Upload(TestFiles.Wav(0.5), "rec.wav", "audio/wav"), AudioSource.Recorded, null, Ct);
        var transcribed = await attachments.TranscribeAsync(OwnerType.Note, note.Id, recorded.Id, new TranscribeRequest(true, null), Ct);
        Assert.Equal("مرحبا من التسجيل", transcribed.Transcript);

        var reloaded = await notes.GetAsync(note.Id, Ct);
        Assert.Contains("مرحبا من التسجيل", reloaded.Body);
        var search = await _host.Get<SearchService>(scope).SearchAsync("التسجيل", null, 1, 20, Ct);
        Assert.Contains(search.Items, r => r.Kind == "audio" && r.MatchedIn == "transcript");
    }

    [Fact]
    public async Task Tts_surfaces_voice_service_outage_as_503()
    {
        await using var scope = _host.Scope();
        var notes = _host.Get<NoteService>(scope);
        var attachments = _host.Get<AttachmentService>(scope);
        var note = await notes.CreateAsync(new SaveNoteRequest("Hi", "", null, null), Ct);
        _host.Voice.Available = false;

        var ex = await Assert.ThrowsAsync<VoiceServiceUnavailableException>(() =>
            attachments.GenerateTtsAsync(OwnerType.Note, note.Id, new TtsRequest("text", null, null), Ct));
        Assert.Equal(503, ex.StatusCode);
    }

    [Fact]
    public async Task Memories_filter_by_person_and_use_first_image_as_cover()
    {
        await using var scope = _host.Scope();
        var memories = _host.Get<MemoryService>(scope);
        var attachments = _host.Get<AttachmentService>(scope);
        var trip = await memories.CreateAsync(new SaveMemoryRequest("Trip", "<p>Sea</p>", new DateOnly(2024, 7, 1), "Jeddah", Mood.Happy, ["travel"], ["Sara", "sara "]), Ct);
        await memories.CreateAsync(new SaveMemoryRequest("Exam", "", new DateOnly(2023, 1, 1), null, null, null, ["Omar"]), Ct);

        Assert.Equal(["Sara"], trip.People); // duplicate spelling collapsed
        var withSara = await memories.ListAsync(new MemoryQuery(Person: "SARA"), Ct);
        Assert.Equal([trip.Id], withSara.Items.Select(m => m.Id));

        var images = await attachments.UploadImagesAsync(OwnerType.Memory, trip.Id, [TestFiles.Upload(TestFiles.Png(), "c.png", "image/png")], Ct);
        var listed = await memories.ListAsync(new MemoryQuery(), Ct);
        Assert.Equal(images[0].ThumbnailPath, listed.Items.Single(m => m.Id == trip.Id).CoverThumbnailPath);
        Assert.Equal(new DateOnly(2024, 7, 1), listed.Items[0].MemoryDate); // newest memory date first

        var people = await _host.Get<TaggingService>(scope).GetPeopleCountsAsync(_host.User.UserId, Ct);
        Assert.Contains(people, p => p.Name == "Sara" && p.Count == 1);
    }
}
