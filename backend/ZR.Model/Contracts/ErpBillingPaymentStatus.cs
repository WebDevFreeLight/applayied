using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Billing Payment Status - حالة دفع الفوترة
    /// Tracks payment status for billing schedules (1:1 with ErpContractBillingSchedule)
    /// </summary>
    [SugarTable("erp_billing_payment_status")]
    [SugarIndex("ix_billing_payment_overdue", nameof(IsOverdue), OrderByType.Asc)]
    [SugarIndex("ix_billing_payment_collection", nameof(CollectionStatus), OrderByType.Asc)]
    public class ErpBillingPaymentStatus : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Schedule ID - معرف الجدول
        /// Primary key and foreign key to ErpContractBillingSchedule (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "schedule_id")]
        public Guid ScheduleId { get; set; }

        #endregion

        #region Payment Tracking - تتبع الدفع

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// Amount paid against this schedule
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "paid_amount")]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// Amount still outstanding
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "remaining_amount")]
        public decimal RemainingAmount { get; set; } = 0;

        /// <summary>
        /// Last Payment Date - تاريخ آخر دفعة
        /// Date of last payment received
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_payment_date")]
        public DateTime? LastPaymentDate { get; set; }

        #endregion

        #region Overdue Tracking - تتبع التأخير

        /// <summary>
        /// Is Overdue - متأخر
        /// Whether this schedule is past due date
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_overdue")]
        public bool IsOverdue { get; set; } = false;

        /// <summary>
        /// Days Overdue - أيام التأخير
        /// Number of days past payment due date
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "days_overdue")]
        public int? DaysOverdue { get; set; } = 0;

        /// <summary>
        /// Overdue Since Date - تاريخ بداية التأخير
        /// Date when the payment became overdue
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "overdue_since_date")]
        public DateTime? OverdueSinceDate { get; set; }

        #endregion

        #region Collection Status - حالة التحصيل

        /// <summary>
        /// Collection Status - حالة التحصيل
        /// Current collection status (Current, Overdue, InCollection, Disputed, WrittenOff, Settled)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "collection_status")]
        public string CollectionStatus { get; set; } = "Current";

        /// <summary>
        /// Collection Notes - ملاحظات التحصيل
        /// Notes about collection efforts
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "collection_notes")]
        public string CollectionNotes { get; set; }

        #endregion

        #region Write-off - الشطب

        /// <summary>
        /// Written Off Amount - المبلغ المشطوب
        /// Amount written off as bad debt
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "written_off_amount")]
        public decimal? WrittenOffAmount { get; set; } = 0;

        /// <summary>
        /// Written Off Date - تاريخ الشطب
        /// Date when amount was written off
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "written_off_date")]
        public DateTime? WrittenOffDate { get; set; }

        /// <summary>
        /// Written Off By - تم الشطب بواسطة
        /// User who wrote off the amount
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "written_off_by")]
        public string WrittenOffBy { get; set; }

        /// <summary>
        /// Write Off Reason - سبب الشطب
        /// Reason for writing off the amount
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "write_off_reason")]
        public string WriteOffReason { get; set; }

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Billing Schedule - جدول الفوترة
        /// Parent billing schedule (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ScheduleId))]
        public ErpContractBillingSchedule BillingSchedule { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// Payment Percentage - نسبة الدفع
        /// Percentage of total amount paid
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal PaymentPercentage
        {
            get
            {
                if (PaidAmount > 0 && (PaidAmount + RemainingAmount) > 0)
                {
                    return Math.Round((PaidAmount / (PaidAmount + RemainingAmount)) * 100, 2);
                }
                return 0;
            }
        }

        /// <summary>
        /// Is Fully Paid - مدفوع بالكامل
        /// Whether the schedule is fully paid
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsFullyPaid => RemainingAmount <= 0;

        #endregion
    }
}
