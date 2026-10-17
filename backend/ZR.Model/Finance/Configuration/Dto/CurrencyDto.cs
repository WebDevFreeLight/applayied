using System;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// Currency Query DTO
    /// </summary>
    public class CurrencyQueryDto
    {
        public string Code { get; set; }
        public bool? IsBaseCurrency { get; set; }
        public bool? IsActive { get; set; }
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Currency Response DTO
    /// </summary>
    public class CurrencyResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Symbol { get; set; }
        public bool IsBaseCurrency { get; set; }
        public int DecimalPlaces { get; set; }
        public bool IsActive { get; set; }
        public int RoundingMethod { get; set; }
        public string DisplayFormat { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Create Currency DTO
    /// </summary>
    public class CreateCurrencyDto
    {
        /// <summary>
        /// ISO 4217 Currency Code (3 letters)
        /// رمز العملة
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Currency Name (English)
        /// اسم العملة بالإنجليزية
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Currency Name (Arabic)
        /// اسم العملة بالعربية
        /// </summary>
        public string NameAr { get; set; }

        /// <summary>
        /// Currency Symbol
        /// رمز العملة
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Decimal Places (0-6)
        /// عدد الخانات العشرية
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// Is Active?
        /// نشط؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Rounding Method
        /// طريقة التقريب
        /// </summary>
        public int RoundingMethod { get; set; } = 0;

        /// <summary>
        /// Display Format
        /// تنسيق العرض
        /// </summary>
        public string DisplayFormat { get; set; }

        /// <summary>
        /// Description
        /// الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Sort Order
        /// ترتيب العرض
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Currency DTO
    /// </summary>
    public class UpdateCurrencyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Symbol { get; set; }
        public int DecimalPlaces { get; set; }
        public bool IsActive { get; set; }
        public int RoundingMethod { get; set; }
        public string DisplayFormat { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }

        // NOTE: Code and IsBaseCurrency cannot be updated after creation
    }

    /// <summary>
    /// Set Base Currency DTO
    /// </summary>
    public class SetBaseCurrencyDto
    {
        public Guid CurrencyId { get; set; }
    }

    /// <summary>
    /// Currency List DTO (alias for CurrencyResponseDto)
    /// </summary>
    public class CurrencyListDto : CurrencyResponseDto
    {
    }

    /// <summary>
    /// Currency Detail DTO (alias for CurrencyResponseDto)
    /// </summary>
    public class CurrencyDetailDto : CurrencyResponseDto
    {
    }
}
