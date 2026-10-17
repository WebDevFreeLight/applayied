using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Zone Type - أنواع المناطقService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IEventZoneTypeService), ServiceLifetime = LifeTime.Transient)]
    public class EventZoneTypeService : BaseService<EventZoneType>, IEventZoneTypeService
    {
        /// <summary>
        /// 查询Zone Type - أنواع المناطق列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventZoneTypeDto> GetList(EventZoneTypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Typeid asc")
                .Where(predicate.ToExpression())
                .ToPage<EventZoneType, EventZoneTypeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        public EventZoneType GetInfo(long Typeid)
        {
            var response = Queryable()
                .Where(x => x.Typeid == Typeid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Zone Type - أنواع المناطق
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EventZoneType AddEventZoneType(EventZoneType model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Zone Type - أنواع المناطق
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateEventZoneType(EventZoneType model)
        {
            return Update(model, true, "修改Zone Type - أنواع المناطق");
        }

        /// <summary>
        /// 清空Zone Type - أنواع المناطق
        /// </summary>
        /// <returns></returns>
        public bool TruncateEventZoneType()
        {
            var newTableName = $"event_zone_types_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("event_zone_types", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Zone Type - أنواع المناطق
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportEventZoneType(List<EventZoneType> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Typecode.IsEmpty(), "Type Code - رمز النوع不能为空")
                .SplitError(x => x.Item.Typename.IsEmpty(), "Type Name (English) - اسم النوع بالإنجليزية不能为空")
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
        /// 导出Zone Type - أنواع المناطق
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventZoneTypeDto> ExportList(EventZoneTypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new EventZoneTypeDto()
                {
                    IsactiveLabel = it.Isactive.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<EventZoneType> QueryExp(EventZoneTypeQueryDto parm)
        {
            var predicate = Expressionable.Create<EventZoneType>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typecode), it => it.Typecode == parm.Typecode);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typename), it => it.Typename.Contains(parm.Typename));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typenamear), it => it.Typenamear.Contains(parm.Typenamear));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Description), it => it.Description.Contains(parm.Description));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Colorcode), it => it.Colorcode == parm.Colorcode);
            predicate = predicate.AndIF(parm.Displayorder != null, it => it.Displayorder == parm.Displayorder);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Isactive), it => it.Isactive == parm.Isactive);
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
