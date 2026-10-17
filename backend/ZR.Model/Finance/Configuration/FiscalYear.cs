using System;
using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Configuration
{
    /// <summary>
    /// Fiscal Year - السنة المالية
    /// </summary>
    [SugarTable("finance_fiscal_year")]
    public class FiscalYear : SysBase
    {
        /// <summary>
        /// Fiscal Year ID - معرف السنة المالية
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        // Note: CompanyID and BranchID are inherited from SysBase - do not duplicate

        /// <summary>
        /// Year Start Date - تاريخ بداية السنة
        /// </summary>
        public DateTime YearStartDate { get; set; }

        /// <summary>
        /// Year End Date - تاريخ نهاية السنة
        /// </summary>
        public DateTime YearEndDate { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        public FiscalYearStatus Status { get; set; } = FiscalYearStatus.Open;

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Description (Arabic) - الوصف بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string DescriptionEn { get; set; }
    }
}


