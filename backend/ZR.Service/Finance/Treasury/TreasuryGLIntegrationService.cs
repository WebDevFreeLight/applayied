using System;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance.Configuration;
using ZR.Service.IService.Finance.GL;
// Alias to resolve ambiguity between Finance.Enums.AccountType and Treasury.Enums.AccountType
using TreasuryAccountType = ZR.Model.Finance.Treasury.Enums.AccountType;
using PaymentHubPaymentType = ZR.Model.Finance.PaymentHub.Enums.PaymentType;

namespace ZR.Service.Finance.Treasury
{
   
    /// <summary>
    /// Treasury GL Account Configuration - إعدادات حسابات دفتر الأستاذ للخزينة
    /// </summary>
    public class TreasuryGLAccountConfig
    {
        public long? TenantId { get; set; }

        // Bank Account GL Mappings
        public Guid DefaultBankAccountGLId { get; set; }
        public Guid DefaultBankChargesGLId { get; set; }
        public Guid DefaultBankInterestIncomeGLId { get; set; }

        // Cash Account GL Mappings
        public Guid DefaultCashAccountGLId { get; set; }
        public Guid DefaultPettyCashGLId { get; set; }

        // Payment GL Mappings
        public Guid DefaultAccountsPayableGLId { get; set; }
        public Guid DefaultAccountsReceivableGLId { get; set; }

        // Cheque GL Mappings
        public Guid DefaultChequesIssuedGLId { get; set; }
        public Guid DefaultChequesReceivedGLId { get; set; }
        public Guid DefaultBouncedChequesGLId { get; set; }

        // Reconciliation GL Mappings
        public Guid DefaultReconciliationDifferenceGLId { get; set; }
        public Guid DefaultSuspenseAccountGLId { get; set; }
    }

    [AppService(ServiceType = typeof(ITreasuryGLIntegrationService), ServiceLifetime = LifeTime.Transient)]
    public class TreasuryGLIntegrationService : ITreasuryGLIntegrationService
    {
        private readonly IGLJournalService _journalService;
        private readonly BankAccountRepository _bankAccountRepository;
        private readonly CashAccountRepository _cashAccountRepository;
        private readonly TreasuryConfigRepository _configRepository;
        private readonly TreasuryGLAccountMappingRepository _glMappingRepository;
        private readonly ITreasuryValidationService _validationService;

        // Fallback GL Account mappings (used when no database config exists)
        private static TreasuryGLAccountConfig _defaultConfig = new();

        public TreasuryGLIntegrationService(
            IGLJournalService journalService,
            BankAccountRepository bankAccountRepository,
            CashAccountRepository cashAccountRepository,
            TreasuryConfigRepository configRepository,
            TreasuryGLAccountMappingRepository glMappingRepository,
            ITreasuryValidationService validationService)
        {
            _journalService = journalService;
            _bankAccountRepository = bankAccountRepository;
            _cashAccountRepository = cashAccountRepository;
            _configRepository = configRepository;
            _glMappingRepository = glMappingRepository;
            _validationService = validationService;
        }

        #region Payment GL Integration

        public GLJournal CreatePaymentJournal(Payment payment, string userName)
        {
            var journalDto = PreviewPaymentJournal(payment);

            // Create and post the journal automatically for proper GL integration
            var journal = _journalService.Create(journalDto, userName);
            _journalService.PostJournal(journal.Id, userName);

            return journal;
        }

        public GLJournal ReversePaymentJournal(Payment payment, string userName, string reason)
        {
            if (!payment.GLJournalId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_PAYMENT_GL_JOURNAL_MISSING)
                    .WithParameter("PaymentId", payment.Id);
            }

            var reverseDto = new ReverseJournalDto
            {
                Id = payment.GLJournalId.Value,
                ReversalDate = DateTime.Now,
                ReversalReason = reason ?? $"Reversal of payment {payment.PaymentNumber}"
            };

            return _journalService.ReverseJournal(reverseDto, userName);
        }

        public CreateJournalDto PreviewPaymentJournal(Payment payment)
        {
            var config = GetGLAccountConfig(null);
            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            string descriptionAr;
            string descriptionEn;

            switch (payment.PaymentType)
            {
                case PaymentHubPaymentType.VendorPayment:
                    // Outgoing payment: DR AP/Expense, CR Bank/Cash
                    descriptionAr = $"دفعة رقم {payment.PaymentNumber}";
                    descriptionEn = $"Payment {payment.PaymentNumber}";

                    // Debit: Accounts Payable or Counterparty account
                    entries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = config.DefaultAccountsPayableGLId,
                        Debit = payment.Amount,
                        Credit = 0,
                        DescriptionAr = $"دفعة إلى {payment.CounterpartyId}",
                        DescriptionEn = $"Payment to counterparty",
                        Currency = payment.Currency,
                        ExchangeRate = 1,
                        ReferenceNumber = payment.PaymentNumber
                    });

                    // Credit: Bank or Cash account
                    var sourceAccountGLId = GetAccountGLId(payment.SourceAccountId.Value);
                    entries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = sourceAccountGLId,
                        Debit = 0,
                        Credit = payment.Amount,
                        DescriptionAr = $"من الحساب {payment.SourceAccountId}",
                        DescriptionEn = $"From account {payment.SourceAccountId}",
                        Currency = payment.Currency,
                        ExchangeRate = 1,
                        ReferenceNumber = payment.PaymentNumber
                    });
                    break;

                case PaymentHubPaymentType.CustomerReceipt:
                    // Incoming receipt: DR Bank/Cash, CR AR/Revenue
                    descriptionAr = $"إيصال استلام رقم {payment.PaymentNumber}";
                    descriptionEn = $"Receipt {payment.PaymentNumber}";

                    // Debit: Bank or Cash account
                    var destAccountGLId = GetAccountGLId(payment.DestinationAccountId.Value);
                    entries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = destAccountGLId,
                        Debit = payment.Amount,
                        Credit = 0,
                        DescriptionAr = $"إلى الحساب {payment.DestinationAccountId}",
                        DescriptionEn = $"To account {payment.DestinationAccountId}",
                        Currency = payment.Currency,
                        ExchangeRate = 1,
                        ReferenceNumber = payment.PaymentNumber
                    });

                    // Credit: Accounts Receivable or Counterparty account
                    entries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = config.DefaultAccountsReceivableGLId,
                        Debit = 0,
                        Credit = payment.Amount,
                        DescriptionAr = $"من {payment.CounterpartyId}",
                        DescriptionEn = $"From counterparty",
                        Currency = payment.Currency,
                        ExchangeRate = 1,
                        ReferenceNumber = payment.PaymentNumber
                    });
                    break;

                case PaymentHubPaymentType.InternalTransfer:
                case PaymentHubPaymentType.IntercompanyTransfer:
                    // Internal transfer: DR Destination Account, CR Source Account
                    descriptionAr = $"تحويل داخلي رقم {payment.PaymentNumber}";
                    descriptionEn = $"Internal Transfer {payment.PaymentNumber}";

                    // Debit: Destination account
                    var toAccountGLId = GetAccountGLId(payment.DestinationAccountId.Value);
                    entries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = toAccountGLId,
                        Debit = payment.Amount,
                        Credit = 0,
                        DescriptionAr = $"تحويل إلى الحساب {payment.DestinationAccountId}",
                        DescriptionEn = $"Transfer to account {payment.DestinationAccountId}",
                        Currency = payment.Currency,
                        ExchangeRate = 1,
                        ReferenceNumber = payment.PaymentNumber
                    });

                    // Credit: Source account
                    var fromAccountGLId = GetAccountGLId(payment.SourceAccountId.Value);
                    entries.Add(new CreateJournalEntryDto
                    {
                        LineNumber = lineNumber++,
                        AccountId = fromAccountGLId,
                        Debit = 0,
                        Credit = payment.Amount,
                        DescriptionAr = $"تحويل من الحساب {payment.SourceAccountId}",
                        DescriptionEn = $"Transfer from account {payment.SourceAccountId}",
                        Currency = payment.Currency,
                        ExchangeRate = 1,
                        ReferenceNumber = payment.PaymentNumber
                    });
                    break;

                default:
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_PAYMENT_TYPE_UNSUPPORTED)
                        .WithParameter("PaymentType", payment.PaymentType.ToString());
            }

            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(payment.PaymentDate, null);

            return new CreateJournalDto
            {
                JournalDate = payment.PaymentDate,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn,
                Source = "Treasury",
                SourceDocumentType = "Payment",
                SourceDocumentId = payment.Id,
                Currency = payment.Currency,
                ReferenceNumber = payment.PaymentNumber,
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = entries
            };
        }

        #endregion

        #region Cheque GL Integration

        public GLJournal CreateChequeIssuedJournal(Cheque cheque, string userName)
        {
            // Validate that GL account mappings are configured before creating journal
            var config = GetGLAccountConfig(cheque.CompanyID);
            if (config.DefaultAccountsPayableGLId == Guid.Empty || config.DefaultChequesIssuedGLId == Guid.Empty)
            {
                // GL accounts not configured - skip journal creation but log warning
                // This allows cheque creation to succeed even without GL integration
                Console.WriteLine($"Warning: GL account mappings not configured for cheque journal. ChequesIssued={config.DefaultChequesIssuedGLId}, AccountsPayable={config.DefaultAccountsPayableGLId}");
                return null;
            }

            var journalDto = PreviewChequeJournal(cheque, ChequeStatus.Issued);

            var journal = _journalService.Create(journalDto, userName);
            _journalService.PostJournal(journal.Id, userName);

            return journal;
        }

        public GLJournal CreateChequeClearedJournal(Cheque cheque, string userName)
        {
            // Validate that GL account mappings are configured before creating journal
            var config = GetGLAccountConfig(cheque.CompanyID);
            if (config.DefaultChequesIssuedGLId == Guid.Empty)
            {
                // GL accounts not configured - skip journal creation
                Console.WriteLine($"Warning: GL account mappings not configured for cheque clearing journal. ChequesIssued={config.DefaultChequesIssuedGLId}");
                return null;
            }

            var journalDto = PreviewChequeJournal(cheque, ChequeStatus.Cleared);

            var journal = _journalService.Create(journalDto, userName);
            _journalService.PostJournal(journal.Id, userName);

            return journal;
        }

        public Guid CreateChequeBouncedJournal(Cheque cheque, string userName)
        {
            // Validate that GL account mappings are configured before creating journal
            var config = GetGLAccountConfig(cheque.CompanyID);
            if (config.DefaultBouncedChequesGLId == Guid.Empty || config.DefaultChequesIssuedGLId == Guid.Empty)
            {
                // GL accounts not configured - skip journal creation
                Console.WriteLine($"Warning: GL account mappings not configured for bounced cheque journal. BouncedCheques={config.DefaultBouncedChequesGLId}, ChequesIssued={config.DefaultChequesIssuedGLId}");
                return Guid.Empty;
            }

            var journalDto = PreviewChequeJournal(cheque, ChequeStatus.Bounced);

            var journal = _journalService.Create(journalDto, userName);
            _journalService.PostJournal(journal.Id, userName);

            return journal.Id;
        }

        /// <summary>
        /// Create GL Journal for bank fees on bounced cheque - إنشاء قيد محاسبي لرسوم البنك
        /// </summary>
        public Guid CreateBankFeesJournal(Cheque cheque, decimal bankFees, string userName)
        {
            if (bankFees <= 0)
            {
                return Guid.Empty;
            }

            var config = GetGLAccountConfig(cheque.CompanyID);

            // Validate that GL account mappings are configured
            if (config.DefaultBankChargesGLId == Guid.Empty)
            {
                Console.WriteLine($"Warning: GL account mappings not configured for bank fees journal. BankCharges={config.DefaultBankChargesGLId}");
                return Guid.Empty;
            }

            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            var bankAccount = _bankAccountRepository.GetById(cheque.BankAccountId);
            if (bankAccount == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                    .WithParameter("BankAccountId", cheque.BankAccountId);
            }

            var bankGLId = GetAccountGLId(cheque.BankAccountId);
            if (bankGLId == Guid.Empty)
            {
                Console.WriteLine($"Warning: Bank account GL mapping not configured. BankAccountId={cheque.BankAccountId}");
                return Guid.Empty;
            }

            string descriptionAr = $"رسوم بنكية - شيك مرتجع {cheque.ChequeNumber}";
            string descriptionEn = $"Bank fees - bounced cheque {cheque.ChequeNumber}";

            // Debit: Bank Charges Expense
            entries.Add(new CreateJournalEntryDto
            {
                LineNumber = lineNumber++,
                AccountId = config.DefaultBankChargesGLId,
                Debit = bankFees,
                Credit = 0,
                DescriptionAr = $"رسوم بنكية - شيك {cheque.ChequeNumber}",
                DescriptionEn = $"Bank charges - cheque {cheque.ChequeNumber}",
                Currency = bankAccount.Currency,
                ExchangeRate = 1,
                ReferenceNumber = cheque.ChequeNumber
            });

            // Credit: Bank Account
            entries.Add(new CreateJournalEntryDto
            {
                LineNumber = lineNumber++,
                AccountId = bankGLId,
                Debit = 0,
                Credit = bankFees,
                DescriptionAr = $"خصم رسوم بنكية - {cheque.ChequeNumber}",
                DescriptionEn = $"Bank charges deducted - {cheque.ChequeNumber}",
                Currency = bankAccount.Currency,
                ExchangeRate = 1,
                ReferenceNumber = cheque.ChequeNumber
            });

            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(DateTime.Now, null);

            var journalDto = new CreateJournalDto
            {
                JournalDate = DateTime.Now,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn,
                Source = "Treasury",
                SourceDocumentType = "ChequeBankFees",
                SourceDocumentId = cheque.Id,
                Currency = bankAccount.Currency,
                ReferenceNumber = $"FEE-{cheque.ChequeNumber}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = entries
            };

            var journal = _journalService.Create(journalDto, userName);
            _journalService.PostJournal(journal.Id, userName);

            return journal.Id;
        }

        public CreateJournalDto PreviewChequeJournal(Cheque cheque, ChequeStatus targetStatus)
        {
            var config = GetGLAccountConfig(null);
            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            var bankAccount = _bankAccountRepository.GetById(cheque.BankAccountId);
            if (bankAccount == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                    .WithParameter("BankAccountId", cheque.BankAccountId);
            }

            string descriptionAr;
            string descriptionEn;

            switch (targetStatus)
            {
                case ChequeStatus.Issued:
                    if (cheque.ChequeType == ChequeType.Outgoing)
                    {
                        // OUTGOING: When cheque is issued: DR Expense/AP, CR Cheques Issued (liability)
                        descriptionAr = $"إصدار شيك رقم {cheque.ChequeNumber} للمستفيد {cheque.PayeeName}";
                        descriptionEn = $"Cheque {cheque.ChequeNumber} issued to {cheque.PayeeName}";

                        // Debit: Expense or Accounts Payable (depends on purpose)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultAccountsPayableGLId,
                            Debit = cheque.Amount,
                            Credit = 0,
                            DescriptionAr = $"شيك للمستفيد {cheque.PayeeName}",
                            DescriptionEn = $"Cheque to {cheque.PayeeName}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });

                        // Credit: Cheques Issued (liability - post-dated cheques)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultChequesIssuedGLId,
                            Debit = 0,
                            Credit = cheque.Amount,
                            DescriptionAr = $"شيكات صادرة - {cheque.ChequeNumber}",
                            DescriptionEn = $"Cheques Issued - {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });
                    }
                    else // Incoming cheque
                    {
                        // INCOMING: When cheque is received: DR Cheques in Transit (asset), CR Accounts Receivable
                        descriptionAr = $"استلام شيك رقم {cheque.ChequeNumber} من {cheque.DrawerName ?? "عميل"}";
                        descriptionEn = $"Received cheque {cheque.ChequeNumber} from {cheque.DrawerName ?? "customer"}";

                        // Debit: Cheques in Transit (Asset)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultChequesReceivedGLId,
                            Debit = cheque.Amount,
                            Credit = 0,
                            DescriptionAr = $"شيك وارد - {cheque.ChequeNumber}",
                            DescriptionEn = $"Cheque received - {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });

                        // Credit: Accounts Receivable
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultAccountsReceivableGLId,
                            Debit = 0,
                            Credit = cheque.Amount,
                            DescriptionAr = $"تحصيل من {cheque.DrawerName ?? "عميل"}",
                            DescriptionEn = $"Collection from {cheque.DrawerName ?? "customer"}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });
                    }
                    break;

                case ChequeStatus.Cleared:
                    if (cheque.ChequeType == ChequeType.Outgoing)
                    {
                        // OUTGOING: When cheque clears: DR Cheques Issued, CR Bank Account
                        descriptionAr = $"تسديد شيك رقم {cheque.ChequeNumber}";
                        descriptionEn = $"Cheque {cheque.ChequeNumber} cleared";

                        // Debit: Cheques Issued (clears the liability)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultChequesIssuedGLId,
                            Debit = cheque.Amount,
                            Credit = 0,
                            DescriptionAr = $"تسديد شيك {cheque.ChequeNumber}",
                            DescriptionEn = $"Clearing cheque {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });

                        // Credit: Bank Account
                        var bankGLId = GetAccountGLId(cheque.BankAccountId);
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = bankGLId,
                            Debit = 0,
                            Credit = cheque.Amount,
                            DescriptionAr = $"خصم من البنك - شيك {cheque.ChequeNumber}",
                            DescriptionEn = $"Bank deduction - cheque {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });
                    }
                    else // Incoming cheque
                    {
                        // INCOMING: When cheque clears: DR Bank Account, CR Cheques in Transit
                        descriptionAr = $"تصفية شيك رقم {cheque.ChequeNumber}";
                        descriptionEn = $"Clearing cheque {cheque.ChequeNumber}";

                        // Debit: Bank Account
                        var bankGLId = GetAccountGLId(cheque.BankAccountId);
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = bankGLId,
                            Debit = cheque.Amount,
                            Credit = 0,
                            DescriptionAr = $"إضافة إلى البنك - شيك {cheque.ChequeNumber}",
                            DescriptionEn = $"Bank addition - cheque {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });

                        // Credit: Cheques in Transit
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultChequesReceivedGLId,
                            Debit = 0,
                            Credit = cheque.Amount,
                            DescriptionAr = $"تصفية شيك وارد - {cheque.ChequeNumber}",
                            DescriptionEn = $"Clearing received cheque - {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });
                    }
                    break;

                case ChequeStatus.Bounced:
                    if (cheque.ChequeType == ChequeType.Outgoing)
                    {
                        // OUTGOING: When cheque bounces: DR Bounced Cheques (asset), CR Cheques Issued
                        descriptionAr = $"شيك مرتجع رقم {cheque.ChequeNumber}";
                        descriptionEn = $"Bounced cheque {cheque.ChequeNumber}";

                        // Debit: Bounced Cheques (receivable from payee)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultBouncedChequesGLId,
                            Debit = cheque.Amount,
                            Credit = 0,
                            DescriptionAr = $"شيك مرتجع من {cheque.PayeeName}",
                            DescriptionEn = $"Bounced cheque from {cheque.PayeeName}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });

                        // Credit: Cheques Issued (reverses the liability)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultChequesIssuedGLId,
                            Debit = 0,
                            Credit = cheque.Amount,
                            DescriptionAr = $"إلغاء شيك مرتجع {cheque.ChequeNumber}",
                            DescriptionEn = $"Reverse bounced cheque {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });
                    }
                    else // Incoming cheque
                    {
                        // INCOMING: When cheque bounces: DR Accounts Receivable, CR Cheques in Transit
                        descriptionAr = $"شيك مرتجع رقم {cheque.ChequeNumber}";
                        descriptionEn = $"Bounced cheque {cheque.ChequeNumber}";

                        // Debit: Accounts Receivable (restore the debt)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultAccountsReceivableGLId,
                            Debit = cheque.Amount,
                            Credit = 0,
                            DescriptionAr = $"إلغاء شيك مرتجع من {cheque.DrawerName ?? "عميل"}",
                            DescriptionEn = $"Reverse bounced cheque from {cheque.DrawerName ?? "customer"}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });

                        // Credit: Cheques in Transit (reverse)
                        entries.Add(new CreateJournalEntryDto
                        {
                            LineNumber = lineNumber++,
                            AccountId = config.DefaultChequesReceivedGLId,
                            Debit = 0,
                            Credit = cheque.Amount,
                            DescriptionAr = $"إلغاء شيك وارد - {cheque.ChequeNumber}",
                            DescriptionEn = $"Reverse received cheque - {cheque.ChequeNumber}",
                            Currency = bankAccount.Currency,
                            ExchangeRate = 1,
                            ReferenceNumber = cheque.ChequeNumber
                        });
                    }
                    break;

                default:
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_GL_JOURNAL_NOT_REQUIRED_FOR_CHEQUE_STATUS)
                        .WithParameter("Status", targetStatus.ToString());
            }

            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(cheque.IssueDate, null);

            return new CreateJournalDto
            {
                JournalDate = targetStatus == ChequeStatus.Issued ? cheque.IssueDate : DateTime.Now,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn,
                Source = "Treasury",
                SourceDocumentType = "Cheque",
                SourceDocumentId = cheque.Id,
                Currency = bankAccount.Currency,
                ReferenceNumber = cheque.ChequeNumber,
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = entries
            };
        }

        #endregion

        #region Bank Reconciliation GL Integration

        public GLJournal CreateReconciliationAdjustmentJournal(BankReconciliation reconciliation, decimal adjustmentAmount, string description, string userName)
        {
            var config = GetGLAccountConfig(null);
            var entries = new List<CreateJournalEntryDto>();
            int lineNumber = 1;

            var bankAccount = _bankAccountRepository.GetById(reconciliation.BankAccountId);
            if (bankAccount == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                    .WithParameter("BankAccountId", reconciliation.BankAccountId);
            }

            var bankGLId = GetAccountGLId(reconciliation.BankAccountId);

            string descriptionAr = $"تسوية بنكية - فرق {adjustmentAmount}";
            string descriptionEn = description ?? $"Bank reconciliation adjustment - {adjustmentAmount}";

            if (adjustmentAmount > 0)
            {
                // Bank statement shows more than ledger - DR Bank, CR Difference account
                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = bankGLId,
                    Debit = Math.Abs(adjustmentAmount),
                    Credit = 0,
                    DescriptionAr = "تعديل تسوية بنكية - زيادة",
                    DescriptionEn = "Bank reconciliation adjustment - increase",
                    Currency = bankAccount.Currency,
                    ExchangeRate = 1,
                    ReferenceNumber = $"RECON-{reconciliation.Id}"
                });

                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = config.DefaultReconciliationDifferenceGLId,
                    Debit = 0,
                    Credit = Math.Abs(adjustmentAmount),
                    DescriptionAr = "فرق تسوية بنكية",
                    DescriptionEn = "Bank reconciliation difference",
                    Currency = bankAccount.Currency,
                    ExchangeRate = 1,
                    ReferenceNumber = $"RECON-{reconciliation.Id}"
                });
            }
            else
            {
                // Bank statement shows less than ledger - DR Difference account, CR Bank
                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = config.DefaultReconciliationDifferenceGLId,
                    Debit = Math.Abs(adjustmentAmount),
                    Credit = 0,
                    DescriptionAr = "فرق تسوية بنكية",
                    DescriptionEn = "Bank reconciliation difference",
                    Currency = bankAccount.Currency,
                    ExchangeRate = 1,
                    ReferenceNumber = $"RECON-{reconciliation.Id}"
                });

                entries.Add(new CreateJournalEntryDto
                {
                    LineNumber = lineNumber++,
                    AccountId = bankGLId,
                    Debit = 0,
                    Credit = Math.Abs(adjustmentAmount),
                    DescriptionAr = "تعديل تسوية بنكية - نقص",
                    DescriptionEn = "Bank reconciliation adjustment - decrease",
                    Currency = bankAccount.Currency,
                    ExchangeRate = 1,
                    ReferenceNumber = $"RECON-{reconciliation.Id}"
                });
            }

            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(reconciliation.PeriodEnd, null);

            var journalDto = new CreateJournalDto
            {
                JournalDate = reconciliation.PeriodEnd,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn,
                Source = "Treasury",
                SourceDocumentType = "BankReconciliation",
                SourceDocumentId = reconciliation.Id,
                Currency = bankAccount.Currency,
                ReferenceNumber = $"RECON-{reconciliation.Id}",
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Entries = entries
            };

            var journal = _journalService.Create(journalDto, userName);
            _journalService.PostJournal(journal.Id, userName);

            return journal;
        }

        #endregion

        #region Configuration

        public TreasuryGLAccountConfig GetGLAccountConfig(long? companyId = null)
        {
            // Load configuration from database based on tenant
            //var companyId = tenantId.HasValue ? (long?)tenantId.Value.GetHashCode() : null;
            var config = new TreasuryGLAccountConfig { TenantId = companyId };

            // Load each mapping type from database
            config.DefaultBankAccountGLId = GetGLMapping(GLMappingType.DefaultBankAccount, companyId);
            config.DefaultBankChargesGLId = GetGLMapping(GLMappingType.BankCharges, companyId);
            config.DefaultBankInterestIncomeGLId = GetGLMapping(GLMappingType.BankInterestIncome, companyId);
            config.DefaultCashAccountGLId = GetGLMapping(GLMappingType.DefaultCashAccount, companyId);
            config.DefaultPettyCashGLId = GetGLMapping(GLMappingType.PettyCash, companyId);
            config.DefaultAccountsPayableGLId = GetGLMapping(GLMappingType.AccountsPayable, companyId);
            config.DefaultAccountsReceivableGLId = GetGLMapping(GLMappingType.AccountsReceivable, companyId);
            config.DefaultChequesIssuedGLId = GetGLMapping(GLMappingType.ChequesIssued, companyId);
            config.DefaultChequesReceivedGLId = GetGLMapping(GLMappingType.ChequesReceived, companyId);
            config.DefaultBouncedChequesGLId = GetGLMapping(GLMappingType.BouncedCheques, companyId);
            config.DefaultReconciliationDifferenceGLId = GetGLMapping(GLMappingType.ReconciliationDifference, companyId);
            config.DefaultSuspenseAccountGLId = GetGLMapping(GLMappingType.SuspenseAccount, companyId);

            return config;
        }

        public void SetGLAccountConfig(TreasuryGLAccountConfig config)
        {
            var companyId = config.TenantId.HasValue ? (long?)config.TenantId.Value.GetHashCode() : null;

            // Save each mapping to database
            SetGLMapping(GLMappingType.DefaultBankAccount, config.DefaultBankAccountGLId, companyId);
            SetGLMapping(GLMappingType.BankCharges, config.DefaultBankChargesGLId, companyId);
            SetGLMapping(GLMappingType.BankInterestIncome, config.DefaultBankInterestIncomeGLId, companyId);
            SetGLMapping(GLMappingType.DefaultCashAccount, config.DefaultCashAccountGLId, companyId);
            SetGLMapping(GLMappingType.PettyCash, config.DefaultPettyCashGLId, companyId);
            SetGLMapping(GLMappingType.AccountsPayable, config.DefaultAccountsPayableGLId, companyId);
            SetGLMapping(GLMappingType.AccountsReceivable, config.DefaultAccountsReceivableGLId, companyId);
            SetGLMapping(GLMappingType.ChequesIssued, config.DefaultChequesIssuedGLId, companyId);
            SetGLMapping(GLMappingType.ChequesReceived, config.DefaultChequesReceivedGLId, companyId);
            SetGLMapping(GLMappingType.BouncedCheques, config.DefaultBouncedChequesGLId, companyId);
            SetGLMapping(GLMappingType.ReconciliationDifference, config.DefaultReconciliationDifferenceGLId, companyId);
            SetGLMapping(GLMappingType.SuspenseAccount, config.DefaultSuspenseAccountGLId, companyId);
        }

        /// <summary>
        /// Get GL mapping from database with fallback - الحصول على تعيين دفتر الأستاذ من قاعدة البيانات
        /// </summary>
        private Guid GetGLMapping(string mappingType, long? companyId, string currency = null)
        {
            var predicate = Expressionable.Create<TreasuryGLAccountMapping>();
            predicate = predicate.And(x => x.MappingType == mappingType);
            predicate = predicate.And(x => x.IsActive);
            // Use DateTime.Now instead of DateTime.UtcNow for consistency with stored dates
            predicate = predicate.And(x => x.EffectiveFrom <= DateTime.Now);
            predicate = predicate.And(x => x.EffectiveTo == null || x.EffectiveTo >= DateTime.Now);

            // Company filter: match specific company or global (null or 0)
            if (companyId.HasValue && companyId.Value > 0)
            {
                // Match specific company OR global mappings (null or 0)
                predicate = predicate.And(x => x.CompanyID == null || x.CompanyID == 0 || x.CompanyID == companyId.Value);
            }
            else
            {
                // Match global mappings only (null or 0)
                predicate = predicate.And(x => x.CompanyID == null || x.CompanyID == 0);
            }

            // Currency filter if specified
            if (!string.IsNullOrEmpty(currency))
            {
                predicate = predicate.And(x => x.Currency == null || x.Currency == currency);
            }

            var mappings = _glMappingRepository.GetList(predicate.ToExpression())
                .OrderBy(x => x.Priority)
                .ToList();

            // Return the first matching mapping or default
            var mapping = mappings.FirstOrDefault();
            if (mapping != null)
            {
                return mapping.GLAccountId;
            }

            // Fallback to static default config
            return GetDefaultFromStaticConfig(mappingType);
        }

        /// <summary>
        /// Set GL mapping in database - تعيين تعيين دفتر الأستاذ في قاعدة البيانات
        /// </summary>
        private void SetGLMapping(string mappingType, Guid glAccountId, long? companyId, string currency = null)
        {
            if (glAccountId == Guid.Empty)
            {
                return; // Don't save empty mappings
            }

            // Find existing mapping
            var predicate = Expressionable.Create<TreasuryGLAccountMapping>();
            predicate = predicate.And(x => x.MappingType == mappingType);
            predicate = predicate.And(x => x.CompanyID == companyId);
            predicate = predicate.And(x => x.Currency == currency);

            var existing = _glMappingRepository.GetList(predicate.ToExpression()).FirstOrDefault();

            if (existing != null)
            {
                // Update existing mapping
                existing.GLAccountId = glAccountId;
                existing.Update_time = DateTime.Now;
                _glMappingRepository.Update(existing);
            }
            else
            {
                // Create new mapping
                var mapping = new TreasuryGLAccountMapping
                {
                    Id = Guid.NewGuid(),
                    MappingType = mappingType,
                    GLAccountId = glAccountId,
                    CompanyID = companyId,
                    Currency = currency,
                    IsActive = true,
                    Priority = 100,
                    EffectiveFrom = DateTime.UtcNow,
                    Create_time = DateTime.Now
                };
                _glMappingRepository.Insert(mapping);
            }
        }

        /// <summary>
        /// Fallback to static default config - العودة إلى الإعدادات الافتراضية الثابتة
        /// </summary>
        private Guid GetDefaultFromStaticConfig(string mappingType)
        {
            return mappingType switch
            {
                GLMappingType.DefaultBankAccount => _defaultConfig.DefaultBankAccountGLId,
                GLMappingType.BankCharges => _defaultConfig.DefaultBankChargesGLId,
                GLMappingType.BankInterestIncome => _defaultConfig.DefaultBankInterestIncomeGLId,
                GLMappingType.DefaultCashAccount => _defaultConfig.DefaultCashAccountGLId,
                GLMappingType.PettyCash => _defaultConfig.DefaultPettyCashGLId,
                GLMappingType.AccountsPayable => _defaultConfig.DefaultAccountsPayableGLId,
                GLMappingType.AccountsReceivable => _defaultConfig.DefaultAccountsReceivableGLId,
                GLMappingType.ChequesIssued => _defaultConfig.DefaultChequesIssuedGLId,
                GLMappingType.ChequesReceived => _defaultConfig.DefaultChequesReceivedGLId,
                GLMappingType.BouncedCheques => _defaultConfig.DefaultBouncedChequesGLId,
                GLMappingType.ReconciliationDifference => _defaultConfig.DefaultReconciliationDifferenceGLId,
                GLMappingType.SuspenseAccount => _defaultConfig.DefaultSuspenseAccountGLId,
                _ => Guid.Empty
            };
        }

        #endregion

        #region Helper Methods

        private Guid GetAccountGLId(Guid treasuryAccountId)
        {
            // First try bank account
            var bankAccount = _bankAccountRepository.GetById(treasuryAccountId);
            if (bankAccount != null)
            {
                // Use the linked GL Account ID from the bank account (mandatory field)
                if (bankAccount.GLAccountId.HasValue && bankAccount.GLAccountId.Value != Guid.Empty)
                {
                    return bankAccount.GLAccountId.Value;
                }
                // Fallback to default bank GL account if not set (legacy data)
                var defaultBankGL = GetGLAccountConfig(null).DefaultBankAccountGLId;
                if (defaultBankGL != Guid.Empty)
                {
                    return defaultBankGL;
                }
                throw new AppException(ExceptionCode.FINANCE_TREASURY_TREASURY_ACCOUNT_GL_LINK_MISSING)
                    .WithParameter("Identifier", bankAccount.AccountNumber)
                    .WithParameter("TreasuryAccountId", treasuryAccountId);
            }

            // Then try cash account
            var cashAccount = _cashAccountRepository.GetById(treasuryAccountId);
            if (cashAccount != null)
            {
                // Use the linked GL Account ID from the cash account (mandatory field)
                if (cashAccount.GLAccountId.HasValue && cashAccount.GLAccountId.Value != Guid.Empty)
                {
                    return cashAccount.GLAccountId.Value;
                }
                // Fallback to default cash GL account if not set (legacy data)
                var config = GetGLAccountConfig(null);
                var defaultCashGL = cashAccount.Type == TreasuryAccountType.PettyCash
                    ? config.DefaultPettyCashGLId
                    : config.DefaultCashAccountGLId;
                if (defaultCashGL != Guid.Empty)
                {
                    return defaultCashGL;
                }
                throw new AppException(ExceptionCode.FINANCE_TREASURY_TREASURY_ACCOUNT_GL_LINK_MISSING)
                    .WithParameter("Identifier", cashAccount.Code)
                    .WithParameter("TreasuryAccountId", treasuryAccountId);
            }

            throw new AppException(ExceptionCode.FINANCE_TREASURY_TREASURY_ACCOUNT_NOT_FOUND_FOR_ID)
                .WithParameter("TreasuryAccountId", treasuryAccountId);
        }

        #endregion
    }
}
