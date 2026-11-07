using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>Custom roles. Admin (all permissions) and User are built in and cannot be deleted.</summary>
public sealed class RoleService(IRepository<Role> roles, IRepository<User> users, AccessCache cache)
{
    public static IReadOnlyList<PermissionDto> ListPermissions() =>
        Permissions.All.Select(p => new PermissionDto(p.Key, p.Group)).ToList();

    public async Task<List<RoleDto>> ListAsync(CancellationToken ct)
    {
        var all = await roles.ListAsync(r => true, ct);
        var counts = (await users.ListAsync(u => true, ct))
            .GroupBy(u => u.RoleId ?? Role.UserId).ToDictionary(g => g.Key, g => g.Count());
        // System roles first (Admin, User), then custom roles alphabetically.
        return all.OrderByDescending(r => r.IsSystem).ThenBy(r => r.Id == Role.AdminId ? 0 : 1).ThenBy(r => r.Name)
            .Select(r => r.ToDto(counts.GetValueOrDefault(r.Id))).ToList();
    }

    public async Task<RoleDto> CreateAsync(SaveRoleRequest request, CancellationToken ct)
    {
        var role = new Role();
        await ApplyAsync(role, request, ct);
        await roles.InsertAsync(role, ct);
        return role.ToDto(0);
    }

    public async Task<RoleDto> UpdateAsync(Guid id, SaveRoleRequest request, CancellationToken ct)
    {
        var role = await roles.GetByIdAsync(id, ct) ?? throw new NotFoundException("Role", id);
        if (role.Id == Role.AdminId)
            throw new BadRequestException("The Admin role always has every permission and cannot be changed.");
        if (role.IsSystem && !string.Equals(role.Name, request.Name.Trim(), StringComparison.Ordinal))
            throw new BadRequestException("Built-in roles cannot be renamed.");

        await ApplyAsync(role, request, ct);
        role.UpdatedAt = DateTime.UtcNow;
        await roles.UpdateAsync(role, ct);
        cache.InvalidateAll();
        return role.ToDto(await users.CountAsync(u => u.RoleId == id, ct));
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var role = await roles.GetByIdAsync(id, ct) ?? throw new NotFoundException("Role", id);
        if (role.IsSystem) throw new BadRequestException("Built-in roles cannot be deleted.");
        var inUse = await users.CountAsync(u => u.RoleId == id, ct);
        if (inUse > 0)
            throw new ConflictException($"{inUse} user(s) still have this role. Move them to another role first.");
        await roles.DeleteAsync(r => r.Id == id, ct);
    }

    private async Task ApplyAsync(Role role, SaveRoleRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        var normalized = TextNormalizer.Normalize(name);
        if (await roles.AnyAsync(r => r.NormalizedName == normalized && r.Id != role.Id, ct))
            throw new ConflictException($"A role named '{name}' already exists.");

        var unknown = (request.Permissions ?? []).Where(p => !Permissions.Keys.Contains(p)).ToList();
        if (unknown.Count > 0) throw new BadRequestException($"Unknown permission(s): {string.Join(", ", unknown)}.");

        role.Name = name;
        role.NormalizedName = normalized;
        role.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        role.SetPermissions(request.Permissions ?? []);
    }
}
