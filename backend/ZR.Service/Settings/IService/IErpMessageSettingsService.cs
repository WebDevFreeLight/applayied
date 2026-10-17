using System.Threading.Tasks;
using ZR.Model.Settings;
using ZR.Service;

namespace ZR.Service.Settings.IService
{
    /// <summary>
    /// ERP Message Settings Service Interface - واجهة خدمة إعدادات الرسائل
    /// </summary>
    public interface IErpMessageSettingsService : IBaseService<ErpMessageSettings>
    {
        /// <summary>
        /// Get message settings - الحصول على إعدادات الرسائل
        /// </summary>
        ErpMessageSettings GetSettings(string eventType = null);

        /// <summary>
        /// Save message settings - حفظ إعدادات الرسائل
        /// </summary>
        int SaveSettings(ErpMessageSettings model);

        /// <summary>
        /// Test SMS - اختبار الرسائل النصية
        /// </summary>
        Task<bool> TestSmsAsync(string phone);

        /// <summary>
        /// Test Email - اختبار البريد الإلكتروني
        /// </summary>
        Task<bool> TestEmailAsync(string email);
    }
}


