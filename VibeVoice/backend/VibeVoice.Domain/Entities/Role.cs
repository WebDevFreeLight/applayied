using SqlSugar;

namespace VibeVoice.Domain.Entities;

/// <summary>A named set of <see cref="Permissions"/>. Admin and User are built in (system) roles.</summary>
[SugarTable("roles")]
[SugarIndex("ux_roles_name", nameof(NormalizedName), OrderByType.Asc, true)]
public class Role
{
    public static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-00000000a001");
    public static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-00000000a002");

    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [SugarColumn(Length = 50)]
    public string Name { get; set; } = "";

    [SugarColumn(Length = 50)]
    public string NormalizedName { get; set; } = "";

    [SugarColumn(Length = 300, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>Comma-separated permission keys (provider-neutral; the list is small).</summary>
    [SugarColumn(Length = 1000)]
    public string PermissionKeys { get; set; } = "";

    /// <summary>Built-in roles cannot be renamed or deleted; Admin's permissions are fixed to "everything".</summary>
    public bool IsSystem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public IReadOnlyList<string> GetPermissions() =>
        Id == AdminId
            ? Permissions.All.Select(p => p.Key).ToList()
            : PermissionKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(Permissions.Keys.Contains).Distinct().ToList();

    public void SetPermissions(IEnumerable<string> keys) =>
        PermissionKeys = string.Join(',', keys.Where(Permissions.Keys.Contains).Distinct().OrderBy(k => k));
}
