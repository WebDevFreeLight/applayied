using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VibeVoice.Application.Services;
using VibeVoice.Domain.Entities;

namespace VibeVoice.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Error dictionary keys must match the camelCase JSON field names the frontend binds to.
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);
        services.AddValidatorsFromAssemblyContaining<TaggingService>(includeInternalTypes: true);
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<TaggingService>();
        services.AddScoped<AttachmentService>();
        services.AddScoped<NoteService>();
        services.AddScoped<MemoryService>();
        services.AddSingleton<AccessCache>();
        services.AddScoped<AccessService>();
        services.AddScoped<AuthService>();
        services.AddScoped<RoleService>();
        services.AddScoped<UserAdminService>();
        services.AddScoped<LibraryService>();
        services.AddScoped<VoiceStudioService>();
        services.AddScoped<JobService>();
        services.AddScoped<JobRunner>();
        services.AddScoped<SearchService>();
        services.AddScoped<DashboardService>();
        return services;
    }
}
