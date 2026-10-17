using System;
using System.ComponentModel.DataAnnotations;
using ZR.Model.CustomerAssets.Enums;

namespace ZR.Model.CustomerAssets.Dto
{
    /// <summary>
    /// Assignment Query DTO - استعلام التعيين
    /// </summary>
    public class AssignmentQueryDto : PagerInfo
    {
        public Guid? AssetId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ContractId { get; set; }
        public AssignmentStatus? Status { get; set; }
        public string CustomerName { get; set; }
        public string AssetCode { get; set; }
    }

    /// <summary>
    /// Assign Asset DTO - تعيين أصل
    /// </summary>
    public class AssignAssetDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid AssetId { get; set; }

        [Required(ErrorMessage = "معرف العميل مطلوب")]
        public Guid CustomerId { get; set; }

        public Guid? ContractId { get; set; }

        [Required(ErrorMessage = "تاريخ التعيين مطلوب")]
        public DateTime AssignedDate { get; set; }

        public DateTime? ExpectedReturnDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Return Asset DTO - إرجاع أصل
    /// </summary>
    public class ReturnAssetDto
    {
        [Required(ErrorMessage = "معرف التعيين مطلوب")]
        public Guid AssignmentId { get; set; }

        [Required(ErrorMessage = "تاريخ الإرجاع مطلوب")]
        public DateTime ActualReturnDate { get; set; }

        [Required(ErrorMessage = "حالة الإرجاع مطلوبة")]
        public AssetCondition ReturnCondition { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Mark Lost/Damaged DTO - تسجيل فقدان/تلف
    /// </summary>
    public class MarkLostDamagedDto
    {
        [Required(ErrorMessage = "معرف التعيين مطلوب")]
        public Guid AssignmentId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Assignment List DTO - قائمة التعيينات
    /// </summary>
    public class AssignmentListDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetNameEn { get; set; }
        public string AssetNameAr { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerNameEn { get; set; }
        public string CustomerNameAr { get; set; }
        public Guid? ContractId { get; set; }
        public string ContractTitle { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public AssignmentStatus Status { get; set; }
        public string AssignedBy { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Assignment Detail DTO - تفاصيل التعيين
    /// </summary>
    public class AssignmentDetailDto : AssignmentListDto
    {
        public string ReturnedBy { get; set; }
        public AssetCondition? ReturnCondition { get; set; }
        public string Notes { get; set; }
        public string AssetTypeName { get; set; }
        public string SerialNumber { get; set; }
    }
}
