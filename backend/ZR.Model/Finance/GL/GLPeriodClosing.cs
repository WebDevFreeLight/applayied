using System;
using SqlSugar;
using ZR.Model.Finance.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.GL
{
    /// <summary>
    /// GL Period Closing - إقفال الفترة المالية
    /// </summary>
    [SugarTable("gl_period_closing")]
    public class GLPeriodClosing : SysBase
    {
        /// <summary>
        /// Closing ID - معرف الإقفال
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Period Type - نوع الفترة (شهر / ربع / سنة)
        /// </summary>
        [SugarColumn(ColumnName = "period_type")]
        public PeriodType PeriodType { get; set; }

        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        [SugarColumn(ColumnName = "period_start_date")]
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        [SugarColumn(ColumnName = "period_end_date")]
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Fiscal Period - رقم الفترة المالية (1-12 للشهور)
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_period")]
        public int FiscalPeriod { get; set; }

        /// <summary>
        /// Status - حالة الفترة
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public PeriodStatus Status { get; set; } = PeriodStatus.Open;

        /// <summary>
        /// Description (Arabic) - الوصف بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_ar")]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_en")]
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Total Vouchers - عدد السندات في الفترة
        /// </summary>
        [SugarColumn(ColumnName = "total_vouchers")]
        public int TotalVouchers { get; set; }

        /// <summary>
        /// Posted Vouchers - عدد السندات المرحّلة
        /// </summary>
        [SugarColumn(ColumnName = "posted_vouchers")]
        public int PostedVouchers { get; set; }

        /// <summary>
        /// Total Debit - إجمالي المدين
        /// </summary>
        [SugarColumn(ColumnName = "total_debit", DecimalDigits = 4)]
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Total Credit - إجمالي الدائن
        /// </summary>
        [SugarColumn(ColumnName = "total_credit", DecimalDigits = 4)]
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Closing Notes - ملاحظات الإقفال
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "closing_notes")]
        public string ClosingNotes { get; set; }

        /// <summary>
        /// Closed At - تاريخ الإقفال
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "closed_at")]
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// Closed By - أقفل بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "closed_by")]
        public Guid? ClosedBy { get; set; }

        /// <summary>
        /// Reopened At - تاريخ إعادة الفتح
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reopened_at")]
        public DateTime? ReopenedAt { get; set; }

        /// <summary>
        /// Reopened By - أعيد فتحه بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reopened_by")]
        public Guid? ReopenedBy { get; set; }
    }
}

