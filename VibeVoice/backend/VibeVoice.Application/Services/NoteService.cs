using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

public sealed class NoteService(
    INoteRepository notes,
    IRepository<ImageAttachment> images,
    IRepository<AudioClip> audio,
    TaggingService tagging,
    AttachmentService attachments,
    IUnitOfWork uow,
    ICurrentUser currentUser)
{
    private Guid UserId => currentUser.UserId;

    public async Task<PagedResult<NoteListItemDto>> ListAsync(NoteQuery query, CancellationToken ct)
    {
        query = query with { Page = Math.Max(1, query.Page), PageSize = Math.Clamp(query.PageSize, 1, 100) };
        var (items, total) = await notes.QueryAsync(UserId, query, ct);
        return new PagedResult<NoteListItemDto>(await ToListItemsAsync(items, ct), query.Page, query.PageSize, total);
    }

    public async Task<List<NoteListItemDto>> ToListItemsAsync(List<Note> items, CancellationToken ct)
    {
        var ids = items.Select(n => n.Id).ToList();
        var tags = await tagging.GetNoteTagsAsync(ids, ct);
        var imgs = ids.Count == 0 ? [] : await images.ListAsync(i => i.OwnerType == OwnerType.Note && ids.Contains(i.OwnerId), ct);
        var clips = ids.Count == 0 ? [] : await audio.ListAsync(a => a.OwnerType == OwnerType.Note && ids.Contains(a.OwnerId), ct);
        var imgsByNote = imgs.GroupBy(i => i.OwnerId).ToDictionary(g => g.Key, g => g.OrderBy(i => i.SortOrder).ToList());
        var clipCounts = clips.GroupBy(c => c.OwnerId).ToDictionary(g => g.Key, g => g.Count());

        return items.Select(n =>
        {
            var noteImages = imgsByNote.GetValueOrDefault(n.Id) ?? [];
            return new NoteListItemDto(
                n.Id, n.Title, HtmlText.Excerpt(n.Body), n.Color, n.IsPinned, n.IsArchived, n.IsDeleted,
                tags.GetValueOrDefault(n.Id) ?? [], noteImages.Count, clipCounts.GetValueOrDefault(n.Id),
                noteImages.FirstOrDefault()?.ThumbnailPath, n.CreatedAt, n.UpdatedAt);
        }).ToList();
    }

    public async Task<NoteDto> GetAsync(Guid id, CancellationToken ct)
    {
        var note = await LoadAsync(id, ct);
        return await ToDtoAsync(note, ct);
    }

    public async Task<NoteDto> CreateAsync(SaveNoteRequest request, CancellationToken ct)
    {
        var note = new Note { UserId = UserId };
        await SaveAsync(note, request, isNew: true, ct);
        return await ToDtoAsync(note, ct);
    }

    public async Task<NoteDto> UpdateAsync(Guid id, SaveNoteRequest request, CancellationToken ct)
    {
        var note = await LoadAsync(id, ct);
        if (note.IsDeleted) throw new ConflictException("Restore the note from the trash before editing it.");
        await SaveAsync(note, request, isNew: false, ct);
        return await ToDtoAsync(note, ct);
    }

    private async Task SaveAsync(Note note, SaveNoteRequest request, bool isNew, CancellationToken ct)
    {
        var tags = TaggingService.Clean(request.Tags, 50);
        note.Title = request.Title.Trim();
        note.Body = request.Body ?? "";
        note.Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim();
        note.UpdatedAt = DateTime.UtcNow;
        note.SearchText = SearchIndex.ForNote(note, tags);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            if (isNew) await notes.InsertAsync(note, ct);
            else await notes.UpdateAsync(note, ct);
            await tagging.SetNoteTagsAsync(UserId, note.Id, tags, ct);
        }, ct);
    }

    public async Task<NoteDto> SetPinnedAsync(Guid id, bool value, CancellationToken ct) =>
        await MutateAsync(id, n => n.IsPinned = value, ct);

    public async Task<NoteDto> SetArchivedAsync(Guid id, bool value, CancellationToken ct) =>
        await MutateAsync(id, n => n.IsArchived = value, ct);

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct) =>
        await MutateAsync(id, n => { n.IsDeleted = true; n.DeletedAt = DateTime.UtcNow; n.IsPinned = false; }, ct);

    public async Task<NoteDto> RestoreAsync(Guid id, CancellationToken ct) =>
        await MutateAsync(id, n => { n.IsDeleted = false; n.DeletedAt = null; }, ct);

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct)
    {
        var note = await LoadAsync(id, ct);
        List<string> files = [];
        await uow.ExecuteInTransactionAsync(async () =>
        {
            await tagging.RemoveNoteAsync(note.Id, ct);
            await notes.DeleteAsync(n => n.Id == note.Id, ct);
            files = await attachments.DeleteAllRowsForOwnerAsync(OwnerType.Note, note.Id, ct);
        }, ct);
        await attachments.DeleteFilesQuietlyAsync(files);
    }

    private async Task<NoteDto> MutateAsync(Guid id, Action<Note> change, CancellationToken ct)
    {
        var note = await LoadAsync(id, ct);
        change(note);
        note.UpdatedAt = DateTime.UtcNow;
        await notes.UpdateAsync(note, ct);
        return await ToDtoAsync(note, ct);
    }

    private async Task<Note> LoadAsync(Guid id, CancellationToken ct) =>
        await notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == UserId, ct)
        ?? throw new NotFoundException("Note", id);

    private async Task<NoteDto> ToDtoAsync(Note n, CancellationToken ct)
    {
        var tags = await tagging.GetNoteTagsAsync([n.Id], ct);
        return new NoteDto(
            n.Id, n.Title, n.Body, n.Color, n.IsPinned, n.IsArchived, n.IsDeleted,
            tags.GetValueOrDefault(n.Id) ?? [],
            await attachments.GetImagesAsync(OwnerType.Note, n.Id, ct),
            await attachments.GetAudioAsync(OwnerType.Note, n.Id, ct),
            n.CreatedAt, n.UpdatedAt, n.DeletedAt);
    }
}
