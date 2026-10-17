using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Termination Inspection - فحص الوحدات للإنهاء
    /// Unit inspection and handover details for contract termination (1:1 with ErpContractTermination)
    /// </summary>
    [SugarTable("erp_contract_termination_inspection")]
    [SugarIndex("ix_termination_inspection_status", nameof(InspectionStatus), OrderByType.Asc)]
    [SugarIndex("ix_termination_inspection_date", nameof(HandoverInspectionDate), OrderByType.Asc)]
    public class ErpContractTerminationInspection : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Termination ID - معرف الإنهاء
        /// Primary key and foreign key to ErpContractTermination (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "termination_id")]
        public Guid TerminationId { get; set; }

        #endregion

        #region Unit Counts - أعداد الوحدات

        /// <summary>
        /// Units to Return - الوحدات للإرجاع
        /// Number of units that need to be returned
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "units_to_return")]
        public int? UnitsToReturn { get; set; } = 0;

        /// <summary>
        /// Units Returned - الوحدات المرجعة
        /// Number of units actually returned
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "units_returned")]
        public int? UnitsReturned { get; set; } = 0;

        /// <summary>
        /// Units Pending - الوحدات المعلقة
        /// Number of units pending return
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "units_pending")]
        public int? UnitsPending { get; set; } = 0;

        #endregion

        #region Inspection Required - الفحص المطلوب

        /// <summary>
        /// Handover Inspection Required - فحص التسليم مطلوب
        /// Whether inspection is required before accepting handover
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "handover_inspection_required")]
        public bool HandoverInspectionRequired { get; set; } = true;

        #endregion

        #region Pre-Inspection - الفحص المسبق

        /// <summary>
        /// Pre-Inspection Date - تاريخ الفحص المسبق
        /// Date of preliminary inspection
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "pre_inspection_date")]
        public DateTime? PreInspectionDate { get; set; }

        /// <summary>
        /// Pre-Inspection By - الفحص المسبق بواسطة
        /// Person who conducted pre-inspection
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "pre_inspection_by")]
        public string PreInspectionBy { get; set; }

        /// <summary>
        /// Pre-Inspection Notes - ملاحظات الفحص المسبق
        /// Notes from pre-inspection
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "pre_inspection_notes")]
        public string PreInspectionNotes { get; set; }

        #endregion

        #region Final Inspection - الفحص النهائي

        /// <summary>
        /// Handover Inspection Date - تاريخ فحص التسليم
        /// Date of final handover inspection
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "handover_inspection_date")]
        public DateTime? HandoverInspectionDate { get; set; }

        /// <summary>
        /// Handover Inspection By - فحص التسليم بواسطة
        /// Person who conducted final inspection
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "handover_inspection_by")]
        public string HandoverInspectionBy { get; set; }

        /// <summary>
        /// Handover Inspection Notes - ملاحظات فحص التسليم
        /// Notes from final inspection
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnName = "handover_inspection_notes")]
        public string HandoverInspectionNotes { get; set; }

        /// <summary>
        /// Inspection Status - حالة الفحص
        /// Current inspection status (Pending, Scheduled, InProgress, Completed, Failed, Waived)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "inspection_status")]
        public string InspectionStatus { get; set; } = "Pending";

        #endregion

        #region Damages - الأضرار

        /// <summary>
        /// Damages Found - أضرار موجودة
        /// Whether any damages were found during inspection
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "damages_found")]
        public bool DamagesFound { get; set; } = false;

        /// <summary>
        /// Damage Assessment Amount - مبلغ تقييم الأضرار
        /// Estimated cost of damages
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "damage_assessment_amount")]
        public decimal? DamageAssessmentAmount { get; set; } = 0;

        /// <summary>
        /// Damage Details - تفاصيل الأضرار
        /// JSON or text describing damages found
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "damage_details")]
        public string DamageDetails { get; set; }

        /// <summary>
        /// Damage Photos Attached - صور الأضرار مرفقة
        /// Whether photos of damages are attached
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "damage_photos_attached")]
        public bool DamagePhotosAttached { get; set; } = false;

        #endregion

        #region Repairs Required - الإصلاحات المطلوبة

        /// <summary>
        /// Repairs Required - الإصلاحات مطلوبة
        /// Whether repairs are needed
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "repairs_required")]
        public bool RepairsRequired { get; set; } = false;

        /// <summary>
        /// Repair Estimate Amount - مبلغ تقدير الإصلاح
        /// Estimated cost of repairs
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "repair_estimate_amount")]
        public decimal? RepairEstimateAmount { get; set; }

        /// <summary>
        /// Repair Completed - الإصلاح مكتمل
        /// Whether repairs have been completed
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "repair_completed")]
        public bool RepairCompleted { get; set; } = false;

        /// <summary>
        /// Repair Completion Date - تاريخ إكمال الإصلاح
        /// Date when repairs were completed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "repair_completion_date")]
        public DateTime? RepairCompletionDate { get; set; }

        /// <summary>
        /// Actual Repair Cost - تكلفة الإصلاح الفعلية
        /// Actual cost of repairs
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "actual_repair_cost")]
        public decimal? ActualRepairCost { get; set; }

        #endregion

        #region Handover Completion - إكمال التسليم

        /// <summary>
        /// Handover Completed - التسليم مكتمل
        /// Whether handover process is complete
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "handover_completed")]
        public bool HandoverCompleted { get; set; } = false;

        /// <summary>
        /// Handover Completion Date - تاريخ إكمال التسليم
        /// Date when handover was completed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "handover_completion_date")]
        public DateTime? HandoverCompletionDate { get; set; }

        /// <summary>
        /// Handover Signed By Tenant - التسليم موقع من المستأجر
        /// Name of tenant representative who signed handover
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "handover_signed_by_tenant")]
        public string HandoverSignedByTenant { get; set; }

        /// <summary>
        /// Handover Signed By Company - التسليم موقع من الشركة
        /// Name of company representative who signed handover
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "handover_signed_by_company")]
        public string HandoverSignedByCompany { get; set; }

        #endregion

        #region Currency - العملة

        /// <summary>
        /// Currency - العملة
        /// Currency for all amounts
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Termination - الإنهاء
        /// Parent termination record (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(TerminationId))]
        public ErpContractTermination Termination { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// All Units Returned - كل الوحدات مرجعة
        /// Whether all units have been returned
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool AllUnitsReturned => (UnitsToReturn ?? 0) > 0 && (UnitsReturned ?? 0) >= (UnitsToReturn ?? 0);

        /// <summary>
        /// Inspection Complete - الفحص مكتمل
        /// Whether inspection process is complete
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool InspectionComplete => InspectionStatus == "Completed" || InspectionStatus == "Waived";

        /// <summary>
        /// Total Damage Cost - إجمالي تكلفة الأضرار
        /// Actual or estimated damage cost
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal TotalDamageCost => ActualRepairCost ?? DamageAssessmentAmount ?? 0;

        #endregion
    }
}
