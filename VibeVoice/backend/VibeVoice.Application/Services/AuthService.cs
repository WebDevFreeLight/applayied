using Microsoft.AspNetCore.Identity;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

public sealed class AuthService(
    IRepository<User> users,
    IPasswordHasher<User> hasher,
    ITokenService tokens,
    ICurrentUser currentUser,
    Microsoft.Extensions.Options.IOptions<AuthOptions> authOptions)
{
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var normalized = NormalizeEmail(request.Email);
        if (await users.AnyAsync(u => u.NormalizedEmail == normalized, ct))
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalized,
            DisplayName = request.DisplayName.Trim(),
            PreferredLanguage = request.PreferredLanguage is "ar" ? "ar" : "en",
            Role = await IsAdminOnRegisterAsync(normalized, ct) ? UserRole.Admin : UserRole.User,
            LastLoginAt = DateTime.UtcNow,
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        await users.InsertAsync(user, ct);
        return Issue(user);
    }

    /// <summary>
    /// With Auth:BootstrapAdminEmail set, only that address becomes Admin. Otherwise the very first account
    /// administers the installation (fine on a private machine, risky on a public URL).
    /// </summary>
    private async Task<bool> IsAdminOnRegisterAsync(string normalizedEmail, CancellationToken ct)
    {
        var bootstrap = authOptions.Value.BootstrapAdminEmail;
        if (!string.IsNullOrWhiteSpace(bootstrap)) return NormalizeEmail(bootstrap) == normalizedEmail;
        return !await users.AnyAsync(u => true, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var normalized = NormalizeEmail(request.Email);
        var user = await users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        // Same message for unknown email and wrong password so accounts can't be enumerated.
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            throw new UnauthorizedAppException("Invalid email or password.");
        if (!user.IsActive) throw new ForbiddenException("This account has been deactivated.");

        user.LastLoginAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        return Issue(user);
    }

    public async Task<UserDto> MeAsync(CancellationToken ct) => (await LoadCurrentAsync(ct)).ToDto();

    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct)
    {
        var user = await LoadCurrentAsync(ct);
        user.DisplayName = request.DisplayName.Trim();
        user.PreferredLanguage = request.PreferredLanguage;
        user.PreferredVoice = string.IsNullOrWhiteSpace(request.PreferredVoice) ? null : request.PreferredVoice.Trim();
        user.Theme = request.Theme;
        if (request.Palette is not null) user.Palette = request.Palette; // older clients omit it
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        return user.ToDto();
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await LoadCurrentAsync(ct);
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
            throw new BadRequestException("The current password is incorrect.",
                new Dictionary<string, string[]> { ["currentPassword"] = ["The current password is incorrect."] });
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
    }

    // ---------- admin ----------

    public async Task<List<UserDto>> ListUsersAsync(CancellationToken ct) =>
        (await users.ListAsync(u => true, ct)).OrderBy(u => u.CreatedAt).Select(u => u.ToDto()).ToList();

    public async Task<UserDto> SetRoleAsync(Guid id, UserRole role, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct) ?? throw new NotFoundException("User", id);
        if (user.Role == UserRole.Admin && role != UserRole.Admin) await EnsureAnotherAdminAsync(user.Id, ct);
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        return user.ToDto();
    }

    public async Task<UserDto> SetActiveAsync(Guid id, bool isActive, CancellationToken ct)
    {
        if (id == currentUser.UserId && !isActive) throw new BadRequestException("You cannot deactivate your own account.");
        var user = await users.GetByIdAsync(id, ct) ?? throw new NotFoundException("User", id);
        if (!isActive && user.Role == UserRole.Admin) await EnsureAnotherAdminAsync(user.Id, ct);
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        return user.ToDto();
    }

    /// <summary>Never leave the installation without an active administrator.</summary>
    private async Task EnsureAnotherAdminAsync(Guid exceptUserId, CancellationToken ct)
    {
        if (!await users.AnyAsync(u => u.Role == UserRole.Admin && u.IsActive && u.Id != exceptUserId, ct))
            throw new BadRequestException("At least one active administrator is required.");
    }

    private async Task<User> LoadCurrentAsync(CancellationToken ct) =>
        await users.GetByIdAsync(currentUser.UserId, ct) ?? throw new UnauthorizedAppException("The account no longer exists.");

    private AuthResponse Issue(User user)
    {
        var (token, expires) = tokens.CreateToken(user);
        return new AuthResponse(token, expires, user.ToDto());
    }
}
