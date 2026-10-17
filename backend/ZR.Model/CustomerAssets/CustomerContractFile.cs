using System;
using SqlSugar;
using ZR.Model.CustomerAssets.Enums;
using ZR.Model.System;

namespace ZR.Model.CustomerAssets
{
    /// <summary>
    /// Customer Contract File - ملف عقد العميل
    /// </summary>
    [SugarTable("customer_contract_file")]
    public class CustomerContractFile : SysBase
    {
        /// <summary>
        /// File ID - معرف الملف
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Assignment ID - معرف التعيين
        /// </summary>
        public Guid AssignmentId { get; set; }

        /// <summary>
        /// File Type - نوع الملف
        /// </summary>
        public ContractFileType FileType { get; set; }

        /// <summary>
        /// File Name - اسم الملف
        /// </summary>
        [SugarColumn(Length = 255)]
        public string FileName { get; set; }

        /// <summary>
        /// File URL - رابط الملف
        /// </summary>
        [SugarColumn(Length = 500)]
        public string FileUrl { get; set; }

        /// <summary>
        /// File Size (bytes) - حجم الملف
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Uploaded By (username) - رفع بواسطة
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = true)]
        public string UploadedBy { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Notes { get; set; }
    }
}
