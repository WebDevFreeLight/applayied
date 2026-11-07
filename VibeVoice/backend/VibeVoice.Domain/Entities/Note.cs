using SqlSugar;

namespace VibeVoice.Domain.Entities;

/// <summary>Common shape of the two "owner" aggregates (notes and memories).</summary>
public interface IOwnedContent
{
    Guid Id { get; }
    Guid UserId { get; }
    string Title { get; }
    bool IsPinned { get; set; }
    bool IsArchived { get; set; }
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; set; }
    string SearchText { get; set; }
}

[SugarTable("notes")]
[SugarIndex("ix_notes_user_updated", nameof(UserId), OrderByType.Asc, nameof(UpdatedAt), OrderByType.Desc)]
public class Note : IOwnedContent
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = "";

    /// <summary>Rich text (HTML produced by the TipTap editor).</summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Body { get; set; } = "";

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? Color { get; set; }

    public bool IsPinned { get; set; }

    public bool IsArchived { get; set; }

    public bool IsDeleted { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lower-cased, Arabic-normalized plain text of title + body + tags. Lets every
    /// provider search with a plain LIKE instead of engine-specific full-text features.
    /// </summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string SearchText { get; set; } = "";
}
