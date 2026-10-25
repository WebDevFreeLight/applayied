using AspNetCoreRateLimit;
using Infrastructure.Converter;
using Infrastructure.Extensions;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using NLog.Web;
using SqlSugar;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZR.Admin.WebApi.Extensions;
using ZR.Common.Cache;
using ZR.HR;
using ZR.Infrastructure.WebExtensions;
using ZR.Pdf;
 
using ZR.Model.HR.EmployeeCore;
using ZR.Model.Notification;
using ZR.ServiceCore.Signalr;
using ZR.ServiceCore.SqlSugar;
using ZR.ServiceCore.Services;
using static QRCoder.PayloadGenerator;
//using SQLitePCL;

// On a cloud/free host (PORT, RENDER or DATABASE_URL provided) force a neutral "Cloud" environment at
// builder creation, so the hard-coded appsettings.Production.json (Kestrel HTTPS on :8887 with a cert,
// a remote DB host) never loads. Only base appsettings.json applies, plus the DATABASE_URL/PORT overrides below.
var builder = new[] { "PORT", "RENDER", "DATABASE_URL", "DB_CONN" }
        .Any(k => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(k)))
    ? WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, EnvironmentName = "Cloud" })
    : WebApplication.CreateBuilder(args);

// ---- Free-host / cloud deploy wiring (Render, Neon, Railway, Fly, …) --------------------------
// A single DATABASE_URL (postgres://user:pass@host:port/db) or a ready DB_CONN overrides the
// PostgreSQL connection for every DbConfig, and PORT drives the listen URL — so no secrets live in
// the repo and the same image runs anywhere. Local dev is untouched (neither env var is set).
{
    var dbConn = Environment.GetEnvironmentVariable("DB_CONN");
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var webPort = Environment.GetEnvironmentVariable("PORT");
    var onCloud = !string.IsNullOrWhiteSpace(webPort) || !string.IsNullOrWhiteSpace(dbUrl) || !string.IsNullOrWhiteSpace(dbConn);

    // On a cloud/free host, drop the environment-specific appsettings.{Environment}.json layer
    // (Production/Staging/Docker) — those hard-code a remote/localhost DB host, port 8887 and HTTPS
    // certificates that don't exist here and crash the container. Keep only base appsettings.json
    // plus the DATABASE_URL / PORT overrides below.
    if (onCloud)
    {
        for (int si = builder.Configuration.Sources.Count - 1; si >= 0; si--)
        {
            if (builder.Configuration.Sources[si] is Microsoft.Extensions.Configuration.Json.JsonConfigurationSource js
                && !string.IsNullOrEmpty(js.Path)
                && js.Path.StartsWith("appsettings.", StringComparison.OrdinalIgnoreCase)
                && !js.Path.Equals("appsettings.json", StringComparison.OrdinalIgnoreCase))
            {
                builder.Configuration.Sources.RemoveAt(si);
            }
        }
    }

    if (string.IsNullOrWhiteSpace(dbConn) && !string.IsNullOrWhiteSpace(dbUrl))
    {
        try
        {
            var u = new Uri(dbUrl);
            var ui = u.UserInfo.Split(':');
            var user = Uri.UnescapeDataString(ui[0]);
            var pass = ui.Length > 1 ? Uri.UnescapeDataString(ui[1]) : "";
            var db = u.AbsolutePath.Trim('/');
            var dbPort = u.Port > 0 ? u.Port : 5432;
            dbConn = $"Host={u.Host};Port={dbPort};Database={db};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=100;Connection Idle Lifetime=300;";
        }
        catch (Exception ex) { Console.WriteLine($"⚠️  Could not parse DATABASE_URL: {ex.Message}"); }
    }
    var overrides = new System.Collections.Generic.Dictionary<string, string?>();
    if (!string.IsNullOrWhiteSpace(dbConn))
    {
        overrides["DbConfigs:0:Conn"] = dbConn;
        overrides["DbConfigs:1:Conn"] = dbConn;
        overrides["CodeGenDbConfig:Conn"] = dbConn;
    }
    if (!string.IsNullOrWhiteSpace(webPort))
    {
        var listen = $"http://0.0.0.0:{webPort}";
        overrides["urls"] = listen;
        overrides["ASPNETCORE_URLS"] = listen;
        overrides["UseHttps"] = "false";
    }
    if (overrides.Count > 0) builder.Configuration.AddInMemoryCollection(overrides);
}

// Configure Kestrel to handle HTTPS properly with SSL certificates chnages 50 088
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var env = builder.Environment;
    var config = builder.Configuration;

    // Check if we should use HTTPS
    var useHttps = config.GetValue<bool>("UseHttps", true);
    var urls = config["urls"] ?? config["ASPNETCORE_URLS"] ?? "";

    Console.WriteLine($"🔐 Configuring Kestrel HTTPS...");
    Console.WriteLine($"   Environment: {env.EnvironmentName}");
    Console.WriteLine($"   URLs: {urls}");
    Console.WriteLine($"   UseHttps: {useHttps}");

    // In production or when HTTPS URLs are specified, configure accordingly
    if (useHttps && urls.Contains("https://", StringComparison.OrdinalIgnoreCase))
    {
        if (env.IsDevelopment()&&false)
        {
            // In development, use developer certificate if available
            Console.WriteLine("   ℹ️  Development mode: Using developer certificate");
            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ServerCertificateSelector = null;
            });
        }
        else
        {
            // In production, check for certificate configuration from multiple sources
            var certPath = config["Kestrel:Endpoints:Https:Certificate:Path"]
                        ?? config["Kestrel:Certificates:Default:Path"]
                        ?? config["ASPNETCORE_Kestrel__Certificates__Default__Path"];

            var certKeyPath = config["Kestrel:Endpoints:Https:Certificate:KeyPath"]
                           ?? config["Kestrel:Certificates:Default:KeyPath"]
                           ?? config["ASPNETCORE_Kestrel__Certificates__Default__KeyPath"];

            var certPassword = config["Kestrel:Certificates:Default:Password"];// d dcdf

            Console.WriteLine($"   Certificate Path: {certPath}");
            Console.WriteLine($"   Key Path: {certKeyPath}");

            if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
            {
                if (!string.IsNullOrEmpty(certKeyPath) && File.Exists(certKeyPath))
                {
                    try
                    {
                        // Load certificate with both cert and key paths
                        Console.WriteLine("✅ Loading X.509 certificate with separate key file...");
                        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath);

                        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            httpsOptions.ServerCertificate = cert;
                        });

                        Console.WriteLine("✅ HTTPS certificate loaded successfully");
                        Console.WriteLine($"   Subject: {cert.Subject}");
                        Console.WriteLine($"   Issuer: {cert.Issuer}");
                        Console.WriteLine($"   Not Before: {cert.NotBefore:yyyy-MM-dd}");
                        Console.WriteLine($"   Not After: {cert.NotAfter:yyyy-MM-dd}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  Error loading certificate: {ex.Message}");
                        Console.WriteLine("   Falling back to HTTP only");
                        urls = urls.Replace("https://", "http://");
                        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", urls);
                    }
                }
                else if (!string.IsNullOrEmpty(certPassword))
                {
                    try
                    {
                        // Load certificate with password (PFX format)
                        Console.WriteLine("✅ Loading X.509 certificate with password...");
                        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, certPassword);

                        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            httpsOptions.ServerCertificate = cert;
                        });

                        Console.WriteLine("✅ HTTPS certificate loaded successfully");
                        Console.WriteLine($"   Subject: {cert.Subject}");
                        Console.WriteLine($"   Issuer: {cert.Issuer}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  Error loading certificate: {ex.Message}");
                        Console.WriteLine("   Falling back to HTTP only");
                        urls = urls.Replace("https://", "http://");
                        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", urls);
                    }
                }
                else
                {
                    try
                    {
                        // Load certificate without password
                        Console.WriteLine("✅ Loading X.509 certificate without password...");
                        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath);

                        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            httpsOptions.ServerCertificate = cert;
                        });

                        Console.WriteLine("✅ HTTPS certificate loaded successfully");
                        Console.WriteLine($"   Subject: {cert.Subject}");
                        Console.WriteLine($"   Issuer: {cert.Issuer}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  Error loading certificate: {ex.Message}");
                        Console.WriteLine("   Falling back to HTTP only");
                        urls = urls.Replace("https://", "http://");
                        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", urls);
                    }
                }
            }
            else
            {
                // Fallback to HTTP only if no certificate is available
                Console.WriteLine("⚠️ No HTTPS certificate found at: " + certPath);
                Console.WriteLine("   Falling back to HTTP only");
                urls = urls.Replace("https://", "http://");
                Environment.SetEnvironmentVariable("ASPNETCORE_URLS", urls);
            }
        }
    }
    else if (!useHttps)
    {
        Console.WriteLine("ℹ️  HTTPS disabled via UseHttps setting");
    }
});

// Configure application configuration sources


Console.WriteLine("Starting application...");
Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine("env"+  builder.Environment.EnvironmentName );
builder.Configuration
    //.SetBasePath(Directory.GetCurrentDirectory())
    //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("codeGen.json", optional: true, reloadOnChange: true)
    .AddJsonFile("iprate.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Log the current environment
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"ContentRoot: {builder.Environment.ContentRootPath}");
Console.WriteLine($"WebRoot: {builder.Environment.WebRootPath}");

// NLog: Setup NLog for Dependency injection
//builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddControllers();

// Configure API Documentation (Swagger and Scalar)
builder.Services.AddApiDocumentation(builder.Configuration);

// NOTE: DynamicApi causes Swagger to hang - commented out for now
// builder.Services.AddDynamicApi();
//Inject HttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// CORS configuration
builder.Services.AddCors(builder.Configuration);
//Eliminate Error unprotecting the session cookie warning
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "DataProtection"));
//Regular captcha
builder.Services.AddCaptcha(builder.Configuration);
//IPRatelimit
builder.Services.AddIPRate(builder.Configuration);
//builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
//Bind entire object to Model
builder.Services.Configure<OptionsSetting>(builder.Configuration);
// Exception Handling System - MUST be registered early
builder.Services.AddExceptionHandling();





 




//JWT authentication
builder.Services.AddJwt();
//Configuration file
builder.Services.AddSingleton(new AppSettings(builder.Configuration));
//App service registration
builder.Services.AddAppService();

// Register Lazy<T> wrappers for services that have circular dependencies
// This allows GLPeriodClosingService to depend on Lazy<IGLJournalService> to break the cycle
builder.Services.AddTransient(typeof(Lazy<>), typeof(LazyResolver<>));

// PMO multi-provider AI gateway (ordered fallback chain configured under "AI:Providers").
// Registered with a typed HttpClient so it gets a pooled, DI-managed client.
builder.Services.AddHttpClient<ZR.Service.PMO.IPmoService.IPmoAiService, ZR.Service.PMO.PmoAiService>();
// PMO Power BI embed-token service (Azure AD service principal -> embed for your organization).
builder.Services.AddHttpClient<ZR.Service.PMO.IPmoService.IPmoPowerBiService, ZR.Service.PMO.PmoPowerBiService>();
// PMO ML bridge -> Python scikit-learn service (ml/, FastAPI). Config section "ML" (BaseUrl, ApiKey).
builder.Services.AddHttpClient<ZR.Service.PMO.IPmoService.IPmoMlService, ZR.Service.PMO.PmoMlService>();
// PMO business-automation background worker (overdue escalation + budget-overrun alerts).
builder.Services.AddHostedService<ZR.Admin.WebApi.Framework.PmoAutomationWorker>();

// ✅ Initialize database - MUST be BEFORE builder.Build()
builder.Services.AddDb(builder.Environment, builder.Configuration);

//HR service registration
//builder.Services.AddHRServices();
//PDF and Puppeteer services registration using extension methods


builder.Services.AddPdfRenderer(builder.Configuration);



//Enable scheduled tasks
builder.Services.AddTaskSchedulers();
//Request size limit
builder.Services.AddRequestLimit(builder.Configuration);
//SQLite package required driver
//Batteries_V2.Init();
//Register REDIS service
var openRedis = builder.Configuration["RedisServer:open"];
if (openRedis == "1")
{
    RedisServer.Initalize();
}

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(GlobalActionMonitor));//Global registration
})
.AddJsonOptions(options =>
{

    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new JsonConverterUtil.DateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonConverterUtil.DateTimeNullConverter());
    options.JsonSerializerOptions.Converters.Add(new StringConverter());
    options.JsonSerializerOptions.Converters.Add(new LongJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableLongJsonConverter());
    //PropertyNamingPolicy property is used for the format strategy of properties passed from frontend, currently only CamelCase is built-in
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    //options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;//Properties can ignore case format, performance will decrease after enabling
});
//Inject SignalR real-time communication, default using json transport
builder.Services.AddSignalR()
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});


// In ConfigureServices or builder.Services






// Swagger configuration is now handled by AddDynamicSwagger
// builder.Services.AddSwaggerConfig(); // Removed - using dynamic configuration
// Display logo
builder.Services.AddLogo();
// Add localization service (must match where ExceptionMessages.resx lives)
// NOTE: AddExceptionHandling() already configures localization with ResourcesPath="Resources";
// keep this consistent to avoid ResourceNotFound -> returning the code string.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
// Call at the very beginning of application startup
var app = builder.Build();
InternalApp.ServiceProvider = app.Services;
InternalApp.Configuration = builder.Configuration;
InternalApp.WebHostEnvironment = app.Environment;
//Initialize db

// Initialize database using the new automatic discovery method
var initDb = builder.Configuration["InitDb"].ParseToBool();
if (initDb)
{
    Console.WriteLine("InitDb is enabled - creating missing tables...");
    // Use InitMissingTables to only create tables that don't exist (safe for restarts)
    try
    {
        ZR.ServiceCore.SqlSugar.InitTable.InitMissingTables();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ InitMissingTables skipped: {ex.Message}");
    }

    // Seed initial data after table creation
    try
    {
        Console.WriteLine("Starting database seeding...");
        //var seedDataService = new SeedDataService();
        var dataPath = Path.Combine(app.Environment.WebRootPath, "data.xlsx");
        var webRootPath = app.Environment.ContentRootPath;

        if (File.Exists(dataPath))
        {
            //var seedResults = seedDataService.InitSeedData(dataPath, webRootPath, false);
            Console.ForegroundColor = ConsoleColor.Green;
            //foreach (var result in seedResults)
            //{
            //    Console.WriteLine(result);
            //}
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Database seeding completed!");
        }
        else
        {
            Console.WriteLine($"Seed data file not found at: {dataPath}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error during database seeding: {ex.Message}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}

// PMO synthetic-data seeding (independent of InitDb). Set "PmoSeed": true once to import the
// reproducible CSV package (data/csv) via the ORM. Idempotent: existing rows are left untouched.
if (builder.Configuration["PmoSeed"].ParseToBool())
{
    try
    {
        var csvDir = builder.Configuration["PmoSeedPath"];
        if (string.IsNullOrWhiteSpace(csvDir))
            csvDir = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "data", "csv"));
        Console.WriteLine($"[PMO seed] Importing from: {csvDir}");
        foreach (var line in new ZR.Service.PMO.PmoDataSeeder().Seed(csvDir))
            Console.WriteLine(line);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[PMO seed] Error: {ex.Message}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}


// Initialize Sales module table (sync deal_type + share_percentage columns)


// // Initialize Operations module table
// builder.Environment.InitDbWithTypes(
//     forceRecreate: false,
//     typeof(SiteVisit));

// Safe migration: CodeFirst.InitTables only adds missing columns/tables, never drops existing
// try
// {
//     var _migrationDb = app.Services.GetRequiredService<ISqlSugarClient>();
//     var migrationTypes = new[] { typeof(BpLead), typeof(SiteVisit), typeof(OpsTicket), typeof(BlvBoulevardSeasonZones), typeof(BlvBoulevardShops) };
//     foreach (var t in migrationTypes)
//     {
//         _migrationDb.CodeFirst.InitTables(t);
//         Console.WriteLine($"Ensured table/columns for: {t.Name}");
//     }
// }
// catch (Exception ex)
// {
//     Console.WriteLine($"Migration skipped: {ex.Message}");
// }


// ==================== MARKETING PR MODULE INITIALIZATION ====================
// Initialize all new Marketing PR tables (safe - only creates if not exists)
try
{
    var _mktDb = app.Services.GetRequiredService<ISqlSugarClient>();
    //var mktTypes = new[]
    //{
    //    // Campaigns
    //    typeof(MktCampaign), typeof(MktCampaignChannel), typeof(MktCampaignPerformance), typeof(MktAbTest),
    //    // AI Engine
    //    typeof(MktAudienceSegment), typeof(MktSegmentMember), typeof(MktPredictiveModel),
    //    typeof(MktBehaviorPrediction), typeof(MktAiAgent), typeof(MktAiAgentTask),
    //    typeof(MktCompetitorAnalysis), typeof(MktBudgetOptimization),
    //    // Customer Journeys
    //    typeof(MktCustomerJourney), typeof(MktJourneyStage), typeof(MktJourneyTrigger),
    //    typeof(MktJourneyExecution), typeof(MktDynamicContent),
    //    // Investor Nexus
    //    typeof(MktInvestmentOpportunity), typeof(MktSponsorPackage),
    //    typeof(MktSponsorApplication), typeof(MktInvestorKpiReport),
    //    // Crowd Analytics
    //    typeof(MktCrowdAnalytics), typeof(MktCrowdHeatmap),
    //    typeof(MktExhibitorReport), typeof(MktRealtimeInsight),
    //    // Social Media
    //    typeof(MktSocialAccount), typeof(MktSocialPost), typeof(MktSocialAnalytics),
    //    // Email Marketing
    //    typeof(MktEmailTemplate), typeof(MktEmailCampaign), typeof(MktSubscriber),
    //    typeof(MktSubscriberList), typeof(MktSubscriberListMember),
    //    // Content Management
    //    typeof(MktContent), typeof(MktAiContentGeneration),
    //    // Public Relations
    //    typeof(MktPressRelease), typeof(MktMediaContact), typeof(MktMediaMention), typeof(MktPrSentiment),
    //    // Advertising
    //    typeof(MktAdCampaign), typeof(MktAdCreative), typeof(MktAdPerformance)
    //};
    //foreach (var t in mktTypes)
    //{
    //    _mktDb.CodeFirst.InitTables(t);
    //}
    //Console.WriteLine($"✅ Marketing PR module: {mktTypes.Length} tables initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Marketing PR table init skipped: {ex.Message}");
}
// ==================== END MARKETING PR MODULE INITIALIZATION ====================

 // ❌ REMOVED: AddDb was moved to BEFORE builder.Build() (line 247)
 // builder.Services.AddDb(app.Environment);//

//builder.Services.InitDb(app.Environment);

//builder.Services.InitDbWithNamespaces(app.Environment, "ZR.Model.HR", "ZR.Mall.Model");
//builder.Services.InitDbWithNamespaces(app.Environment, "ZR.Model");
//builder.Services.InitDbWithNamespaces(app.Environment);

// ==================== SALES DEALS MODULE INITIALIZATION ====================

// ==================== END SALES DEALS MODULE INITIALIZATION ====================

// ==================== OPERATIONS PERMITS MODULE INITIALIZATION ====================
try
{
    var _opsPermitDb = app.Services.GetRequiredService<ISqlSugarClient>();
    //var opsPermitTypes = new[] {
    //    typeof(ZR.Model.Operations.OpsCustomerWorker),
    //    typeof(ZR.Model.Operations.OpsPermitType),
    //    typeof(ZR.Model.Operations.OpsPermit),
    //    typeof(ZR.Model.Operations.OpsPermitVerification),
    //    typeof(ZR.Model.Operations.OpsGeoTemporalRule)
    //};
    //foreach (var t in opsPermitTypes)
    //{
    //    _opsPermitDb.CodeFirst.InitTables(t);
    //}
    //Console.WriteLine($"✅ Operations Permits module: {opsPermitTypes.Length} tables initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Operations Permits table init skipped: {ex.Message}");
}
// ==================== END OPERATIONS PERMITS MODULE INITIALIZATION ====================

// ==================== STORE READINESS & HANDOVER MODULE INITIALIZATION ====================

// ==================== END STORE READINESS & HANDOVER MODULE INITIALIZATION ====================

//   builder.Environment.InitDbWithTypes(
//   forceRecreate: true,
//   typeof(DmCategory)
//  );

//    // HR Employee Core (Normalized) - new tables
//    typeof(BpMeeting),
//    typeof(BpMeetingConflictLock),
//    typeof(BpMeetingOutcome),
//    typeof(BpMeetingParticipant)
//);

// ==================== NEW ATTENDANCE MODULE INITIALIZATION ====================
// Initialize all new Attendance module tables
//builder.Environment.InitDbWithTypes(
//    forceRecreate: false, // Set to true to recreate tables

//    // NEW: Employee Calendar Assignment
//    typeof(HrEmployeeCalendarAssignment),

//    // NEW: Payroll Attendance Summary (CRITICAL for payroll)
//    typeof(HrPayrollAttendanceSummary)
//);

// Console.WriteLine("✅ Attendance module tables initialized successfully");
// // ==================== END ATTENDANCE MODULE INITIALIZATION ====================
// // ==================== NOTIFICATION MODULE INITIALIZATION ====================
// builder.Environment.InitDbWithTypes(
//       forceRecreate: true,
//       typeof(ErpNotification),
//       typeof(ErpNotificationRecipient)
// );
// ==================== END NOTIFICATION MODULE ====================
//curl - X POST "https://localhost:8885/finance/accounts/seed" \  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkJlYXJlciJ9.eyJwcmltYXJ5c2lkIjoiMSIsIm5hbWVpZCI6IjEiLCJ1bmlxdWVfbmFtZSI6ImFkbWluIiwiZ3JvdXBzaWQiOiIwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy91c2VyZGF0YSI6IntcIlVzZXJJZFwiOjEsXCJEZXB0SWRcIjowLFwiVXNlck5hbWVcIjpcImFkbWluXCIsXCJOaWNrTmFtZVwiOlwi2YXYr9mK2LFcIixcIlJvbGVLZXlzXCI6W1wiYWRtaW5cIl0sXCJSb2xlc1wiOlt7XCJSb2xlSWRcIjoxLFwiUm9sZUtleVwiOlwiYWRtaW5cIixcIkRhdGFTY29wZVwiOjF9XSxcIkV4cGlyZVRpbWVcIjpcIjAwMDEtMDEtMDFUMDA6MDA6MDBcIixcIlRlbmFudElkXCI6bnVsbCxcIlBlcm1pc3Npb25zXCI6W1wiKjoqOipcIl0sXCJDb21wYW55SURcIjowLFwiQnJhbmNoSURcIjowfSIsIkF1ZGllbmNlIjoibWFoYW0tYWkuTkVUIiwiSXNzdWVyIjoibWFoYW0tYWkuTkVUIiwibmJmIjoxNzY0MDgzMzA2LCJleHAiOjE3NjQxNjk3MDYsImlhdCI6MTc2NDA4MzMwNiwiaXNzIjoibWFoYW0tYWkuTkVUIiwiYXVkIjoibWFoYW0tYWkuTkVUIn0.XcOiTC7daflHQE7JHEzLKZW5-QspyB5c5QtNRofKsKI
// Force recreate only these specific tables (drops and recreates them, ignores global InitDb setting)
//builder.Environment.InitDbWithTypes(
//      forceRecreate: true,
//       // Configuration Models
//     typeof(ERPSettings));
//       typeof(Currency),
//       typeof(TaxRule),
//       typeof(AutoPostingRule),
//       typeof(ERPSettings),
//       // Core Finance Models
//       typeof(GLAccount),
//       typeof(GLAccountOpeningBalance),
//       typeof(GLAccountBalanceSummary),
//       typeof(GLFinancialReport),
//       typeof(GLJournal),
//       typeof(GLJournalEntry),
//       typeof(GLPeriodClosing),
//       typeof(SystemConfiguration),
//       typeof(Currency),
//       typeof(CurrencySetting),
//       typeof(ExchangeRate),
//       // ARAP Models
//       typeof(ZR.Model.Finance.ARAP.Customer),
//       typeof(ZR.Model.Finance.ARAP.Vendor),
//       typeof(ZR.Model.Finance.ARAP.ARInvoice),
//       typeof(ZR.Model.Finance.ARAP.APInvoice),
//       typeof(ZR.Model.Finance.ARAP.APInvoiceLine),
//       typeof(ZR.Model.Finance.ARAP.ARInvoiceLine),
//       typeof(ZR.Model.Finance.ARAP.CreditDebitNote),
//       typeof(ZR.Model.Finance.ARAP.InvoiceNumberSequence),
//       typeof(ZR.Model.Finance.ARAP.PaymentTerms),

//       // Treasury Models
//       typeof(ZR.Model.Finance.Treasury.BankAccount),
//       typeof(ZR.Model.Finance.Treasury.BankReconciliation),
//       typeof(ZR.Model.Finance.Treasury.BankReconciliationLine),
//       typeof(ZR.Model.Finance.Treasury.BankStatement),
//       typeof(ZR.Model.Finance.Treasury.BankStatementLine),
//       typeof(ZR.Model.Finance.Treasury.CashAccount),
//       typeof(ZR.Model.Finance.Treasury.Cheque),
//       typeof(ZR.Model.Finance.Treasury.BalanceLedger),

//       typeof(ZR.Model.Finance.Treasury.TreasuryConfig),
//       typeof(ZR.Model.Finance.Treasury.TreasuryGLAccountMapping),
//       typeof(ZR.Model.Finance.Treasury.ApprovalLimit),
//       typeof(ZR.Model.Finance.Treasury.TreasuryNumberSequence),
//       typeof(ZR.Model.Finance.Treasury.TreasuryDataRetentionConfig),
//       typeof(ZR.Model.Finance.Treasury.BankParsingProfile),
//       typeof(ZR.Model.Finance.Treasury.TreasuryAuditLog),
//      // Assets Models
//      typeof(ZR.Model.Finance.Assets.AssetCategory),
//      typeof(ZR.Model.Finance.Assets.AssetDepreciation),
//      typeof(ZR.Model.Finance.Assets.AssetTransaction),
//      typeof(ZR.Model.Finance.Assets.CurrentAsset),
//      typeof(ZR.Model.Finance.Assets.FixedAsset),

//     // Core Finance Models
//     typeof(ZR.Model.Finance.Tax.TaxCode),
//     typeof(ZR.Model.Finance.Tax.TaxGroup),
//     typeof(ZR.Model.Finance.Tax.TaxJournalLine),
//     typeof(ZR.Model.Finance.Tax.TaxRate),
//     typeof(ZR.Model.Finance.Tax.TaxGroupLine),

//     //  Payment Hub
//     typeof(ZR.Model.Finance.PaymentHub.Payment),
//     typeof(ZR.Model.Finance.PaymentHub.PaymentAllocation),
//     typeof(ZR.Model.Finance.PaymentHub.PaymentEvent)
//);


// Force recreate these additional tables
//builder.Environment.InitDbWithTypes(
//      forceRecreate: true,
//       typeof(ZR.Model.Finance.Treasury.Cheque),
//       typeof(ZR.Model.Finance.Treasury.ChequeStatusHistory),

// typeof(ZR.Model.Finance.CostCenter.CostCenter),

//typeof(ZR.Model.Finance.ARAP.ARInvoice),
// typeof(ZR.Model.Finance.ARAP.ARInvoiceLine),
//    typeof(ZR.Model.Finance.ARAP.CreditDebitNote),
//typeof(ZR.Model.Finance.ARAP.Customer),
//   typeof(ZR.Model.Finance.ARAP.Vendor)

//typeof(ZR.Model.Finance.Zatca.ZatcaConfiguration),
//typeof(ZR.Model.Finance.Zatca.ZatcaDevice),
//  typeof(ZR.Model.Finance.Zatca.ZatcaInvoiceHash),
//    typeof(ZR.Model.Finance.Zatca.ZatcaSubmissionLog),
//   typeof(ZR.Model.Finance.PaymentHub.Payment)
//);

  
//builder.Environment.InitDbWithTypes(
//  forceRecreate: true,
//  typeof(GLAccount),
//    typeof(GLJournal),

//      typeof(GLJournalEntry),
//        typeof(BudgetHeader),
//    typeof(BudgetLine),

//      typeof(BudgetVarianceSummary),
//          typeof(CostCenter),
//   typeof(ARInvoice),
//   typeof(ARInvoiceLine),
////Main Contract Tables
//typeof(ErpContract),
//typeof(ErpContractAmendment),
//typeof(ErpContractBillingSchedule),
//typeof(ErpContractBillingScheduleLine),
//typeof(ErpContractPricingRule),
//typeof(ErpContractRenewal),
//typeof(ErpContractTermination),
//typeof(ErpContractUnit),
//// Contract Configuration Tables
//typeof(ErpContractFinancial),
//typeof(ErpContractBillingConfig),
//typeof(ErpContractLifecycle),
//// Pricing Rule Extensions
//typeof(ErpContractEscalationRule),
//typeof(ErpContractSeasonalRule),
//// Billing & Payment
//typeof(ErpBillingPaymentStatus),
//// Termination Related Tables
//typeof(ErpContractTerminationFinancial),
//typeof(ErpContractTerminationDeposit),
//typeof(ErpContractTerminationInspection),
//// Unit Related Tables
//typeof(ErpContractUnitGuarantee)
//);



//app.Environment.InitDbWithTypes(
////    // Main tables
////    //typeof(Event), typeof(EventZone), typeof(EventGate),
////    //typeof(EventVisitor), typeof(EventTicket), typeof(EventPermit),
////    //typeof(EventBooth), typeof(EventTenant), typeof(EventContract),
////    //typeof(EventIncident), typeof(EventPayment), typeof(EventInvoice),

////    //// Lookup tables
////    //typeof(EventType), typeof(EventZoneType), typeof(EventGateType),
////    //typeof(VisitorType), typeof(TicketType), typeof(PermitType),
////    //typeof(BoothType), typeof(IncidentType), typeof(PaymentMethod)
////,
//typeof(EventStatus)
//,
//typeof(EventLocation)
//);

//app.Environment.InitDbWithTypes(

//typeof(Bp_PartnerDocsments)
//   //,
//   //typeof(BlvBoulevardSeasonZones),
//   //typeof(BlvBoulevardShopStatus),
//   //typeof(BlvBoulevardShops)
//   //, typeof(HrInsuranceEnrollment)
//   //, typeof(HrInsuranceCoveragelevel)
//   //, typeof(BusinessPartnerkind)
//   //, typeof(ContactMethod)
//   //, typeof(BusinessPartner)
//   //, typeof(PartnerDetails)
//   //,typeof(OrganizationType)
//   //,typeof(HrAttendance)
//   );


//app.Environment.InitDbWithTypes( 


//    typeof(SysMenu)

//    //typeof(AgentMode)
//    //, typeof(TeamMode)
//    //, typeof(ModelProvider)
//    //, typeof(ToolType)
//    //, typeof(MemoryType)
//    //, typeof(ChunkingStrategy)
//    //, typeof(VectorDbType)
//    //, typeof(EmbeddingModel)
//    //, typeof(WorkflowStatus)
//    //, typeof(KnowledgeType)
//    //, typeof(EvaluationMetric)
//    //, typeof(ApplicationType)
//    //, typeof(ModelVersion)
//    //, typeof(AIModel)
//    //, typeof(Agent)
//    //, typeof(AgentTool)
//    //, typeof(Team)
//    //, typeof(TeamMember)
//    //, typeof(Tool)
//    //, typeof(Memory)
//    //, typeof(AgentMemory)
//    //, typeof(KnowledgeBase)
//    //, typeof(KnowledgeBaseAccess)
//    //, typeof(KnowledgeDocument)
//    //, typeof(DocumentChunk)
//    //, typeof(EmbeddingConfig)
//    //, typeof(TeamSession)
//    //, typeof(SessionMessage)
//    //, typeof(ToolExecution)
//);

// Initialize ERP Module Tables
//builder.Environment.InitDbWithTypes(
//    // Sales Module
//    typeof(ErpCustomer),
//    typeof(ErpCustomerBalance),
//    typeof(ErpSalesInvoice),
//    typeof(ErpSalesInvoiceItem),
//    typeof(ErpSalesReturn),
//    typeof(ErpSalesReturnItem),
//    typeof(ErpAdvancePayment),

//    // Inventory Module
//    typeof(ErpProduct),
//    typeof(ErpWarehouse),
//    typeof(ErpInventoryTransaction),

//    // Subscriptions Module
//    typeof(ErpMember),
//    typeof(ErpMemberGroup),
//    typeof(ErpMemberVisit),
//    typeof(ErpMemberFingerprintLog),
//    typeof(ErpSubscription),
//    typeof(ErpSubscriptionPackage),
//    typeof(ErpSubscriptionFreeze),
//    typeof(ErpActivity),
//    typeof(ErpActivityGroup),
//    typeof(ErpActivityAttendance),
//    typeof(ErpBooking),
//    typeof(ErpResource),

//    // Marketing Module
//    typeof(ErpPromoCode),
//    typeof(ErpGiftCard),
//    typeof(ErpNotification),

//    // Accounts Module
//    typeof(ErpAccount),
//    typeof(ErpJournalEntry),
//    typeof(ErpJournalEntryLine),
//    typeof(ErpExpense),
//    typeof(ErpCostCenter),
//    typeof(ErpFinancialPeriod),
//    typeof(ErpReceipt),
//    typeof(ErpPaymentVoucher),

//    // Treasury Module
//    typeof(ErpTreasury),
//    typeof(ErpTreasuryTransaction),
//    typeof(ErpTreasuryTransfer),

//    // HR Module
//    typeof(ErpEmployee),
//    typeof(ErpDepartment),
//    typeof(ErpPosition),
//    typeof(ErpEmployeeAttendance),
//    typeof(ErpCommission),
//    typeof(ErpCommissionTier),
//    typeof(ErpServiceStaff),
//    typeof(ErpBiometricDevice),
//    typeof(ErpBiometricZone),
//    typeof(ErpIncentiveSetting),

//    // Operations Module
//    typeof(ErpShift),
//    typeof(ErpDailyReport),

//    // Suppliers Module
//    typeof(ErpSupplier),
//    typeof(ErpSupplierBalance),

//    // AI Module
//    typeof(ErpAiAgent),
//    typeof(ErpAiAgentTask),
//    typeof(ErpAiDecision),
//    typeof(ErpAiThinkingLog),

//    // ZATCA Module
//    typeof(ErpZatcaInvoice),
//    typeof(ErpZatcaSettings),

//    // Settings Module
//    typeof(ErpBranch),
//    typeof(ErpPaymentMethod),
//    typeof(ErpExpenseType),
//    typeof(ErpUnitOfMeasure),
//    typeof(ErpServiceCategory),
//    typeof(ErpMessageSettings)
//);

//ActionPermissionFilterInfo.PrintAllFiltersWithPermissions(); 
//try
//{
//    builder.Environment.InitDbWithTypes(
//     forceRecreate: true,
//      typeof(OpsPermit),
//      typeof(OpsPermitType),
//      typeof(OpsPermitVerification),
//      typeof(OpsCustomerWorker),
//      typeof(OpsGeoTemporalRule),
//      typeof(OpsWorkType),
//      typeof(OpsWorkCategory),
//      typeof(OpsSafetyPrecaution),
//      typeof(OpsPermitWorker),
//      typeof(OpsPermitSafetyCheck));
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"⚠️ Ops permit table init skipped: {ex.Message}");
//}
//   typeof(HrAttendanceLog),

//   // Attendance Processing
//   typeof(HrAttendanceDay),
//   typeof(HrAttendanceDaySnapshot),
//   typeof(HrAttendanceAdjustment),
//   typeof(HrAttendanceEvent),
//   typeof(HrAttendanceSegmentResult),
//   typeof(HrPayrollAttendanceSummary),

//   // Overtime
//   typeof(HrOvertimePolicy),
//   typeof(HrOvertimeRequest),
//   typeof(HrOvertimeResult),
//   typeof(HrOvertimeSegment),
//   typeof(HrOvertimeVerification),

//   // Shifts Scheduling
//   typeof(HrShift),
//   typeof(HrShiftCalendarOverride),
//   typeof(HrShiftSegment),
//   typeof(HrEmployeeShiftAssignment),

//   // Leave Management
//   typeof(HrLeave),
//   typeof(HrLeaveBalance),
//   typeof(HrLeavePolicy),
//   typeof(HrLeaveRequest),
//   typeof(HrLeaveType),
//   typeof(HrLeaveAttendanceEffect),

//   // Configuration
//   typeof(HrAttendancePolicy),
//   typeof(HrEmployeeCalendarAssignment),
//   typeof(HrHoliday),
//   typeof(HrRoundingRule),
//   typeof(HrWorkCalendar),

//   // Audit
//   typeof(HrAttendanceAuditLog),
//    typeof(BpLead),
//     typeof(MarketingTeamMember)
//  );
// builder.Environment.InitDbWithTypes(
//   forceRecreate: true,

//     typeof(ZR.Model.CustomerAssets.AssetType),
//     typeof(ZR.Model.CustomerAssets.CustomerAsset),
//     typeof(ZR.Model.CustomerAssets.CustomerAssetAssignment),
//     typeof(ZR.Model.CustomerAssets.CustomerContractFile));

// ==================== DATA MINE MODULE INITIALIZATION ====================
// try
// {
//     builder.Environment.InitDbWithTypes(
//         forceRecreate: false,
//         typeof(DmContact),
//         typeof(DmCategory));
//     Console.WriteLine("✅ Data Mine module: DmContact + DmCategory tables initialized successfully");
// }
// catch (Exception ex)
// {
//     Console.WriteLine($"⚠️ Data Mine table init skipped: {ex.Message}");
// }
// ==================== END DATA MINE MODULE INITIALIZATION ====================
// builder.Environment.InitDbWithTypes(
//   forceRecreate: true,

//     typeof(DmPhoneReveal),

//       typeof(OpsHandoverRequirement),
//         typeof(OpsStoreHandover),
//             typeof(OpsStoreHandoverItem)
     
//     );
  

if (initDb)
{
    Console.WriteLine("Initializing database tables...");

    Console.WriteLine("Database initialization complete.");
}



var workId = builder.Configuration["workId"].ParseToInt();
if (app.Environment.IsDevelopment())
{
    workId += 1;
}
SnowFlakeSingle.WorkId = workId;
//Use global exception middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure middleware to support localization
var supportedCultures = new List<CultureInfo> {
                    new CultureInfo("zh-Hant"),
                    new CultureInfo("zh-CN"),
                    new CultureInfo("ar"),
                    new CultureInfo("en")
                };
app.UseRequestLocalization(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.FallBackToParentCultures = true;
});

//Request header forwarding  
//ForwardedHeaders middleware automatically fills X-Forwarded-For (client real IP) and X-Forwarded-Proto (client request protocol) forwarded from reverse proxy server into HttpContext.Connection.RemoteIPAddress and HttpContext.Request.Scheme, so the application code reads the real IP and real protocol without special handling.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.Use((context, next) =>
{
    //Set to allow multiple reads of body content
    context.Request.EnableBuffering();
    if (context.Request.Query.TryGetValue("access_token", out var token))
    {
        context.Request.Headers.Append("Authorization", $"Bearer {token}");
    }
    return next();
});
//Enable access to static files/wwwroot directory files, must be placed before UseRouting
app.UseStaticFiles();
//Enable routing access
app.UseRouting();
app.UseCors("Policy");//Must be placed before app.UseEndpoints.
//app.UseHttpsRedirection();

// Inject fixed CompanyId/BranchId into HttpContext.Items EARLY (before auth) for development scoping
app.UseFixedCompanyBranchScoping();

app.UseAuthentication();
app.UseMiddleware<JwtAuthMiddleware>();
app.UseMiddleware<Infrastructure.Middleware.IdempotencyMiddleware>();
app.UseAuthorization();

//Enable caching
app.UseResponseCaching();
if (builder.Environment.IsProduction())
{
    //Restore/start tasks
    app.UseAddTaskSchedulers();
}
//Initialize dictionary data
app.UseInit();

// Use API Documentation (Swagger and/or Scalar based on configuration)
app.UseApiDocumentation(builder.Configuration);
app.LogApiDocumentationUrls(builder.Configuration);

//Enable client IP rate limiting
app.UseIpRateLimiting();
app.UseRateLimiter();
//Enable WebSocket support (required for SignalR WebSocket transport)
app.UseWebSockets();
//Set socket connection
app.MapHub<MessageHub>("/msgHub");






app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
// Use Scalar that works with Swagger's configuration
if (app is WebApplication webApp)
{
    webApp.UseScalarWithSwagger(builder.Configuration);
    webApp.UseStaticFilesOn(builder.Configuration);
}

app.MapFallbackToController("Index", "Common");

app.Run();

/// <summary>
/// Generic Lazy resolver for dependency injection
/// Allows injecting Lazy<T> to break circular dependencies
/// The service is resolved only when .Value is accessed
/// </summary>
public class LazyResolver<T> : Lazy<T> where T : class
{
    public LazyResolver(IServiceProvider serviceProvider)
        : base(() => serviceProvider.GetRequiredService<T>())
    {
    }
}

