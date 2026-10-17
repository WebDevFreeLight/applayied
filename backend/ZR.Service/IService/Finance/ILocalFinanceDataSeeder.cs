using System.Threading.Tasks;
using ZR.Service.Finance;

namespace ZR.Service.IService.Finance
{
    /// <summary>
    /// Local Finance Data Seeder Interface - واجهة زراعة البيانات المالية المحلية
    /// Seeds default data for all finance modules directly to local database
    /// </summary>
    public interface ILocalFinanceDataSeeder
    {
        /// <summary>
        /// Seed all finance data locally
        /// زراعة جميع البيانات المالية محلياً
        /// Includes: Currencies, Asset Categories, Customers, Vendors
        /// </summary>
        /// <param name="companyId">Company ID to seed data for</param>
        /// <param name="createdBy">Username of the creator</param>
        /// <returns>Seeder result with counts and status</returns>
        Task<LocalSeederResult> SeedAllLocalDataAsync(long? companyId, string createdBy);
    }
}























