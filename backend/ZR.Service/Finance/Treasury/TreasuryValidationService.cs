using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using SqlSugar;
using ZR.Model.Finance.Exceptions;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.PaymentHub.Dto;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Repository.Finance.PaymentHub;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance.Configuration;
using ZR.Service.IService.Finance.GL;
using PaymentStatus = ZR.Model.Finance.PaymentHub.Enums.PaymentStatus;
using PaymentType = ZR.Model.Finance.PaymentHub.Enums.PaymentType;
using AccountNature = ZR.Model.Finance.Enums.AccountNature;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury Validation Service Interface - واجهة خدمة التحقق من صحة الخزينة
    /// </summary>
    public interface ITreasuryValidationService
    {
        // Payment Validations
        // TODO: Re-enable when CreatePaymentDto property names are aligned with PaymentHub Payment model
        // void ValidateCreatePayment(CreatePaymentDto dto);
        // TODO: Re-enable when UpdatePaymentDto is created in PaymentHub
        // void ValidateUpdatePayment(UpdatePaymentDto dto, Payment existingPayment);
        void ValidateDeletePayment(Payment payment);
        void ValidateSubmitPayment(Payment payment);
        void ValidateApprovePayment(Payment payment);
        void ValidatePostPayment(Payment payment);
        void ValidateCancelPayment(Payment payment);
        void ValidatePaymentAllocation(AllocatePaymentDto dto, Payment payment);

        // Cheque Validations
        void ValidateCreateCheque(CreateChequeDto dto);
        void ValidateUpdateCheque(UpdateChequeDto dto, Cheque existingCheque);
        void ValidateDeleteCheque(Cheque cheque);
        void ValidateChequeStatusChange(Cheque cheque, ChequeStatus newStatus);

        // Bank Account Validations
        void ValidateCreateBankAccount(CreateBankAccountDto dto);
        void ValidateUpdateBankAccount(UpdateBankAccountDto dto, BankAccount existingAccount);
        void ValidateDeleteBankAccount(Guid bankAccountId);
        void ValidateIBAN(string iban);
        void ValidateSwiftCode(string swiftCode);

        // Cash Account Validations
        void ValidateCreateCashAccount(CreateCashAccountDto dto);
        void ValidateUpdateCashAccount(UpdateCashAccountDto dto, CashAccount existingAccount);
        void ValidateDeleteCashAccount(Guid cashAccountId);

        // Bank Statement Validations
        void ValidateCreateBankStatement(CreateBankStatementDto dto);
        void ValidateBankStatementImport(UploadBankStatementDto dto);

        // Bank Reconciliation Validations
        void ValidateCreateReconciliation(CreateReconciliationDto dto);
        void ValidateCompleteReconciliation(BankReconciliation reconciliation);
        void ValidateMatchReconciliationLine(MatchReconciliationLineDto dto);

        // Balance Validations
        void ValidateSufficientBalance(Guid accountId, decimal amount, bool isBankAccount);
        void ValidateAvailableBalance(Guid accountId, decimal amount, bool isBankAccount);
        void ValidateNegativeBalanceAllowed(Guid accountId, bool isBankAccount);

        // Fiscal Period Validations
        void ValidateFiscalPeriodOpen(DateTime transactionDate, long? tenantId = null);
        (int FiscalYear, int FiscalPeriod) GetFiscalPeriodForDate(DateTime date, long? tenantId = null);

        // Intercompany Validations
        void ValidateIntercompanyTransfer(Guid fromAccountId, Guid toAccountId);
        void ValidateCompanyIdMatch(Guid accountId, long? expectedCompanyId);

        // Reversal Validations
        void ValidateReversePayment(Payment payment);
        void ValidatePaymentNotAlreadyReversed(Payment payment);

        // Idempotency Validations
        bool CheckIdempotencyKey(string idempotencyKey, TreasuryEntityType entityType);

        // Approval Validations
        void ValidateApprovalThreshold(decimal amount, string userId);

        // Attachment Validations (Phase 1.7)
        void ValidateChequeAttachment(Cheque cheque, bool requireAttachment = true);
        void ValidateReconciliationLineAttachment(Guid lineId, bool requireAttachment = false);
        void ValidateChequeAttachmentRequired(CreateChequeDto dto, ChequeType chequeType);
        bool IsAttachmentRequired(TreasuryEntityType entityType, string operationType = null);

        // Future Date Validation
        void ValidateFutureDate(DateTime transactionDate, bool allowFuture = false);

        // Cheque Lifecycle Validation (Full)
        void ValidateChequeLifecycleTransition(Cheque cheque, ChequeStatus newStatus);

        // Cash Account Cheque Validation
        void ValidateCashAccountAcceptsCheques(Guid cashAccountId, bool isAcceptingCheque);

        // Reconciliation Tolerance Validation (requirement #31)
        void ValidateReconciliationTolerance(BankReconciliation reconciliation, decimal toleranceAmount);
    }

    [AppService(ServiceType = typeof(ITreasuryValidationService), ServiceLifetime = LifeTime.Transient)]
    public class TreasuryValidationService : ITreasuryValidationService
    {
        private readonly PaymentHubRepository _paymentRepository;
        private readonly ChequeRepository _chequeRepository;
        private readonly BankAccountRepository _bankAccountRepository;
        private readonly CashAccountRepository _cashAccountRepository;
        private readonly BankStatementRepository _bankStatementRepository;
        private readonly BankReconciliationRepository _reconciliationRepository;
        private readonly PaymentAllocationRepository _allocationRepository;
        private readonly IGLPeriodClosingService _periodClosingService;
        private readonly ApprovalLimitRepository _approvalLimitRepository;
        private readonly IGLAccountService _glAccountService;

        // IBAN patterns for GCC countries
        private static readonly Dictionary<string, string> IBANPatterns = new()
        {
            { "SA", @"^SA\d{2}\d{2}[A-Z0-9]{18}$" },  // Saudi Arabia: SA + 2 check + 2 bank + 18 alphanumeric
            { "AE", @"^AE\d{2}\d{3}\d{16}$" },       // UAE: AE + 2 check + 3 bank + 16 digits
            { "KW", @"^KW\d{2}[A-Z]{4}\d{22}$" },    // Kuwait: KW + 2 check + 4 alpha + 22 digits
            { "BH", @"^BH\d{2}[A-Z]{4}[A-Z0-9]{14}$" }, // Bahrain: BH + 2 check + 4 alpha + 14 alphanumeric
            { "QA", @"^QA\d{2}[A-Z]{4}[A-Z0-9]{21}$" }, // Qatar: QA + 2 check + 4 alpha + 21 alphanumeric
            { "OM", @"^OM\d{2}\d{3}[A-Z0-9]{16}$" }  // Oman: OM + 2 check + 3 digits + 16 alphanumeric
        };

        // SWIFT/BIC code pattern: 8 or 11 characters (bank code 4, country 2, location 2, branch 3 optional)
        private static readonly Regex SwiftPattern = new(@"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", RegexOptions.Compiled);

        public TreasuryValidationService(
            PaymentHubRepository paymentRepository,
            ChequeRepository chequeRepository,
            BankAccountRepository bankAccountRepository,
            CashAccountRepository cashAccountRepository,
            BankStatementRepository bankStatementRepository,
            BankReconciliationRepository reconciliationRepository,
            PaymentAllocationRepository allocationRepository,
            IGLPeriodClosingService periodClosingService,
            ApprovalLimitRepository approvalLimitRepository,
            IGLAccountService glAccountService)
        {
            _paymentRepository = paymentRepository;
            _chequeRepository = chequeRepository;
            _bankAccountRepository = bankAccountRepository;
            _cashAccountRepository = cashAccountRepository;
            _bankStatementRepository = bankStatementRepository;
            _reconciliationRepository = reconciliationRepository;
            _allocationRepository = allocationRepository;
            _periodClosingService = periodClosingService;
            _approvalLimitRepository = approvalLimitRepository;
            _glAccountService = glAccountService;
        }

        private static void ThrowValidationErrors(List<string> errors)
        {
            throw new AppException(ExceptionCode.FINANCE_TREASURY_VALIDATION_FAILED)
                .WithParameter("Errors", string.Join("; ", errors ?? new List<string>()));
        }

        #region Payment Validations

        // TODO: Re-enable when CreatePaymentDto property names are aligned with PaymentHub Payment model
        /*public void ValidateCreatePayment(CreatePaymentDto dto)
        {
            var errors = new List<string>();

            // Amount > 0
            if (dto.Amount <= 0)
            {
                errors.Add("Payment amount must be greater than zero - مبلغ الدفعة يجب أن يكون أكبر من صفر");
            }

            // Currency is required (3 characters)
            if (string.IsNullOrWhiteSpace(dto.Currency) || dto.Currency.Length != 3)
            {
                errors.Add("Valid currency code (3 characters) is required - رمز العملة المكون من 3 أحرف مطلوب");
            }

            // Payment date validation
            if (dto.PaymentDate == default)
            {
                errors.Add("Payment date is required - تاريخ الدفعة مطلوب");
            }

            // Validate fiscal period is open
            if (dto.PaymentDate != default)
            {
                try
                {
                    ValidateFiscalPeriodOpen(dto.PaymentDate, dto.TenantId);
                }
                catch (PeriodClosedException ex)
                {
                    errors.Add($"Fiscal period is closed - الفترة المالية مغلقة: {ex.Message}");
                }
            }

            // Validate account requirements based on payment type
            switch (dto.PaymentType)
            {
                case PaymentType.Payment:
                    // Outgoing payment requires FromAccount
                    if (!dto.FromAccountId.HasValue)
                    {
                        errors.Add("Source account is required for outgoing payments - الحساب المصدر مطلوب للدفعات الصادرة");
                    }
                    else
                    {
                        ValidateAccountExists(dto.FromAccountId.Value, errors, "Source account");
                        ValidateAccountCurrency(dto.FromAccountId.Value, dto.Currency, errors, "Source account");
                    }
                    break;

                case PaymentType.Receipt:
                    // Incoming receipt requires ToAccount
                    if (!dto.ToAccountId.HasValue)
                    {
                        errors.Add("Destination account is required for receipts - الحساب الهدف مطلوب للإيصالات");
                    }
                    else
                    {
                        ValidateAccountExists(dto.ToAccountId.Value, errors, "Destination account");
                        ValidateAccountCurrency(dto.ToAccountId.Value, dto.Currency, errors, "Destination account");
                    }
                    break;

                case PaymentType.Transfer:
                    // Internal transfer requires both accounts
                    if (!dto.FromAccountId.HasValue)
                    {
                        errors.Add("Source account is required for transfers - الحساب المصدر مطلوب للتحويلات");
                    }
                    if (!dto.ToAccountId.HasValue)
                    {
                        errors.Add("Destination account is required for transfers - الحساب الهدف مطلوب للتحويلات");
                    }
                    if (dto.FromAccountId.HasValue && dto.ToAccountId.HasValue && dto.FromAccountId == dto.ToAccountId)
                    {
                        errors.Add("Source and destination accounts must be different for transfers - الحساب المصدر والهدف يجب أن يكونا مختلفين");
                    }
                    if (dto.FromAccountId.HasValue)
                    {
                        ValidateAccountExists(dto.FromAccountId.Value, errors, "Source account");
                        ValidateAccountCurrency(dto.FromAccountId.Value, dto.Currency, errors, "Source account");
                    }
                    if (dto.ToAccountId.HasValue)
                    {
                        ValidateAccountExists(dto.ToAccountId.Value, errors, "Destination account");
                        ValidateAccountCurrency(dto.ToAccountId.Value, dto.Currency, errors, "Destination account");
                    }
                    break;
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }*/

        // TODO: Re-enable when UpdatePaymentDto is created in PaymentHub
        /*public void ValidateUpdatePayment(UpdatePaymentDto dto, Payment existingPayment)
        {
            var errors = new List<string>();

            // Can only update Draft payments
            if (existingPayment.Status != PaymentStatus.Draft)
            {
                errors.Add($"Cannot update payment with status {existingPayment.Status}. Only Draft payments can be updated - لا يمكن تحديث دفعة بحالة {existingPayment.Status}. فقط الدفعات المسودة يمكن تحديثها");
            }

            // Amount > 0
            if (dto.Amount <= 0)
            {
                errors.Add("Payment amount must be greater than zero - مبلغ الدفعة يجب أن يكون أكبر من صفر");
            }

            // Currency validation
            if (string.IsNullOrWhiteSpace(dto.Currency) || dto.Currency.Length != 3)
            {
                errors.Add("Valid currency code (3 characters) is required - رمز العملة المكون من 3 أحرف مطلوب");
            }

            // Validate fiscal period is open
            if (dto.PaymentDate != default)
            {
                try
                {
                    ValidateFiscalPeriodOpen(dto.PaymentDate, dto.TenantId);
                }
                catch (PeriodClosedException ex)
                {
                    errors.Add($"Fiscal period is closed - الفترة المالية مغلقة: {ex.Message}");
                }
            }

            // Validate account requirements based on payment type (same as create)
            switch (dto.PaymentType)
            {
                case PaymentType.Payment:
                    if (!dto.FromAccountId.HasValue)
                    {
                        errors.Add("Source account is required for outgoing payments - الحساب المصدر مطلوب للدفعات الصادرة");
                    }
                    else
                    {
                        ValidateAccountExists(dto.FromAccountId.Value, errors, "Source account");
                        ValidateAccountCurrency(dto.FromAccountId.Value, dto.Currency, errors, "Source account");
                    }
                    break;

                case PaymentType.Receipt:
                    if (!dto.ToAccountId.HasValue)
                    {
                        errors.Add("Destination account is required for receipts - الحساب الهدف مطلوب للإيصالات");
                    }
                    else
                    {
                        ValidateAccountExists(dto.ToAccountId.Value, errors, "Destination account");
                        ValidateAccountCurrency(dto.ToAccountId.Value, dto.Currency, errors, "Destination account");
                    }
                    break;

                case PaymentType.Transfer:
                    if (!dto.FromAccountId.HasValue)
                    {
                        errors.Add("Source account is required for transfers - الحساب المصدر مطلوب للتحويلات");
                    }
                    if (!dto.ToAccountId.HasValue)
                    {
                        errors.Add("Destination account is required for transfers - الحساب الهدف مطلوب للتحويلات");
                    }
                    if (dto.FromAccountId.HasValue && dto.ToAccountId.HasValue && dto.FromAccountId == dto.ToAccountId)
                    {
                        errors.Add("Source and destination accounts must be different - الحساب المصدر والهدف يجب أن يكونا مختلفين");
                    }
                    break;
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }*/

        public void ValidateDeletePayment(Payment payment)
        {
            var errors = new List<string>();

            // Can only delete Draft payments
            if (payment.Status != PaymentStatus.Draft)
            {
                errors.Add($"Cannot delete payment with status {payment.Status}. Only Draft payments can be deleted - لا يمكن حذف دفعة بحالة {payment.Status}. فقط الدفعات المسودة يمكن حذفها");
            }

            // Check if payment has allocations
            var hasAllocations = HasPaymentAllocations(payment.Id);
            if (hasAllocations)
            {
                errors.Add("Cannot delete payment with allocations. Remove allocations first - لا يمكن حذف دفعة لها تخصيصات. قم بإزالة التخصيصات أولاً");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateSubmitPayment(Payment payment)
        {
            var errors = new List<string>();

            // Payment must be in Draft status
            if (payment.Status != PaymentStatus.Draft)
            {
                errors.Add($"Payment must be in Draft status to submit. Current status: {payment.Status} - الدفعة يجب أن تكون في حالة المسودة للتقديم. الحالة الحالية: {payment.Status}");
            }

            // Validate amount > 0
            if (payment.Amount <= 0)
            {
                errors.Add("Payment amount must be greater than zero - مبلغ الدفعة يجب أن يكون أكبر من صفر");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateApprovePayment(Payment payment)
        {
            var errors = new List<string>();

            // Payment must be in Draft status
            if (payment.Status != PaymentStatus.Draft)
            {
                errors.Add($"Payment must be in Draft status to approve. Current status: {payment.Status} - الدفعة يجب أن تكون في حالة مسودة للموافقة. الحالة الحالية: {payment.Status}");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidatePostPayment(Payment payment)
        {
            var errors = new List<string>();

            // Payment must be in Approved status
            if (payment.Status != PaymentStatus.Approved)
            {
                errors.Add($"Payment must be in Approved status to post. Current status: {payment.Status} - الدفعة يجب أن تكون في حالة معتمدة للتسجيل. الحالة الحالية: {payment.Status}");
            }

            // Validate fiscal period is open
            try
            {
                ValidateFiscalPeriodOpen(payment.PaymentDate, null);
            }
            catch (PeriodClosedException ex)
            {
                errors.Add($"Fiscal period is closed - الفترة المالية مغلقة: {ex.Message}");
            }

            // Validate sufficient balance for outgoing payments
            if (payment.PaymentType == PaymentType.VendorPayment ||
                payment.PaymentType == PaymentType.InternalTransfer ||
                payment.PaymentType == PaymentType.IntercompanyTransfer ||
                payment.PaymentType == PaymentType.CustomerRefund)
            {
                if (payment.SourceAccountId.HasValue)
                {
                    try
                    {
                        ValidateSufficientBalance(payment.SourceAccountId.Value, payment.Amount, IsBankAccount(payment.SourceAccountId.Value));
                    }
                    catch (AppException ex) when (ex.Code == ExceptionCode.FINANCE_PAYMENT_INSUFFICIENT_BALANCE)
                    {
                        errors.Add($"Insufficient balance in source account - رصيد غير كافي في الحساب المصدر");
                    }
                }
            }

            // Validate intercompany transfers (Phase 1.5 requirement)
            if ((payment.PaymentType == PaymentType.InternalTransfer || payment.PaymentType == PaymentType.IntercompanyTransfer)
                && payment.SourceAccountId.HasValue && payment.DestinationAccountId.HasValue)
            {
                var fromCompanyId = GetAccountCompanyId(payment.SourceAccountId.Value);
                var toCompanyId = GetAccountCompanyId(payment.DestinationAccountId.Value);

                if (fromCompanyId != toCompanyId)
                {
                    // This is an intercompany transfer - block unless it's specifically marked as IntercompanyTransfer
                    errors.Add("Intercompany transfers between different companies require proper intercompany journal. " +
                        $"Source company: {fromCompanyId}, Destination company: {toCompanyId}. " +
                        "Please create an intercompany payment type. - " +
                        "التحويلات بين الشركات المختلفة تتطلب قيد شركات متقابلة. " +
                        $"الشركة المصدر: {fromCompanyId}، الشركة الهدف: {toCompanyId}. " +
                        "يرجى إنشاء نوع دفعة بين الشركات.");
                }
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateCancelPayment(Payment payment)
        {
            var errors = new List<string>();

            // Cannot cancel Posted payments (must reverse instead)
            if (payment.Status == PaymentStatus.Posted)
            {
                errors.Add("Cannot cancel Posted payments. Use reversal instead - لا يمكن إلغاء الدفعات المسجلة. استخدم العكس بدلاً من ذلك");
            }

            // Cannot cancel already cancelled payments
            if (payment.Status == PaymentStatus.Cancelled)
            {
                errors.Add("Payment is already cancelled - الدفعة ملغية بالفعل");
            }

            // Check if payment has allocations
            if (HasPaymentAllocations(payment.Id))
            {
                errors.Add("Cannot cancel payment with allocations. Remove allocations first - لا يمكن إلغاء دفعة لها تخصيصات. قم بإزالة التخصيصات أولاً");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidatePaymentAllocation(AllocatePaymentDto dto, Payment payment)
        {
            var errors = new List<string>();

            // Payment must be Posted
            if (payment.Status != PaymentStatus.Posted)
            {
                errors.Add($"Payment must be Posted to allocate. Current status: {payment.Status} - الدفعة يجب أن تكون مسجلة للتخصيص. الحالة الحالية: {payment.Status}");
            }

            // Validate allocations list
            if (dto.Allocations == null || dto.Allocations.Count == 0)
            {
                errors.Add("At least one allocation is required - مطلوب تخصيص واحد على الأقل");
            }
            else
            {
                // Calculate total allocation amount
                decimal totalRequestedAmount = dto.Allocations.Sum(a => a.AllocatedAmount);

                // Check if allocation amount exceeds remaining unallocated amount
                var totalAllocated = GetTotalAllocatedAmount(payment.Id);
                var remainingAmount = payment.Amount - totalAllocated;
                if (totalRequestedAmount > remainingAmount)
                {
                    errors.Add($"Total allocation amount ({totalRequestedAmount}) exceeds remaining unallocated amount ({remainingAmount}) - مبلغ التخصيص الإجمالي ({totalRequestedAmount}) يتجاوز المبلغ غير المخصص المتبقي ({remainingAmount})");
                }

                // Validate each allocation
                foreach (var alloc in dto.Allocations)
                {
                    if (alloc.AllocatedAmount <= 0)
                    {
                        errors.Add("Allocation amount must be greater than zero - مبلغ التخصيص يجب أن يكون أكبر من صفر");
                    }
                }
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        #endregion

        #region Cheque Validations

        public void ValidateCreateCheque(CreateChequeDto dto)
        {
            var errors = new List<string>();

            // Amount > 0
            if (dto.Amount <= 0)
            {
                errors.Add("Cheque amount must be greater than zero - مبلغ الشيك يجب أن يكون أكبر من صفر");
            }

            // Cheque number is required
            if (string.IsNullOrWhiteSpace(dto.ChequeNumber))
            {
                errors.Add("Cheque number is required - رقم الشيك مطلوب");
            }

            // Cheque number uniqueness per bank account
            if (!string.IsNullOrWhiteSpace(dto.ChequeNumber))
            {
                var isDuplicate = IsChequeNumberDuplicate(dto.BankAccountId, dto.ChequeNumber, null);
                if (isDuplicate)
                {
                    errors.Add($"Cheque number {dto.ChequeNumber} already exists for this bank account - رقم الشيك {dto.ChequeNumber} موجود بالفعل لهذا الحساب البنكي");
                }
            }

            // Payee name is required
            if (string.IsNullOrWhiteSpace(dto.PayeeName))
            {
                errors.Add("Payee name is required - اسم المستفيد مطلوب");
            }

            // Due date must be >= Issue date
            if (dto.DueDate < dto.IssueDate)
            {
                errors.Add("Due date must be on or after issue date - تاريخ الاستحقاق يجب أن يكون في أو بعد تاريخ الإصدار");
            }

            // Bank account must exist and be active
            var bankAccount = _bankAccountRepository.GetById(dto.BankAccountId);
            if (bankAccount == null)
            {
                errors.Add("Bank account not found - الحساب البنكي غير موجود");
            }
            else if (bankAccount.Status != AccountStatus.Active)
            {
                errors.Add("Bank account is not active - الحساب البنكي غير نشط");
            }

            // Validate fiscal period is open
            try
            {
                ValidateFiscalPeriodOpen(dto.IssueDate, dto.TenantId);
            }
            catch (PeriodClosedException ex)
            {
                errors.Add($"Fiscal period is closed - الفترة المالية مغلقة: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateUpdateCheque(UpdateChequeDto dto, Cheque existingCheque)
        {
            var errors = new List<string>();

            // Can only update Issued cheques
            if (existingCheque.Status != ChequeStatus.Issued)
            {
                errors.Add($"Cannot update cheque with status {existingCheque.Status}. Only Issued cheques can be updated - لا يمكن تحديث شيك بحالة {existingCheque.Status}. فقط الشيكات الصادرة يمكن تحديثها");
            }

            // Amount > 0
            if (dto.Amount <= 0)
            {
                errors.Add("Cheque amount must be greater than zero - مبلغ الشيك يجب أن يكون أكبر من صفر");
            }

            // Cheque number uniqueness (excluding current cheque)
            if (!string.IsNullOrWhiteSpace(dto.ChequeNumber))
            {
                var isDuplicate = IsChequeNumberDuplicate(dto.BankAccountId, dto.ChequeNumber, dto.Id);
                if (isDuplicate)
                {
                    errors.Add($"Cheque number {dto.ChequeNumber} already exists for this bank account - رقم الشيك {dto.ChequeNumber} موجود بالفعل لهذا الحساب البنكي");
                }
            }

            // Due date must be >= Issue date
            if (dto.DueDate < dto.IssueDate)
            {
                errors.Add("Due date must be on or after issue date - تاريخ الاستحقاق يجب أن يكون في أو بعد تاريخ الإصدار");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateDeleteCheque(Cheque cheque)
        {
            var errors = new List<string>();

            // Can only delete Issued or Cancelled cheques
            if (cheque.Status != ChequeStatus.Issued && cheque.Status != ChequeStatus.Cancelled)
            {
                errors.Add($"Cannot delete cheque with status {cheque.Status}. Only Issued or Cancelled cheques can be deleted - لا يمكن حذف شيك بحالة {cheque.Status}. فقط الشيكات الصادرة أو الملغية يمكن حذفها");
            }

            // Cannot delete if linked to a payment
            if (cheque.RelatedPaymentId.HasValue)
            {
                errors.Add("Cannot delete cheque linked to a payment - لا يمكن حذف شيك مرتبط بدفعة");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateChequeStatusChange(Cheque cheque, ChequeStatus newStatus)
        {
            var errors = new List<string>();

            // Define valid status transitions
            // Flow: Issued → Presented → Cleared/Bounced
            var validTransitions = new Dictionary<ChequeStatus, List<ChequeStatus>>
            {
                { ChequeStatus.Issued, new List<ChequeStatus> { ChequeStatus.Presented, ChequeStatus.Cancelled } },
                { ChequeStatus.Presented, new List<ChequeStatus> { ChequeStatus.Cleared, ChequeStatus.Bounced, ChequeStatus.Cancelled } },
                { ChequeStatus.Cleared, new List<ChequeStatus>() }, // Terminal state
                { ChequeStatus.Bounced, new List<ChequeStatus> { ChequeStatus.Presented, ChequeStatus.Cancelled } }, // Can re-present or cancel
                { ChequeStatus.Cancelled, new List<ChequeStatus>() } // Terminal state
            };

            if (!validTransitions.ContainsKey(cheque.Status) || !validTransitions[cheque.Status].Contains(newStatus))
            {
                errors.Add($"Invalid status transition from {cheque.Status} to {newStatus} - انتقال حالة غير صالح من {cheque.Status} إلى {newStatus}");
            }

            // If clearing, validate sufficient bank balance
            // For OUTGOING cheques: Skip validation if already in Presented status
            // (funds were already reserved when issued)
            // For INCOMING cheques: Always validate (money is being deposited)
            if (newStatus == ChequeStatus.Cleared)
            {
                // Only validate balance for OUTGOING cheques that are NOT yet presented
                // OR for INCOMING cheques (which need to deposit funds)
                bool shouldValidateBalance = cheque.ChequeType == ChequeType.Incoming;

                if (shouldValidateBalance)
                {
                    try
                    {
                        ValidateSufficientBalance(cheque.BankAccountId, cheque.Amount, true);
                    }
                    catch (AppException ex) when (ex.Code == ExceptionCode.FINANCE_PAYMENT_INSUFFICIENT_BALANCE)
                    {
                        errors.Add("Insufficient bank account balance to clear cheque - رصيد الحساب البنكي غير كافي لتسديد الشيك");
                    }
                }
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        #endregion

        #region Bank Account Validations

        public void ValidateCreateBankAccount(CreateBankAccountDto dto)
        {
            var errors = new List<string>();

            // Bank code is required
            if (string.IsNullOrWhiteSpace(dto.BankCode))
            {
                errors.Add("Bank code is required - رمز البنك مطلوب");
            }

            // Account number is required
            if (string.IsNullOrWhiteSpace(dto.AccountNumber))
            {
                errors.Add("Account number is required - رقم الحساب مطلوب");
            }

            // Validate GLAccount has IsBankAccount flag
            if (dto.GLAccountId != Guid.Empty)
            {
                var glAccount = _glAccountService.GetById(dto.GLAccountId);
                if (glAccount == null)
                {
                    errors.Add($"GL Account with ID {dto.GLAccountId} not found - حساب دفتر الأستاذ بالمعرف {dto.GLAccountId} غير موجود");
                }
                else if (glAccount.AccountNature != AccountNature.Bank)
                {
                    errors.Add(
                        $"حساب دفتر الأستاذ {glAccount.FullAccountNumber} ({glAccount.AccountNameAr}) غير معلم كحساب بنكي. " +
                        $"يجب تعيين AccountNature = Bank على الحساب أولاً. - " +
                        $"GL Account {glAccount.FullAccountNumber} ({glAccount.AccountNameEn}) is not marked as a bank account. " +
                        $"Please set AccountNature = Bank on the account first.");
                }
            }

            // Account number uniqueness per bank
            if (!string.IsNullOrWhiteSpace(dto.AccountNumber) && !string.IsNullOrWhiteSpace(dto.BankCode))
            {
                var isDuplicate = IsBankAccountDuplicate(dto.BankCode, dto.AccountNumber, null);
                if (isDuplicate)
                {
                    errors.Add($"Account number {dto.AccountNumber} already exists for bank {dto.BankCode} - رقم الحساب {dto.AccountNumber} موجود بالفعل للبنك {dto.BankCode}");
                }
            }

            // Currency is required
            if (string.IsNullOrWhiteSpace(dto.Currency) || dto.Currency.Length != 3)
            {
                errors.Add("Valid currency code (3 characters) is required - رمز العملة المكون من 3 أحرف مطلوب");
            }

            // Validate IBAN format if provided
            if (!string.IsNullOrWhiteSpace(dto.IBAN))
            {
                try
                {
                    ValidateIBAN(dto.IBAN);
                }
                catch (CustomException ex)
                {
                    errors.Add(ex.Message);
                }
            }

            // Validate SWIFT code format if provided
            if (!string.IsNullOrWhiteSpace(dto.SwiftCode))
            {
                try
                {
                    ValidateSwiftCode(dto.SwiftCode);
                }
                catch (CustomException ex)
                {
                    errors.Add(ex.Message);
                }
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateUpdateBankAccount(UpdateBankAccountDto dto, BankAccount existingAccount)
        {
            var errors = new List<string>();

            // Cannot change currency if account has transactions
            if (dto.Currency != existingAccount.Currency)
            {
                if (HasBankAccountTransactions(existingAccount.Id))
                {
                    errors.Add("Cannot change currency when transactions exist - لا يمكن تغيير العملة عند وجود معاملات");
                }
            }

            // Account number uniqueness (excluding current account)
            if (!string.IsNullOrWhiteSpace(dto.AccountNumber) && !string.IsNullOrWhiteSpace(dto.BankCode))
            {
                var isDuplicate = IsBankAccountDuplicate(dto.BankCode, dto.AccountNumber, dto.Id);
                if (isDuplicate)
                {
                    errors.Add($"Account number {dto.AccountNumber} already exists for bank {dto.BankCode} - رقم الحساب {dto.AccountNumber} موجود بالفعل للبنك {dto.BankCode}");
                }
            }

            // Validate IBAN format if provided
            if (!string.IsNullOrWhiteSpace(dto.IBAN))
            {
                try
                {
                    ValidateIBAN(dto.IBAN);
                }
                catch (CustomException ex)
                {
                    errors.Add(ex.Message);
                }
            }

            // Validate SWIFT code format if provided
            if (!string.IsNullOrWhiteSpace(dto.SwiftCode))
            {
                try
                {
                    ValidateSwiftCode(dto.SwiftCode);
                }
                catch (CustomException ex)
                {
                    errors.Add(ex.Message);
                }
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateDeleteBankAccount(Guid bankAccountId)
        {
            var errors = new List<string>();

            // Cannot delete if transactions exist
            if (HasBankAccountTransactions(bankAccountId))
            {
                errors.Add("Cannot delete bank account with existing transactions - لا يمكن حذف حساب بنكي له معاملات");
            }

            // Cannot delete if cheques exist
            if (HasBankAccountCheques(bankAccountId))
            {
                errors.Add("Cannot delete bank account with existing cheques - لا يمكن حذف حساب بنكي له شيكات");
            }

            // Cannot delete if bank statements exist
            if (HasBankAccountStatements(bankAccountId))
            {
                errors.Add("Cannot delete bank account with existing statements - لا يمكن حذف حساب بنكي له كشوف حساب");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateIBAN(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_IBAN_REQUIRED);
            }

            // Remove spaces and convert to uppercase
            iban = iban.Replace(" ", "").ToUpperInvariant();

            // Check minimum length
            if (iban.Length < 15 || iban.Length > 34)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_IBAN_INVALID_LENGTH);
            }

            // Get country code
            var countryCode = iban.Substring(0, 2);

            // Validate against GCC patterns if applicable
            if (IBANPatterns.TryGetValue(countryCode, out var pattern))
            {
                if (!Regex.IsMatch(iban, pattern))
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_IBAN_INVALID_FORMAT)
                        .WithParameter("CountryCode", countryCode);
                }
            }

            // Validate IBAN checksum using MOD97 algorithm
            if (!ValidateIBANChecksum(iban))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_IBAN_INVALID_CHECKSUM);
            }
        }

        public void ValidateSwiftCode(string swiftCode)
        {
            if (string.IsNullOrWhiteSpace(swiftCode))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_SWIFT_REQUIRED);
            }

            swiftCode = swiftCode.ToUpperInvariant().Trim();

            if (!SwiftPattern.IsMatch(swiftCode))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_SWIFT_INVALID_FORMAT);
            }
        }

        #endregion

        #region Cash Account Validations

        public void ValidateCreateCashAccount(CreateCashAccountDto dto)
        {
            var errors = new List<string>();

            // Code is required
            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                errors.Add("Account code is required - رمز الحساب مطلوب");
            }

            // Validate GLAccount has IsCashAccount flag
            if (dto.GLAccountId != Guid.Empty)
            {
                var glAccount = _glAccountService.GetById(dto.GLAccountId);
                if (glAccount == null)
                {
                    errors.Add($"GL Account with ID {dto.GLAccountId} not found - حساب دفتر الأستاذ بالمعرف {dto.GLAccountId} غير موجود");
                }
                else if (glAccount.AccountNature != AccountNature.Cash)
                {
                    errors.Add(
                        $"حساب دفتر الأستاذ {glAccount.FullAccountNumber} ({glAccount.AccountNameAr}) غير معلم كحساب نقدي. " +
                        $"يجب تعيين AccountNature = Cash على الحساب أولاً. - " +
                        $"GL Account {glAccount.FullAccountNumber} ({glAccount.AccountNameEn}) is not marked as a cash account. " +
                        $"Please set AccountNature = Cash on the account first.");
                }
            }

            // Code uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                var isDuplicate = IsCashAccountCodeDuplicate(dto.Code, null, dto.TenantId);
                if (isDuplicate)
                {
                    errors.Add($"Account code {dto.Code} already exists - رمز الحساب {dto.Code} موجود بالفعل");
                }
            }

            // Currency is required
            if (string.IsNullOrWhiteSpace(dto.Currency) || dto.Currency.Length != 3)
            {
                errors.Add("Valid currency code (3 characters) is required - رمز العملة المكون من 3 أحرف مطلوب");
            }

            // Names are required
            if (string.IsNullOrWhiteSpace(dto.NameAr))
            {
                errors.Add("Arabic name is required - الاسم بالعربي مطلوب");
            }
            if (string.IsNullOrWhiteSpace(dto.NameEn))
            {
                errors.Add("English name is required - الاسم بالإنجليزي مطلوب");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateUpdateCashAccount(UpdateCashAccountDto dto, CashAccount existingAccount)
        {
            var errors = new List<string>();

            // Cannot change currency if transactions exist
            if (dto.Currency != existingAccount.Currency)
            {
                if (HasCashAccountTransactions(existingAccount.Id))
                {
                    errors.Add("Cannot change currency when transactions exist - لا يمكن تغيير العملة عند وجود معاملات");
                }
            }

            // Code uniqueness (excluding current account)
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                var isDuplicate = IsCashAccountCodeDuplicate(dto.Code, dto.Id, dto.TenantId);
                if (isDuplicate)
                {
                    errors.Add($"Account code {dto.Code} already exists - رمز الحساب {dto.Code} موجود بالفعل");
                }
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateDeleteCashAccount(Guid cashAccountId)
        {
            var errors = new List<string>();

            // Cannot delete if transactions exist
            if (HasCashAccountTransactions(cashAccountId))
            {
                errors.Add("Cannot delete cash account with existing transactions - لا يمكن حذف حساب نقدي له معاملات");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        #endregion

        #region Bank Statement Validations

        public void ValidateCreateBankStatement(CreateBankStatementDto dto)
        {
            var errors = new List<string>();

            // Bank account must exist
            var bankAccount = _bankAccountRepository.GetById(dto.BankAccountId);
            if (bankAccount == null)
            {
                errors.Add("Bank account not found - الحساب البنكي غير موجود");
            }

            // Statement reference is required
            if (string.IsNullOrWhiteSpace(dto.StatementReference))
            {
                errors.Add("Statement reference is required - مرجع الكشف مطلوب");
            }

            // Opening balance should match previous statement's closing balance
            var latestStatement = GetLatestBankStatement(dto.BankAccountId);
            if (latestStatement != null)
            {
                if (dto.OpeningBalance != latestStatement.ClosingBalance)
                {
                    errors.Add($"Opening balance ({dto.OpeningBalance}) does not match previous statement's closing balance ({latestStatement.ClosingBalance}) - الرصيد الافتتاحي ({dto.OpeningBalance}) لا يتطابق مع الرصيد الختامي للكشف السابق ({latestStatement.ClosingBalance})");
                }

                // Statement date should be after previous statement
                if (dto.StatementDate <= latestStatement.StatementDate)
                {
                    errors.Add($"Statement date must be after previous statement date ({latestStatement.StatementDate:yyyy-MM-dd}) - تاريخ الكشف يجب أن يكون بعد تاريخ الكشف السابق ({latestStatement.StatementDate:yyyy-MM-dd})");
                }
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateBankStatementImport(UploadBankStatementDto dto)
        {
            var errors = new List<string>();

            // Bank account must exist
            var bankAccount = _bankAccountRepository.GetById(dto.BankAccountId);
            if (bankAccount == null)
            {
                errors.Add("Bank account not found - الحساب البنكي غير موجود");
            }

            // File name is required
            if (string.IsNullOrWhiteSpace(dto.FileName))
            {
                errors.Add("File name is required - اسم الملف مطلوب");
            }

            // Validate supported import formats
            if (dto.ImportFormat != BankStatementFormat.GenericCSV && dto.ImportFormat != BankStatementFormat.MT940)
            {
                errors.Add("Only CSV and MT940 import formats are supported - فقط تنسيقات CSV و MT940 مدعومة للاستيراد");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        #endregion

        #region Bank Reconciliation Validations

        public void ValidateCreateReconciliation(CreateReconciliationDto dto)
        {
            var errors = new List<string>();

            // Bank account must exist
            var bankAccount = _bankAccountRepository.GetById(dto.BankAccountId);
            if (bankAccount == null)
            {
                errors.Add("Bank account not found - الحساب البنكي غير موجود");
            }

            // Period end must be after period start
            if (dto.PeriodEnd <= dto.PeriodStart)
            {
                errors.Add("Period end date must be after period start date - تاريخ نهاية الفترة يجب أن يكون بعد تاريخ بداية الفترة");
            }

            // Check for overlapping reconciliation periods
            if (HasOverlappingReconciliation(dto.BankAccountId, dto.PeriodStart, dto.PeriodEnd, null))
            {
                errors.Add("Reconciliation period overlaps with existing reconciliation - فترة التسوية تتداخل مع تسوية موجودة");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateCompleteReconciliation(BankReconciliation reconciliation)
        {
            var errors = new List<string>();

            // Reconciliation must be in progress
            if (reconciliation.Status != ReconciliationStatus.InProgress)
            {
                errors.Add($"Reconciliation must be in progress to complete. Current status: {reconciliation.Status} - التسوية يجب أن تكون قيد التنفيذ للإكمال. الحالة الحالية: {reconciliation.Status}");
            }

            // Check if there's a significant difference between statement and ledger balances
            var difference = Math.Abs(reconciliation.StatementClosingBalance - reconciliation.LedgerClosingBalance);
            if (difference > 0.01m) // Allow small rounding differences
            {
                errors.Add($"Cannot complete reconciliation with balance difference of {difference}. Statement: {reconciliation.StatementClosingBalance}, Ledger: {reconciliation.LedgerClosingBalance} - لا يمكن إكمال التسوية مع فرق رصيد {difference}. الكشف: {reconciliation.StatementClosingBalance}، السجل: {reconciliation.LedgerClosingBalance}");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidateMatchReconciliationLine(MatchReconciliationLineDto dto)
        {
            var errors = new List<string>();

            // Reconciliation must exist and be in progress
            var reconciliation = _reconciliationRepository.GetById(dto.ReconciliationId);
            if (reconciliation == null)
            {
                errors.Add("Reconciliation not found - التسوية غير موجودة");
            }
            else if (reconciliation.Status != ReconciliationStatus.InProgress)
            {
                errors.Add($"Reconciliation must be in progress to match lines. Current status: {reconciliation.Status} - التسوية يجب أن تكون قيد التنفيذ لمطابقة البنود. الحالة الحالية: {reconciliation.Status}");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        #endregion

        #region Balance Validations

        /// <summary>
        /// Validate sufficient balance using GLAccountOpeningBalance + journal entries
        /// </summary>
        public void ValidateSufficientBalance(Guid accountId, decimal amount, bool isBankAccount)
        {
            if (isBankAccount)
            {
                // Get bank account
                var bankAccount = _bankAccountRepository.GetById(accountId);
                if (bankAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                        .WithParameter("BankAccountId", accountId);
                }

                // Check if GL account is linked
                if (!bankAccount.GLAccountId.HasValue)
                {
                    // If no GL account linked, skip validation (backward compatibility)
                    return;
                }

                // Get current fiscal year
                var currentYear = DateTime.Now.Year;

                // Get GL account balance
                var balance = _glAccountService.GetAccountBalance(bankAccount.GLAccountId.Value, currentYear, DateTime.Now);

                if (balance == null)
                {
                    // No balance record found, assume zero balance
                    if (amount > 0)
                    {
                        throw new AppException(ExceptionCode.FINANCE_PAYMENT_INSUFFICIENT_BALANCE)
                            .WithParameter("RequiredAmount", amount)
                            .WithParameter("AvailableAmount", 0.00m);
                    }
                    return;
                }

                // Check if current balance is sufficient
                if (balance.CurrentBalance < amount)
                {
                    throw new AppException(ExceptionCode.FINANCE_PAYMENT_INSUFFICIENT_BALANCE)
                        .WithParameter("RequiredAmount", amount)
                        .WithParameter("AvailableAmount", balance.CurrentBalance);
                }
            }
            else
            {
                // Cash account validation
                var cashAccount = _cashAccountRepository.GetById(accountId);
                if (cashAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_NOT_FOUND)
                        .WithParameter("CashAccountId", accountId);
                }

                // Check if GL account is linked
                if (!cashAccount.GLAccountId.HasValue)
                {
                    // If no GL account linked, skip validation (backward compatibility)
                    return;
                }

                // Get current fiscal year
                var currentYear = DateTime.Now.Year;

                // Get GL account balance
                var balance = _glAccountService.GetAccountBalance(cashAccount.GLAccountId.Value, currentYear, DateTime.Now);

                if (balance == null)
                {
                    // No balance record found, assume zero balance
                    if (amount > 0)
                    {
                        throw new AppException(ExceptionCode.FINANCE_PAYMENT_INSUFFICIENT_BALANCE)
                            .WithParameter("RequiredAmount", amount)
                            .WithParameter("AvailableAmount", 0.00m);
                    }
                    return;
                }

                // Check if current balance is sufficient
                if (balance.CurrentBalance < amount)
                {
                    throw new AppException(ExceptionCode.FINANCE_PAYMENT_INSUFFICIENT_BALANCE)
                        .WithParameter("RequiredAmount", amount)
                        .WithParameter("AvailableAmount", balance.CurrentBalance);
                }
            }
        }

        #endregion

        #region Fiscal Period Validations

        public void ValidateFiscalPeriodOpen(DateTime transactionDate, long? tenantId = null)
        {
            try
            {
                long? companyId = tenantId.HasValue ? (long?)0 : null;
                _periodClosingService.CheckIfDateInClosedOrLockedPeriod(transactionDate, companyId, false);
            }
            catch (PeriodNotFoundException ex)
            {
                // Convert to PeriodClosedException with user-friendly message
                throw new PeriodClosedException(
                    transactionDate,
                    $"لا توجد فترة مالية للتاريخ {transactionDate:yyyy-MM-dd}. يرجى إنشاء الفترة أولاً - {ex.MessageAr}",
                    $"No fiscal period found for date {transactionDate:yyyy-MM-dd}. Please create the period first - {ex.MessageEn}"
                );
            }
            catch (PeriodLockedException)
            {
                throw new PeriodClosedException(transactionDate);
            }
            catch (PeriodClosedException)
            {
                throw;
            }
        }

        public (int FiscalYear, int FiscalPeriod) GetFiscalPeriodForDate(DateTime date, long? companyId = null)
        {
            var openPeriods = _periodClosingService.GetOpenPeriods(companyId);

            foreach (var period in openPeriods)
            {
                if (date >= period.PeriodStartDate && date <= period.PeriodEndDate)
                {
                    return (period.FiscalYear, period.FiscalPeriod);
                }
            }

            // Default: use transaction date's year and month
            return (date.Year, date.Month);
        }

        #endregion

        #region Helper Methods

        private void ValidateAccountExists(Guid accountId, List<string> errors, string accountType)
        {
            // First check bank accounts
            var bankAccount = _bankAccountRepository.GetById(accountId);
            if (bankAccount != null)
            {
                if (bankAccount.Status != AccountStatus.Active)
                {
                    errors.Add($"{accountType} is not active - {accountType} غير نشط");
                }
                return;
            }

            // Then check cash accounts
            var cashAccount = _cashAccountRepository.GetById(accountId);
            if (cashAccount != null)
            {
                if (cashAccount.Status != AccountStatus.Active)
                {
                    errors.Add($"{accountType} is not active - {accountType} غير نشط");
                }
                return;
            }

            errors.Add($"{accountType} not found - {accountType} غير موجود");
        }

        private void ValidateAccountCurrency(Guid accountId, string expectedCurrency, List<string> errors, string accountType)
        {
            string accountCurrency = null;

            var bankAccount = _bankAccountRepository.GetById(accountId);
            if (bankAccount != null)
            {
                accountCurrency = bankAccount.Currency;
            }
            else
            {
                var cashAccount = _cashAccountRepository.GetById(accountId);
                if (cashAccount != null)
                {
                    accountCurrency = cashAccount.Currency;
                }
            }

            if (!string.IsNullOrEmpty(accountCurrency) && accountCurrency != expectedCurrency)
            {
                errors.Add($"{accountType} currency ({accountCurrency}) does not match payment currency ({expectedCurrency}) - عملة {accountType} ({accountCurrency}) لا تتطابق مع عملة الدفعة ({expectedCurrency})");
            }
        }

        private bool IsBankAccount(Guid accountId)
        {
            return _bankAccountRepository.GetById(accountId) != null;
        }

        private bool HasPaymentAllocations(Guid paymentId)
        {
            var predicate = Expressionable.Create<PaymentAllocation>();
            predicate = predicate.And(x => x.PaymentId == paymentId);
            return _allocationRepository.Count(predicate.ToExpression()) > 0;
        }

        private decimal GetTotalAllocatedAmount(Guid paymentId)
        {
            var predicate = Expressionable.Create<PaymentAllocation>();
            predicate = predicate.And(x => x.PaymentId == paymentId);
            var allocations = _allocationRepository.GetList(predicate.ToExpression());
            return allocations.Sum(a => a.AllocatedAmount);
        }

        private bool IsChequeNumberDuplicate(Guid bankAccountId, string chequeNumber, Guid? excludeId)
        {
            var predicate = Expressionable.Create<Cheque>();
            predicate = predicate.And(x => x.BankAccountId == bankAccountId && x.ChequeNumber == chequeNumber);
            if (excludeId.HasValue)
            {
                predicate = predicate.And(x => x.Id != excludeId.Value);
            }
            return _chequeRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool IsBankAccountDuplicate(string bankCode, string accountNumber, Guid? excludeId)
        {
            var predicate = Expressionable.Create<BankAccount>();
            predicate = predicate.And(x => x.BankCode == bankCode && x.AccountNumber == accountNumber);
            if (excludeId.HasValue)
            {
                predicate = predicate.And(x => x.Id != excludeId.Value);
            }
            return _bankAccountRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool IsCashAccountCodeDuplicate(string code, Guid? excludeId, long? tenantId)
        {
            var predicate = Expressionable.Create<CashAccount>();
            predicate = predicate.And(x => x.Code == code);
            if (excludeId.HasValue)
            {
                predicate = predicate.And(x => x.Id != excludeId.Value);
            }
            return _cashAccountRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool HasBankAccountTransactions(Guid bankAccountId)
        {
            var predicate = Expressionable.Create<Payment>();
            predicate = predicate.Or(x => x.SourceAccountId == bankAccountId);
            predicate = predicate.Or(x => x.DestinationAccountId == bankAccountId);
            return _paymentRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool HasBankAccountCheques(Guid bankAccountId)
        {
            var predicate = Expressionable.Create<Cheque>();
            predicate = predicate.And(x => x.BankAccountId == bankAccountId);
            return _chequeRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool HasBankAccountStatements(Guid bankAccountId)
        {
            var predicate = Expressionable.Create<BankStatement>();
            predicate = predicate.And(x => x.BankAccountId == bankAccountId);
            return _bankStatementRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool HasCashAccountTransactions(Guid cashAccountId)
        {
            var predicate = Expressionable.Create<Payment>();
            predicate = predicate.Or(x => x.SourceAccountId == cashAccountId);
            predicate = predicate.Or(x => x.DestinationAccountId == cashAccountId);
            return _paymentRepository.Count(predicate.ToExpression()) > 0;
        }

        private BankStatement GetLatestBankStatement(Guid bankAccountId)
        {
            var predicate = Expressionable.Create<BankStatement>();
            predicate = predicate.And(x => x.BankAccountId == bankAccountId);
            var statements = _bankStatementRepository.GetList(predicate.ToExpression());
            return statements.OrderByDescending(s => s.StatementDate).FirstOrDefault();
        }

        private bool HasOverlappingReconciliation(Guid bankAccountId, DateTime periodStart, DateTime periodEnd, Guid? excludeId)
        {
            var predicate = Expressionable.Create<BankReconciliation>();
            predicate = predicate.And(x => x.BankAccountId == bankAccountId);
            predicate = predicate.And(x =>
                (x.PeriodStart <= periodEnd && x.PeriodEnd >= periodStart)); // Overlap condition
            if (excludeId.HasValue)
            {
                predicate = predicate.And(x => x.Id != excludeId.Value);
            }
            return _reconciliationRepository.Count(predicate.ToExpression()) > 0;
        }

        private bool ValidateIBANChecksum(string iban)
        {
            // Move first 4 characters to the end
            var rearranged = iban.Substring(4) + iban.Substring(0, 4);

            // Replace letters with numbers (A=10, B=11, etc.)
            var numericString = "";
            foreach (var c in rearranged)
            {
                if (char.IsLetter(c))
                {
                    numericString += (c - 'A' + 10).ToString();
                }
                else
                {
                    numericString += c;
                }
            }

            // Calculate MOD 97
            // For large numbers, process in chunks
            int remainder = 0;
            foreach (var c in numericString)
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
            }

            return remainder == 1;
        }

        #endregion

        #region Additional Validations (Requirements A11-30)

        /// <summary>
        /// REQUIRES REFACTORING: Should validate using GLAccountOpeningBalance + journal entries
        /// </summary>
        [Obsolete("Should validate using GLAccountOpeningBalance + journal entries")]
        public void ValidateAvailableBalance(Guid accountId, decimal amount, bool isBankAccount)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Available balance validation should use GLAccountOpeningBalance + journal entries");
        }

        public void ValidateNegativeBalanceAllowed(Guid accountId, bool isBankAccount)
        {
            if (isBankAccount)
            {
                var bankAccount = _bankAccountRepository.GetById(accountId);
                if (bankAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                        .WithParameter("BankAccountId", accountId);
                }
                if (!bankAccount.IsOverdraftAllowed)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_NEGATIVE_BALANCE_NOT_ALLOWED)
                        .WithParameter("AccountType", "BankAccount");
                }
            }
            else
            {
                // Cash accounts typically don't allow negative balances
                throw new AppException(ExceptionCode.FINANCE_TREASURY_NEGATIVE_BALANCE_NOT_ALLOWED)
                    .WithParameter("AccountType", "CashAccount");
            }
        }

        public void ValidateIntercompanyTransfer(Guid fromAccountId, Guid toAccountId)
        {
            var fromCompanyId = GetAccountCompanyId(fromAccountId);
            var toCompanyId = GetAccountCompanyId(toAccountId);

            if (fromCompanyId != toCompanyId)
            {
                // This is an intercompany transfer - must have intercompany journal
                throw new AppException(ExceptionCode.FINANCE_TREASURY_INTERCOMPANY_JOURNAL_REQUIRED);
            }
        }

        public void ValidateCompanyIdMatch(Guid accountId, long? expectedCompanyId)
        {
            var actualCompanyId = GetAccountCompanyId(accountId);
            if (actualCompanyId != expectedCompanyId)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_ACCOUNT_COMPANY_MISMATCH)
                    .WithParameter("ActualCompanyId", actualCompanyId?.ToString() ?? string.Empty)
                    .WithParameter("ExpectedCompanyId", expectedCompanyId?.ToString() ?? string.Empty);
            }
        }

        public void ValidateReversePayment(Payment payment)
        {
            var errors = new List<string>();

            // Payment must be Posted
            if (payment.Status != PaymentStatus.Posted)
            {
                errors.Add($"Only Posted payments can be reversed. Current status: {payment.Status} - فقط الدفعات المسجلة يمكن عكسها. الحالة الحالية: {payment.Status}");
            }

            // Cannot reverse already reversed payment
            if (payment.IsReversed)
            {
                errors.Add("Payment has already been reversed - الدفعة معكوسة بالفعل");
            }

            // Cannot reverse a reversal payment
            if (payment.ReversalOfId.HasValue)
            {
                errors.Add("Cannot reverse a reversal payment - لا يمكن عكس دفعة عكس");
            }

            // Validate fiscal period is open for reversal
            try
            {
                ValidateFiscalPeriodOpen(payment.PaymentDate, null);
            }
            catch (PeriodClosedException)
            {
                errors.Add("Cannot reverse payment in closed fiscal period - لا يمكن عكس الدفعة في فترة مالية مغلقة");
            }

            if (errors.Count > 0)
            {
                ThrowValidationErrors(errors);
            }
        }

        public void ValidatePaymentNotAlreadyReversed(Payment payment)
        {
            if (payment.IsReversed)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_PAYMENT_ALREADY_REVERSED)
                    .WithParameter("PaymentId", payment.Id);
            }
        }

        public bool CheckIdempotencyKey(string idempotencyKey, TreasuryEntityType entityType)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return false; // No idempotency key provided
            }

            // Check based on entity type
            switch (entityType)
            {
                case TreasuryEntityType.Payment:
                    var paymentPredicate = Expressionable.Create<Payment>();
                    paymentPredicate = paymentPredicate.And(x => x.IdempotencyKey == idempotencyKey);
                    return _paymentRepository.Count(paymentPredicate.ToExpression()) > 0;

                case TreasuryEntityType.Cheque:
                    var chequePredicate = Expressionable.Create<Cheque>();
                    chequePredicate = chequePredicate.And(x => x.IdempotencyKey == idempotencyKey);
                    return _chequeRepository.Count(chequePredicate.ToExpression()) > 0;

                default:
                    return false;
            }
        }

        public void ValidateApprovalThreshold(decimal amount, string userId)
        {
            ValidateApprovalThreshold(amount, userId, "All", null);
        }

        /// <summary>
        /// Validate approval threshold with transaction type - التحقق من حد الموافقة حسب نوع المعاملة
        /// Phase 2.1: Role-based approval limits from ApprovalLimit table
        /// </summary>
        public void ValidateApprovalThreshold(decimal amount, string userId, string transactionType, long? companyId)
        {
            // Get applicable approval limits for this user
            var approvalLimit = GetUserApprovalLimit(userId, transactionType, companyId);

            if (approvalLimit == null)
            {
                // No limit configured - use default
                const decimal defaultApprovalLimit = 100000m;
                if (amount > defaultApprovalLimit)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_APPROVAL_LIMIT_EXCEEDED)
                        .WithParameter("Limit", defaultApprovalLimit)
                        .WithParameter("Amount", amount);
                }
                return;
            }

            // Check against maximum amount
            if (amount > approvalLimit.MaxAmount)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_APPROVAL_LIMIT_EXCEEDED)
                    .WithParameter("Limit", approvalLimit.MaxAmount)
                    .WithParameter("Amount", amount);
            }

            // Check if dual approval is required
            if (approvalLimit.RequiresDualApproval && amount > approvalLimit.DualApprovalThreshold)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_APPROVAL_LIMIT_EXCEEDED)
                    .WithParameter("Limit", approvalLimit.DualApprovalThreshold)
                    .WithParameter("Amount", amount);
            }
        }

        /// <summary>
        /// Get user's approval limit from database - الحصول على حد الموافقة للمستخدم من قاعدة البيانات
        /// </summary>
        private ApprovalLimit GetUserApprovalLimit(string userId, string transactionType, long? companyId)
        {
            // Get user's roles (placeholder - in full implementation, query SysUserRole)
            var userRoleIds = GetUserRoleIds(userId);

            if (userRoleIds.Count == 0)
            {
                return null;
            }

            // Find applicable approval limit
            var predicate = Expressionable.Create<ApprovalLimit>();
            predicate = predicate.And(x => x.IsActive);
            predicate = predicate.And(x => x.EffectiveFrom <= DateTime.UtcNow);
            predicate = predicate.And(x => x.EffectiveTo == null || x.EffectiveTo >= DateTime.UtcNow);
            predicate = predicate.And(x => userRoleIds.Contains(x.RoleId));
            predicate = predicate.And(x => x.TransactionType == transactionType || x.TransactionType == "All");

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == null || x.CompanyID == companyId.Value);
            }

            // Get limits ordered by priority (lower priority number = higher priority)
            var limits = _approvalLimitRepository.GetList(predicate.ToExpression())
                .OrderBy(x => x.Priority)
                .ToList();

            // Return the highest approval limit among all user's roles
            return limits.OrderByDescending(x => x.MaxAmount).FirstOrDefault();
        }

        /// <summary>
        /// Get user's role IDs - الحصول على معرفات أدوار المستخدم
        /// </summary>
        private List<long> GetUserRoleIds(string userId)
        {
            // In a full implementation, this would query the SysUserRole table
            // For now, return empty list which will fall back to default limit
            return new List<long>();
        }

        public void ValidateFutureDate(DateTime transactionDate, bool allowFuture = false)
        {
            if (!allowFuture && transactionDate.Date > DateTime.Today)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_FUTURE_DATE_NOT_ALLOWED)
                    .WithParameter("Date", transactionDate.ToString("yyyy-MM-dd"));
            }
        }

        #region Attachment Validations (Phase 1.7)

        /// <summary>
        /// Validate that cheque has required attachment - التحقق من وجود مرفق مطلوب للشيك
        /// </summary>
        public void ValidateChequeAttachment(Cheque cheque, bool requireAttachment = true)
        {
            if (!requireAttachment)
            {
                return;
            }

            // Check if attachment is required for this cheque type
            if (!IsAttachmentRequired(TreasuryEntityType.Cheque, cheque.ChequeType.ToString()))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(cheque.AttachmentPath))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_REQUIRED_FIELD_MISSING)
                    .WithParameter("Field", "Attachment")
                    .WithParameter("ChequeNumber", cheque.ChequeNumber ?? string.Empty)
                    .WithParameter("ChequeId", cheque.Id);
            }
        }

        /// <summary>
        /// Validate cheque attachment during creation - التحقق من مرفق الشيك عند الإنشاء
        /// </summary>
        public void ValidateChequeAttachmentRequired(CreateChequeDto dto, ChequeType chequeType)
        {
            // Check if attachment is required for this cheque type
            if (!IsAttachmentRequired(TreasuryEntityType.Cheque, chequeType.ToString()))
            {
                return;
            }

            // For incoming cheques, attachment of the physical cheque is recommended
            // This is a soft validation - could be made hard requirement in config
        }

        /// <summary>
        /// Validate reconciliation line attachment - التحقق من مرفق سطر التسوية
        /// </summary>
        public void ValidateReconciliationLineAttachment(Guid lineId, bool requireAttachment = false)
        {
            if (!requireAttachment)
            {
                return;
            }

            // For reconciliation lines that require manual review, attachment may be needed
            // This is implemented as a soft validation that can be configured per tenant
        }

        /// <summary>
        /// Check if attachment is required for entity type - التحقق من متطلبات المرفقات
        /// </summary>
        public bool IsAttachmentRequired(TreasuryEntityType entityType, string operationType = null)
        {
            // Attachment requirements based on entity type and operation
            // In production, this could be fetched from TreasuryConfig table per tenant
            return entityType switch
            {
                TreasuryEntityType.Cheque => operationType switch
                {
                    "Incoming" => true,   // Incoming cheques should have a scan
                    "Outgoing" => false,  // Outgoing cheques don't require attachment at creation
                    _ => false
                },
                TreasuryEntityType.Payment => false,  // Payments don't require attachments by default
                TreasuryEntityType.BankReconciliation => false,  // Reconciliations don't require attachments
                _ => false
            };
        }

        #endregion

        private long? GetAccountCompanyId(Guid accountId)
        {
            var bankAccount = _bankAccountRepository.GetById(accountId);
            if (bankAccount != null)
            {
                return bankAccount.CompanyID;
            }

            var cashAccount = _cashAccountRepository.GetById(accountId);
            if (cashAccount != null)
            {
                return cashAccount.CompanyID;
            }

            return null;
        }

        #endregion

        #region Cheque Lifecycle Validations (Full Lifecycle: Issued → UnderCollection → Cleared/Bounced → Cancelled)

        public void ValidateChequeLifecycleTransition(Cheque cheque, ChequeStatus newStatus)
        {
            // Full lifecycle transitions for outgoing cheques:
            // Issued → Presented → Cleared (end)
            //       → Presented → Bounced → Presented (re-present)
            //       → Cancelled (end)

            // Full lifecycle transitions for incoming cheques:
            // Issued → Presented → Cleared (end)
            //       → Presented → Bounced → Presented (re-present)
            //       → Cancelled (end)

            var validTransitions = new Dictionary<ChequeStatus, List<ChequeStatus>>
            {
                { ChequeStatus.Issued, new List<ChequeStatus> { ChequeStatus.Presented, ChequeStatus.Cancelled } },
                { ChequeStatus.Presented, new List<ChequeStatus> { ChequeStatus.Cleared, ChequeStatus.Bounced, ChequeStatus.Cancelled } },
                { ChequeStatus.Cleared, new List<ChequeStatus>() }, // Terminal state
                { ChequeStatus.Bounced, new List<ChequeStatus> { ChequeStatus.Presented, ChequeStatus.Cancelled } }, // Can re-present
                { ChequeStatus.Cancelled, new List<ChequeStatus>() } // Terminal state
            };

            if (!validTransitions.ContainsKey(cheque.Status) || !validTransitions[cheque.Status].Contains(newStatus))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_STATUS_TRANSITION_INVALID)
                    .WithParameter("FromStatus", cheque.Status.ToString())
                    .WithParameter("ToStatus", newStatus.ToString());
            }
        }

        #endregion

        #region Cash Account Cheque Validation

        public void ValidateCashAccountAcceptsCheques(Guid cashAccountId, bool isAcceptingCheque)
        {
            if (!isAcceptingCheque) return;

            var cashAccount = _cashAccountRepository.GetById(cashAccountId);
            if (cashAccount == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_NOT_FOUND)
                    .WithParameter("CashAccountId", cashAccountId);
            }

            if (!cashAccount.AcceptsCheques)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_DOES_NOT_ACCEPT_CHEQUES)
                    .WithParameter("CashAccountId", cashAccountId);
            }
        }

        #endregion

        #region Reconciliation Tolerance Validation

        public void ValidateReconciliationTolerance(BankReconciliation reconciliation, decimal toleranceAmount)
        {
            if (Math.Abs(reconciliation.FinalDifference) > toleranceAmount)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_RECONCILIATION_TOLERANCE_EXCEEDED)
                    .WithParameter("Difference", reconciliation.FinalDifference)
                    .WithParameter("Tolerance", toleranceAmount);
            }
        }

        #endregion
    }
}
