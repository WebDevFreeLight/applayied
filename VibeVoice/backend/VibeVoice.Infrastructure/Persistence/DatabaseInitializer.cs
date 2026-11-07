using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using VibeVoice.Application.Common;
using VibeVoice.Application.Services;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Infrastructure.Persistence;

/// <summary>Creates/updates tables with SqlSugar CodeFirst and optionally seeds demo data.</summary>
public sealed class DatabaseInitializer(
    ISqlSugarClient db,
    IOptions<DatabaseOptions> options,
    IPasswordHasher<User> hasher,
    ILogger<DatabaseInitializer> logger)
{
    public const string DemoEmail = "admin@vibevoice.local";
    public const string DemoPassword = "Admin123!";

    public static readonly Type[] EntityTypes =
    [
        typeof(User), typeof(Note), typeof(Memory), typeof(Tag), typeof(NoteTag), typeof(MemoryTag),
        typeof(Person), typeof(MemoryPerson), typeof(AudioClip), typeof(ImageAttachment),
    ];

    public void Initialize()
    {
        if (db.CurrentConnectionConfig.DbType != DbType.Sqlite)
        {
            // Creates the database itself when the server allows it (PostgreSQL/SQL Server/MySQL).
            try { db.DbMaintenance.CreateDatabase(); }
            catch (Exception ex) { logger.LogWarning(ex, "Could not create the database; assuming it already exists"); }
        }

        db.CodeFirst.InitTables(EntityTypes);
        Backfill();
        logger.LogInformation("Database schema is up to date ({Provider})", db.CurrentConnectionConfig.DbType);

        if (options.Value.Seed && !db.Queryable<User>().Any()) Seed();
    }

    /// <summary>
    /// Fixes rows written before a column existed. Column defaults aren't applied to existing rows by every
    /// provider (SQLite leaves 0), so clips from before video support are marked as audio here.
    /// </summary>
    private void Backfill()
    {
        var fixedClips = db.Updateable<AudioClip>()
            .SetColumns(a => a.MediaKind == MediaKind.Audio)
            .Where(a => a.MediaKind != MediaKind.Audio && a.MediaKind != MediaKind.Video)
            .ExecuteCommand();
        if (fixedClips > 0) logger.LogInformation("Marked {Count} existing clip(s) as audio", fixedClips);

        db.Updateable<User>().SetColumns(u => u.Palette == "sunset").Where(u => u.Palette == null || u.Palette == "").ExecuteCommand();
    }

    private void Seed()
    {
        var user = new User
        {
            Email = DemoEmail,
            NormalizedEmail = AuthService.NormalizeEmail(DemoEmail),
            DisplayName = "Demo Admin",
            Role = UserRole.Admin,
        };
        user.PasswordHash = hasher.HashPassword(user, DemoPassword);

        var now = DateTime.UtcNow;
        var notes = new (string Title, string Body, string? Color, bool Pinned, string[] Tags)[]
        {
            ("Welcome to VibeVoice 👋",
             "<p>Write notes, record your voice, attach photos and let <strong>Microsoft VibeVoice</strong> read them aloud.</p><ul><li>Press <code>Ctrl+S</code> to save — autosave also runs while you type.</li><li>Use <em>Read aloud</em> to generate an AI voice clip.</li></ul>",
             "violet", true, ["getting-started"]),
            ("أفكار المشروع",
             "<p dir=\"auto\">هذه ملاحظة باللغة العربية. يمكنك البحث عنها بكلمة <strong>مشروع</strong> أو <strong>افكار</strong> حتى بدون همزة.</p>",
             "amber", false, ["أفكار", "work"]),
            ("Shopping list", "<ul><li>Coffee beans</li><li>Dates (تمر)</li><li>Notebook</li></ul>", null, false, ["personal"]),
        };
        var memories = new (string Title, string Story, DateTime Date, string? Location, Mood Mood, string[] Tags, string[] People)[]
        {
            ("First day at the new job", "<p>Nervous in the morning, proud by the evening. The team welcomed me with Arabic coffee.</p>",
             now.Date.AddYears(-1), "Riyadh", Mood.Proud, ["work"], ["Sara", "Omar"]),
            ("رحلة إلى البحر", "<p dir=\"auto\">يوم جميل على الشاطئ مع العائلة، غروب الشمس كان رائعاً.</p>",
             now.Date.AddMonths(-2), "جدة", Mood.Happy, ["عائلة", "travel"], ["أحمد"]),
        };

        var tags = new Dictionary<string, Tag>();
        Tag TagFor(string name)
        {
            var key = TextNormalizer.Normalize(name);
            if (!tags.TryGetValue(key, out var tag))
                tags[key] = tag = new Tag { UserId = user.Id, Name = name, NormalizedName = key };
            return tag;
        }

        var noteRows = new List<Note>();
        var noteTags = new List<NoteTag>();
        foreach (var (title, body, color, pinned, tagNames) in notes)
        {
            var note = new Note { UserId = user.Id, Title = title, Body = body, Color = color, IsPinned = pinned, CreatedAt = now, UpdatedAt = now };
            note.SearchText = SearchIndex.ForNote(note, tagNames);
            noteRows.Add(note);
            noteTags.AddRange(tagNames.Select(t => new NoteTag { NoteId = note.Id, TagId = TagFor(t).Id }));
        }

        var memoryRows = new List<Memory>();
        var memoryTags = new List<MemoryTag>();
        var people = new Dictionary<string, Person>();
        var memoryPeople = new List<MemoryPerson>();
        foreach (var (title, story, date, location, mood, tagNames, personNames) in memories)
        {
            var memory = new Memory
            {
                UserId = user.Id, Title = title, Story = story, MemoryDate = date, Location = location, Mood = mood,
                CreatedAt = now, UpdatedAt = now,
            };
            memory.SearchText = SearchIndex.ForMemory(memory, tagNames, personNames);
            memoryRows.Add(memory);
            memoryTags.AddRange(tagNames.Select(t => new MemoryTag { MemoryId = memory.Id, TagId = TagFor(t).Id }));
            foreach (var name in personNames)
            {
                var key = TextNormalizer.Normalize(name);
                if (!people.TryGetValue(key, out var person))
                    people[key] = person = new Person { UserId = user.Id, Name = name, NormalizedName = key };
                memoryPeople.Add(new MemoryPerson { MemoryId = memory.Id, PersonId = person.Id });
            }
        }

        var result = db.Ado.UseTran(() =>
        {
            db.Insertable(user).ExecuteCommand();
            db.Insertable(tags.Values.ToList()).ExecuteCommand();
            db.Insertable(noteRows).ExecuteCommand();
            db.Insertable(noteTags).ExecuteCommand();
            db.Insertable(memoryRows).ExecuteCommand();
            db.Insertable(memoryTags).ExecuteCommand();
            db.Insertable(people.Values.ToList()).ExecuteCommand();
            db.Insertable(memoryPeople).ExecuteCommand();
        });
        if (!result.IsSuccess) throw new InvalidOperationException("Seeding the database failed.", result.ErrorException);

        logger.LogInformation("Seeded demo data. Sign in with {Email} / {Password}", DemoEmail, DemoPassword);
    }
}
