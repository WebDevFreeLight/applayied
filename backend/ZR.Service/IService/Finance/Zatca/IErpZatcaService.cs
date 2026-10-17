using ZR.Model;
using ZR.Model.Finance.ZATCA;
using ZR.Service;

namespace ZR.Service.IService.Finance.ZATCA
{
    public interface IErpZatcaInvoiceService : IBaseService<ErpZatcaInvoice>
    {
        PagedInfo<ErpZatcaInvoice> GetList(ErpZatcaInvoice parm, PagerInfo pager);
        ErpZatcaInvoice GetInfo(long zatcaInvoiceId);
        ErpZatcaInvoice CreateZatcaInvoice(long invoiceId);
        Task<bool> SubmitToZatcaAsync(long zatcaInvoiceId);
        Task<bool> CheckStatusAsync(long zatcaInvoiceId);
        int UpdateStatus(long zatcaInvoiceId, string status, string response);
        List<ErpZatcaInvoice> GetPendingInvoices();
        List<ErpZatcaInvoice> GetFailedInvoices();
        string GenerateInvoiceXml(long invoiceId);
        string SignInvoice(string invoiceXml);
    }

    public interface IErpZatcaSettingsService : IBaseService<ErpZatcaSettings>
    {
        ErpZatcaSettings GetSettings(long? BranchID = null);
        int SaveSettings(ErpZatcaSettings model);
        Task<bool> ValidateCertificateAsync(string certificatePath);
        Task<bool> TestConnectionAsync();
    }
}

