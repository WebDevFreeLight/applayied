using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Data Dictionary Information
    /// </summary>
    [Route("system/dict/data")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysDictDataController : BaseController
    {
        private readonly ISysDictDataService SysDictDataService;
        private readonly ISysDictService SysDictService;

        public SysDictDataController(ISysDictService sysDictService, ISysDictDataService sysDictDataService)
        {
            SysDictService = sysDictService;
            SysDictDataService = sysDictDataService;
        }

        /// <summary>
        /// Search
        /// </summary>
        /// <param name="dictData"></param>
        /// <param name="pagerInfo"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:list")]
        [HttpGet("list")]
        public IActionResult List([FromQuery] SysDictData dictData, [FromQuery] PagerInfo pagerInfo)
        {
            var list = SysDictDataService.SelectDictDataList(dictData, pagerInfo);

            if (dictData.DictType.StartsWith("sql_"))
            {
                var result = SysDictService.SelectDictDataByCustomSql(dictData.DictType);

                list.Result.AddRange(result.Adapt<List<SysDictData>>());
                list.TotalNum += result.Count;
            }
            return SUCCESS(list);
        }

        /// <summary>
        /// Query dictionary data information by dictionary type
        /// </summary>
        /// <param name="dictType"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("type/{dictType}")]
        public IActionResult DictType(string dictType)
        {
            return SUCCESS(SysDictDataService.SelectDictDataByType(dictType));
        }

        /// <summary>
        /// Query dictionary data information by dictionary type
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("types")]
        public IActionResult DictTypes([FromBody] List<SysdictDataParamDto> dto)
        {
            var list = SysDictDataService.SelectDictDataByTypes(dto.Select(f => f.DictType).ToArray());
            var dataVos = GetDicts(dto.Select(f => f.DictType).ToArray());

            return SUCCESS(dataVos);
        }

        /// <summary>
        /// Mobile app using uniapp
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("dicts")]
        public async Task<IActionResult> GetDictTypes()
        {
            var data = await HttpContext.GetBodyAsync();
            return SUCCESS(GetDicts(data.ToObject<string[]>()));
        }

        private List<SysdictDataParamDto> GetDicts([FromBody]string[] dicts)
        {
            List<SysdictDataParamDto> dataVos = new();
            var list = SysDictDataService.SelectDictDataByTypes(dicts);

            foreach (var dic in dicts)
            {
                SysdictDataParamDto vo = new()
                {
                    DictType = dic,
                    List = list.FindAll(f => f.DictType == dic)
                };
                if (dic.StartsWith("cus_") || dic.StartsWith("sql_"))
                {
                    vo.List.AddRange(SysDictService.SelectDictDataByCustomSql(dic));
                }
                dataVos.Add(vo);
            }
            return dataVos;
        }

        /// <summary>
        /// Query dictionary data details
        /// </summary>
        /// <param name="dictCode"></param>
        /// <returns></returns>
        [HttpGet("info/{dictCode}")]
        public IActionResult GetInfo(long dictCode)
        {
            return SUCCESS(SysDictDataService.SelectDictDataById(dictCode));
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:add")]
        [Log(Title = "Dictionary Data", BusinessType = BusinessType.INSERT)]
        [HttpPost()]
        public IActionResult Add([FromBody] SysDictData dict)
        {
            dict.Create_by = HttpContext.GetName();
            dict.Create_time = DateTime.Now;
            return SUCCESS(SysDictDataService.InsertDictData(dict));
        }

        /// <summary>
        /// Modify
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:edit")]
        [Log(Title = "Dictionary Data", BusinessType = BusinessType.UPDATE)]
        [HttpPut()]
        public IActionResult Edit([FromBody] SysDictData dict)
        {
            dict.Update_by = HttpContext.GetName();
            return SUCCESS(SysDictDataService.UpdateDictData(dict));
        }

        /// <summary>
        /// Modify dictionary data status
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("changeStatus")]
        [Log(Title = "Modify Dictionary Data Status", BusinessType = BusinessType.UPDATE)]
        [ActionPermissionFilter(Permission = "system:dict:edit")]
        public IActionResult ChangeStatus([FromBody] SysDictData dto)
        {
            int result = SysDictDataService.UpdateStatus(dto);

            return ToResponse(result);
        }

        /// <summary>
        /// Delete dictionary type
        /// </summary>
        /// <param name="dictCode"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dict:remove")]
        [Log(Title = "Dictionary Type", BusinessType = BusinessType.DELETE)]
        [HttpDelete("{dictCode}")]
        public IActionResult Remove(string dictCode)
        {
            long[] dictCodes = Common.Tools.SpitLongArrary(dictCode);

            return SUCCESS(SysDictDataService.DeleteDictDataByIds(dictCodes));
        }
    }
}

