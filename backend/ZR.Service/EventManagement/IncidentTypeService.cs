using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Incident Type - أنواع الحوادثService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IIncidentTypeService), ServiceLifetime = LifeTime.Transient)]
    public class IncidentTypeService : BaseService<IncidentType>, IIncidentTypeService
    {
        /// <summary>
        /// 查询Incident Type - أنواع الحوادث列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<IncidentTypeDto> GetList(IncidentTypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Typeid asc")
                .Where(predicate.ToExpression())
                .ToPage<IncidentType, IncidentTypeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        public IncidentType GetInfo(long Typeid)
        {
            var response = Queryable()
                .Where(x => x.Typeid == Typeid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Incident Type - أنواع الحوادث
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IncidentType AddIncidentType(IncidentType model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Incident Type - أنواع الحوادث
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateIncidentType(IncidentType model)
        {
            return Update(model, true, "修改Incident Type - أنواع الحوادث");
        }

        /// <summary>
        /// 清空Incident Type - أنواع الحوادث
        /// </summary>
        /// <returns></returns>
        public bool TruncateIncidentType()
        {
            var newTableName = $"event_incident_types_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("event_incident_types", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Incident Type - أنواع الحوادث
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportIncidentType(List<IncidentType> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Typecode.IsEmpty(), "Type Code - رمز النوع不能为空")
                .SplitError(x => x.Item.Typename.IsEmpty(), "Type Name (English) - اسم النوع بالإنجليزية不能为空")
                .SplitError(x => x.Item.Defaultpriority.IsEmpty(), "Default Priority - الأولوية الافتراضية不能为空")
                .SplitError(x => x.Item.Defaultseverity.IsEmpty(), "Default Severity - الخطورة الافتراضية不能为空")
                //.WhereColumns(it => it.UserName)//如果不是主键可以这样实现（多字段it=>new{it.x1,it.x2}）
                .ToStorage();
            var result = x.AsInsertable.ExecuteCommand();//插入可插入部分;

            string msg = $"插入{x.InsertList.Count} 更新{x.UpdateList.Count} 错误数据{x.ErrorList.Count} 不计算数据{x.IgnoreList.Count} 删除数据{x.DeleteList.Count} 总共{x.TotalList.Count}";                    
            Console.WriteLine(msg);

            //输出错误信息               
            foreach (var item in x.ErrorList)
            {
                Console.WriteLine("错误" + item.StorageMessage);
            }
            foreach (var item in x.IgnoreList)
            {
                Console.WriteLine("忽略" + item.StorageMessage);
            }

            return (msg, x.ErrorList, x.IgnoreList);
        }

        /// <summary>
        /// 导出Incident Type - أنواع الحوادث
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<IncidentTypeDto> ExportList(IncidentTypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new IncidentTypeDto()
                {
                    RequiresimmediateactionLabel = it.Requiresimmediateaction.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<IncidentType> QueryExp(IncidentTypeQueryDto parm)
        {
            var predicate = Expressionable.Create<IncidentType>();

            predicate = predicate.AndIF(parm.Typeid != null, it => it.Typeid == parm.Typeid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typecode), it => it.Typecode.Contains(parm.Typecode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typename), it => it.Typename.Contains(parm.Typename));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typenamear), it => it.Typenamear.Contains(parm.Typenamear));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Description), it => it.Description.Contains(parm.Description));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Defaultpriority), it => it.Defaultpriority.Contains(parm.Defaultpriority));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Defaultseverity), it => it.Defaultseverity.Contains(parm.Defaultseverity));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Assignedteam), it => it.Assignedteam.Contains(parm.Assignedteam));
            predicate = predicate.AndIF(parm.Slaresponseminutes != null, it => it.Slaresponseminutes >= parm.Slaresponseminutes);
            predicate = predicate.AndIF(parm.Slaresolutionminutes != null, it => it.Slaresolutionminutes >= parm.Slaresolutionminutes);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Requiresimmediateaction), it => it.Requiresimmediateaction == parm.Requiresimmediateaction);
            predicate = predicate.AndIF(parm.Displayorder != null, it => it.Displayorder >= parm.Displayorder);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Isactive), it => it.Isactive == parm.Isactive);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Severity), it => it.Severity.Contains(parm.Severity));
          //  predicate = predicate.AndIF(parm.BeginCreateTime == null, it => it.CreateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
          //  predicate = predicate.AndIF(parm.BeginUpdateTime == null, it => it.UpdateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            return predicate;
        }
    }
}
