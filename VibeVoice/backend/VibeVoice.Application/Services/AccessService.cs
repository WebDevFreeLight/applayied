using System.Collections.Concurrent;
using VibeVoice.Application.Abstractions;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>What a signed-in user may do right now (checked on every request, not baked into the JWT).</summary>
public sealed record UserAccess(bool IsActive, Guid RoleId, IReadOnlySet<string> Permissions)
{
    public bool Has(string permission) => IsActive && Permissions.Contains(permission);
}

/// <summary>
/// Short-lived per-user cache so permission checks don't hit the database on every request.
/// Any user/role change calls <see cref="InvalidateAll"/>, so edits apply immediately.
/// </summary>
public sealed class AccessCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<Guid, (UserAccess Access, DateTime Until)> _entries = new();

    public bool TryGet(Guid userId, out UserAccess access)
    {
        if (_entries.TryGetValue(userId, out var e) && e.Until > DateTime.UtcNow)
        {
            access = e.Access;
            return true;
        }
        access = null!;
        return false;
    }

    public void Set(Guid userId, UserAccess access) => _entries[userId] = (access, DateTime.UtcNow + Ttl);

    public void InvalidateAll() => _entries.Clear();
}

public sealed class AccessService(IRepository<User> users, IRepository<Role> roles, AccessCache cache)
{
    /// <summary>Null when the user no longer exists.</summary>
    public async Task<UserAccess?> GetAsync(Guid userId, CancellationToken ct)
    {
        if (cache.TryGet(userId, out var cached)) return cached;
        var user = await users.GetByIdAsync(userId, ct);
        if (user is null) return null;
        var role = await roles.GetByIdAsync(user.RoleId ?? Role.UserId, ct);
        var access = new UserAccess(user.IsActive, user.RoleId ?? Role.UserId, (role?.GetPermissions() ?? []).ToHashSet());
        cache.Set(userId, access);
        return access;
    }
}
