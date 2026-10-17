using System;
using System.Collections.Generic;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Parser Service Interface - واجهة خدمة تحليل كشف الحساب البنكي
    /// Supports CSV and MT940 SWIFT format parsing
    /// </summary>
    public interface IBankStatementParserService
    {
        /// <summary>
        /// Parse bank statement file - تحليل ملف كشف الحساب
        /// </summary>
        /// <param name="fileContent">Raw file content</param>
        /// <param name="format">Statement format (CSV, MT940, etc.)</param>
        /// <param name="profile">Optional parsing profile for bank-specific rules</param>
        /// <returns>Parsed statement with lines</returns>
        BankStatementParseResult Parse(byte[] fileContent, BankStatementFormat format, BankParsingProfile profile = null);

        /// <summary>
        /// Parse CSV bank statement - تحليل كشف حساب CSV
        /// </summary>
        BankStatementParseResult ParseCSV(byte[] fileContent, BankParsingProfile profile);

        /// <summary>
        /// Parse MT940 SWIFT bank statement - تحليل كشف حساب MT940
        /// </summary>
        BankStatementParseResult ParseMT940(byte[] fileContent);

        /// <summary>
        /// Validate parsed statement - التحقق من الكشف المحلل
        /// </summary>
        BankStatementValidationResult Validate(BankStatementParseResult result);

        /// <summary>
        /// Preview import (dry-run) - معاينة الاستيراد
        /// </summary>
        BankStatementPreviewResult Preview(byte[] fileContent, BankStatementFormat format, BankParsingProfile profile = null);

        /// <summary>
        /// Get supported formats - الحصول على التنسيقات المدعومة
        /// </summary>
        List<BankStatementFormat> GetSupportedFormats();
    }

    #region Result Classes

    /// <summary>
    /// Bank Statement Parse Result - نتيجة تحليل كشف الحساب
    /// </summary>
    public class BankStatementParseResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();

        // Statement Header Info (from MT940 :20:, :25:, :28:, :60: tags)
        public string TransactionReferenceNumber { get; set; }
        public string AccountIdentification { get; set; }
        public string StatementNumber { get; set; }
        public string SequenceNumber { get; set; }

        // Opening Balance (from MT940 :60F: or :60M:)
        public decimal OpeningBalance { get; set; }
        public string OpeningBalanceCurrency { get; set; }
        public DateTime? OpeningBalanceDate { get; set; }
        public bool OpeningBalanceIsCredit { get; set; }

        // Closing Balance (from MT940 :62F: or :62M:)
        public decimal ClosingBalance { get; set; }
        public string ClosingBalanceCurrency { get; set; }
        public DateTime? ClosingBalanceDate { get; set; }
        public bool ClosingBalanceIsCredit { get; set; }

        // Available Balance (from MT940 :64:)
        public decimal? AvailableBalance { get; set; }
        public DateTime? AvailableBalanceDate { get; set; }

        // Parsed Lines
        public List<ParsedBankStatementLine> Lines { get; set; } = new();

        // Summary
        public int TotalLines => Lines.Count;
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public int DebitCount { get; set; }
        public int CreditCount { get; set; }
    }

    /// <summary>
    /// Parsed Bank Statement Line - بند كشف حساب محلل
    /// </summary>
    public class ParsedBankStatementLine
    {
        public int LineNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? ValueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string TransactionType { get; set; }
        public string BankReference { get; set; }
        public string CustomerReference { get; set; }
        public string SupplementaryDetails { get; set; }
        public string ChequeNumber { get; set; }
        public string PayeeName { get; set; }
        public decimal? RunningBalance { get; set; }

        // MT940 specific fields
        public string SwiftCode { get; set; } // Transaction type indicator
        public string IdentificationCode { get; set; }

        // Parsing metadata
        public string RawData { get; set; }
        public List<string> ParseWarnings { get; set; } = new();
    }

    /// <summary>
    /// Bank Statement Validation Result - نتيجة التحقق من كشف الحساب
    /// </summary>
    public class BankStatementValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Balance checks
        public bool OpeningBalanceValid { get; set; }
        public bool ClosingBalanceValid { get; set; }
        public bool BalanceReconciles { get; set; }
        public decimal CalculatedClosingBalance { get; set; }
        public decimal BalanceDifference { get; set; }

        // Line validation
        public int ValidLines { get; set; }
        public int InvalidLines { get; set; }
        public List<int> LinesWithErrors { get; set; } = new();
    }

    /// <summary>
    /// Bank Statement Preview Result - نتيجة معاينة كشف الحساب
    /// </summary>
    public class BankStatementPreviewResult
    {
        public bool CanImport { get; set; }
        public BankStatementParseResult ParseResult { get; set; }
        public BankStatementValidationResult ValidationResult { get; set; }

        // Preview summary
        public int TotalTransactions { get; set; }
        public int NewTransactions { get; set; } // Not yet in system
        public int PotentialDuplicates { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public string Currency { get; set; }
        public DateTime? StatementStartDate { get; set; }
        public DateTime? StatementEndDate { get; set; }

        // Suggested matches
        public List<SuggestedMatch> SuggestedMatches { get; set; } = new();
    }

    /// <summary>
    /// Suggested Match - مطابقة مقترحة
    /// </summary>
    public class SuggestedMatch
    {
        public int LineNumber { get; set; }
        public Guid? MatchedPaymentId { get; set; }
        public string MatchedPaymentNumber { get; set; }
        public decimal Confidence { get; set; } // 0-100
        public string MatchType { get; set; } // "Exact", "Amount", "Reference", "Fuzzy"
    }

    #endregion
}
