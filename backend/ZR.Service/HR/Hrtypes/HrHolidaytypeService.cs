using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.HR.Hrtypes.Dto;
using ZR.Model.HR.Hrtypes;
using ZR.Repository;
using ZR.Service.HR.Hrtypes.IHrtypesService;

namespace ZR.Service.HR.Hrtypes
{
    /// <summary>
    /// HolidayType TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrHolidaytypeService), ServiceLifetime = LifeTime.Transient)]
    public class HrHolidaytypeService : BaseService<HrHolidaytype>, IHrHolidaytypeService
    {
        /// <summary>
        /// 查询HolidayType Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrHolidaytypeDto> GetList(HrHolidaytypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<HrHolidaytype, HrHolidaytypeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public HrHolidaytype GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加HolidayType Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrHolidaytype AddHrHolidaytype(HrHolidaytype model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改HolidayType Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrHolidaytype(HrHolidaytype model)
        {
            return Update(model, true, "修改HolidayType Table");
        }

        /// <summary>
        /// 清空HolidayType Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrHolidaytype()
        {
            var newTableName = $"hr_holidaytype_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_holidaytype", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入HolidayType Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrHolidaytype(List<HrHolidaytype> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "Name不能为空")
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
        /// 导出HolidayType Table
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrHolidaytypeDto> ExportList(HrHolidaytypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrHolidaytypeDto()
                {
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<HrHolidaytype> QueryExp(HrHolidaytypeQueryDto parm)
        {
            var predicate = Expressionable.Create<HrHolidaytype>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            return predicate;
        }
    }
}
