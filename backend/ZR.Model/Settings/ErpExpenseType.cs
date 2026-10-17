using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Settings
{
    /// <summary>
    /// Expense Type - نوع المصروف
    /// </summary>
    [SugarTable("erp_expense_types")]
    [SqlDict(
        dictType: "erp_expense_type",
        dictName: "Expense Type - نوع المصروف",
        valueColumn: "expensetypeid",
        labelColumns: "typename",
        RemarkColumn = "typenamear",
        OrderBy = "typename ASC",
        Description = "Auto-generated from ErpExpenseType table"
    )]
    public class ErpExpenseType : SysBase
    {
        /// <summary>
        /// Expense Type ID - معرف نوع المصروف
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ExpenseTypeId { get; set; }

        /// <summary>
        /// Type Code - كود النوع
        /// </summary>
        public string TypeCode { get; set; }

        /// <summary>
        /// Type Name - اسم النوع
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Type Name Arabic - اسم النوع بالعربي
        /// </summary>
        public string TypeNameAr { get; set; }

        /// <summary>
        /// Parent Type ID - معرف النوع الأب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentTypeId { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب المرتبط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccountId { get; set; }

        /// <summary>
        /// Default VAT Percent - نسبة الضريبة الافتراضية
        /// </summary>
        public decimal DefaultVatPercent { get; set; } = 15;

        /// <summary>
        /// Requires Attachment - يتطلب مرفق
        /// </summary>
        public bool RequiresAttachment { get; set; }

        /// <summary>
        /// Max Amount Per Transaction - الحد الأقصى للعملية
        /// </summary>
        public decimal? MaxAmountPerTransaction { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

