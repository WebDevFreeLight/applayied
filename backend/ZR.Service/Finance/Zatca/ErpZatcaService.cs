using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ZATCA;
using ZR.Model.Sales;
using ZR.Repository;
using ZR.Service.IService.Finance.ZATCA;

namespace ZR.Service.Finance.ZATCA
{
    [AppService(ServiceType = typeof(IErpZatcaInvoiceService), ServiceLifetime = LifeTime.Transient)]
    public class ErpZatcaInvoiceService : BaseService<ErpZatcaInvoice>, IErpZatcaInvoiceService
    {
     



        public PagedInfo<ErpZatcaInvoice> GetList(ErpZatcaInvoice parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpZatcaInvoice>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceNumber), it => it.InvoiceNumber.Contains(parm.InvoiceNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.SubmissionStatus), it => it.SubmissionStatus == parm.SubmissionStatus);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.Create_time).ToPage(pager);
        }

        public ErpZatcaInvoice GetInfo(long zatcaInvoiceId) => GetFirst(x => x.ZatcaInvoiceId == zatcaInvoiceId);

        public ErpZatcaInvoice CreateZatcaInvoice(long invoiceId)
        {
            var invoice = Context.Queryable<ErpSalesInvoice>().First(x => x.InvoiceId == invoiceId);
            if (invoice == null) return null;

            var zatcaInvoice = new ErpZatcaInvoice
            {
                ZatcaInvoiceId = SnowFlakeSingle.Instance.NextId(),
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceType = invoice.InvoiceType == "simplified" ? "388" : "380",
                SubmissionStatus = "pending",
                Create_time = DateTime.Now
            };

            zatcaInvoice.XmlContent = GenerateInvoiceXml(invoiceId);
            Insert(zatcaInvoice);
            return zatcaInvoice;
        }

        public async Task<bool> SubmitToZatcaAsync(long zatcaInvoiceId)
        {
            var zatcaInvoice = GetInfo(zatcaInvoiceId);
            if (zatcaInvoice == null || string.IsNullOrEmpty(zatcaInvoice.XmlContent)) return false;

            try
            {
                // Sign the invoice
                var signedXml = SignInvoice(zatcaInvoice.XmlContent);

                // TODO: Implement actual ZATCA API call
                // Using ZATCA SDK or direct API integration
                // var response = await zatcaClient.SubmitInvoiceAsync(signedXml);

                await Task.Delay(100); // Simulate API call

                Context.Updateable<ErpZatcaInvoice>()
                    .SetColumns(it => new ErpZatcaInvoice { SubmissionStatus = "submitted", SignedXml = signedXml, SubmissionDate = DateTime.Now, Update_time = DateTime.Now })
                    .Where(it => it.ZatcaInvoiceId == zatcaInvoiceId).ExecuteCommand();

                return true;
            }
            catch (Exception ex)
            {
                Context.Updateable<ErpZatcaInvoice>()
                    .SetColumns(it => new ErpZatcaInvoice { SubmissionStatus = "failed", ErrorMessages = ex.Message, Update_time = DateTime.Now })
                    .Where(it => it.ZatcaInvoiceId == zatcaInvoiceId).ExecuteCommand();
                return false;
            }
        }

        public async Task<bool> CheckStatusAsync(long zatcaInvoiceId)
        {
            var zatcaInvoice = GetInfo(zatcaInvoiceId);
            if (zatcaInvoice == null) return false;

            await Task.CompletedTask;
            // TODO: Implement ZATCA status check API
            return zatcaInvoice.SubmissionStatus == "accepted";
        }

        public int UpdateStatus(long zatcaInvoiceId, string status, string response) => Context.Updateable<ErpZatcaInvoice>().SetColumns(it => new ErpZatcaInvoice { SubmissionStatus = status, ZatcaResponse = response, Update_time = DateTime.Now }).Where(it => it.ZatcaInvoiceId == zatcaInvoiceId).ExecuteCommand();
        public List<ErpZatcaInvoice> GetPendingInvoices() => Queryable().Where(x => x.SubmissionStatus == "pending").ToList();
        public List<ErpZatcaInvoice> GetFailedInvoices() => Queryable().Where(x => x.SubmissionStatus == "failed").ToList();

        public string GenerateInvoiceXml(long invoiceId)
        {
            var invoice = Context.Queryable<ErpSalesInvoice>().First(x => x.InvoiceId == invoiceId);
            if (invoice == null) return null;

            // Generate UBL 2.1 compliant XML for ZATCA
            var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"">
    <cbc:ID>{invoice.InvoiceNumber}</cbc:ID>
    <cbc:IssueDate>{invoice.InvoiceDate:yyyy-MM-dd}</cbc:IssueDate>
    <cbc:InvoiceTypeCode>{(invoice.InvoiceType == "simplified" ? "388" : "380")}</cbc:InvoiceTypeCode>
    <cac:LegalMonetaryTotal>
        <cbc:TaxExclusiveAmount currencyID=""SAR"">{invoice.Subtotal}</cbc:TaxExclusiveAmount>
        <cbc:TaxInclusiveAmount currencyID=""SAR"">{invoice.TotalAmount}</cbc:TaxInclusiveAmount>
        <cbc:PayableAmount currencyID=""SAR"">{invoice.TotalAmount}</cbc:PayableAmount>
    </cac:LegalMonetaryTotal>
</Invoice>";
            return xml;
        }

        public string SignInvoice(string invoiceXml)
        {
            // TODO: Implement proper XML signing with ZATCA certificate
            // This would involve:
            // 1. Loading the ZATCA certificate
            // 2. Creating XML signature
            // 3. Adding signature to invoice XML
            return invoiceXml; // Placeholder
        }
    }
}

