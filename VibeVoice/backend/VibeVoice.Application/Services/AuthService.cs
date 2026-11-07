using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Application.Dtos;
using VibeVoice.Domain;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application.Services;

/// <summary>Self-service account actions: register, sign in, profile, password. Administration lives in <see cref="UserAdminService"/>.</summary>
public sealed class AuthService(
    IRepository<User> users,
    IRepository<Role> roles,
    IPasswordHasher<User> hasher,
    ITokenService tokens,
    ICurrentUser currentUser,
    IOptions<AuthOptions> authOptions)
{
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var normalized = NormalizeEmail(request.Email);
        if (await users.AnyAsync(u => u.NormalizedEmail == normalized, ct))
            throw new ConflictException("An account with this email already exists.");

        var isAdmin = await IsAdminOnRegisterAsync(normalized, ct);
        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalized,
            DisplayName = request.DisplayName.Trim(),
            PreferredLanguage = request.PreferredLanguage is "ar" ? "ar" : "en",
            RoleId = isAdmin ? Role.AdminId : Role.UserId,
            Role = isAdmin ? UserRole.Admin : UserRole.User,
            LastLoginAt = DateTime.UtcNow,
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        await users.InsertAsync(user, ct);
        return await IssueAsync(user, ct);
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
        return await IssueAsync(user, ct);
    }

    public async Task<UserDto> MeAsync(CancellationToken ct)
    {
        var user = await LoadCurrentAsync(ct);
        return user.ToDto(await RoleOfAsync(user, ct));
    }

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
        return user.ToDto(await RoleOfAsync(user, ct));
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

    private async Task<Role> RoleOfAsync(User user, CancellationToken ct) =>
        await roles.GetByIdAsync(user.RoleId ?? Role.UserId, ct)
        ?? await roles.GetByIdAsync(Role.UserId, ct)
        ?? throw new InvalidOperationException("Built-in roles are missing; the database was not initialized.");

    private async Task<User> LoadCurrentAsync(CancellationToken ct) =>
        await users.GetByIdAsync(currentUser.UserId, ct) ?? throw new UnauthorizedAppException("The account no longer exists.");

    private async Task<AuthResponse> IssueAsync(User user, CancellationToken ct)
    {
        var role = await RoleOfAsync(user, ct);
        var (token, expires) = tokens.CreateToken(user, role.Name);
        return new AuthResponse(token, expires, user.ToDto(role));
    }
}
