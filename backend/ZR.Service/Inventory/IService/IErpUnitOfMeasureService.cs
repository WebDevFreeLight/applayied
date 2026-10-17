using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Settings;
using ZR.Service;

namespace ZR.Service.Inventory.IService
{
    /// <summary>
    /// ERP Unit of Measure Service Interface - واجهة خدمة وحدات القياس
    /// </summary>
    public interface IErpUnitOfMeasureService : IBaseService<ErpUnitOfMeasure>
    {
        /// <summary>
        /// Get paginated list of units of measure - الحصول على قائمة وحدات القياس بالترقيم
        /// </summary>
        PagedInfo<ErpUnitOfMeasure> GetList(ErpUnitOfMeasure parm, PagerInfo pager);

        /// <summary>
        /// Get unit of measure by ID - الحصول على وحدة القياس بالمعرف
        /// </summary>
        ErpUnitOfMeasure GetInfo(long unitId);

        /// <summary>
        /// Add unit of measure - إضافة وحدة قياس
        /// </summary>
        ErpUnitOfMeasure AddUnit(ErpUnitOfMeasure model);

        /// <summary>
        /// Update unit of measure - تحديث وحدة قياس
        /// </summary>
        int UpdateUnit(ErpUnitOfMeasure model);

        /// <summary>
        /// Delete units of measure - حذف وحدات القياس
        /// </summary>
        int Delete(long[] ids);

        /// <summary>
        /// Get active units of measure - الحصول على وحدات القياس النشطة
        /// </summary>
        List<ErpUnitOfMeasure> GetActiveUnits();
    }
}


