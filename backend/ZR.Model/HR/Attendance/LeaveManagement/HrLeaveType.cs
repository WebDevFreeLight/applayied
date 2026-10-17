using ZR.Model.System;

namespace ZR.Model.HR.Attendance.LeaveManagement
{
    /// <summary>
    /// Leave Type - HR Leave Type Configuration
    /// أنواع الإجازات
    /// </summary>
    [SugarTable("hr_leave_type")]
    [SqlDict(
        dictType: "leave_type",
        dictName: "Hr Leave Type - أنواع الإجازات",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "remark",
        OrderBy = "typecode ASC",
        Description = "Leave type configuration for HR module"
    )]
    public class HrLeaveType : SysBase
    {
        /// <summary>
        /// Leave Type ID - Primary Key
        /// معرف نوع الإجازة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "leavetypeid", IsPrimaryKey = true, IsIdentity = true)]
        public long LeaveTypeId { get; set; }

        /// <summary>
        /// Type Code - Unique identifier code for leave type
        /// رمز نوع الإجازة
        /// </summary>
        [SugarColumn(ColumnName = "typecode")]
        [Required(ErrorMessage = "Type code is required")]
        public string TypeCode { get; set; }

        /// <summary>
        /// Type Name (English)
        /// اسم نوع الإجازة بالإنجليزية
        /// </summary>
        [SugarColumn(ColumnName = "typename")]
        [Required(ErrorMessage = "Type name is required")]
        public string TypeName { get; set; }

        /// <summary>
        /// Type Name (Arabic)
        /// اسم نوع الإجازة بالعربية
        /// </summary>
        [SugarColumn(ColumnName = "typenamear")]
        public string TypeNameAr { get; set; }

        /// <summary>
        /// Days Per Year - Default entitlement per year
        /// عدد الأيام في السنة
        /// </summary>
        [SugarColumn(ColumnName = "daysperyear")]
        public decimal DaysPerYear { get; set; }

        /// <summary>
        /// Is Paid - Whether this leave type is paid
        /// DB stores: "0" = paid (true), "1" = unpaid (false)
        /// هل الإجازة مدفوعة الأجر
        /// </summary>
        [SugarColumn(ColumnName = "ispaid")]
        public bool IsPaid { get; set; }

        /// <summary>
        /// Requires Approval - Whether this leave requires manager approval
        /// DB stores: "0" = requires approval (true), "1" = no approval needed (false)
        /// هل تتطلب موافقة
        /// </summary>
        [SugarColumn(ColumnName = "requiresapproval")]
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Requires Document - Whether supporting documents are required
        /// DB stores: "0" = required (true), "1" = not required (false)
        /// هل تتطلب مستندات
        /// </summary>
        [SugarColumn(ColumnName = "requiresdocument")]
        public bool RequiresDocument { get; set; }

        /// <summary>
        /// Minimum Days - Minimum number of days that can be requested
        /// الحد الأدنى للأيام
        /// </summary>
        [SugarColumn(ColumnName = "mindays")]
        public int? MinDays { get; set; }

        /// <summary>
        /// Maximum Days - Maximum number of days that can be requested
        /// الحد الأقصى للأيام
        /// </summary>
        [SugarColumn(ColumnName = "maxdays")]
        public int? MaxDays { get; set; }

        /// <summary>
        /// Advance Notice Days - Number of days advance notice required
        /// عدد أيام الإشعار المسبق
        /// </summary>
        [SugarColumn(ColumnName = "advancenoticedays")]
        public int? AdvanceNoticeDays { get; set; }

        /// <summary>
        /// Is Active - Whether this leave type is currently active
        /// DB stores: "0" = active (true), "1" = inactive (false)
        /// هل النوع نشط
        /// </summary>
        [SugarColumn(ColumnName = "isactive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Leave Code - Alternative code for leave type
        /// رمز بديل للإجازة
        /// </summary>
        [SugarColumn(ColumnName = "leavecode")]
        public string LeaveCode { get; set; }

        /// <summary>
        /// Default Days - Default number of days for this leave type
        /// عدد الأيام الافتراضي
        /// </summary>
        [SugarColumn(ColumnName = "defaultdays")]
        public decimal DefaultDays { get; set; }

        /// <summary>
        /// Allow Carry Forward - Whether unused days can be carried forward to next year
        /// DB stores: "0" = allow (true), "1" = don't allow (false)
        /// هل يسمح بترحيل الأيام غير المستخدمة
        /// </summary>
        [SugarColumn(ColumnName = "allowcarryforward")]
        public bool AllowCarryForward { get; set; }

        /// <summary>
        /// Max Carry Forward - Maximum number of days that can be carried forward
        /// الحد الأقصى للأيام المرحلة
        /// </summary>
        [SugarColumn(ColumnName = "maxcarryforward")]
        public decimal MaxCarryForward { get; set; }
    }
}
