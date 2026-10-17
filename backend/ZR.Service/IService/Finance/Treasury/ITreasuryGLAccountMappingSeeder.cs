using System.Threading.Tasks;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Treasury GL Account Mapping Seeder Interface - واجهة زراعة تعيينات حسابات الخزينة
    /// Provides functionality to seed default treasury GL account mappings
    /// </summary>
    public interface ITreasuryGLAccountMappingSeeder
    {
        /// <summary>
        /// Seed default treasury GL account mappings for a company
        /// زراعة تعيينات حسابات الخزينة الافتراضية
        /// </summary>
        /// <param name="companyId">Company ID to seed mappings for</param>
        /// <param name="createdBy">Username of the creator</param>
        /// <returns>Number of mappings created</returns>
        Task<int> SeedDefaultMappingsAsync(long? companyId, string createdBy);

        /// <summary>
        /// Check if default mappings already exist for a company
        /// التحقق من وجود التعيينات الافتراضية
        /// </summary>
        /// <param name="companyId">Company ID to check</param>
        /// <returns>True if mappings already exist</returns>
        Task<bool> DefaultMappingsExistAsync(long? companyId);

        /// <summary>
        /// Update existing mappings with GL account information
        /// تحديث التعيينات الموجودة بمعلومات حساب دفتر الأستاذ
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="updatedBy">Username of the updater</param>
        /// <returns>Number of mappings updated</returns>
        Task<int> UpdateMappingsWithGLInfoAsync(long? companyId, string updatedBy);
    }
}
