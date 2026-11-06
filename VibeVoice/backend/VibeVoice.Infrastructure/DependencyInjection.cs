using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using VibeVoice.Application.Abstractions;
using VibeVoice.Application.Common;
using VibeVoice.Infrastructure.Auth;
using VibeVoice.Infrastructure.Persistence;
using VibeVoice.Infrastructure.Storage;
using VibeVoice.Infrastructure.Voice;

namespace VibeVoice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, string contentRoot)
    {
        services.Configure<DatabaseOptions>(config.GetSection(DatabaseOptions.Section));
        services.Configure<StorageOptions>(config.GetSection(StorageOptions.Section));
        services.Configure<UploadOptions>(config.GetSection(UploadOptions.Section));

        // ---- SqlSugar: one thread-safe SqlSugarScope for the app (AsyncLocal keeps transactions per request) ----
        services.AddSingleton<ISqlSugarClient>(sp =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SqlSugar");
            return new SqlSugarScope(DbProviderFactory.CreateConfig(dbOptions, contentRoot), db =>
            {
                if (dbOptions.LogSql)
                    db.Aop.OnLogExecuting = (sql, _) => logger.LogDebug("{Sql}", sql);
            });
        });
        services.AddScoped(typeof(IRepository<>), typeof(SqlSugarRepository<>));
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IMemoryRepository, MemoryRepository>();
        services.AddScoped<IUnitOfWork, SqlSugarUnitOfWork>();
        services.AddTransient<DatabaseInitializer>();

        // ---- Files & images ----
        services.AddSingleton<IFileStorage>(sp =>
            new LocalFileStorage(sp.GetRequiredService<IOptions<StorageOptions>>(), contentRoot, sp.GetRequiredService<TimeProvider>()));
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();

        // ---- Auth ----
        var jwt = config.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
        services.AddSingleton(jwt);
        services.AddSingleton<ITokenService, JwtTokenService>();

        // ---- Voice service: typed HttpClient + retry / timeout / circuit breaker ----
        var voice = config.GetSection(VoiceServiceOptions.Section).Get<VoiceServiceOptions>() ?? new VoiceServiceOptions();
        services.AddSingleton(voice);
        services.AddHttpClient<IVoiceServiceClient, VoiceServiceClient>(c =>
            {
                c.BaseAddress = new Uri(voice.BaseUrl.TrimEnd('/') + "/");
                c.Timeout = Timeout.InfiniteTimeSpan; // the resilience pipeline owns timeouts
            })
            .AddStandardResilienceHandler(o =>
            {
                var attempt = TimeSpan.FromSeconds(voice.RequestTimeoutSeconds);
                o.AttemptTimeout.Timeout = attempt;
                o.TotalRequestTimeout.Timeout = attempt * 2 + TimeSpan.FromSeconds(10);
                o.Retry.MaxRetryAttempts = 1;
                o.Retry.Delay = TimeSpan.FromMilliseconds(500);
                o.CircuitBreaker.SamplingDuration = attempt * 2;
                o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
            });

        return services;
    }
}
