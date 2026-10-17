namespace ZR.Model.HR.Dto.Attendance
{
    /// <summary>
    /// Leave Type Query DTO
    /// </summary>
    public class HrLeaveTypeQueryDto : PagerInfo
    {
        /// <summary>
        /// Type Code filter
        /// </summary>
        public string TypeCode { get; set; }

        /// <summary>
        /// Type Name filter (English)
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Type Name filter (Arabic)
        /// </summary>
        public string TypeNameAr { get; set; }

        /// <summary>
        /// Days Per Year filter
        /// </summary>
        public decimal? DaysPerYear { get; set; }

        /// <summary>
        /// Is Paid filter
        /// </summary>
        public bool? IsPaid { get; set; }

        /// <summary>
        /// Requires Approval filter
        /// </summary>
        public bool? RequiresApproval { get; set; }

        /// <summary>
        /// Requires Document filter
        /// </summary>
        public bool? RequiresDocument { get; set; }

        /// <summary>
        /// Min Days filter
        /// </summary>
        public int? MinDays { get; set; }

        /// <summary>
        /// Max Days filter
        /// </summary>
        public int? MaxDays { get; set; }

        /// <summary>
        /// Advance Notice Days filter
        /// </summary>
        public int? AdvanceNoticeDays { get; set; }

        /// <summary>
        /// Is Active filter
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Leave Code filter
        /// </summary>
        public string LeaveCode { get; set; }

        /// <summary>
        /// Default Days filter
        /// </summary>
        public decimal? DefaultDays { get; set; }

        /// <summary>
        /// Allow Carry Forward filter
        /// </summary>
        public bool? AllowCarryForward { get; set; }

        /// <summary>
        /// Max Carry Forward filter
        /// </summary>
        public decimal? MaxCarryForward { get; set; }

        /// <summary>
        /// Begin Create Time for range filter
        /// </summary>
        public DateTime? BeginCreateTime { get; set; }

        /// <summary>
        /// End Create Time for range filter
        /// </summary>
        public DateTime? EndCreateTime { get; set; }

        /// <summary>
        /// Begin Update Time for range filter
        /// </summary>
        public DateTime? BeginUpdateTime { get; set; }

        /// <summary>
        /// End Update Time for range filter
        /// </summary>
        public DateTime? EndUpdateTime { get; set; }

        /// <summary>
        /// Remark filter
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// Leave Type DTO - for read/display operations
    /// </summary>
    public class HrLeaveTypeDto
    {
        [ExcelColumn(Name = "Leave Type ID")]
        [ExcelColumnName("Leave Type ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Type code is required")]
        [ExcelColumn(Name = "Type Code")]
        [ExcelColumnName("Type Code")]
        public string TypeCode { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        [ExcelColumn(Name = "Type Name (EN)")]
        [ExcelColumnName("Type Name (EN)")]
        public string TypeName { get; set; }

        [ExcelColumn(Name = "Type Name (AR)")]
        [ExcelColumnName("Type Name (AR)")]
        public string TypeNameAr { get; set; }

        [ExcelColumn(Name = "Days Per Year")]
        [ExcelColumnName("Days Per Year")]
        public decimal DaysPerYear { get; set; }

        [ExcelColumn(Name = "Is Paid")]
        [ExcelColumnName("Is Paid")]
        public bool IsPaid { get; set; }

        [ExcelColumn(Name = "Requires Approval")]
        [ExcelColumnName("Requires Approval")]
        public bool RequiresApproval { get; set; }

        [ExcelColumn(Name = "Requires Document")]
        [ExcelColumnName("Requires Document")]
        public bool RequiresDocument { get; set; }

        [ExcelColumn(Name = "Min Days")]
        [ExcelColumnName("Min Days")]
        public int? MinDays { get; set; }

        [ExcelColumn(Name = "Max Days")]
        [ExcelColumnName("Max Days")]
        public int? MaxDays { get; set; }

        [ExcelColumn(Name = "Advance Notice Days")]
        [ExcelColumnName("Advance Notice Days")]
        public int? AdvanceNoticeDays { get; set; }

        [ExcelColumn(Name = "Is Active")]
        [ExcelColumnName("Is Active")]
        public bool IsActive { get; set; }

        [ExcelColumn(Name = "Leave Code")]
        [ExcelColumnName("Leave Code")]
        public string LeaveCode { get; set; }

        [ExcelColumn(Name = "Default Days")]
        [ExcelColumnName("Default Days")]
        public decimal DefaultDays { get; set; }

        [ExcelColumn(Name = "Allow Carry Forward")]
        [ExcelColumnName("Allow Carry Forward")]
        public bool AllowCarryForward { get; set; }

        [ExcelColumn(Name = "Max Carry Forward")]
        [ExcelColumnName("Max Carry Forward")]
        public decimal MaxCarryForward { get; set; }

        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }

        [ExcelColumn(Name = "Create Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create Time")]
        public DateTime? CreateTime { get; set; }

        [ExcelColumn(Name = "Updater")]
        [ExcelColumnName("Updater")]
        public string UpdateBy { get; set; }

        [ExcelColumn(Name = "Update Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Update Time")]
        public DateTime? UpdateTime { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        [ExcelColumn(Name = "Company ID")]
        [ExcelColumnName("Company ID")]
        public int? Companyid { get; set; }

        [ExcelColumn(Name = "Branch ID")]
        [ExcelColumnName("Branch ID")]
        public int? BranchID { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsPaidLabel => IsPaid ? "Paid" : "Unpaid";

        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";

        [ExcelIgnore]
        public string AllowCarryForwardLabel => AllowCarryForward ? "Yes" : "No";

        [ExcelIgnore]
        public string RequiresApprovalLabel => RequiresApproval ? "Yes" : "No";

        [ExcelIgnore]
        public string RequiresDocumentLabel => RequiresDocument ? "Yes" : "No";
    }

    /// <summary>
    /// Create Leave Type DTO
    /// </summary>
    public class CreateHrLeaveTypeDto
    {
        [Required(ErrorMessage = "Type code is required")]
        public string TypeCode { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        public string TypeName { get; set; }

        public string TypeNameAr { get; set; }

        public decimal DaysPerYear { get; set; }

        public bool IsPaid { get; set; }

        public bool RequiresApproval { get; set; }

        public bool RequiresDocument { get; set; }

        public int? MinDays { get; set; }

        public int? MaxDays { get; set; }

        public int? AdvanceNoticeDays { get; set; }

        public bool IsActive { get; set; } = true;

        public string LeaveCode { get; set; }

        public decimal DefaultDays { get; set; }

        public bool AllowCarryForward { get; set; }

        public decimal MaxCarryForward { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Update Leave Type DTO
    /// </summary>
    public class UpdateHrLeaveTypeDto
    {
        [Required(ErrorMessage = "Leave type ID is required")]
        public long LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Type code is required")]
        public string TypeCode { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        public string TypeName { get; set; }

        public string TypeNameAr { get; set; }

        public decimal DaysPerYear { get; set; }

        public bool IsPaid { get; set; }

        public bool RequiresApproval { get; set; }

        public bool RequiresDocument { get; set; }

        public int? MinDays { get; set; }

        public int? MaxDays { get; set; }

        public int? AdvanceNoticeDays { get; set; }

        public bool IsActive { get; set; }

        public string LeaveCode { get; set; }

        public decimal DefaultDays { get; set; }

        public bool AllowCarryForward { get; set; }

        public decimal MaxCarryForward { get; set; }

        public string Remark { get; set; }
    }
}
