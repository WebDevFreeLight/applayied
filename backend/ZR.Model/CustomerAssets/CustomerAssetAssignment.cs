using System;
using SqlSugar;
using ZR.Model.CustomerAssets.Enums;
using ZR.Model.System;

namespace ZR.Model.CustomerAssets
{
    /// <summary>
    /// Customer Asset Assignment - تعيين أصل للعميل
    /// </summary>
    [SugarTable("customer_asset_assignment")]
    public class CustomerAssetAssignment : SysBase
    {
        /// <summary>
        /// Assignment ID - معرف التعيين
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Asset ID - معرف الأصل
        /// </summary>
        public Guid AssetId { get; set; }

        /// <summary>
        /// Customer ID (FK → arap_customer) - معرف العميل
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Contract ID (FK → erp_contract, nullable) - معرف العقد
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Assigned Date - تاريخ التعيين
        /// </summary>
        public DateTime AssignedDate { get; set; }

        /// <summary>
        /// Expected Return Date - تاريخ الإرجاع المتوقع
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ExpectedReturnDate { get; set; }

        /// <summary>
        /// Actual Return Date - تاريخ الإرجاع الفعلي
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ActualReturnDate { get; set; }

        /// <summary>
        /// Assignment Status - حالة التعيين
        /// </summary>
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Active;

        /// <summary>
        /// Assigned By (username) - المعيّن بواسطة
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = true)]
        public string AssignedBy { get; set; }

        /// <summary>
        /// Returned By (username) - المستلم بواسطة
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = true)]
        public string ReturnedBy { get; set; }

        /// <summary>
        /// Return Condition - حالة الإرجاع
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public AssetCondition? ReturnCondition { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string Notes { get; set; }

        #region Navigation Properties

        [SugarColumn(IsIgnore = true)]
        public CustomerAsset Asset { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string CustomerNameEn { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string CustomerNameAr { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string CustomerCode { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ContractTitle { get; set; }

        #endregion
    }
}
