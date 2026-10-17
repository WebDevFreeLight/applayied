using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.HR.Business.Dto;
using ZR.Repository;
using ZR.Service.HR.Business.IBusinessService;
using ZR.Model.HR.Performance;

namespace ZR.Service.HR.Business
{
    /// <summary>
    /// Performance Review TableService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IHrPerformanceReviewService), ServiceLifetime = LifeTime.Transient)]
    public class HrPerformanceReviewService : BaseService<HrPerformanceReview>, IHrPerformanceReviewService
    {
        /// <summary>
        /// 查询Performance Review Table列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrPerformanceReviewDto> GetList(HrPerformanceReviewQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Reviewid asc")
                .Where(predicate.ToExpression())
                .ToPage<HrPerformanceReview, HrPerformanceReviewDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Reviewid"></param>
        /// <returns></returns>
        public HrPerformanceReview GetInfo(long Reviewid)
        {
            var response = Queryable()
                .Where(x => x.ReviewId == Reviewid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Performance Review Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HrPerformanceReview AddHrPerformanceReview(HrPerformanceReview model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Performance Review Table
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateHrPerformanceReview(HrPerformanceReview model)
        {
            return Update(model, true, "修改Performance Review Table");
        }

        /// <summary>
        /// 清空Performance Review Table
        /// </summary>
        /// <returns></returns>
        public bool TruncateHrPerformanceReview()
        {
            var newTableName = $"hr_performance_review_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("hr_performance_review", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Performance Review Table
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportHrPerformanceReview(List<HrPerformanceReview> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Reviewtype.IsEmpty(), "Reviewtype不能为空")
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
        /// 导出Performance Review Table
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<HrPerformanceReviewDto> ExportList(HrPerformanceReviewQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new HrPerformanceReviewDto()
                {
                    ReviewtypeLabel = it.Reviewtype.GetConfigValue<Model.System.SysDictData>("sql_reviewtype"),
                    EmployeeidLabel = it.Employeeid.GetConfigValue<Model.System.SysDictData>("sql_hremployee"),
                    StatusLabel = it.Status.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<HrPerformanceReview> QueryExp(HrPerformanceReviewQueryDto parm)
        {
            var predicate = Expressionable.Create<HrPerformanceReview>();

            predicate = predicate.AndIF(parm.Reviewid != null, it => it.ReviewId == parm.Reviewid);
            predicate = predicate.AndIF(parm.BeginDuedate != null, it => it.Duedate >= parm.BeginDuedate);
            predicate = predicate.AndIF(parm.EndDuedate != null, it => it.Duedate <= parm.EndDuedate);
            predicate = predicate.AndIF(parm.Reviewyear != null, it => it.Reviewyear == parm.Reviewyear);
            predicate = predicate.AndIF(parm.BeginReviewdate != null, it => it.Reviewdate >= parm.BeginReviewdate);
            predicate = predicate.AndIF(parm.EndReviewdate != null, it => it.Reviewdate <= parm.EndReviewdate);
            predicate = predicate.AndIF(parm.Reviewquarter != null, it => it.Reviewquarter==parm.Reviewquarter);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Reviewtype), it => it.Reviewtype == parm.Reviewtype);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Reviewperiod), it => it.Reviewperiod.Contains(parm.Reviewperiod));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            predicate = predicate.AndIF(parm.Employeeid != null, it => it.Employeeid == parm.Employeeid);
            predicate = predicate.AndIF(parm.Reviewerid != null, it => it.Reviewerid == parm.Reviewerid);
            predicate = predicate.AndIF(parm.BeginReviewperiodstart != null, it => it.Reviewperiodstart >= parm.BeginReviewperiodstart);
            predicate = predicate.AndIF(parm.EndReviewperiodstart != null, it => it.Reviewperiodstart <= parm.EndReviewperiodstart);
            predicate = predicate.AndIF(parm.BeginReviewperiodend != null, it => it.Reviewperiodend >= parm.BeginReviewperiodend);
            predicate = predicate.AndIF(parm.EndReviewperiodend != null, it => it.Reviewperiodend <= parm.EndReviewperiodend);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Strengths), it => it.Strengths == parm.Strengths);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Improvements), it => it.Improvements == parm.Improvements);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Goals), it => it.Goals.Contains(parm.Goals));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Comments), it => it.Comments.Contains(parm.Comments));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Areasforimprovement), it => it.Areasforimprovement == parm.Areasforimprovement);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Goalsachieved), it => it.Goalsachieved == parm.Goalsachieved);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Trainingneeds), it => it.Trainingneeds == parm.Trainingneeds);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Careeraspirations), it => it.Careeraspirations.Contains(parm.Careeraspirations));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Managercomments), it => it.Managercomments.Contains(parm.Managercomments));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Employeecomments), it => it.Employeecomments.Contains(parm.Employeecomments));
            predicate = predicate.AndIF(parm.BeginSubmitteddate != null, it => it.Submitteddate >= parm.BeginSubmitteddate);
            predicate = predicate.AndIF(parm.EndSubmitteddate != null, it => it.Submitteddate <= parm.EndSubmitteddate);
            predicate = predicate.AndIF(parm.BeginApproveddate != null, it => it.Approveddate >= parm.BeginApproveddate);
            predicate = predicate.AndIF(parm.EndApproveddate != null, it => it.Approveddate <= parm.EndApproveddate);
            predicate = predicate.AndIF(parm.BeginSelfreviewdate != null, it => it.Selfreviewdate >= parm.BeginSelfreviewdate);
            predicate = predicate.AndIF(parm.EndSelfreviewdate != null, it => it.Selfreviewdate <= parm.EndSelfreviewdate);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Developmentareas), it => it.Developmentareas.Contains(parm.Developmentareas));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Category), it => it.Category.Contains(parm.Category));
            return predicate;
        }
    }
}
