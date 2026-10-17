namespace ZR.Model.Finance.Zatca.Enums
{
    /// <summary>
    /// ZATCA Tax Category - فئة الضريبة لزاتكا
    /// Based on UN/ECE 5305 code list
    /// </summary>
    public enum ZatcaTaxCategory
    {
        /// <summary>
        /// Standard Rate (S) - معدل قياسي
        /// Normal VAT rate (15%)
        /// </summary>
        StandardRate = 0,

        /// <summary>
        /// Zero Rated (Z) - معفى بنسبة صفر
        /// Zero-rated supplies (exports, etc.)
        /// </summary>
        ZeroRated = 1,

        /// <summary>
        /// Exempt (E) - معفى
        /// VAT exempt supplies
        /// </summary>
        Exempt = 2,

        /// <summary>
        /// Out of Scope (O) - خارج النطاق
        /// Not subject to VAT
        /// </summary>
        OutOfScope = 3
    }

    /// <summary>
    /// ZATCA Tax Exemption Reason Codes - أكواد أسباب الإعفاء الضريبي
    /// </summary>
    public static class ZatcaTaxExemptionReasonCode
    {
        // Zero-rated (Z) reasons
        public const string ExportOfGoods = "VATEX-SA-32";
        public const string ExportOfServices = "VATEX-SA-33";
        public const string InternationalTransport = "VATEX-SA-34-1";
        public const string InternationalTransportServices = "VATEX-SA-34-2";
        public const string QualifiedSupplies = "VATEX-SA-34-3";
        public const string QualifiedMedicines = "VATEX-SA-34-4";
        public const string QualifiedMetals = "VATEX-SA-34-5";

        // Exempt (E) reasons
        public const string FinancialServices = "VATEX-SA-29";
        public const string LifeInsurance = "VATEX-SA-29-7";
        public const string RealEstateTransactions = "VATEX-SA-30";

        // Out of Scope (O) reasons
        public const string OutOfScopeNotSubject = "VATEX-SA-OOS";
    }
}
