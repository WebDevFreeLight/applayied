using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    [AppService(ServiceType = typeof(IOpsGeoTemporalRuleService), ServiceLifetime = LifeTime.Transient)]
    public class OpsGeoTemporalRuleService : BaseService<OpsGeoTemporalRule>, IOpsGeoTemporalRuleService
    {
        public PagedInfo<OpsGeoTemporalRuleDto> GetList(OpsGeoTemporalRuleQueryDto parm)
        {
            var predicate = Expressionable.Create<OpsGeoTemporalRule>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.RuleName), it => it.RuleName.Contains(parm.RuleName));
            predicate = predicate.AndIF(parm.SeasonId != null, it => it.SeasonId == parm.SeasonId);
            predicate = predicate.AndIF(parm.ZoneId != null, it => it.ZoneId == parm.ZoneId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.RuleType), it => it.RuleType == parm.RuleType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.Create_time, OrderByType.Desc)
                .ToPage<OpsGeoTemporalRule, OpsGeoTemporalRuleDto>(parm);

            return response;
        }

        public OpsGeoTemporalRule GetInfo(long RuleId)
        {
            return Queryable().Where(x => x.RuleId == RuleId).First();
        }

        public OpsGeoTemporalRule AddOpsGeoTemporalRule(OpsGeoTemporalRule model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateOpsGeoTemporalRule(OpsGeoTemporalRule model)
        {
            return Update(model, true, "Update geo-temporal rule");
        }

        public PagedInfo<OpsGeoTemporalRuleDto> ExportList(OpsGeoTemporalRuleQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }
    }
}
