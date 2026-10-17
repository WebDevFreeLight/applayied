using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Parameter Configuration Controller
    /// </summary>
    [Route("system/config")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysConfigController : BaseController
    {
        /// <summary>
        /// Parameter configuration interface
        /// </summary>
        private readonly ISysConfigService _SysConfigService;

        public SysConfigController(ISysConfigService SysConfigService)
        {
            _SysConfigService = SysConfigService;
        }

        /// <summary>
        /// Query parameter configuration list
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:config:list")]
        public IActionResult QuerySysConfig([FromQuery] SysConfigQueryDto parm)
        {
            var predicate = Expressionable.Create<SysConfig>();

            predicate = predicate.AndIF(!parm.ConfigType.IsEmpty(), m => m.ConfigType == parm.ConfigType);
            predicate = predicate.AndIF(!parm.ConfigName.IsEmpty(), m => m.ConfigName.Contains(parm.ConfigType));
            predicate = predicate.AndIF(!parm.ConfigKey.IsEmpty(), m => m.ConfigKey.Contains(parm.ConfigKey));
            predicate = predicate.AndIF(!parm.BeginTime.IsEmpty(), m => m.Create_time >= parm.BeginTime);
            predicate = predicate.AndIF(!parm.BeginTime.IsEmpty(), m => m.Create_time <= parm.EndTime);

            var response = _SysConfigService.GetPages(predicate.ToExpression(), parm);

            return SUCCESS(response);
        }

        /// <summary>
        /// Query parameter configuration details
        /// </summary>
        /// <param name="ConfigId"></param>
        /// <returns></returns>
        [HttpGet("{ConfigId}")]
        [ActionPermissionFilter(Permission = "system:config:query")]
        public IActionResult GetSysConfig(int ConfigId)
        {
            var response = _SysConfigService.GetId(ConfigId);

            return SUCCESS(response);
        }

        /// <summary>
        /// Query parameter value by parameter key name
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        [HttpGet("configKey/{configKey}")]
        [AllowAnonymous]
        public IActionResult GetConfigKey(string configKey)
        {
            var response = _SysConfigService.Queryable().First(f => f.ConfigKey == configKey);

            return SUCCESS(response?.ConfigValue);
        }

        /// <summary>
        /// Add parameter configuration
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "system:config:add")]
        [Log(Title = "Parameter Configuration Add", BusinessType = BusinessType.INSERT)]
        public IActionResult AddSysConfig([FromBody] SysConfigDto parm)
        {
            if (parm == null)
            {
                throw new CustomException("Request parameters are invalid");
            }
            var model = parm.Adapt<SysConfig>().ToCreate(HttpContext);

            return SUCCESS(_SysConfigService.Insert(model, it => new
            {
                it.ConfigName,
                it.ConfigKey,
                it.ConfigValue,
                it.ConfigType,
                it.Create_by,
                it.Create_time,
                it.Remark,
            }));
        }

        /// <summary>
        /// Update parameter configuration
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "system:config:update")]
        [Log(Title = "Parameter Configuration Modify", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateSysConfig([FromBody] SysConfigDto parm)
        {
            if (parm == null)
            {
                throw new CustomException("Request entity cannot be empty");
            }
            var model = parm.Adapt<SysConfig>().ToUpdate(HttpContext);

            var response = _SysConfigService.Update(w => w.ConfigId == model.ConfigId, it => new SysConfig()
            {
                ConfigName = model.ConfigName,
                ConfigKey = model.ConfigKey,
                ConfigValue = model.ConfigValue,
                ConfigType = model.ConfigType,
                Update_by = model.Update_by,
                Update_time = model.Update_time,
                Remark = model.Remark
            });

            return SUCCESS(response);
        }

        /// <summary>
        /// Delete parameter configuration
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "system:config:remove")]
        [Log(Title = "Parameter Configuration Delete", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteSysConfig(string ids)
        {
            int[] idsArr = Tools.SpitIntArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }
            int sysCount = _SysConfigService.Count(s => s.ConfigType == "Y" && idsArr.Contains(s.ConfigId));
            if (sysCount > 0) { return ToResponse(ApiResult.Error($"Delete failed Id: System built-in parameters cannot be deleted!")); }
            var response = _SysConfigService.Delete(idsArr);

            return SUCCESS(response);
        }
    }
}
