using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Assets.Dto;
using ZR.Service.IService.Finance.Assets;

namespace ZR.Admin.WebApi.Controllers.Finance.Assets
{
    /// <summary>
    /// Asset Transaction Controller - تحكم معاملات الأصول
    /// </summary>
    [Verify]
    [Route("erp/assets/[controller]")]
    [ApiController]
    public class AssetTransactionController : BaseController
    {
        private readonly IAssetTransactionService _assetTransactionService;

        public AssetTransactionController(IAssetTransactionService assetTransactionService)
        {
            _assetTransactionService = assetTransactionService;
        }

        /// <summary>
        /// Get transaction list with paging - الحصول على قائمة المعاملات مع ترقيم الصفحات
        /// </summary>
        /// <param name="parm">Query parameters</param>
        /// <returns>Paged list of transactions</returns>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] AssetTransactionQueryDto parm)
        {
            var response = _assetTransactionService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get transaction by ID - الحصول على معاملة بالمعرف
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <returns>Transaction details</returns>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _assetTransactionService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Transaction not found - المعاملة غير موجودة");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get transactions for an asset - الحصول على معاملات لأصل معين
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        /// <returns>List of transactions</returns>
        [HttpGet("asset/{assetId}")]
        public IActionResult GetAssetTransactions(Guid assetId)
        {
            var response = _assetTransactionService.GetAssetTransactions(assetId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Record purchase transaction - تسجيل معاملة شراء
        /// </summary>
        /// <param name="dto">Purchase data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("purchase")]
        [Log(Title = "Record Asset Purchase", BusinessType = BusinessType.INSERT)]
        public IActionResult RecordPurchase([FromBody] CreateAssetTransactionDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _assetTransactionService.RecordPurchase(
                dto.AssetId,
                dto.Amount,
                dto.TransactionDate,
                userId,
                dto.Notes,
                dto.ReferenceNumber);
            return SUCCESS(result);
        }

        /// <summary>
        /// Record depreciation transaction - تسجيل معاملة إهلاك
        /// </summary>
        /// <param name="dto">Depreciation data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("depreciation")]
        [Log(Title = "Record Asset Depreciation", BusinessType = BusinessType.INSERT)]
        public IActionResult RecordDepreciation([FromBody] CreateAssetTransactionDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _assetTransactionService.RecordDepreciation(
                dto.AssetId,
                dto.Amount,
                dto.TransactionDate,
                userId,
                dto.GLJournalId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Record sale transaction - تسجيل معاملة بيع
        /// </summary>
        /// <param name="dto">Sale data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("sale")]
        [Log(Title = "Record Asset Sale", BusinessType = BusinessType.INSERT)]
        public IActionResult RecordSale([FromBody] CreateAssetTransactionDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _assetTransactionService.RecordSale(
                dto.AssetId,
                dto.Amount,
                dto.GainLoss ?? 0,
                dto.TransactionDate,
                userId,
                dto.Notes,
                dto.ReferenceNumber);
            return SUCCESS(result);
        }

        /// <summary>
        /// Record disposal transaction - تسجيل معاملة استبعاد
        /// </summary>
        /// <param name="dto">Disposal data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("disposal")]
        [Log(Title = "Record Asset Disposal", BusinessType = BusinessType.INSERT)]
        public IActionResult RecordDisposal([FromBody] CreateAssetTransactionDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _assetTransactionService.RecordDisposal(
                dto.AssetId,
                dto.Amount,
                dto.TransactionDate,
                userId,
                dto.Notes,
                dto.ReferenceNumber);
            return SUCCESS(result);
        }

        /// <summary>
        /// Record revaluation transaction - تسجيل معاملة إعادة تقييم
        /// </summary>
        /// <param name="dto">Revaluation data</param>
        /// <returns>Created transaction</returns>
        [HttpPost("revaluation")]
        [Log(Title = "Record Asset Revaluation", BusinessType = BusinessType.INSERT)]
        public IActionResult RecordRevaluation([FromBody] CreateAssetTransactionDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _assetTransactionService.RecordRevaluation(
                dto.AssetId,
                dto.Amount,
                dto.TransactionDate,
                userId,
                dto.Notes,
                dto.ReferenceNumber);
            return SUCCESS(result);
        }
    }
}

