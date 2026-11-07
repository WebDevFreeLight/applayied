using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

public sealed class DashboardService(
    IRepository<Note> notes,
    IRepository<Memory> memories,
    IRepository<AudioClip> audio,
    IRepository<ImageAttachment> images,
    NoteService noteService,
    MemoryService memoryService,
    LibraryService library,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    public async Task<DashboardDto> GetAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId;

        var stats = new DashboardStatsDto(
            Notes: await notes.CountAsync(n => n.UserId == userId && !n.IsDeleted, ct),
            Memories: await memories.CountAsync(m => m.UserId == userId && !m.IsDeleted, ct),
            AudioClips: await audio.CountAsync(a => a.UserId == userId, ct),
            Images: await images.CountAsync(i => i.UserId == userId, ct),
            AiVoiceClips: await audio.CountAsync(a => a.UserId == userId && a.Source == AudioSource.Tts, ct),
            Transcripts: await audio.CountAsync(a => a.UserId == userId && a.Transcript != null, ct),
            Pinned: await notes.CountAsync(n => n.UserId == userId && !n.IsDeleted && n.IsPinned, ct)
                    + await memories.CountAsync(m => m.UserId == userId && !m.IsDeleted && m.IsPinned, ct),
            Trash: await notes.CountAsync(n => n.UserId == userId && n.IsDeleted, ct)
                   + await memories.CountAsync(m => m.UserId == userId && m.IsDeleted, ct));

        var recentNotes = await notes.ListAsync(n => n.UserId == userId && !n.IsDeleted && !n.IsArchived, n => n.UpdatedAt, true, 6, ct);
        var recentMemories = await memories.ListAsync(m => m.UserId == userId && !m.IsDeleted && !m.IsArchived, m => m.UpdatedAt, true, 6, ct);

        // "On this day": same month & day in an earlier year. Filtered in memory to stay provider-neutral.
        var today = clock.GetUtcNow().UtcDateTime.Date;
        var allDates = await memories.ListAsync(m => m.UserId == userId && !m.IsDeleted, ct);
        var onThisDay = allDates
            .Where(m => m.MemoryDate.Month == today.Month && m.MemoryDate.Day == today.Day && m.MemoryDate.Year < today.Year)
            .OrderByDescending(m => m.MemoryDate).Take(6).ToList();

        return new DashboardDto(
            stats,
            await noteService.ToListItemsAsync(recentNotes, ct),
            await memoryService.ToListItemsAsync(recentMemories, ct),
            await memoryService.ToListItemsAsync(onThisDay, ct),
            await ActivityAsync(userId, ct));
    }

    private async Task<List<ActivityDto>> ActivityAsync(Guid userId, CancellationToken ct)
    {
        const int take = 12;
        var n = await notes.ListAsync(x => x.UserId == userId && !x.IsDeleted, x => x.UpdatedAt, true, take, ct);
        var m = await memories.ListAsync(x => x.UserId == userId && !x.IsDeleted, x => x.UpdatedAt, true, take, ct);
        var a = await audio.ListAsync(x => x.UserId == userId, x => x.CreatedAt, true, take, ct);
        var i = await images.ListAsync(x => x.UserId == userId, x => x.CreatedAt, true, take, ct);
        var titles = await library.OwnerTitlesAsync(a.Select(x => (x.OwnerType, x.OwnerId)).Concat(i.Select(x => (x.OwnerType, x.OwnerId))), ct);

        static string Action(DateTime created, DateTime updated) => (updated - created).TotalSeconds < 5 ? "created" : "updated";

        return n.Select(x => new ActivityDto("note", Action(x.CreatedAt, x.UpdatedAt), x.Id, null, x.Title, x.UpdatedAt))
            .Concat(m.Select(x => new ActivityDto("memory", Action(x.CreatedAt, x.UpdatedAt), x.Id, null, x.Title, x.UpdatedAt)))
            .Concat(a.Select(x => new ActivityDto("audio", "created", x.Id, x.OwnerType is OwnerType.Note or OwnerType.Memory ? x.OwnerId : null,
                titles.GetValueOrDefault(x.OwnerId) ?? (x.Source == AudioSource.Tts ? "VibeVoice" : "Audio"), x.CreatedAt)))
            .Concat(i.Select(x => new ActivityDto("image", "created", x.Id, x.OwnerType is OwnerType.Note or OwnerType.Memory ? x.OwnerId : null,
                titles.GetValueOrDefault(x.OwnerId) ?? x.Caption ?? "Image", x.CreatedAt)))
            .OrderByDescending(x => x.At)
            .Take(take)
            .ToList();
    }
}
