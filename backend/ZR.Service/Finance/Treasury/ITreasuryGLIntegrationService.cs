using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury GL Integration Service Interface - واجهة خدمة تكامل الخزينة مع دفتر الأستاذ
    /// Handles creation of GL journals for treasury transactions
    /// </summary>
    public interface ITreasuryGLIntegrationService
    {
        // Payment GL Integration
        GLJournal CreatePaymentJournal(Payment payment, string userName);
        GLJournal ReversePaymentJournal(Payment payment, string userName, string reason);
        CreateJournalDto PreviewPaymentJournal(Payment payment);

        // Cheque GL Integration
        GLJournal CreateChequeIssuedJournal(Cheque cheque, string userName);
        GLJournal CreateChequeClearedJournal(Cheque cheque, string userName);
        Guid CreateChequeBouncedJournal(Cheque cheque, string userName);
        Guid CreateBankFeesJournal(Cheque cheque, decimal bankFees, string userName);
        CreateJournalDto PreviewChequeJournal(Cheque cheque, ChequeStatus targetStatus);

        // Bank Reconciliation GL Integration
        GLJournal CreateReconciliationAdjustmentJournal(BankReconciliation reconciliation, decimal adjustmentAmount, string description, string userName);

        // Configuration
        TreasuryGLAccountConfig GetGLAccountConfig(long? tenantId = null);
        void SetGLAccountConfig(TreasuryGLAccountConfig config);
    }

}
