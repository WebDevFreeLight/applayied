using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using VibeVoice.Api.Endpoints;
using VibeVoice.Api.Infrastructure;
using VibeVoice.Application;
using VibeVoice.Application.Abstractions;
using VibeVoice.Infrastructure;
using VibeVoice.Infrastructure.Auth;
using VibeVoice.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

// ---- Layers ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

// ---- JSON: camelCase, enums as strings, UTC timestamps ----
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
});

// ---- Errors ----
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();

// ---- Upload limits (audio can be large) ----
var maxBody = builder.Configuration.GetValue<long?>("Uploads:MaxRequestBytes") ?? 120L * 1024 * 1024;
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = maxBody);
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = maxBody);

// ---- Auth: JWT bearer; file URLs used by <img>/<audio> may pass ?access_token= ----
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.MapInboundClaims = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = jwt.SigningKey(),
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(1),
    };
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api/files") && ctx.Request.Query.TryGetValue("access_token", out var token))
                ctx.Token = token;
            return Task.CompletedTask;
        },
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole("Admin"));

// Slows down password guessing on the anonymous auth endpoints.
var authPermits = builder.Configuration.GetValue<int?>("RateLimiting:AuthPermitsPerMinute") ?? 20;
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = authPermits, Window = TimeSpan.FromMinutes(1) }));
});

// ---- CORS for the Vite dev server (and any configured origins) ----
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("X-Voice-Service", "Content-Disposition")));

// ---- OpenAPI / Swagger ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VibeVoice API", Version = "v1", Description = "Notes & memories with text, audio, images and Microsoft VibeVoice speech." });
    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token from POST /api/auth/login",
    });
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("bearer", doc)] = [] });
    c.CustomSchemaIds(t => t.FullName?.Replace('+', '.'));
});

var app = builder.Build();

// ---- Database: CodeFirst create/update + optional seed ----
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.DocumentTitle = "VibeVoice API");
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/", () => Results.Redirect(app.Environment.IsDevelopment() ? "/swagger" : "/api/health")).ExcludeFromDescription();
app.MapAuth();
app.MapNotes();
app.MapMemories();
app.MapLibrary();
app.MapVoice();
app.MapFilesAndHealth();

app.Run();

/// <summary>Exposed for WebApplicationFactory in the integration tests.</summary>
public partial class Program;
