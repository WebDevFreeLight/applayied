using System;
using System.Threading;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.Treasury;
using ZR.Repository.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{
  
    /// <summary>
    /// Treasury Number Sequence Service - خدمة تسلسل أرقام الخزينة
    /// </summary>
    [AppService(ServiceType = typeof(ITreasuryNumberSequenceService), ServiceLifetime = LifeTime.Transient)]
    public class TreasuryNumberSequenceService : ITreasuryNumberSequenceService
    {
        private readonly TreasuryNumberSequenceRepository _sequenceRepository;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // Sequence type constants
        public const string PAYMENT_SEQ = "PAY";
        public const string RECEIPT_SEQ = "RCP";
        public const string TRANSFER_SEQ = "TRF";
        public const string CHEQUE_SEQ = "CHQ";
        public const string RECONCILIATION_SEQ = "REC";
        public const string RECEIPT_VOUCHER_SEQ = "RQ";      // سند قبض
        public const string DISBURSEMENT_VOUCHER_SEQ = "DS"; // سند صرف

        public TreasuryNumberSequenceService(TreasuryNumberSequenceRepository sequenceRepository)
        {
            _sequenceRepository = sequenceRepository;
        }

        public string GetNextPaymentNumber(long? companyId = null)
        {
            return GetNextNumber(PAYMENT_SEQ, companyId);
        }

        public string GetNextReceiptNumber(long? companyId = null)
        {
            return GetNextNumber(RECEIPT_SEQ, companyId);
        }

        public string GetNextTransferNumber(long? companyId = null)
        {
            return GetNextNumber(TRANSFER_SEQ, companyId);
        }

        public string GetNextChequeNumber(Guid bankAccountId, long? companyId = null)
        {
            // For cheques, we might want per-bank-account sequences
            // But for simplicity, using company-wide sequence
            return GetNextNumber(CHEQUE_SEQ, companyId);
        }

        public string GetNextReconciliationNumber(long? companyId = null)
        {
            return GetNextNumber(RECONCILIATION_SEQ, companyId);
        }

        /// <summary>
        /// Get next voucher number - الحصول على رقم السند التالي
        /// Format: {prefix}-YYYYMM-NNNNN (monthly reset)
        /// </summary>
        public string GetNextVoucherNumber(string prefix, long? companyId = null)
        {
            _semaphore.Wait();
            try
            {
                string generatedNumber = null;
                var sequenceType = prefix.ToUpper() switch
                {
                    "RQ" => RECEIPT_VOUCHER_SEQ,
                    "DS" => DISBURSEMENT_VOUCHER_SEQ,
                    _ => prefix.ToUpper()
                };

                _sequenceRepository.UseTran(() =>
                {
                    var currentYear = DateTime.Now.Year;
                    var currentMonth = DateTime.Now.Month;
                    var yearMonth = $"{currentYear}{currentMonth:D2}"; // YYYYMM format

                    // Find or create sequence for this month
                    var predicate = Expressionable.Create<TreasuryNumberSequence>();
                    predicate = predicate.And(x => x.SequenceType == sequenceType);
                    predicate = predicate.And(x => x.FiscalYear == currentYear);
                    predicate = predicate.And(x => x.Suffix == yearMonth); // Use Suffix to track month
                    predicate = predicate.And(x => x.IsActive);

                    if (companyId.HasValue)
                    {
                        predicate = predicate.And(x => x.CompanyID == companyId.Value);
                    }
                    else
                    {
                        predicate = predicate.And(x => x.CompanyID == null);
                    }

                    var sequence = _sequenceRepository.GetFirst(predicate.ToExpression());

                    if (sequence == null)
                    {
                        // Create new sequence for this month
                        sequence = new TreasuryNumberSequence
                        {
                            Id = Guid.NewGuid(),
                            SequenceType = sequenceType,
                            CompanyID = companyId,
                            FiscalYear = currentYear,
                            Prefix = prefix.ToUpper(),
                            Suffix = yearMonth, // YYYYMM to identify monthly sequences
                            CurrentNumber = 0,
                            IncrementBy = 1,
                            MinDigits = 5,
                            ResetOnNewYear = false, // We reset monthly, not yearly
                            IsActive = true,
                            Create_time = DateTime.Now
                        };
                        _sequenceRepository.Insert(sequence);
                    }

                    // Increment and get next number
                    sequence.CurrentNumber += sequence.IncrementBy;
                    sequence.LastUsedAt = DateTime.Now;

                    // Update sequence
                    _sequenceRepository.Update(sequence);

                    // Generate formatted number: RQ-202512-00001 or DS-202512-00001
                    var numberPart = sequence.CurrentNumber.ToString().PadLeft(sequence.MinDigits, '0');
                    generatedNumber = $"{sequence.Prefix}-{yearMonth}-{numberPart}";
                });

                return generatedNumber;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public string GetNextNumber(string sequenceType, long? companyId = null)
        {
            _semaphore.Wait();
            try
            {
                string generatedNumber = null;

                _sequenceRepository.UseTran(() =>
                {
                    var currentYear = DateTime.Now.Year;

                    // Find or create sequence
                    var predicate = Expressionable.Create<TreasuryNumberSequence>();
                    predicate = predicate.And(x => x.SequenceType == sequenceType);
                    predicate = predicate.And(x => x.FiscalYear == currentYear);
                    predicate = predicate.And(x => x.IsActive);

                    if (companyId.HasValue)
                    {
                        predicate = predicate.And(x => x.CompanyID == companyId.Value);
                    }
                    else
                    {
                        predicate = predicate.And(x => x.CompanyID == null);
                    }

                    var sequence = _sequenceRepository.GetFirst(predicate.ToExpression());

                    if (sequence == null)
                    {
                        // Create new sequence for this year
                        sequence = CreateNewSequence(sequenceType, companyId, currentYear);
                    }
                    else if (sequence.ResetOnNewYear && sequence.FiscalYear != currentYear)
                    {
                        // Reset sequence for new year
                        sequence.CurrentNumber = 0;
                        sequence.FiscalYear = currentYear;
                    }

                    // Increment and get next number
                    sequence.CurrentNumber += sequence.IncrementBy;
                    sequence.LastUsedAt = DateTime.Now;

                    // Update sequence
                    _sequenceRepository.Update(sequence);

                    // Generate formatted number
                    generatedNumber = FormatNumber(sequence);
                });

                return generatedNumber;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void InitializeSequence(string sequenceType, long? companyId, string prefix)
        {
            var currentYear = DateTime.Now.Year;

            var predicate = Expressionable.Create<TreasuryNumberSequence>();
            predicate = predicate.And(x => x.SequenceType == sequenceType);
            predicate = predicate.And(x => x.FiscalYear == currentYear);

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }
            else
            {
                predicate = predicate.And(x => x.CompanyID == null);
            }

            var exists = _sequenceRepository.Count(predicate.ToExpression()) > 0;
            if (!exists)
            {
                var sequence = new TreasuryNumberSequence
                {
                    Id = Guid.NewGuid(),
                    SequenceType = sequenceType,
                    CompanyID = companyId,
                    FiscalYear = currentYear,
                    Prefix = prefix,
                    CurrentNumber = 0,
                    IncrementBy = 1,
                    MinDigits = 6,
                    ResetOnNewYear = true,
                    IsActive = true,
                    Create_time = DateTime.Now
                };

                _sequenceRepository.Insert(sequence);
            }
        }

        private TreasuryNumberSequence CreateNewSequence(string sequenceType, long? companyId, int fiscalYear)
        {
            var prefix = GetDefaultPrefix(sequenceType);

            var sequence = new TreasuryNumberSequence
            {
                Id = Guid.NewGuid(),
                SequenceType = sequenceType,
                CompanyID = companyId,
                FiscalYear = fiscalYear,
                Prefix = prefix,
                CurrentNumber = 0,
                IncrementBy = 1,
                MinDigits = 6,
                ResetOnNewYear = true,
                IsActive = true,
                Create_time = DateTime.Now
            };

            _sequenceRepository.Insert(sequence);
            return sequence;
        }

        private string GetDefaultPrefix(string sequenceType)
        {
            return sequenceType switch
            {
                PAYMENT_SEQ => "PAY",
                RECEIPT_SEQ => "RCP",
                TRANSFER_SEQ => "TRF",
                CHEQUE_SEQ => "CHQ",
                RECONCILIATION_SEQ => "REC",
                _ => sequenceType
            };
        }

        private string FormatNumber(TreasuryNumberSequence sequence)
        {
            // Format: PREFIX-YYYY-NNNNNN
            // Example: PAY-2024-000001
            var numberPart = sequence.CurrentNumber.ToString().PadLeft(sequence.MinDigits, '0');
            var yearPart = sequence.FiscalYear.ToString();

            var result = $"{sequence.Prefix}-{yearPart}-{numberPart}";

            if (!string.IsNullOrEmpty(sequence.Suffix))
            {
                result += $"-{sequence.Suffix}";
            }

            return result;
        }
    }
}
