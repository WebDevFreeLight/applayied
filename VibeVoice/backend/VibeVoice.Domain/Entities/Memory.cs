using SqlSugar;

namespace VibeVoice.Domain.Entities;

[SugarTable("memories")]
[SugarIndex("ix_memories_user_date", nameof(UserId), OrderByType.Asc, nameof(MemoryDate), OrderByType.Desc)]
public class Memory : IOwnedContent
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = "";

    /// <summary>The story of the memory (HTML from the rich-text editor).</summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Story { get; set; } = "";

    /// <summary>Date the memory happened (time part is always 00:00).</summary>
    public DateTime MemoryDate { get; set; } = DateTime.UtcNow.Date;

    [SugarColumn(Length = 200, IsNullable = true)]
    public string? Location { get; set; }

    [SugarColumn(IsNullable = true)]
    public Mood? Mood { get; set; }

    [SugarColumn(IsNullable = true)]
    public Guid? CoverImageId { get; set; }

    public bool IsPinned { get; set; }

    public bool IsArchived { get; set; }

    public bool IsDeleted { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Normalized title + story + location + tags + people, see <see cref="Note.SearchText"/>.</summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string SearchText { get; set; } = "";
}
