using System.Threading.Tasks;
using ZR.Service.Finance;

namespace ZR.Service.IService.Finance
{
    /// <summary>
    /// Unified Finance Data Seeder Interface - واجهة زراعة البيانات المالية الموحدة
    /// Seeds all finance module data from external API
    /// </summary>
    public interface IFinanceDataSeeder
    {
        /// <summary>
        /// Seed all finance data from external API
        /// زراعة جميع البيانات المالية من النظام الخارجي
        /// Includes: GL Accounts, ARAP, Assets, Configuration, Treasury
        /// </summary>
        /// <param name="apiBaseUrl">Base URL of the external API</param>
        /// <param name="bearerToken">Bearer token for authentication</param>
        /// <param name="companyId">Company ID to seed data for</param>
        /// <param name="createdBy">Username of the creator</param>
        /// <returns>Seeder result with counts and status</returns>
        Task<FinanceSeederResult> SeedAllFinanceDataAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy);
    }
}

