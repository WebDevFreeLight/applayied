using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using ZR.Model.Finance.Tax.Dto;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Tax;
using ZR.Repository.Finance.Tax;
using ZR.Service.IService.Finance.Tax;

namespace ZR.Service.Finance.Tax
{
    /// <summary>
    /// Tax Calculation Service Implementation
    /// THE CORE TAX CALCULATION ENGINE
    /// محرك حساب الضرائب الأساسي
    ///
    /// This service handles all tax calculations with support for:
    /// - Tax-inclusive and tax-exclusive amounts
    /// - Per-line and per-invoice rounding modes
    /// - Multi-currency with exchange rate conversion
    /// - Effective-dated tax rates
    /// - Tax groups (compound and non-compound)
    /// - Reverse charge handling
    /// </summary>
    [AppService(ServiceType = typeof(ITaxCalculationService), ServiceLifetime = LifeTime.Transient)]
    public class TaxCalculationService : ITaxCalculationService
    {
        private readonly ITaxCodeRepository _taxCodeRepository;
        private readonly ITaxRateRepository _taxRateRepository;
        private readonly ITaxGroupRepository _taxGroupRepository;
        private readonly ITaxJournalLineRepository _taxJournalLineRepository;

        public TaxCalculationService(
            ITaxCodeRepository taxCodeRepository,
            ITaxRateRepository taxRateRepository,
            ITaxGroupRepository taxGroupRepository,
            ITaxJournalLineRepository taxJournalLineRepository)
        {
            _taxCodeRepository = taxCodeRepository;
            _taxRateRepository = taxRateRepository;
            _taxGroupRepository = taxGroupRepository;
            _taxJournalLineRepository = taxJournalLineRepository;
        }

        #region Main Calculation Operations

        /// <summary>
        /// Calculate tax for a request (preview or actual)
        /// This is the MAIN calculation method
        /// </summary>
        public TaxCalculationResultDto Calculate(TaxCalculationRequestDto request)
        {
            var result = new TaxCalculationResultDto
            {
                CalculatedAt = DateTime.Now,
                Input = new TaxCalculationInputEchoDto
                {
                    CompanyId = request.CompanyId,
                    Currency = request.Currency,
                    Date = request.Date,
                    TaxInclusive = request.TaxInclusive,
                    RoundingMode = request.RoundingMode,
                    DecimalPlaces = request.DecimalPlaces,
                    LineCount = request.Lines.Count
                },
                RoundingModeUsed = request.RoundingMode,
                DecimalPlacesUsed = request.DecimalPlaces
            };

            // Validate request
            var validation = ValidateRequest(request);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Errors = validation.Errors.Select(e => e.MessageEn).ToList();
                return result;
            }

            try
            {
                // Get exchange rate
                decimal exchangeRate = request.ExchangeRate ?? 1m;
                result.ExchangeRateUsed = exchangeRate;

                // Calculate each line
                var lineResults = new List<TaxCalculationLineResultDto>();
                foreach (var line in request.Lines)
                {
                    var lineResult = CalculateLine(line, request);
                    lineResults.Add(lineResult);
                }

                result.LineResults = lineResults;

                // Apply per-invoice rounding if needed
                if (request.RoundingMode == TaxRoundingMode.RoundPerInvoice)
                {
                    ApplyInvoiceLevelRounding(result, request.DecimalPlaces);
                }

                // Calculate summary by tax code
                result.TaxSummaryByCode = CalculateTaxSummaryByCode(lineResults);

                // Calculate totals
                result.Totals = CalculateTotals(lineResults, request.Currency, exchangeRate);

                // Generate hash
                result.CalculationHash = GenerateCalculationHash(result);

                result.Success = !lineResults.Any(l => l.Errors.Any());

                // Collect warnings
                result.Warnings = lineResults.SelectMany(l => l.Warnings).Distinct().ToList();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Calculation error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Calculate tax for a request (async)
        /// </summary>
        public async Task<TaxCalculationResultDto> CalculateAsync(TaxCalculationRequestDto request)
        {
            // For async version, we'll use async repository methods
            return await Task.Run(() => Calculate(request));
        }

        /// <summary>
        /// Calculate tax for a single line item
        /// </summary>
        public TaxCalculationLineResultDto CalculateLine(TaxCalculationLineDto line, TaxCalculationRequestDto context)
        {
            var result = new TaxCalculationLineResultDto
            {
                LineId = line.LineId,
                AccountId = line.AccountId,
                TaxCode = line.TaxCode,
                OriginalAmountForeign = line.AmountForeign,
                IsTaxInclusive = context.TaxInclusive
            };

            try
            {
                // Get tax code
                var taxCode = _taxCodeRepository.GetByCode(line.TaxCode, context.CompanyId);
                if (taxCode == null)
                {
                    // Try as tax group
                    var taxGroup = _taxGroupRepository.GetByCode(line.TaxCode, context.CompanyId);
                    if (taxGroup == null)
                    {
                        result.Errors.Add($"Tax code '{line.TaxCode}' not found - رمز الضريبة '{line.TaxCode}' غير موجود");
                        return result;
                    }

                    // Calculate using tax group
                    return CalculateLineWithTaxGroup(line, context, taxGroup);
                }

                result.TaxCodeId = taxCode.Id;

                // Get effective rate
                decimal taxRate;
                if (line.OverrideRate.HasValue)
                {
                    taxRate = line.OverrideRate.Value;
                    result.Warnings.Add($"Using override rate {taxRate}% instead of effective rate");
                }
                else
                {
                    var effectiveRate = _taxRateRepository.GetEffectiveRate(taxCode.Id, context.Date);
                    if (effectiveRate == null)
                    {
                        result.Warnings.Add($"No effective rate found for tax code '{line.TaxCode}' on {context.Date:yyyy-MM-dd}, using 0%");
                        taxRate = 0;
                    }
                    else
                    {
                        taxRate = effectiveRate.Rate;
                    }
                }

                result.TaxRate = taxRate;

                // Get exchange rate
                decimal exchangeRate = context.ExchangeRate ?? 1m;

                // Calculate based on tax-inclusive or exclusive
                if (context.TaxInclusive)
                {
                    // Tax-inclusive calculation
                    // taxable = amount / (1 + rate/100)
                    // tax = amount - taxable
                    result.TaxableAmountForeign = GetTaxableFromInclusive(line.AmountForeign, taxRate, context.DecimalPlaces);
                    result.TaxAmountForeign = line.AmountForeign - result.TaxableAmountForeign;
                    result.TotalAmountForeign = line.AmountForeign;
                }
                else
                {
                    // Tax-exclusive calculation
                    // tax = amount * rate/100
                    // total = amount + tax
                    result.TaxableAmountForeign = line.AmountForeign;
                    result.TaxAmountForeign = CalculateTaxOnExclusive(line.AmountForeign, taxRate, context.DecimalPlaces);
                    result.TotalAmountForeign = line.AmountForeign + result.TaxAmountForeign;
                }

                // Apply per-line rounding if needed
                if (context.RoundingMode == TaxRoundingMode.RoundPerLine)
                {
                    var originalTax = result.TaxAmountForeign;
                    result.TaxAmountForeign = Round(result.TaxAmountForeign, context.DecimalPlaces);
                    result.RoundingDelta = result.TaxAmountForeign - originalTax;

                    // Recalculate total after rounding
                    if (context.TaxInclusive)
                    {
                        result.TaxableAmountForeign = result.TotalAmountForeign - result.TaxAmountForeign;
                    }
                    else
                    {
                        result.TotalAmountForeign = result.TaxableAmountForeign + result.TaxAmountForeign;
                    }
                }

                // Convert to base currency
                result.TaxableAmountBase = ConvertToBase(result.TaxableAmountForeign, exchangeRate, context.DecimalPlaces);
                result.TaxAmountBase = ConvertToBase(result.TaxAmountForeign, exchangeRate, context.DecimalPlaces);
                result.TotalAmountBase = ConvertToBase(result.TotalAmountForeign, exchangeRate, context.DecimalPlaces);

                // Set tax account
                result.IsRecoverable = taxCode.IsRecoverable;
                result.IsReverseCharge = taxCode.TaxType == TaxType.ReverseCharge;

                if (result.IsRecoverable)
                {
                    result.TaxAccountId = taxCode.DefaultRecoverableAccountId;
                }
                else
                {
                    result.TaxAccountId = taxCode.DefaultPayableAccountId;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line calculation error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Calculate tax for a single line item (async)
        /// </summary>
        public async Task<TaxCalculationLineResultDto> CalculateLineAsync(TaxCalculationLineDto line, TaxCalculationRequestDto context)
        {
            return await Task.Run(() => CalculateLine(line, context));
        }

        /// <summary>
        /// Calculate line using tax group (handles compound tax)
        /// </summary>
        private TaxCalculationLineResultDto CalculateLineWithTaxGroup(
            TaxCalculationLineDto line,
            TaxCalculationRequestDto context,
            TaxGroup taxGroup)
        {
            var result = new TaxCalculationLineResultDto
            {
                LineId = line.LineId,
                AccountId = line.AccountId,
                TaxCode = line.TaxCode,
                OriginalAmountForeign = line.AmountForeign,
                IsTaxInclusive = context.TaxInclusive
            };

            if (taxGroup.Lines == null || !taxGroup.Lines.Any())
            {
                result.Errors.Add($"Tax group '{line.TaxCode}' has no tax codes configured");
                return result;
            }

            var activeLines = taxGroup.Lines.Where(l => l.IsActive).OrderBy(l => l.Sequence).ToList();

            decimal exchangeRate = context.ExchangeRate ?? 1m;
            decimal totalTaxForeign = 0;
            decimal currentBase = line.AmountForeign;

            if (context.TaxInclusive)
            {
                // For inclusive, we need to back-calculate the original base
                decimal combinedRate = _taxGroupRepository.CalculateCombinedRate(taxGroup.Id, context.Date);
                result.TaxableAmountForeign = GetTaxableFromInclusive(line.AmountForeign, combinedRate, context.DecimalPlaces);
                result.TaxAmountForeign = line.AmountForeign - result.TaxableAmountForeign;
                result.TotalAmountForeign = line.AmountForeign;
            }
            else
            {
                // Tax-exclusive with group
                if (taxGroup.IsCompound)
                {
                    // Compound: each tax is calculated on base + previous taxes
                    foreach (var groupLine in activeLines)
                    {
                        var rate = groupLine.OverrideRate ??
                            (_taxRateRepository.GetEffectiveRate(groupLine.TaxCodeId, context.Date)?.Rate ?? 0);
                        var lineTax = CalculateTaxOnExclusive(currentBase, rate, context.DecimalPlaces);
                        totalTaxForeign += lineTax;
                        currentBase += lineTax; // compound effect
                    }
                }
                else
                {
                    // Non-compound: all taxes calculated on original base
                    foreach (var groupLine in activeLines)
                    {
                        var rate = groupLine.OverrideRate ??
                            (_taxRateRepository.GetEffectiveRate(groupLine.TaxCodeId, context.Date)?.Rate ?? 0);
                        var lineTax = CalculateTaxOnExclusive(line.AmountForeign, rate, context.DecimalPlaces);
                        totalTaxForeign += lineTax;
                    }
                }

                result.TaxableAmountForeign = line.AmountForeign;
                result.TaxAmountForeign = totalTaxForeign;
                result.TotalAmountForeign = line.AmountForeign + totalTaxForeign;
            }

            // Use the combined rate for display
            result.TaxRate = _taxGroupRepository.CalculateCombinedRate(taxGroup.Id, context.Date);

            // Apply per-line rounding if needed
            if (context.RoundingMode == TaxRoundingMode.RoundPerLine)
            {
                var originalTax = result.TaxAmountForeign;
                result.TaxAmountForeign = Round(result.TaxAmountForeign, context.DecimalPlaces);
                result.RoundingDelta = result.TaxAmountForeign - originalTax;
            }

            // Convert to base currency
            result.TaxableAmountBase = ConvertToBase(result.TaxableAmountForeign, exchangeRate, context.DecimalPlaces);
            result.TaxAmountBase = ConvertToBase(result.TaxAmountForeign, exchangeRate, context.DecimalPlaces);
            result.TotalAmountBase = ConvertToBase(result.TotalAmountForeign, exchangeRate, context.DecimalPlaces);

            return result;
        }

        #endregion

        #region Tax-Inclusive Calculations

        /// <summary>
        /// Extract tax from a tax-inclusive amount
        /// Formula: tax = amount - (amount / (1 + rate/100))
        /// </summary>
        public decimal ExtractTaxFromInclusive(decimal inclusiveAmount, decimal taxRate, int decimalPlaces = 2)
        {
            if (taxRate == 0) return 0;
            var taxable = GetTaxableFromInclusive(inclusiveAmount, taxRate, decimalPlaces);
            return inclusiveAmount - taxable;
        }

        /// <summary>
        /// Get taxable amount from tax-inclusive amount
        /// Formula: taxable = amount / (1 + rate/100)
        /// </summary>
        public decimal GetTaxableFromInclusive(decimal inclusiveAmount, decimal taxRate, int decimalPlaces = 2)
        {
            if (taxRate == 0) return inclusiveAmount;
            var divisor = 1 + (taxRate / 100);
            return Round(inclusiveAmount / divisor, decimalPlaces);
        }

        #endregion

        #region Tax-Exclusive Calculations

        /// <summary>
        /// Calculate tax on a tax-exclusive amount
        /// Formula: tax = amount * (rate/100)
        /// </summary>
        public decimal CalculateTaxOnExclusive(decimal exclusiveAmount, decimal taxRate, int decimalPlaces = 2)
        {
            if (taxRate == 0) return 0;
            return Round(exclusiveAmount * (taxRate / 100), decimalPlaces);
        }

        /// <summary>
        /// Get total from tax-exclusive amount
        /// Formula: total = amount * (1 + rate/100)
        /// </summary>
        public decimal GetTotalFromExclusive(decimal exclusiveAmount, decimal taxRate, int decimalPlaces = 2)
        {
            if (taxRate == 0) return exclusiveAmount;
            return Round(exclusiveAmount * (1 + taxRate / 100), decimalPlaces);
        }

        #endregion

        #region Rounding Operations

        /// <summary>
        /// Apply rounding based on mode
        /// Uses banker's rounding (MidpointRounding.ToEven)
        /// </summary>
        public decimal Round(decimal amount, int decimalPlaces)
        {
            return Math.Round(amount, decimalPlaces, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Calculate rounding delta
        /// </summary>
        public decimal CalculateRoundingDelta(decimal original, decimal rounded)
        {
            return rounded - original;
        }

        /// <summary>
        /// Apply invoice-level rounding to result
        /// </summary>
        private void ApplyInvoiceLevelRounding(TaxCalculationResultDto result, int decimalPlaces)
        {
            // Sum all unrounded taxes
            decimal totalUnroundedTax = result.LineResults.Sum(l => l.TaxAmountForeign);

            // Round the total
            decimal roundedTotalTax = Round(totalUnroundedTax, decimalPlaces);

            // Calculate the adjustment needed
            decimal adjustment = roundedTotalTax - totalUnroundedTax;

            if (adjustment != 0 && result.LineResults.Any())
            {
                // Apply adjustment to the last line
                var lastLine = result.LineResults.Last();
                lastLine.TaxAmountForeign += adjustment;
                lastLine.RoundingDelta = adjustment;
                lastLine.TotalAmountForeign = lastLine.TaxableAmountForeign + lastLine.TaxAmountForeign;

                // Recalculate base amounts for adjusted line
                decimal exchangeRate = result.ExchangeRateUsed;
                lastLine.TaxAmountBase = ConvertToBase(lastLine.TaxAmountForeign, exchangeRate, decimalPlaces);
                lastLine.TotalAmountBase = ConvertToBase(lastLine.TotalAmountForeign, exchangeRate, decimalPlaces);
            }
        }

        #endregion

        #region Exchange Rate Operations

        /// <summary>
        /// Convert amount to base currency
        /// </summary>
        public decimal ConvertToBase(decimal foreignAmount, decimal exchangeRate, int decimalPlaces = 2)
        {
            if (exchangeRate == 0 || exchangeRate == 1) return foreignAmount;
            return Round(foreignAmount * exchangeRate, decimalPlaces);
        }

        /// <summary>
        /// Get exchange rate for currency on date
        /// </summary>
        public async Task<decimal> GetExchangeRateAsync(string currency, DateTime date, long companyId)
        {
            // TODO: Integrate with ExchangeRateService
            // For now, return 1 (same as base currency)
            return await Task.FromResult(1m);
        }

        #endregion

        #region Summary Calculations

        /// <summary>
        /// Calculate tax summary by tax code
        /// </summary>
        private List<TaxSummaryByCodeDto> CalculateTaxSummaryByCode(List<TaxCalculationLineResultDto> lineResults)
        {
            return lineResults
                .Where(l => l.TaxCodeId != Guid.Empty)
                .GroupBy(l => new { l.TaxCodeId, l.TaxCode, l.TaxRate })
                .Select(g =>
                {
                    var taxCode = _taxCodeRepository.GetById(g.Key.TaxCodeId);
                    return new TaxSummaryByCodeDto
                    {
                        TaxCodeId = g.Key.TaxCodeId,
                        TaxCode = g.Key.TaxCode,
                        TaxCodeNameEn = taxCode?.NameEn,
                        TaxCodeNameAr = taxCode?.NameAr,
                        TaxRate = g.Key.TaxRate,
                        LineCount = g.Count(),
                        TaxableAmountForeign = g.Sum(l => l.TaxableAmountForeign),
                        TaxAmountForeign = g.Sum(l => l.TaxAmountForeign),
                        TaxableAmountBase = g.Sum(l => l.TaxableAmountBase),
                        TaxAmountBase = g.Sum(l => l.TaxAmountBase),
                        TaxAccountId = g.First().TaxAccountId
                    };
                })
                .ToList();
        }

        /// <summary>
        /// Calculate totals from line results
        /// </summary>
        private TaxCalculationTotalsDto CalculateTotals(
            List<TaxCalculationLineResultDto> lineResults,
            string currency,
            decimal exchangeRate)
        {
            return new TaxCalculationTotalsDto
            {
                SubtotalForeign = lineResults.Sum(l => l.TaxableAmountForeign),
                TotalTaxForeign = lineResults.Sum(l => l.TaxAmountForeign),
                GrandTotalForeign = lineResults.Sum(l => l.TotalAmountForeign),
                SubtotalBase = lineResults.Sum(l => l.TaxableAmountBase),
                TotalTaxBase = lineResults.Sum(l => l.TaxAmountBase),
                GrandTotalBase = lineResults.Sum(l => l.TotalAmountBase),
                TotalRoundingDelta = lineResults.Sum(l => l.RoundingDelta),
                Currency = currency,
                BaseCurrency = "SAR", // TODO: Get from company settings
                ExchangeRate = exchangeRate
            };
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validate calculation request
        /// </summary>
        public TaxValidationResult ValidateRequest(TaxCalculationRequestDto request)
        {
            var result = new TaxValidationResult();

            if (request == null)
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "NULL_REQUEST",
                    MessageEn = "Request cannot be null",
                    MessageAr = "الطلب لا يمكن أن يكون فارغاً"
                });
                return result;
            }

            if (request.CompanyId <= 0)
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "INVALID_COMPANY",
                    Field = "CompanyId",
                    MessageEn = "Company ID is required",
                    MessageAr = "معرف الشركة مطلوب"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "INVALID_CURRENCY",
                    Field = "Currency",
                    MessageEn = "Currency is required",
                    MessageAr = "العملة مطلوبة"
                });
            }

            if (request.Date == default)
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "INVALID_DATE",
                    Field = "Date",
                    MessageEn = "Date is required",
                    MessageAr = "التاريخ مطلوب"
                });
            }

            if (request.Lines == null || !request.Lines.Any())
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "NO_LINES",
                    Field = "Lines",
                    MessageEn = "At least one line item is required",
                    MessageAr = "مطلوب سطر واحد على الأقل"
                });
            }
            else
            {
                foreach (var line in request.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line.LineId))
                    {
                        result.Errors.Add(new TaxValidationError
                        {
                            Code = "INVALID_LINE_ID",
                            Field = "LineId",
                            MessageEn = "Line ID is required",
                            MessageAr = "معرف السطر مطلوب"
                        });
                    }

                    if (string.IsNullOrWhiteSpace(line.TaxCode))
                    {
                        result.Errors.Add(new TaxValidationError
                        {
                            Code = "INVALID_TAX_CODE",
                            Field = "TaxCode",
                            LineId = line.LineId,
                            MessageEn = "Tax code is required",
                            MessageAr = "رمز الضريبة مطلوب"
                        });
                    }
                }
            }

            if (request.DecimalPlaces < 0 || request.DecimalPlaces > 6)
            {
                result.Errors.Add(new TaxValidationError
                {
                    Code = "INVALID_DECIMAL_PLACES",
                    Field = "DecimalPlaces",
                    MessageEn = "Decimal places must be between 0 and 6",
                    MessageAr = "عدد المنازل العشرية يجب أن يكون بين 0 و 6"
                });
            }

            return result;
        }

        /// <summary>
        /// Validate calculation request (async)
        /// </summary>
        public async Task<TaxValidationResult> ValidateRequestAsync(TaxCalculationRequestDto request)
        {
            return await Task.Run(() => ValidateRequest(request));
        }

        #endregion

        #region Apply to Documents

        /// <summary>
        /// Apply tax calculation to an invoice
        /// </summary>
        public ApplyTaxToInvoiceResponseDto ApplyToInvoice(ApplyTaxToInvoiceRequestDto request)
        {
            // TODO: Implement invoice integration
            throw new NotImplementedException("Invoice integration not yet implemented");
        }

        /// <summary>
        /// Apply tax calculation to an invoice (async)
        /// </summary>
        public async Task<ApplyTaxToInvoiceResponseDto> ApplyToInvoiceAsync(ApplyTaxToInvoiceRequestDto request)
        {
            return await Task.Run(() => ApplyToInvoice(request));
        }

        #endregion

        #region Hash/Verification

        /// <summary>
        /// Generate calculation hash for audit
        /// </summary>
        public string GenerateCalculationHash(TaxCalculationResultDto result)
        {
            var hashInput = new
            {
                result.Input?.CompanyId,
                result.Input?.Currency,
                result.Input?.Date,
                result.Totals?.SubtotalForeign,
                result.Totals?.TotalTaxForeign,
                result.Totals?.GrandTotalForeign,
                result.CalculatedAt
            };

            var json = System.Text.Json.JsonSerializer.Serialize(hashInput);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verify calculation hash
        /// </summary>
        public bool VerifyCalculationHash(TaxCalculationResultDto result, string hash)
        {
            var generatedHash = GenerateCalculationHash(result);
            return generatedHash == hash;
        }

        #endregion
    }
}
