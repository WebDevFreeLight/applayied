using SqlSugar;
using ZR.ServiceCore.Seeders;

namespace ZR.Admin.WebApi
{
    /// <summary>
    /// Standalone Seeder Runner
    /// Can be executed from Program.cs or command line
    /// </summary>
    public class SeederRunner
    {
        private readonly ISqlSugarClient _db;
        private readonly ILogger<SeederRunner> _logger;

        public SeederRunner(ISqlSugarClient db, ILogger<SeederRunner> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Run menu seeder
        /// </summary>
        public async Task RunMenuSeederAsync(bool truncateFirst = false)
        {
            _logger.LogInformation("🌱 Starting Menu Seeder...");
            _logger.LogInformation($"Truncate first: {truncateFirst}");

            try
            {
                var seeder = new MenuSeeder(_db);
                var result = await seeder.SeedAsync(truncateFirst);

                if (result.Success)
                {
                    _logger.LogInformation("✅ {Message}", result.Message);
                    _logger.LogInformation("   📊 Inserted: {InsertedCount}", result.InsertedCount);
                    if (truncateFirst)
                    {
                        _logger.LogInformation("   🗑️  Deleted: {DeletedCount}", result.DeletedCount);
                    }
                }
                else
                {
                    _logger.LogError("❌ {Message}", result.Message);
                    if (result.Exception != null)
                    {
                        _logger.LogError(result.Exception, "Exception details:");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Fatal error during menu seeding");
            }
        }

        /// <summary>
        /// Display permissions summary
        /// </summary>
        public async Task DisplayPermissionsSummaryAsync()
        {
            _logger.LogInformation("📋 Loading Permissions Summary...");

            try
            {
                var seeder = new MenuSeeder(_db);
                var permissions = await seeder.GetPermissionsAsync();

                _logger.LogInformation("📊 Total Modules: {ModuleCount}", permissions.Count);

                foreach (var module in permissions)
                {
                    _logger.LogInformation("   📁 {Module}: {PermissionCount} permissions",
                        module.Module, module.Permissions.Count);
                }

                var totalPermissions = permissions.Sum(m => m.Permissions.Count);
                _logger.LogInformation("✅ Total Permissions: {TotalPermissions}", totalPermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load permissions");
            }
        }

        /// <summary>
        /// Display mapping review summary
        /// </summary>
        public async Task DisplayMappingReviewAsync()
        {
            _logger.LogInformation("🔍 Loading Mapping Review...");

            try
            {
                var seeder = new MenuSeeder(_db);
                var mapping = await seeder.GetMappingReviewAsync();

                _logger.LogInformation("📊 Mapping Statistics:");
                _logger.LogInformation("   Total Routes: {RouteCount}", mapping.Mappings.Count);

                var highConfidence = mapping.Mappings.Count(m => m.Value.Confidence == "high");
                var mediumConfidence = mapping.Mappings.Count(m => m.Value.Confidence == "medium");
                var lowConfidence = mapping.Mappings.Count(m => m.Value.Confidence == "low");

                _logger.LogInformation("   🟢 High Confidence: {HighCount}", highConfidence);
                _logger.LogInformation("   🟡 Medium Confidence: {MediumCount}", mediumConfidence);
                _logger.LogInformation("   🔴 Low Confidence: {LowCount}", lowConfidence);

                if (mapping.AmbiguousCases != null && mapping.AmbiguousCases.Count > 0)
                {
                    _logger.LogWarning("⚠️  Ambiguous Cases: {AmbiguousCount}", mapping.AmbiguousCases.Count);
                    foreach (var ambiguous in mapping.AmbiguousCases)
                    {
                        _logger.LogWarning("   - {Route}: {Issue}", ambiguous.Route, ambiguous.Issue);
                    }
                }

                if (mapping.Recommendations != null && mapping.Recommendations.Count > 0)
                {
                    _logger.LogInformation("💡 Recommendations:");
                    foreach (var recommendation in mapping.Recommendations)
                    {
                        _logger.LogInformation("   - {Recommendation}", recommendation);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load mapping review");
            }
        }

        /// <summary>
        /// Run all seeders and display summaries
        /// </summary>
        public async Task RunAllAsync(bool truncateFirst = false)
        {
            _logger.LogInformation("🚀 Running All Seeders...");
            _logger.LogInformation(new string('=', 60));

            await RunMenuSeederAsync(truncateFirst);

            _logger.LogInformation(new string('-', 60));
            await DisplayPermissionsSummaryAsync();

            _logger.LogInformation(new string('-', 60));
            await DisplayMappingReviewAsync();

            _logger.LogInformation(new string('=', 60));
            _logger.LogInformation("✅ Seeder execution completed");
        }
    }
}
