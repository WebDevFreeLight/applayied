using System;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Lifecycle Events - أحداث دورة حياة العقد
    /// Tracks lifecycle state changes for a contract (1:1 relationship with ErpContract)
    /// </summary>
    [SugarTable("erp_contract_lifecycle")]
    public class ErpContractLifecycle : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Contract ID - معرف العقد
        /// Primary key and foreign key to ErpContract (1:1 relationship)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        #endregion

        #region Activation - التفعيل

        /// <summary>
        /// Activated Date - تاريخ التفعيل
        /// Date when contract was activated
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "activated_date")]
        public DateTime? ActivatedDate { get; set; }

        /// <summary>
        /// Activated By - تم التفعيل بواسطة
        /// User who activated the contract
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "activated_by")]
        public string ActivatedBy { get; set; }

        #endregion

        #region Termination - الإنهاء

        /// <summary>
        /// Terminated Date - تاريخ الإنهاء
        /// Date when contract was terminated
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "terminated_date")]
        public DateTime? TerminatedDate { get; set; }

        /// <summary>
        /// Terminated By - تم الإنهاء بواسطة
        /// User who terminated the contract
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "terminated_by")]
        public string TerminatedBy { get; set; }

        /// <summary>
        /// Termination Reason - سبب الإنهاء
        /// Reason for contract termination
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "termination_reason")]
        public TerminationReason? TerminationReason { get; set; }

        #endregion

        #region Signing - التوقيع

        /// <summary>
        /// Signed Date - تاريخ التوقيع
        /// Date when contract was signed
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "signed_date")]
        public DateTime? SignedDate { get; set; }

        /// <summary>
        /// Signatory Name - اسم الموقع
        /// Name of person who signed the contract
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "signatory_name")]
        public string SignatoryName { get; set; }

        /// <summary>
        /// Signatory Title - منصب الموقع
        /// Title/position of the signatory
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "signatory_title")]
        public string SignatoryTitle { get; set; }

        #endregion

        #region Suspension - التعليق

        /// <summary>
        /// Suspended Date - تاريخ التعليق
        /// Date when contract was suspended
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "suspended_date")]
        public DateTime? SuspendedDate { get; set; }

        /// <summary>
        /// Suspended By - تم التعليق بواسطة
        /// User who suspended the contract
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "suspended_by")]
        public string SuspendedBy { get; set; }

        /// <summary>
        /// Suspension Reason - سبب التعليق
        /// Reason for contract suspension
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "suspension_reason")]
        public string SuspensionReason { get; set; }

        /// <summary>
        /// Resumed Date - تاريخ الاستئناف
        /// Date when contract was resumed after suspension
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "resumed_date")]
        public DateTime? ResumedDate { get; set; }

        /// <summary>
        /// Resumed By - تم الاستئناف بواسطة
        /// User who resumed the contract
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "resumed_by")]
        public string ResumedBy { get; set; }

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Contract - العقد
        /// Parent contract (navigation property)
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(ContractId))]
        public ErpContract Contract { get; set; }

        #endregion
    }
}
