using System;
using System.Collections.Generic;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;

namespace ZR.Service.Contracts.IService
{
    /// <summary>
    /// Contract Pricing Rule Service Interface - واجهة خدمة قواعد تسعير العقد
    /// </summary>
    public interface IContractPricingRuleService
    {
        /// <summary>
        /// Get pricing rules by contract ID
        /// الحصول على قواعد التسعير بمعرف العقد
        /// </summary>
        List<ErpContractPricingRuleListDto> GetByContractId(Guid contractId, bool? isActive = null);

        /// <summary>
        /// Get pricing rule by ID
        /// الحصول على قاعدة التسعير بالمعرف
        /// </summary>
        ErpContractPricingRule GetById(Guid ruleId);

        /// <summary>
        /// Create pricing rule
        /// إنشاء قاعدة تسعير
        /// </summary>
        Guid Create(CreateErpContractPricingRuleDto dto, string createdBy);

        /// <summary>
        /// Update pricing rule
        /// تحديث قاعدة التسعير
        /// </summary>
        int Update(UpdateErpContractPricingRuleDto dto, string updatedBy);

        /// <summary>
        /// Delete pricing rule
        /// حذف قاعدة التسعير
        /// </summary>
        int Delete(Guid ruleId);

        /// <summary>
        /// Toggle rule active status
        /// تبديل حالة نشاط القاعدة
        /// </summary>
        int ToggleActive(Guid ruleId);

        /// <summary>
        /// Calculate amount with pricing rules
        /// حساب المبلغ مع قواعد التسعير
        /// </summary>
        decimal CalculateAmount(Guid contractId, decimal baseAmount, DateTime billingDate);

        /// <summary>
        /// Get active rules for date
        /// الحصول على القواعد النشطة للتاريخ
        /// </summary>
        List<ErpContractPricingRule> GetActiveRulesForDate(Guid contractId, DateTime date);
    }
}
