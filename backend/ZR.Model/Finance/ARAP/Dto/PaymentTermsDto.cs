using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace ZR.Model.Finance.ARAP.Dto
{
    /// <summary>
    /// Payment Terms Query DTO - استعلام شروط الدفع
    /// </summary>
    public class PaymentTermsQueryDto : PagerInfo
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public bool? IsActive { get; set; }
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Payment Terms DTO - إنشاء شروط دفع جديدة
    /// </summary>
    public class CreatePaymentTermsDto
    {
        [Required(ErrorMessage = "رمز شروط الدفع مطلوب")]
        [StringLength(50, ErrorMessage = "رمز شروط الدفع يجب ألا يتجاوز 50 حرف")]
        public string Code { get; set; }

        [Required(ErrorMessage = "اسم شروط الدفع بالإنجليزي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم شروط الدفع بالإنجليزي يجب ألا يتجاوز 200 حرف")]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "عدد أيام الاستحقاق مطلوب")]
        [Range(0, 365, ErrorMessage = "عدد أيام الاستحقاق يجب أن يكون بين 0 و 365")]
        public int NetDays { get; set; } = 30;

        [Range(0, 365)]
        public int? DiscountDays { get; set; }

        [Range(0, 100)]
        public decimal? DiscountPercent { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
    }

    /// <summary>
    /// Update Payment Terms DTO - تحديث شروط الدفع
    /// </summary>
    public class UpdatePaymentTermsDto
    {
        [Required(ErrorMessage = "معرف شروط الدفع مطلوب")]
        public int Id { get; set; }

        [Required(ErrorMessage = "رمز شروط الدفع مطلوب")]
        [StringLength(50, ErrorMessage = "رمز شروط الدفع يجب ألا يتجاوز 50 حرف")]
        public string Code { get; set; }

        [Required(ErrorMessage = "اسم شروط الدفع بالإنجليزي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم شروط الدفع بالإنجليزي يجب ألا يتجاوز 200 حرف")]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "عدد أيام الاستحقاق مطلوب")]
        [Range(0, 365, ErrorMessage = "عدد أيام الاستحقاق يجب أن يكون بين 0 و 365")]
        public int NetDays { get; set; } = 30;

        [Range(0, 365)]
        public int? DiscountDays { get; set; }

        [Range(0, 100)]
        public decimal? DiscountPercent { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// Payment Terms List DTO - قائمة شروط الدفع
    /// </summary>
    public class PaymentTermsListDto
    {
        [ExcelColumn(Name = "ID")]
        public int Id { get; set; }

        [ExcelColumn(Name = "Code")]
        public string Code { get; set; }

        [ExcelColumn(Name = "Name (English)")]
        public string NameEn { get; set; }

        [ExcelColumn(Name = "Name (Arabic)")]
        public string NameAr { get; set; }

        [ExcelColumn(Name = "Net Days")]
        public int NetDays { get; set; }

        [ExcelColumn(Name = "Discount Days")]
        public int? DiscountDays { get; set; }

        [ExcelColumn(Name = "Discount %")]
        public decimal? DiscountPercent { get; set; }

        [ExcelColumn(Name = "Is Active")]
        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public long? CompanyId { get; set; }
        public DateTime? Create_time { get; set; }
        public string Create_by { get; set; }
    }

    /// <summary>
    /// Payment Terms Detail DTO - تفاصيل شروط الدفع
    /// </summary>
    public class PaymentTermsDetailDto : PaymentTermsListDto
    {
        public long? BranchId { get; set; }
        public string Remark { get; set; }
        public DateTime? Update_time { get; set; }
        public string Update_by { get; set; }
    }

    /// <summary>
    /// Payment Terms Select DTO - شروط الدفع للقائمة المنسدلة
    /// </summary>
    public class PaymentTermsSelectDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int NetDays { get; set; }
        public int? DiscountDays { get; set; }
        public decimal? DiscountPercent { get; set; }
    }
}
