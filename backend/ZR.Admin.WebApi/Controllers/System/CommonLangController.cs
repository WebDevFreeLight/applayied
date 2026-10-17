using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Dto;
using ZR.Model.Models;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Multi-language Configuration Controller
    /// </summary>
    [Route("system/CommonLang")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class CommonLangController : BaseController
    {
        /// <summary>
        /// Multi-language configuration interface
        /// </summary>
        private readonly ICommonLangService _CommonLangService;

        public CommonLangController(ICommonLangService CommonLangService)
        {
            _CommonLangService = CommonLangService;
        }

        /// <summary>
        /// Query multi-language configuration list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:lang:list")]
        public IActionResult QueryCommonLang([FromQuery] CommonLangQueryDto parm)
        {
            if (parm.ShowMode == 2)
            {
                PagedInfo<dynamic> pagedInfo = new()
                {
                    Result = _CommonLangService.GetListToPivot(parm)
                };

                return SUCCESS(pagedInfo);
            }

            return SUCCESS(_CommonLangService.GetList(parm));
        }

        /// <summary>
        /// Query multi-language configuration list
        /// </summary>
        /// <returns></returns>
        [HttpGet("list/{lang}")]
        [AllowAnonymous]
        public IActionResult QueryCommonLangs(string lang)
        {
            var msgList = _CommonLangService.GetLangList(new CommonLangQueryDto() { LangCode = lang });

            return SUCCESS(_CommonLangService.SetLang(msgList));
        }

        /// <summary>
        /// Query multi-language configuration details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "system:lang:query")]
        public IActionResult GetCommonLang(long Id)
        {
            var response = _CommonLangService.GetFirst(x => x.Id == Id);

            var list = _CommonLangService.GetList(x => x.LangKey == response.LangKey);
            var vo = list.Adapt<List<CommonLangDto>>();
            var modal = new CommonLangDto() { LangKey = response.LangKey, LangList = vo };
            return SUCCESS(modal);
        }

        /// <summary>
        /// Query multi-language configuration details
        /// </summary>
        /// <param name="langKey"></param>
        /// <returns></returns>
        [HttpGet("key/{langKey}")]
        [ActionPermissionFilter(Permission = "system:lang:query")]
        public IActionResult GetCommonLangByKey(string langKey)
        {
            var list = _CommonLangService.GetList(x => x.LangKey == langKey);
            var vo = list.Adapt<List<CommonLangDto>>();
            var modal = new CommonLangDto() { LangKey = langKey, LangList = vo };

            return SUCCESS(modal);
        }

        /// <summary>
        /// Update multi-language configuration
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "system:lang:edit")]
        [Log(Title = "Multi-language Configuration", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateCommonLang([FromBody] CommonLangDto parm)
        {
            if (parm == null || parm.LangKey.IsEmpty())
            {
                throw new CustomException("Request entity cannot be empty");
            }

            _CommonLangService.StorageCommonLang(parm);

            return ToResponse(1);
        }

        /// <summary>
        /// Delete multi-language configuration
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "system:lang:delete")]
        [Log(Title = "Multi-language Configuration", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCommonLang(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _CommonLangService.Delete(idsArr);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete multi-language configuration
        /// </summary>
        /// <returns></returns>
        [HttpDelete("ByKey")]
        [ActionPermissionFilter(Permission = "system:lang:delete")]
        [Log(Title = "Multi-language Configuration", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCommonLangByKey(string langkey)
        {
            if (langkey.IsEmpty()) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _CommonLangService
                .Deleteable()
                .EnableDiffLogEvent()
                .Where(f => f.LangKey == langkey)
                .ExecuteCommand();
            return ToResponse(response);
        }

        /// <summary>
        /// Export multi-language configuration
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Multi-language Configuration", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "system:lang:export")]
        public IActionResult Export([FromQuery] CommonLangQueryDto parm)
        {
            parm.PageSize = 10000;
            var list = _CommonLangService.GetListToPivot(parm);

            string sFileName = ExportExcel(list, "CommonLang", "Multi-language Configuration");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }

        /// <summary>
        /// Import
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Multi-language Settings Import", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false, IsSaveResponseData = true)]
        [ActionPermissionFilter(Permission = "system:lang:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<CommonLang> list = new();
            var nowTime = DateTime.Now;
            using (var stream = formFile.OpenReadStream())
            {
                var rows = stream.Query(startCell: "A2").ToList();

                foreach (var item in rows)
                {
                    list.Add(new CommonLang() { LangCode = "zh-cn", LangKey = item.A, LangName = item.B, Addtime = nowTime });
                    list.Add(new CommonLang() { LangCode = "en", LangKey = item.A, LangName = item.C, Addtime = nowTime });
                    list.Add(new CommonLang() { LangCode = "zh-tw", LangKey = item.A, LangName = item.D, Addtime = nowTime });
                }
            }

            return SUCCESS(_CommonLangService.ImportCommonLang(list));
        }

        /// <summary>
        /// Multi-language settings import template download
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Multi-language Settings Template", BusinessType = BusinessType.EXPORT, IsSaveRequestData = true, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<CommonLang>() { }, "lang");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
