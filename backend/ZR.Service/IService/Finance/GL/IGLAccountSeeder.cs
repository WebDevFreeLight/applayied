using System.Threading.Tasks;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// GL Account Seeder Interface - واجهة زراعة الحسابات
    /// Provides functionality to seed default IFRS chart of accounts
    /// </summary>
    public interface IGLAccountSeeder
    {
        /// <summary>
        /// Seed default IFRS chart of accounts for a company
        /// </summary>
        /// <param name="companyId">Company ID to seed accounts for</param>
        /// <param name="createdBy">Username of the creator</param>
        /// <returns>Number of accounts created</returns>
        Task<int> SeedDefaultAccountsAsync(long? companyId, string createdBy);

        /// <summary>
        /// Check if default accounts already exist for a company
        /// </summary>
        /// <param name="companyId">Company ID to check</param>
        /// <returns>True if accounts already exist</returns>
        Task<bool> DefaultAccountsExistAsync(long? companyId);

        /// <summary>
        /// Recalculate all counts and paths after seeding
        /// </summary>
        Task RecalculateHierarchyAsync();

        /// <summary>
        /// Seed accounts from external API (onemay.sa system)
        /// زراعة الحسابات من النظام الخارجي
        /// </summary>
        /// <param name="apiBaseUrl">Base URL of the external API (e.g., https://onemay.sa:8887)</param>
        /// <param name="bearerToken">Bearer token for authentication</param>
        /// <param name="companyId">Company ID to seed accounts for</param>
        /// <param name="createdBy">Username of the creator</param>
        /// <returns>Number of accounts created</returns>
        Task<int> SeedAccountsFromExternalApiAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy);

        /// <summary>
        /// Link existing treasury accounts (bank and cash) to their corresponding GL accounts
        /// ربط حسابات الخزينة الموجودة بحسابات دفتر الأستاذ المقابلة
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="updatedBy">Username of the updater</param>
        /// <returns>Number of treasury accounts linked</returns>
        Task<int> LinkTreasuryAccountsToGLAsync(long? companyId, string updatedBy);
    }
}
