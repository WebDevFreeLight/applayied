using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>Administration of accounts: list, create, edit, reset passwords, (de)activate, change role, delete.</summary>
public sealed class UserAdminService(
    IRepository<User> users,
    IRepository<Role> roles,
    IRepository<Note> notes,
    IRepository<Memory> memories,
    IRepository<AudioClip> audio,
    IRepository<ImageAttachment> images,
    IRepository<NoteTag> noteTags,
    IRepository<MemoryTag> memoryTags,
    IRepository<MemoryPerson> memoryPeople,
    IRepository<Tag> tags,
    IRepository<Person> people,
    IPasswordHasher<User> hasher,
    IUnitOfWork uow,
    AttachmentService attachments,
    VoiceStudioService voice,
    AccessCache cache,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<AdminUserDto>> ListAsync(UserQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        Expression<Func<User, bool>> where = u => true;
        if (query.RoleId is { } roleId)
            where = roleId == Role.UserId
                ? where.And(u => u.RoleId == roleId || u.RoleId == null)
                : where.And(u => u.RoleId == roleId);
        if (query.Active is { } active) where = where.And(u => u.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToUpperInvariant();
            where = where.And(u => u.NormalizedEmail.Contains(term) || u.DisplayName.Contains(query.Search.Trim()));
        }

        var (items, total) = await users.PageAsync(where, u => u.CreatedAt, true, page, pageSize, ct);
        var roleMap = await RoleMapAsync(ct);
        var ids = items.Select(u => u.Id).ToList();
        var userNotes = ids.Count == 0 ? [] : await notes.ListAsync(n => ids.Contains(n.UserId) && !n.IsDeleted, ct);
        var userMemories = ids.Count == 0 ? [] : await memories.ListAsync(m => ids.Contains(m.UserId) && !m.IsDeleted, ct);
        var userAudio = ids.Count == 0 ? [] : await audio.ListAsync(a => ids.Contains(a.UserId), ct);
        var userImages = ids.Count == 0 ? [] : await images.ListAsync(i => ids.Contains(i.UserId), ct);

        var rows = items.Select(u => new AdminUserDto(
            u.ToDto(RoleOf(u, roleMap)),
            userNotes.Count(n => n.UserId == u.Id),
            userMemories.Count(m => m.UserId == u.Id),
            userAudio.Count(a => a.UserId == u.Id) + userImages.Count(i => i.UserId == u.Id),
            userAudio.Where(a => a.UserId == u.Id).Sum(a => a.SizeBytes) + userImages.Where(i => i.UserId == u.Id).Sum(i => i.SizeBytes)))
            .ToList();
        return new(rows, page, pageSize, total);
    }

    public async Task<UserDto> GetAsync(Guid id, CancellationToken ct)
    {
        var user = await LoadAsync(id, ct);
        return user.ToDto(await RoleAsync(user.RoleId, ct));
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        var normalized = AuthService.NormalizeEmail(request.Email);
        if (await users.AnyAsync(u => u.NormalizedEmail == normalized, ct))
            throw new ConflictException("An account with this email already exists.");
        var role = await RoleAsync(request.RoleId, ct);

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalized,
            DisplayName = request.DisplayName.Trim(),
            PreferredLanguage = request.PreferredLanguage is "ar" ? "ar" : "en",
            RoleId = role.Id,
            Role = role.Id == Role.AdminId ? Domain.UserRole.Admin : Domain.UserRole.User,
            IsActive = request.IsActive,
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        await users.InsertAsync(user, ct);
        return user.ToDto(role);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var user = await LoadAsync(id, ct);
        var role = await RoleAsync(request.RoleId, ct);
        var normalized = AuthService.NormalizeEmail(request.Email);
        if (normalized != user.NormalizedEmail && await users.AnyAsync(u => u.NormalizedEmail == normalized && u.Id != id, ct))
            throw new ConflictException("An account with this email already exists.");

        await GuardAdminChangeAsync(user, newRoleId: role.Id, newActive: request.IsActive, ct);

        user.Email = request.Email.Trim();
        user.NormalizedEmail = normalized;
        user.DisplayName = request.DisplayName.Trim();
        user.PreferredLanguage = request.PreferredLanguage is "ar" ? "ar" : "en";
        user.RoleId = role.Id;
        user.Role = role.Id == Role.AdminId ? Domain.UserRole.Admin : Domain.UserRole.User;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        cache.InvalidateAll();
        return user.ToDto(role);
    }

    public async Task<UserDto> SetRoleAsync(Guid id, Guid roleId, CancellationToken ct)
    {
        var user = await LoadAsync(id, ct);
        var role = await RoleAsync(roleId, ct);
        await GuardAdminChangeAsync(user, newRoleId: role.Id, newActive: user.IsActive, ct);
        user.RoleId = role.Id;
        user.Role = role.Id == Role.AdminId ? Domain.UserRole.Admin : Domain.UserRole.User;
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        cache.InvalidateAll();
        return user.ToDto(role);
    }

    public async Task<UserDto> SetActiveAsync(Guid id, bool isActive, CancellationToken ct)
    {
        var user = await LoadAsync(id, ct);
        await GuardAdminChangeAsync(user, newRoleId: user.RoleId ?? Role.UserId, newActive: isActive, ct);
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        cache.InvalidateAll(); // a deactivated user is locked out on their next request
        return user.ToDto(await RoleAsync(user.RoleId, ct));
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken ct)
    {
        var user = await LoadAsync(id, ct);
        user.PasswordHash = hasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
    }

    /// <summary>Deletes the account and everything it owns (notes, memories, media files, tags, people).</summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var user = await LoadAsync(id, ct);
        if (user.Id == currentUser.UserId) throw new BadRequestException("You cannot delete your own account.");
        await GuardAdminChangeAsync(user, newRoleId: Role.UserId, newActive: false, ct);

        var noteIds = (await notes.ListAsync(n => n.UserId == id, ct)).Select(n => n.Id).ToList();
        var memoryIds = (await memories.ListAsync(m => m.UserId == id, ct)).Select(m => m.Id).ToList();
        var clipFiles = (await audio.ListAsync(a => a.UserId == id, ct)).Select(a => a.FilePath);
        var imageFiles = (await images.ListAsync(i => i.UserId == id, ct)).SelectMany(i => new[] { i.FilePath, i.ThumbnailPath });
        var files = clipFiles.Concat(imageFiles).ToList();

        await uow.ExecuteInTransactionAsync(async () =>
        {
            if (noteIds.Count > 0) await noteTags.DeleteAsync(x => noteIds.Contains(x.NoteId), ct);
            if (memoryIds.Count > 0)
            {
                await memoryTags.DeleteAsync(x => memoryIds.Contains(x.MemoryId), ct);
                await memoryPeople.DeleteAsync(x => memoryIds.Contains(x.MemoryId), ct);
            }
            await audio.DeleteAsync(a => a.UserId == id, ct);
            await images.DeleteAsync(i => i.UserId == id, ct);
            await notes.DeleteAsync(n => n.UserId == id, ct);
            await memories.DeleteAsync(m => m.UserId == id, ct);
            await tags.DeleteAsync(t => t.UserId == id, ct);
            await people.DeleteAsync(p => p.UserId == id, ct);
            await users.DeleteAsync(u => u.Id == id, ct);
        }, ct);
        await attachments.DeleteFilesQuietlyAsync(files);
        cache.InvalidateAll();
    }

    public async Task<AdminOverviewDto> OverviewAsync(CancellationToken ct)
    {
        var all = await users.ListAsync(u => true, ct);
        var roleMap = await RoleMapAsync(ct);
        var audioRows = await audio.ListAsync(a => true, ct);
        var imageRows = await images.ListAsync(i => true, ct);
        return new AdminOverviewDto(
            Users: all.Count,
            ActiveUsers: all.Count(u => u.IsActive),
            Admins: all.Count(u => u.IsActive && (u.RoleId ?? Role.UserId) == Role.AdminId),
            Roles: roleMap.Count,
            Notes: await notes.CountAsync(n => !n.IsDeleted, ct),
            Memories: await memories.CountAsync(m => !m.IsDeleted, ct),
            AudioClips: audioRows.Count,
            Images: imageRows.Count,
            StorageBytes: audioRows.Sum(a => a.SizeBytes) + imageRows.Sum(i => i.SizeBytes),
            Voice: await voice.StatusAsync(ct),
            RecentUsers: all.OrderByDescending(u => u.CreatedAt).Take(5).Select(u => u.ToDto(RoleOf(u, roleMap))).ToList());
    }

    /// <summary>
    /// Never lock everyone out: the last active Admin can't be demoted, deactivated or deleted, and
    /// admins can't deactivate or demote themselves (another admin has to do it).
    /// </summary>
    private async Task GuardAdminChangeAsync(User user, Guid newRoleId, bool newActive, CancellationToken ct)
    {
        var isAdminNow = user.IsActive && (user.RoleId ?? Role.UserId) == Role.AdminId;
        var staysAdmin = newActive && newRoleId == Role.AdminId;
        if (user.Id == currentUser.UserId && !newActive) throw new BadRequestException("You cannot deactivate your own account.");
        if (user.Id == currentUser.UserId && isAdminNow && !staysAdmin)
            throw new BadRequestException("You cannot remove your own Admin role. Ask another admin.");
        if (isAdminNow && !staysAdmin &&
            !await users.AnyAsync(u => u.Id != user.Id && u.IsActive && u.RoleId == Role.AdminId, ct))
            throw new BadRequestException("At least one active administrator is required.");
    }

    private async Task<User> LoadAsync(Guid id, CancellationToken ct) =>
        await users.GetByIdAsync(id, ct) ?? throw new NotFoundException("User", id);

    private async Task<Role> RoleAsync(Guid? roleId, CancellationToken ct) =>
        await roles.GetByIdAsync(roleId ?? Role.UserId, ct) ?? throw new BadRequestException("That role does not exist.");

    private async Task<Dictionary<Guid, Role>> RoleMapAsync(CancellationToken ct) =>
        (await roles.ListAsync(r => true, ct)).ToDictionary(r => r.Id);

    private static Role RoleOf(User u, Dictionary<Guid, Role> map) =>
        map.GetValueOrDefault(u.RoleId ?? Role.UserId) ?? map[Role.UserId];
}
