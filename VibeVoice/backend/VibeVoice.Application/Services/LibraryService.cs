using System.Linq.Expressions;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>Cross-owner listings: the image library, the audio list and the voice-studio history.</summary>
public sealed class LibraryService(
    IRepository<ImageAttachment> images,
    IRepository<AudioClip> audio,
    IRepository<Note> notes,
    IRepository<Memory> memories,
    ICurrentUser currentUser)
{
    private Guid UserId => currentUser.UserId;

    public async Task<PagedResult<ImageWithOwnerDto>> ListImagesAsync(int page, int pageSize, OwnerType? ownerType, CancellationToken ct)
    {
        (page, pageSize) = Clamp(page, pageSize);
        var userId = UserId;
        Expression<Func<ImageAttachment, bool>> where = i => i.UserId == userId;
        if (ownerType is { } t) where = where.And(i => i.OwnerType == t);
        var (items, total) = await images.PageAsync(where, i => i.CreatedAt, true, page, pageSize, ct);
        var titles = await OwnerTitlesAsync(items.Select(i => (i.OwnerType, i.OwnerId)), ct);
        return new(items.Select(i => i.ToDto(titles.GetValueOrDefault(i.OwnerId))).ToList(), page, pageSize, total);
    }

    public async Task<PagedResult<AudioWithOwnerDto>> ListAudioAsync(
        int page, int pageSize, AudioSource? source, OwnerType? ownerType, string? search, CancellationToken ct, MediaKind? kind = null)
    {
        (page, pageSize) = Clamp(page, pageSize);
        var userId = UserId;
        var term = TextNormalizer.Normalize(search);
        Expression<Func<AudioClip, bool>> where = a => a.UserId == userId;
        if (source is { } s) where = where.And(a => a.Source == s);
        if (kind is { } k) where = where.And(a => a.MediaKind == k);
        if (ownerType is { } o) where = where.And(a => a.OwnerType == o);
        if (term.Length > 0) where = where.And(a => a.SearchText!.Contains(term));
        var (items, total) = await audio.PageAsync(where, a => a.CreatedAt, true, page, pageSize, ct);
        var titles = await OwnerTitlesAsync(items.Select(a => (a.OwnerType, a.OwnerId)), ct);
        return new(items.Select(a => a.ToDto(titles.GetValueOrDefault(a.OwnerId))).ToList(), page, pageSize, total);
    }

    public async Task<PagedResult<AudioDto>> StudioHistoryAsync(int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = Clamp(page, pageSize);
        var (items, total) = await audio.PageAsync(
            a => a.UserId == UserId && a.OwnerType == OwnerType.Studio, a => a.CreatedAt, true, page, pageSize, ct);
        return new(items.Select(a => a.ToDto()).ToList(), page, pageSize, total);
    }

    /// <summary>Titles of the notes/memories that own the given attachments (Library/Studio have none).</summary>
    public async Task<Dictionary<Guid, string>> OwnerTitlesAsync(IEnumerable<(OwnerType Type, Guid Id)> owners, CancellationToken ct)
    {
        var list = owners.ToList();
        var noteIds = list.Where(o => o.Type == OwnerType.Note).Select(o => o.Id).Distinct().ToList();
        var memoryIds = list.Where(o => o.Type == OwnerType.Memory).Select(o => o.Id).Distinct().ToList();
        var result = new Dictionary<Guid, string>();
        if (noteIds.Count > 0)
            foreach (var n in await notes.ListAsync(n => noteIds.Contains(n.Id), ct)) result[n.Id] = n.Title;
        if (memoryIds.Count > 0)
            foreach (var m in await memories.ListAsync(m => memoryIds.Contains(m.Id), ct)) result[m.Id] = m.Title;
        return result;
    }

    private static (int, int) Clamp(int page, int pageSize) => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
