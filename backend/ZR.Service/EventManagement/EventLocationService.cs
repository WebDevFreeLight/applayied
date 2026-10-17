using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Location Information - معلومات موقع الفعاليةService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IEventLocationService), ServiceLifetime = LifeTime.Transient)]
    public class EventLocationService : BaseService<EventLocation>, IEventLocationService
    {
        /// <summary>
        /// 查询Event Location Information - معلومات موقع الفعالية列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventLocationDto> GetList(EventLocationQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Locationid asc")
                .Where(predicate.ToExpression())
                .ToPage<EventLocation, EventLocationDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Locationid"></param>
        /// <returns></returns>
        public EventLocation GetInfo(long Locationid)
        {
            var response = Queryable()
                .Where(x => x.Locationid == Locationid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public EventLocation AddEventLocation(EventLocation model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateEventLocation(EventLocation model)
        {
            return Update(model, true, "修改Event Location Information - معلومات موقع الفعالية");
        }

        /// <summary>
        /// 清空Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <returns></returns>
        public bool TruncateEventLocation()
        {
            var newTableName = $"event_locations_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("event_locations", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportEventLocation(List<EventLocation> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Locationname.IsEmpty(), "Location Name - اسم الموقع不能为空")
                .SplitError(x => x.Item.Address.IsEmpty(), "Address - العنوان不能为空")
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
        /// 导出Event Location Information - معلومات موقع الفعالية
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventLocationDto> ExportList(EventLocationQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new EventLocationDto()
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
        private static Expressionable<EventLocation> QueryExp(EventLocationQueryDto parm)
        {
            var predicate = Expressionable.Create<EventLocation>();

            predicate = predicate.AndIF(parm.Locationid != null, it => it.Locationid == parm.Locationid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Locationname), it => it.Locationname.Contains(parm.Locationname));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Venue), it => it.Venue.Contains(parm.Venue));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Address), it => it.Address.Contains(parm.Address));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.City), it => it.City.Contains(parm.City));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Country), it => it.Country.Contains(parm.Country));
            predicate = predicate.AndIF(parm.Maxcapacity != null, it => it.Maxcapacity >= parm.Maxcapacity);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Isactive), it => it.Isactive == parm.Isactive);
          //  predicate = predicate.AndIF(parm.BeginCreateTime == null, it => it.CreateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
          //  predicate = predicate.AndIF(parm.BeginUpdateTime == null, it => it.UpdateTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
