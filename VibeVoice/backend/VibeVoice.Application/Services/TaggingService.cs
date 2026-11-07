using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>Tags (notes + memories) and people (memories). Names are unique per user after normalization.</summary>
public sealed class TaggingService(
    IRepository<Tag> tags,
    IRepository<NoteTag> noteTags,
    IRepository<MemoryTag> memoryTags,
    IRepository<Person> people,
    IRepository<MemoryPerson> memoryPeople,
    IRepository<Note> notes,
    IRepository<Memory> memories)
{
    public const int MaxNames = 20;

    /// <summary>Trims, drops blanks and case/diacritic duplicates, keeps the first spelling.</summary>
    public static List<string> Clean(IEnumerable<string>? names, int maxLength) =>
        (names ?? [])
            .Select(n => n.Trim().TrimStart('#'))
            .Where(n => n.Length > 0)
            .Select(n => n.Length > maxLength ? n[..maxLength] : n)
            .DistinctBy(TextNormalizer.Normalize)
            .Take(MaxNames)
            .ToList();

    // ---------- writes ----------

    public async Task SetNoteTagsAsync(Guid userId, Guid noteId, IReadOnlyList<string> names, CancellationToken ct)
    {
        var ensured = await EnsureTagsAsync(userId, names, ct);
        await noteTags.DeleteAsync(x => x.NoteId == noteId, ct);
        if (ensured.Count > 0)
            await noteTags.InsertRangeAsync(ensured.Select(t => new NoteTag { NoteId = noteId, TagId = t.Id }).ToList(), ct);
    }

    public async Task SetMemoryTagsAsync(Guid userId, Guid memoryId, IReadOnlyList<string> names, CancellationToken ct)
    {
        var ensured = await EnsureTagsAsync(userId, names, ct);
        await memoryTags.DeleteAsync(x => x.MemoryId == memoryId, ct);
        if (ensured.Count > 0)
            await memoryTags.InsertRangeAsync(ensured.Select(t => new MemoryTag { MemoryId = memoryId, TagId = t.Id }).ToList(), ct);
    }

    public async Task SetMemoryPeopleAsync(Guid userId, Guid memoryId, IReadOnlyList<string> names, CancellationToken ct)
    {
        var normalized = names.Select(TextNormalizer.Normalize).ToList();
        var existing = await people.ListAsync(p => p.UserId == userId && normalized.Contains(p.NormalizedName), ct);
        var missing = names
            .Where(n => existing.All(e => e.NormalizedName != TextNormalizer.Normalize(n)))
            .Select(n => new Person { UserId = userId, Name = n, NormalizedName = TextNormalizer.Normalize(n) })
            .ToList();
        if (missing.Count > 0) await people.InsertRangeAsync(missing, ct);

        await memoryPeople.DeleteAsync(x => x.MemoryId == memoryId, ct);
        var all = existing.Concat(missing).ToList();
        if (all.Count > 0)
            await memoryPeople.InsertRangeAsync(all.Select(p => new MemoryPerson { MemoryId = memoryId, PersonId = p.Id }).ToList(), ct);
    }

    public async Task RemoveNoteAsync(Guid noteId, CancellationToken ct) =>
        await noteTags.DeleteAsync(x => x.NoteId == noteId, ct);

    public async Task RemoveMemoryAsync(Guid memoryId, CancellationToken ct)
    {
        await memoryTags.DeleteAsync(x => x.MemoryId == memoryId, ct);
        await memoryPeople.DeleteAsync(x => x.MemoryId == memoryId, ct);
    }

    private async Task<List<Tag>> EnsureTagsAsync(Guid userId, IReadOnlyList<string> names, CancellationToken ct)
    {
        if (names.Count == 0) return [];
        var normalized = names.Select(TextNormalizer.Normalize).ToList();
        var existing = await tags.ListAsync(t => t.UserId == userId && normalized.Contains(t.NormalizedName), ct);
        var missing = names
            .Where(n => existing.All(e => e.NormalizedName != TextNormalizer.Normalize(n)))
            .Select(n => new Tag { UserId = userId, Name = n, NormalizedName = TextNormalizer.Normalize(n) })
            .ToList();
        if (missing.Count > 0) await tags.InsertRangeAsync(missing, ct);
        return existing.Concat(missing).ToList();
    }

    // ---------- reads ----------

    public async Task<Dictionary<Guid, List<string>>> GetNoteTagsAsync(IReadOnlyCollection<Guid> noteIds, CancellationToken ct)
    {
        if (noteIds.Count == 0) return [];
        var links = await noteTags.ListAsync(x => noteIds.Contains(x.NoteId), ct);
        var names = await TagNamesAsync(links.Select(l => l.TagId), ct);
        return links.GroupBy(l => l.NoteId)
            .ToDictionary(g => g.Key, g => g.Select(l => names.GetValueOrDefault(l.TagId)).OfType<string>().Order().ToList());
    }

    public async Task<Dictionary<Guid, List<string>>> GetMemoryTagsAsync(IReadOnlyCollection<Guid> memoryIds, CancellationToken ct)
    {
        if (memoryIds.Count == 0) return [];
        var links = await memoryTags.ListAsync(x => memoryIds.Contains(x.MemoryId), ct);
        var names = await TagNamesAsync(links.Select(l => l.TagId), ct);
        return links.GroupBy(l => l.MemoryId)
            .ToDictionary(g => g.Key, g => g.Select(l => names.GetValueOrDefault(l.TagId)).OfType<string>().Order().ToList());
    }

    public async Task<Dictionary<Guid, List<string>>> GetMemoryPeopleAsync(IReadOnlyCollection<Guid> memoryIds, CancellationToken ct)
    {
        if (memoryIds.Count == 0) return [];
        var links = await memoryPeople.ListAsync(x => memoryIds.Contains(x.MemoryId), ct);
        var personIds = links.Select(l => l.PersonId).Distinct().ToList();
        var names = (await people.ListAsync(p => personIds.Contains(p.Id), ct)).ToDictionary(p => p.Id, p => p.Name);
        return links.GroupBy(l => l.MemoryId)
            .ToDictionary(g => g.Key, g => g.Select(l => names.GetValueOrDefault(l.PersonId)).OfType<string>().Order().ToList());
    }

    /// <summary>Ids of the user's notes carrying <paramref name="tagName"/> (empty when the tag does not exist).</summary>
    public async Task<List<Guid>> NoteIdsWithTagAsync(Guid userId, string tagName, CancellationToken ct)
    {
        var norm = TextNormalizer.Normalize(tagName);
        var tag = await tags.FirstOrDefaultAsync(t => t.UserId == userId && t.NormalizedName == norm, ct);
        if (tag is null) return [];
        return (await noteTags.ListAsync(x => x.TagId == tag.Id, ct)).Select(x => x.NoteId).ToList();
    }

    public async Task<List<Guid>> MemoryIdsWithTagAsync(Guid userId, string tagName, CancellationToken ct)
    {
        var norm = TextNormalizer.Normalize(tagName);
        var tag = await tags.FirstOrDefaultAsync(t => t.UserId == userId && t.NormalizedName == norm, ct);
        if (tag is null) return [];
        return (await memoryTags.ListAsync(x => x.TagId == tag.Id, ct)).Select(x => x.MemoryId).ToList();
    }

    public async Task<List<Guid>> MemoryIdsWithPersonAsync(Guid userId, string personName, CancellationToken ct)
    {
        var norm = TextNormalizer.Normalize(personName);
        var person = await people.FirstOrDefaultAsync(p => p.UserId == userId && p.NormalizedName == norm, ct);
        if (person is null) return [];
        return (await memoryPeople.ListAsync(x => x.PersonId == person.Id, ct)).Select(x => x.MemoryId).ToList();
    }

    /// <summary>Tag usage counts over the user's live (not trashed) notes and memories.</summary>
    public async Task<List<NameCountDto>> GetTagCountsAsync(Guid userId, CancellationToken ct)
    {
        var userTags = await tags.ListAsync(t => t.UserId == userId, ct);
        if (userTags.Count == 0) return [];
        var liveNotes = (await notes.ListAsync(n => n.UserId == userId && !n.IsDeleted, ct)).Select(n => n.Id).ToHashSet();
        var liveMemories = (await memories.ListAsync(m => m.UserId == userId && !m.IsDeleted, ct)).Select(m => m.Id).ToHashSet();
        var tagIds = userTags.Select(t => t.Id).ToList();
        var nt = await noteTags.ListAsync(x => tagIds.Contains(x.TagId), ct);
        var mt = await memoryTags.ListAsync(x => tagIds.Contains(x.TagId), ct);

        var counts = nt.Where(x => liveNotes.Contains(x.NoteId)).Select(x => x.TagId)
            .Concat(mt.Where(x => liveMemories.Contains(x.MemoryId)).Select(x => x.TagId))
            .GroupBy(id => id).ToDictionary(g => g.Key, g => g.Count());

        return userTags.Where(t => counts.ContainsKey(t.Id))
            .Select(t => new NameCountDto(t.Name, counts[t.Id]))
            .OrderByDescending(x => x.Count).ThenBy(x => x.Name).ToList();
    }

    public async Task<List<NameCountDto>> GetPeopleCountsAsync(Guid userId, CancellationToken ct)
    {
        var userPeople = await people.ListAsync(p => p.UserId == userId, ct);
        if (userPeople.Count == 0) return [];
        var liveMemories = (await memories.ListAsync(m => m.UserId == userId && !m.IsDeleted, ct)).Select(m => m.Id).ToHashSet();
        var ids = userPeople.Select(p => p.Id).ToList();
        var counts = (await memoryPeople.ListAsync(x => ids.Contains(x.PersonId), ct))
            .Where(x => liveMemories.Contains(x.MemoryId))
            .GroupBy(x => x.PersonId).ToDictionary(g => g.Key, g => g.Count());
        return userPeople.Where(p => counts.ContainsKey(p.Id))
            .Select(p => new NameCountDto(p.Name, counts[p.Id]))
            .OrderByDescending(x => x.Count).ThenBy(x => x.Name).ToList();
    }

    private async Task<Dictionary<Guid, string>> TagNamesAsync(IEnumerable<Guid> tagIds, CancellationToken ct)
    {
        var ids = tagIds.Distinct().ToList();
        if (ids.Count == 0) return [];
        return (await tags.ListAsync(t => ids.Contains(t.Id), ct)).ToDictionary(t => t.Id, t => t.Name);
    }
}
