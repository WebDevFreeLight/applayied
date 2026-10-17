using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Hr.Dto;
using ZR.Repository;
using ZR.Service.Hr.IHrService;
using ZR.Model.HR.Attendance.Configuration;

namespace ZR.Service.Hr
{
    /// <summary>
    /// Holiday TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrHolidayService), ServiceLifetime = LifeTime.Transient)]
    public class HrHolidayService : BaseService<HrHoliday>, IHrHolidayService
    {
        /// <summary>
        /// 查询Holiday Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrHolidayDto> GetList(HrHolidayQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Holidayid asc")
                .Where(predicate.ToExpression())
                .ToPage<HrHoliday, HrHolidayDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Holidayid"></param>
        /// <returns></returns>
        public HrHoliday GetInfo(long Holidayid)
        {
            var response = Queryable()
                .Where(x => x.Holidayid == Holidayid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Holiday Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrHoliday AddHrHoliday(HrHoliday model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Holiday Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrHoliday(HrHoliday model)
        {
            return Update(model, true, "修改Holiday Table");
        }

        /// <summary>
        /// 清空Holiday Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrHoliday()
        {
            var newTableName = $"hr_holiday_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_holiday", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Holiday Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrHoliday(List<HrHoliday> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Holidayid.IsEmpty(), "Holidayid不能为空")
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
        /// 导出Holiday Table
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrHolidayDto> ExportList(HrHolidayQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrHolidayDto()
                {
                    HolidaytypeLabel = it.Holidaytype.GetConfigValue<Model.System.SysDictData>("sql_hr_holidaytype"),
                    IspaidLabel = it.Ispaid.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<HrHoliday> QueryExp(HrHolidayQueryDto parm)
        {
            var predicate = Expressionable.Create<HrHoliday>();

            predicate = predicate.AndIF(parm.Holidayid != null, it => it.Holidayid == parm.Holidayid);
            predicate = predicate.AndIF(parm.BeginHolidaydate != null, it => it.Holidaydate >= parm.BeginHolidaydate);
            predicate = predicate.AndIF(parm.EndHolidaydate != null, it => it.Holidaydate <= parm.EndHolidaydate);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Holidayname), it => it.Holidayname.Contains(parm.Holidayname));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Holidaynamear), it => it.Holidaynamear.Contains(parm.Holidaynamear));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Holidaytype), it => it.Holidaytype == parm.Holidaytype);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Ispaid), it => it.Ispaid == parm.Ispaid);
            predicate = predicate.AndIF(parm.Year != null, it => it.Year == parm.Year);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Description), it => it.Description.Contains(parm.Description));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
