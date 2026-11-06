using SqlSugar;

namespace VibeVoice.Domain.Entities;

[SugarTable("tags")]
[SugarIndex("ux_tags_user_name", nameof(UserId), OrderByType.Asc, nameof(NormalizedName), OrderByType.Asc, true)]
public class Tag
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [SugarColumn(Length = 50)]
    public string Name { get; set; } = "";

    [SugarColumn(Length = 50)]
    public string NormalizedName { get; set; } = "";
}

[SugarTable("note_tags")]
public class NoteTag
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid NoteId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public Guid TagId { get; set; }
}

[SugarTable("memory_tags")]
public class MemoryTag
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid MemoryId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public Guid TagId { get; set; }
}

/// <summary>Someone who appears in a memory.</summary>
[SugarTable("people")]
[SugarIndex("ux_people_user_name", nameof(UserId), OrderByType.Asc, nameof(NormalizedName), OrderByType.Asc, true)]
public class Person
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [SugarColumn(Length = 100)]
    public string Name { get; set; } = "";

    [SugarColumn(Length = 100)]
    public string NormalizedName { get; set; } = "";
}

[SugarTable("memory_people")]
public class MemoryPerson
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid MemoryId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public Guid PersonId { get; set; }
}
