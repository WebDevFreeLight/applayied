using SqlSugar;

namespace VibeVoice.Domain.Entities;

[SugarTable("users")]
[SugarIndex("ux_users_email", nameof(NormalizedEmail), OrderByType.Asc, true)]
public class User
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [SugarColumn(Length = 256)]
    public string Email { get; set; } = "";

    [SugarColumn(Length = 256)]
    public string NormalizedEmail { get; set; } = "";

    [SugarColumn(Length = 100)]
    public string DisplayName { get; set; } = "";

    [SugarColumn(Length = 500)]
    public string PasswordHash { get; set; } = "";

    public UserRole Role { get; set; } = UserRole.User;

    [SugarColumn(Length = 5)]
    public string PreferredLanguage { get; set; } = "en";

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? PreferredVoice { get; set; }

    [SugarColumn(Length = 10)]
    public string Theme { get; set; } = "system";

    /// <summary>Visual identity: sunset | oasis | ocean | neon. Default lets CodeFirst add the column to existing rows.</summary>
    [SugarColumn(Length = 20, DefaultValue = "sunset")]
    public string Palette { get; set; } = "sunset";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [SugarColumn(IsNullable = true)]
    public DateTime? LastLoginAt { get; set; }
}
