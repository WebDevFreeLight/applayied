using System;
using System.ComponentModel.DataAnnotations;
using ZR.Model.CustomerAssets.Enums;

namespace ZR.Model.CustomerAssets.Dto
{
    /// <summary>
    /// Create Contract File DTO - إنشاء ملف عقد
    /// </summary>
    public class CreateContractFileDto
    {
        [Required(ErrorMessage = "معرف التعيين مطلوب")]
        public Guid AssignmentId { get; set; }

        [Required(ErrorMessage = "نوع الملف مطلوب")]
        public ContractFileType FileType { get; set; }

        [Required(ErrorMessage = "اسم الملف مطلوب")]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required(ErrorMessage = "رابط الملف مطلوب")]
        [StringLength(500)]
        public string FileUrl { get; set; }

        public long FileSize { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Contract File List DTO - قائمة ملفات العقد
    /// </summary>
    public class ContractFileListDto
    {
        public Guid Id { get; set; }
        public Guid AssignmentId { get; set; }
        public ContractFileType FileType { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public string UploadedBy { get; set; }
        public string Notes { get; set; }
        public DateTime Create_time { get; set; }
    }
}
