using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using VibeVoice.Application.Services;
using VibeVoice.Domain;

namespace VibeVoice.Api.Infrastructure;

/// <summary>The signed-in account must still exist and be active (a deactivated user is locked out at once).</summary>
public sealed class ActiveUserRequirement : IAuthorizationRequirement;

/// <summary>The signed-in account's role must grant <see cref="Permission"/>.</summary>
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

/// <summary>
/// Checks both requirements against the database (through a short cache) instead of trusting JWT claims,
/// so role edits and deactivations apply on the next request rather than when the token expires.
/// </summary>
public sealed class AccessAuthorizationHandler(AccessService access) : IAuthorizationHandler
{
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pending = context.PendingRequirements.Where(r => r is ActiveUserRequirement or PermissionRequirement).ToList();
        if (pending.Count == 0) return;

        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId)) return;
        var http = (context.Resource as HttpContext) ?? (context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)?.HttpContext;
        var userAccess = await access.GetAsync(userId, http?.RequestAborted ?? CancellationToken.None);
        if (userAccess is not { IsActive: true }) return; // leaves requirements unmet → 403

        foreach (var requirement in pending)
        {
            if (requirement is ActiveUserRequirement || (requirement is PermissionRequirement p && userAccess.Has(p.Permission)))
                context.Succeed(requirement);
        }
    }
}

public static class PermissionPolicies
{
    public static string For(string permission) => $"perm:{permission}";

    /// <summary>Default policy (authenticated + active) and one policy per permission.</summary>
    public static AuthorizationBuilder AddPermissionPolicies(this AuthorizationBuilder builder)
    {
        builder.SetDefaultPolicy(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new ActiveUserRequirement())
            .Build());
        foreach (var p in Permissions.All)
            builder.AddPolicy(For(p.Key), policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new ActiveUserRequirement(), new PermissionRequirement(p.Key)));
        return builder;
    }

    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder =>
        builder.RequireAuthorization(For(permission));
}
