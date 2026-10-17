using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Information - معلومات الفعاليةService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IEventService), ServiceLifetime = LifeTime.Transient)]
    public class EventService : BaseService<Event>, IEventService
    {
        /// <summary>
        /// 查询Event Information - معلومات الفعالية列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventDto> GetList(EventQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Eventid asc")
                .Where(predicate.ToExpression())
                .ToPage<Event, EventDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Eventid"></param>
        /// <returns></returns>
        public Event GetInfo(long Eventid)
        {
            var response = Queryable()
                .Where(x => x.Eventid == Eventid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Event Information - معلومات الفعالية
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Event AddEvent(Event model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Event Information - معلومات الفعالية
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateEvent(Event model)
        {
            return Update(model, true, "修改Event Information - معلومات الفعالية");
        }

        /// <summary>
        /// 清空Event Information - معلومات الفعالية
        /// </summary>
        /// <returns></returns>
        public bool TruncateEvent()
        {
            var newTableName = $"event_events_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("event_events", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Event Information - معلومات الفعالية
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportEvent(List<Event> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Eventcode.IsEmpty(), "Event Code - رمز الفعالية不能为空")
                .SplitError(x => x.Item.Eventname.IsEmpty(), "Event Name (English) - اسم الفعالية بالإنجليزية不能为空")
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
        /// 导出Event Information - معلومات الفعالية
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<EventDto> ExportList(EventQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new EventDto()
                {
                    EventtypeLabel = it.Eventtype.GetConfigValue<Model.System.SysDictData>("sql_type"),
                    StatusLabel = it.Status.GetConfigValue<Model.System.SysDictData>("sql_status"),
                    LocationidLabel = it.Locationid.GetConfigValue<Model.System.SysDictData>("sql_location"),
                    OrganizerLabel = it.Organizer.GetConfigValue<Model.System.SysDictData>("sql_hr_employee"),
                    //IspublicLabel = it.Ispublic.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<Event> QueryExp(EventQueryDto parm)
        {
            var predicate = Expressionable.Create<Event>();

            predicate = predicate.AndIF(parm.Eventid != null, it => it.Eventid == parm.Eventid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Eventcode), it => it.Eventcode == parm.Eventcode);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Eventname), it => it.Eventname.Contains(parm.Eventname));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Eventnamear), it => it.Eventnamear.Contains(parm.Eventnamear));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Description), it => it.Description.Contains(parm.Description));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Descriptionar), it => it.Descriptionar.Contains(parm.Descriptionar));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Eventtype), it => it.Eventtype == parm.Eventtype);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
          //  predicate = predicate.AndIF(parm.BeginStartdate == null, it => it.Startdate >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginStartdate != null, it => it.Startdate >= parm.BeginStartdate);
            predicate = predicate.AndIF(parm.EndStartdate != null, it => it.Startdate <= parm.EndStartdate);
          //  predicate = predicate.AndIF(parm.BeginEnddate == null, it => it.Enddate >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginEnddate != null, it => it.Enddate >= parm.BeginEnddate);
            predicate = predicate.AndIF(parm.EndEnddate != null, it => it.Enddate <= parm.EndEnddate);
          //  predicate = predicate.AndIF(parm.BeginRegistrationstartdate == null, it => it.Registrationstartdate >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginRegistrationstartdate != null, it => it.Registrationstartdate >= parm.BeginRegistrationstartdate);
            predicate = predicate.AndIF(parm.EndRegistrationstartdate != null, it => it.Registrationstartdate <= parm.EndRegistrationstartdate);
          //  predicate = predicate.AndIF(parm.BeginRegistrationenddate == null, it => it.Registrationenddate >= DateTime.Now.ToShortDateString().ParseToDateTime());
            predicate = predicate.AndIF(parm.BeginRegistrationenddate != null, it => it.Registrationenddate >= parm.BeginRegistrationenddate);
            predicate = predicate.AndIF(parm.EndRegistrationenddate != null, it => it.Registrationenddate <= parm.EndRegistrationenddate);
            predicate = predicate.AndIF(parm.Locationid != null, it => it.Locationid == parm.Locationid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Locationname), it => it.Locationname.Contains(parm.Locationname));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Address), it => it.Address.Contains(parm.Address));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.City), it => it.City.Contains(parm.City));
            predicate = predicate.AndIF(parm.Expectedvisitors != null, it => it.Expectedvisitors >= parm.Expectedvisitors);
            predicate = predicate.AndIF(parm.Maxcapacity != null, it => it.Maxcapacity >= parm.Maxcapacity);
            predicate = predicate.AndIF(parm.Currentoccupancy != null, it => it.Currentoccupancy >= parm.Currentoccupancy);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Organizer), it => it.Organizer.Contains(parm.Organizer));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Contactemail), it => it.Contactemail.Contains(parm.Contactemail));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Contactphone), it => it.Contactphone.Contains(parm.Contactphone));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Website), it => it.Website.Contains(parm.Website));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Ispublic), it => it.Ispublic == parm.Ispublic);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Requiresregistration), it => it.Requiresregistration == parm.Requiresregistration);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Ispaidevent), it => it.Ispaidevent == parm.Ispaidevent);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Currency), it => it.Currency.Contains(parm.Currency));
            predicate = predicate.AndIF(parm.Eventmanagerid != null, it => it.Eventmanagerid == parm.Eventmanagerid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Tags), it => it.Tags.Contains(parm.Tags));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Isactive), it => it.Isactive == parm.Isactive);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Country), it => it.Country.Contains(parm.Country));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Venue), it => it.Venue.Contains(parm.Venue));
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
