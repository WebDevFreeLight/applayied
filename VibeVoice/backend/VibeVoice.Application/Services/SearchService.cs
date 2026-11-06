using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>One search box over notes, memories and audio transcripts / TTS text.</summary>
public sealed class SearchService(
    IRepository<Note> notes,
    IRepository<Memory> memories,
    IRepository<AudioClip> audio,
    TaggingService tagging,
    LibraryService library,
    ICurrentUser currentUser)
{
    /// <summary>Per-kind candidate cap; results are merged and paged in memory.</summary>
    private const int MaxPerKind = 200;

    public async Task<PagedResult<SearchResultDto>> SearchAsync(string? q, string? types, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var term = TextNormalizer.Normalize(q);
        if (term.Length == 0) return new([], page, pageSize, 0);

        var kinds = string.IsNullOrWhiteSpace(types)
            ? ["notes", "memories", "audio"]
            : types.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToLowerInvariant()).ToHashSet();
        var userId = currentUser.UserId;
        var results = new List<SearchResultDto>();

        if (kinds.Contains("notes"))
        {
            var found = await notes.ListAsync(n => n.UserId == userId && !n.IsDeleted && n.SearchText.Contains(term), n => n.UpdatedAt, true, MaxPerKind, ct);
            var tags = await tagging.GetNoteTagsAsync(found.Select(n => n.Id).ToList(), ct);
            foreach (var n in found)
            {
                var (matched, snippet) = Match(term,
                    ("title", n.Title), ("body", HtmlText.ToPlainText(n.Body)), ("tag", string.Join(", ", tags.GetValueOrDefault(n.Id) ?? [])));
                results.Add(new("note", n.Id, null, null, n.Title, snippet, matched, n.UpdatedAt));
            }
        }

        if (kinds.Contains("memories"))
        {
            var found = await memories.ListAsync(m => m.UserId == userId && !m.IsDeleted && m.SearchText.Contains(term), m => m.MemoryDate, true, MaxPerKind, ct);
            var ids = found.Select(m => m.Id).ToList();
            var tags = await tagging.GetMemoryTagsAsync(ids, ct);
            var people = await tagging.GetMemoryPeopleAsync(ids, ct);
            foreach (var m in found)
            {
                var (matched, snippet) = Match(term,
                    ("title", m.Title), ("body", HtmlText.ToPlainText(m.Story)), ("location", m.Location ?? ""),
                    ("tag", string.Join(", ", tags.GetValueOrDefault(m.Id) ?? [])),
                    ("person", string.Join(", ", people.GetValueOrDefault(m.Id) ?? [])));
                results.Add(new("memory", m.Id, null, null, m.Title, snippet, matched, m.MemoryDate));
            }
        }

        if (kinds.Contains("audio"))
        {
            var found = await audio.ListAsync(a => a.UserId == userId && a.SearchText!.Contains(term), a => a.CreatedAt, true, MaxPerKind, ct);
            var titles = await library.OwnerTitlesAsync(found.Select(a => (a.OwnerType, a.OwnerId)), ct);
            // Clips that belong to trashed notes/memories are hidden, like their owners.
            var liveOwners = await LiveOwnerIdsAsync(found, ct);
            foreach (var a in found.Where(a => a.OwnerType == OwnerType.Studio || liveOwners.Contains(a.OwnerId)))
            {
                var (_, snippet) = Match(term, ("transcript", a.Transcript ?? ""), ("transcript", a.Text ?? ""));
                var ownerKind = a.OwnerType switch { OwnerType.Note => "note", OwnerType.Memory => "memory", _ => "studio" };
                var title = titles.GetValueOrDefault(a.OwnerId) ?? (a.Source == AudioSource.Tts ? "VibeVoice" : "Audio");
                results.Add(new("audio", a.Id, a.OwnerId, ownerKind, title, snippet, "transcript", a.CreatedAt));
            }
        }

        var ordered = results
            .OrderBy(r => r.MatchedIn == "title" ? 0 : 1)
            .ThenByDescending(r => r.Date)
            .ToList();
        return new(ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(), page, pageSize, ordered.Count);
    }

    private async Task<HashSet<Guid>> LiveOwnerIdsAsync(List<AudioClip> clips, CancellationToken ct)
    {
        var noteIds = clips.Where(c => c.OwnerType == OwnerType.Note).Select(c => c.OwnerId).Distinct().ToList();
        var memoryIds = clips.Where(c => c.OwnerType == OwnerType.Memory).Select(c => c.OwnerId).Distinct().ToList();
        var live = new HashSet<Guid>();
        if (noteIds.Count > 0) live.UnionWith((await notes.ListAsync(n => noteIds.Contains(n.Id) && !n.IsDeleted, ct)).Select(n => n.Id));
        if (memoryIds.Count > 0) live.UnionWith((await memories.ListAsync(m => memoryIds.Contains(m.Id) && !m.IsDeleted, ct)).Select(m => m.Id));
        return live;
    }

    /// <summary>First field (in priority order) whose normalized text contains the term, plus a snippet from it.</summary>
    private static (string MatchedIn, string Snippet) Match(string term, params (string Field, string Text)[] fields)
    {
        foreach (var (field, text) in fields)
            if (TextNormalizer.Normalize(text).Contains(term, StringComparison.Ordinal))
                return (field, HtmlText.Snippet(text, term));
        var fallback = fields.FirstOrDefault(f => f.Field != "title" && f.Text.Length > 0);
        return (fields[0].Field, HtmlText.Excerpt(fallback.Text ?? "", 140));
    }
}
