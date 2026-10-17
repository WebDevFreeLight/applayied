using System;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Bank Parsing Profile - ملف تعريف تحليل البنك
    /// Bank-specific parsing rules for statement import (requirement #45)
    /// </summary>
    [SugarTable("bank_parsing_profile")]
    public class BankParsingProfile : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Profile ID - معرف الملف
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Profile Name - اسم الملف
        /// </summary>
        [SugarColumn(Length = 100, ColumnName = "profile_name")]
        public string ProfileName { get; set; }

        /// <summary>
        /// Profile Name in Arabic - اسم الملف بالعربية
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "profile_name_ar")]
        public string ProfileNameAr { get; set; }

        /// <summary>
        /// Bank Code - رمز البنك
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "bank_code")]
        public string BankCode { get; set; }

        /// <summary>
        /// Bank Name - اسم البنك
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "bank_name")]
        public string BankName { get; set; }

        /// <summary>
        /// Country Code - رمز الدولة
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = true, ColumnName = "country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Statement Format - تنسيق الكشف
        /// </summary>
        [SugarColumn(ColumnName = "statement_format")]
        public BankStatementFormat StatementFormat { get; set; }

        /// <summary>
        /// File Extension - امتداد الملف
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "file_extension")]
        public string FileExtension { get; set; }

        /// <summary>
        /// Encoding - الترميز
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "encoding")]
        public string Encoding { get; set; } = "UTF-8";

        /// <summary>
        /// CSV Delimiter - فاصل CSV
        /// </summary>
        [SugarColumn(Length = 5, IsNullable = true, ColumnName = "csv_delimiter")]
        public string CsvDelimiter { get; set; } = ",";

        /// <summary>
        /// Date Format - تنسيق التاريخ
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "date_format")]
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Decimal Separator - فاصل الأرقام العشرية
        /// </summary>
        [SugarColumn(Length = 5, IsNullable = true, ColumnName = "decimal_separator")]
        public string DecimalSeparator { get; set; } = ".";

        /// <summary>
        /// Thousand Separator - فاصل الآلاف
        /// </summary>
        [SugarColumn(Length = 5, IsNullable = true, ColumnName = "thousand_separator")]
        public string ThousandSeparator { get; set; } = ",";

        /// <summary>
        /// Has Header Row - يحتوي على صف عناوين
        /// </summary>
        [SugarColumn(ColumnName = "has_header_row")]
        public bool HasHeaderRow { get; set; } = true;

        /// <summary>
        /// Skip Rows - عدد الصفوف المتجاوزة
        /// </summary>
        [SugarColumn(ColumnName = "skip_rows")]
        public int SkipRows { get; set; } = 0;

        /// <summary>
        /// Column Mapping - تعيين الأعمدة (JSON)
        /// Maps CSV/Excel column indices to statement fields
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "column_mapping")]
        public string ColumnMapping { get; set; }

        /// <summary>
        /// Transaction Type Mapping - تعيين نوع المعاملة (JSON)
        /// Maps bank-specific transaction codes to standard types
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "transaction_type_mapping")]
        public string TransactionTypeMapping { get; set; }

        /// <summary>
        /// Reference Pattern - نمط المرجع (Regex)
        /// Pattern to extract reference number from description
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "reference_pattern")]
        public string ReferencePattern { get; set; }

        /// <summary>
        /// Cheque Number Pattern - نمط رقم الشيك (Regex)
        /// Pattern to extract cheque number from description
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "cheque_number_pattern")]
        public string ChequeNumberPattern { get; set; }

        /// <summary>
        /// Credit Indicator - مؤشر الدائن
        /// Values indicating credit transactions
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "credit_indicator")]
        public string CreditIndicator { get; set; } = "CR,C,+";

        /// <summary>
        /// Debit Indicator - مؤشر المدين
        /// Values indicating debit transactions
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "debit_indicator")]
        public string DebitIndicator { get; set; } = "DR,D,-";

        /// <summary>
        /// Amount Format - تنسيق المبلغ
        /// "signed" (positive/negative), "indicator" (separate DR/CR column), "separate" (separate columns)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "amount_format")]
        public string AmountFormat { get; set; } = "signed";

        /// <summary>
        /// Validation Rules - قواعد التحقق (JSON)
        /// Bank-specific validation rules
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "validation_rules")]
        public string ValidationRules { get; set; }

        /// <summary>
        /// Sample Data - بيانات نموذجية
        /// Sample rows for testing/reference
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "sample_data")]
        public string SampleData { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        // Note: CompanyID and BranchID are inherited from SysBase - do not duplicate

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Bank Parsing Column Mapping - تعيين أعمدة التحليل
    /// Defines which CSV/Excel column contains which data
    /// </summary>
    public class BankParsingColumnMapping
    {
        public int DateColumn { get; set; } = 0;
        public int ValueDateColumn { get; set; } = -1; // -1 means not present
        public int DescriptionColumn { get; set; } = 1;
        public int AmountColumn { get; set; } = 2;
        public int DebitColumn { get; set; } = -1; // For separate debit column
        public int CreditColumn { get; set; } = -1; // For separate credit column
        public int IndicatorColumn { get; set; } = -1; // For DR/CR indicator
        public int BalanceColumn { get; set; } = -1;
        public int ReferenceColumn { get; set; } = -1;
        public int TransactionTypeColumn { get; set; } = -1;
        public int ChequeNumberColumn { get; set; } = -1;
        public int PayeeColumn { get; set; } = -1;
        public int CategoryColumn { get; set; } = -1;
    }
}
