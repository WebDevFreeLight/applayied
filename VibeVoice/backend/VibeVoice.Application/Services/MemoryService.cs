using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

public sealed class MemoryService(
    IMemoryRepository memories,
    IRepository<ImageAttachment> images,
    IRepository<AudioClip> audio,
    TaggingService tagging,
    AttachmentService attachments,
    IUnitOfWork uow,
    ICurrentUser currentUser)
{
    private Guid UserId => currentUser.UserId;

    public async Task<PagedResult<MemoryListItemDto>> ListAsync(MemoryQuery query, CancellationToken ct)
    {
        query = query with { Page = Math.Max(1, query.Page), PageSize = Math.Clamp(query.PageSize, 1, 100) };
        var (items, total) = await memories.QueryAsync(UserId, query, ct);
        return new PagedResult<MemoryListItemDto>(await ToListItemsAsync(items, ct), query.Page, query.PageSize, total);
    }

    public async Task<List<MemoryListItemDto>> ToListItemsAsync(List<Memory> items, CancellationToken ct)
    {
        var ids = items.Select(m => m.Id).ToList();
        var tags = await tagging.GetMemoryTagsAsync(ids, ct);
        var people = await tagging.GetMemoryPeopleAsync(ids, ct);
        var imgs = ids.Count == 0 ? [] : await images.ListAsync(i => i.OwnerType == OwnerType.Memory && ids.Contains(i.OwnerId), ct);
        var clips = ids.Count == 0 ? [] : await audio.ListAsync(a => a.OwnerType == OwnerType.Memory && ids.Contains(a.OwnerId), ct);
        var imgsByMemory = imgs.GroupBy(i => i.OwnerId).ToDictionary(g => g.Key, g => g.OrderBy(i => i.SortOrder).ToList());
        var clipCounts = clips.GroupBy(c => c.OwnerId).ToDictionary(g => g.Key, g => g.Count());

        return items.Select(m =>
        {
            var memoryImages = imgsByMemory.GetValueOrDefault(m.Id) ?? [];
            // Explicit cover wins; otherwise the first image in gallery order.
            var cover = memoryImages.FirstOrDefault(i => i.Id == m.CoverImageId) ?? memoryImages.FirstOrDefault();
            return new MemoryListItemDto(
                m.Id, m.Title, HtmlText.Excerpt(m.Story), m.MemoryDate.ToDateOnly(), m.Location, m.Mood,
                m.IsPinned, m.IsArchived, m.IsDeleted, tags.GetValueOrDefault(m.Id) ?? [], people.GetValueOrDefault(m.Id) ?? [],
                memoryImages.Count, clipCounts.GetValueOrDefault(m.Id), cover?.ThumbnailPath, m.CreatedAt, m.UpdatedAt);
        }).ToList();
    }

    public async Task<MemoryDto> GetAsync(Guid id, CancellationToken ct) => await ToDtoAsync(await LoadAsync(id, ct), ct);

    public async Task<MemoryDto> CreateAsync(SaveMemoryRequest request, CancellationToken ct)
    {
        var memory = new Memory { UserId = UserId };
        await SaveAsync(memory, request, isNew: true, ct);
        return await ToDtoAsync(memory, ct);
    }

    public async Task<MemoryDto> UpdateAsync(Guid id, SaveMemoryRequest request, CancellationToken ct)
    {
        var memory = await LoadAsync(id, ct);
        if (memory.IsDeleted) throw new ConflictException("Restore the memory from the trash before editing it.");
        await SaveAsync(memory, request, isNew: false, ct);
        return await ToDtoAsync(memory, ct);
    }

    private async Task SaveAsync(Memory memory, SaveMemoryRequest request, bool isNew, CancellationToken ct)
    {
        var tags = TaggingService.Clean(request.Tags, 50);
        var people = TaggingService.Clean(request.People, 100);
        memory.Title = request.Title.Trim();
        memory.Story = request.Story ?? "";
        memory.MemoryDate = request.MemoryDate.ToDateTime();
        memory.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        memory.Mood = request.Mood;
        memory.UpdatedAt = DateTime.UtcNow;
        memory.SearchText = SearchIndex.ForMemory(memory, tags, people);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            if (isNew) await memories.InsertAsync(memory, ct);
            else await memories.UpdateAsync(memory, ct);
            await tagging.SetMemoryTagsAsync(UserId, memory.Id, tags, ct);
            await tagging.SetMemoryPeopleAsync(UserId, memory.Id, people, ct);
        }, ct);
    }

    public async Task<MemoryDto> SetCoverAsync(Guid id, Guid? imageId, CancellationToken ct)
    {
        if (imageId is { } imgId &&
            !await images.AnyAsync(i => i.Id == imgId && i.OwnerType == OwnerType.Memory && i.OwnerId == id && i.UserId == UserId, ct))
            throw new BadRequestException("The cover must be one of this memory's images.");
        return await MutateAsync(id, m => m.CoverImageId = imageId, ct);
    }

    public async Task<MemoryDto> SetPinnedAsync(Guid id, bool value, CancellationToken ct) =>
        await MutateAsync(id, m => m.IsPinned = value, ct);

    public async Task<MemoryDto> SetArchivedAsync(Guid id, bool value, CancellationToken ct) =>
        await MutateAsync(id, m => m.IsArchived = value, ct);

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct) =>
        await MutateAsync(id, m => { m.IsDeleted = true; m.DeletedAt = DateTime.UtcNow; m.IsPinned = false; }, ct);

    public async Task<MemoryDto> RestoreAsync(Guid id, CancellationToken ct) =>
        await MutateAsync(id, m => { m.IsDeleted = false; m.DeletedAt = null; }, ct);

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct)
    {
        var memory = await LoadAsync(id, ct);
        List<string> files = [];
        await uow.ExecuteInTransactionAsync(async () =>
        {
            await tagging.RemoveMemoryAsync(memory.Id, ct);
            await memories.DeleteAsync(m => m.Id == memory.Id, ct);
            files = await attachments.DeleteAllRowsForOwnerAsync(OwnerType.Memory, memory.Id, ct);
        }, ct);
        await attachments.DeleteFilesQuietlyAsync(files);
    }

    private async Task<MemoryDto> MutateAsync(Guid id, Action<Memory> change, CancellationToken ct)
    {
        var memory = await LoadAsync(id, ct);
        change(memory);
        memory.UpdatedAt = DateTime.UtcNow;
        await memories.UpdateAsync(memory, ct);
        return await ToDtoAsync(memory, ct);
    }

    private async Task<Memory> LoadAsync(Guid id, CancellationToken ct) =>
        await memories.FirstOrDefaultAsync(m => m.Id == id && m.UserId == UserId, ct)
        ?? throw new NotFoundException("Memory", id);

    private async Task<MemoryDto> ToDtoAsync(Memory m, CancellationToken ct)
    {
        var tags = await tagging.GetMemoryTagsAsync([m.Id], ct);
        var people = await tagging.GetMemoryPeopleAsync([m.Id], ct);
        return new MemoryDto(
            m.Id, m.Title, m.Story, m.MemoryDate.ToDateOnly(), m.Location, m.Mood, m.CoverImageId,
            m.IsPinned, m.IsArchived, m.IsDeleted, tags.GetValueOrDefault(m.Id) ?? [], people.GetValueOrDefault(m.Id) ?? [],
            await attachments.GetImagesAsync(OwnerType.Memory, m.Id, ct),
            await attachments.GetAudioAsync(OwnerType.Memory, m.Id, ct),
            m.CreatedAt, m.UpdatedAt, m.DeletedAt);
    }
}
