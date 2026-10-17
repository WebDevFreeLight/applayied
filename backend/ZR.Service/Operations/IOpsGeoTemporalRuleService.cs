using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsGeoTemporalRuleService : IBaseService<OpsGeoTemporalRule>
    {
        PagedInfo<OpsGeoTemporalRuleDto> GetList(OpsGeoTemporalRuleQueryDto parm);
        OpsGeoTemporalRule GetInfo(long RuleId);
        OpsGeoTemporalRule AddOpsGeoTemporalRule(OpsGeoTemporalRule model);
        int UpdateOpsGeoTemporalRule(OpsGeoTemporalRule model);
        PagedInfo<OpsGeoTemporalRuleDto> ExportList(OpsGeoTemporalRuleQueryDto parm);
    }
}
