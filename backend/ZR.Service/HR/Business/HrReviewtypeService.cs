using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;
using ZR.Repository;
using ZR.Service.HR.Business.IBusinessService;

namespace ZR.Service.HR.Business
{
    /// <summary>
    /// Reviewtype Information TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrReviewtypeService), ServiceLifetime = LifeTime.Transient)]
    public class HrReviewtypeService : BaseService<HrReviewtype>, IHrReviewtypeService
    {
        /// <summary>
        /// 查询Reviewtype Information Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrReviewtypeDto> GetList(HrReviewtypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Reviewtypeid asc")
                .Where(predicate.ToExpression())
                .ToPage<HrReviewtype, HrReviewtypeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Reviewtypeid"></param>
        /// <returns></returns>
        public HrReviewtype GetInfo(long Reviewtypeid)
        {
            var response = Queryable()
                .Where(x => x.Reviewtypeid == Reviewtypeid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Reviewtype Information Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrReviewtype AddHrReviewtype(HrReviewtype model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Reviewtype Information Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrReviewtype(HrReviewtype model)
        {
            return Update(model, true, "修改Reviewtype Information Table");
        }

        /// <summary>
        /// 清空Reviewtype Information Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrReviewtype()
        {
            var newTableName = $"hr_reviewtype_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_reviewtype", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Reviewtype Information Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrReviewtype(List<HrReviewtype> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "Review  Type name 不能为空")
                .SplitError(x => x.Item.Create_time.IsEmpty(), "Create time不能为空")
                .SplitError(x => x.Item.Update_time.IsEmpty(), "Update time不能为空")
                .SplitError(x => x.Item.Remark.IsEmpty(), "Remark不能为空")
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
        /// 导出Reviewtype Information Table
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrReviewtypeDto> ExportList(HrReviewtypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrReviewtypeDto()
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
        private static Expressionable<HrReviewtype> QueryExp(HrReviewtypeQueryDto parm)
        {
            var predicate = Expressionable.Create<HrReviewtype>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
