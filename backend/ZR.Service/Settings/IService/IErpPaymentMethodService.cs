using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Settings;
using ZR.Service;

namespace ZR.Service.Settings.IService
{
    /// <summary>
    /// ERP Payment Method Service Interface - واجهة خدمة طرق الدفع
    /// </summary>
    public interface IErpPaymentMethodService : IBaseService<ErpPaymentMethod>
    {
        /// <summary>
        /// Get paginated list of payment methods - الحصول على قائمة طرق الدفع بالترقيم
        /// </summary>
        PagedInfo<ErpPaymentMethod> GetList(ErpPaymentMethod parm, PagerInfo pager);

        /// <summary>
        /// Get payment method by ID - الحصول على طريقة الدفع بالمعرف
        /// </summary>
        ErpPaymentMethod GetInfo(long paymentMethodId);

        /// <summary>
        /// Add payment method - إضافة طريقة دفع
        /// </summary>
        ErpPaymentMethod AddPaymentMethod(ErpPaymentMethod model);

        /// <summary>
        /// Update payment method - تحديث طريقة دفع
        /// </summary>
        int UpdatePaymentMethod(ErpPaymentMethod model);

        /// <summary>
        /// Delete payment methods - حذف طرق الدفع
        /// </summary>
        int Delete(long[] ids);

        /// <summary>
        /// Get active payment methods - الحصول على طرق الدفع النشطة
        /// </summary>
        List<ErpPaymentMethod> GetActivePaymentMethods();
    }
}


