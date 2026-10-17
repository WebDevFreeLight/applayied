using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Repository;
using ZR.Service.BusinessPartners.IService;

namespace ZR.Service.BusinessPartners
{
    /// <summary>
    /// Bp_PartnerDocsmentsService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBpPartnerdocsmentsService), ServiceLifetime = LifeTime.Transient)]
    public class BpPartnerdocsmentsService : BaseService<BpPartnerdocsments>, IBpPartnerdocsmentsService
    {
        /// <summary>
        /// 查询Bp_PartnerDocsments列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpPartnerdocsmentsDto> GetList(BpPartnerdocsmentsQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<BpPartnerdocsments, BpPartnerdocsmentsDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public BpPartnerdocsments GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Bp_PartnerDocsments
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BpPartnerdocsments AddBpPartnerdocsments(BpPartnerdocsments model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Bp_PartnerDocsments
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBpPartnerdocsments(BpPartnerdocsments model)
        {
            return Update(model, true, "修改Bp_PartnerDocsments");
        }

        /// <summary>
        /// 清空Bp_PartnerDocsments
        /// </summary>
        /// <returns></returns>
        public bool TruncateBpPartnerdocsments()
        {
            var newTableName = $"bp_partnerdocsments_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("bp_partnerdocsments", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Bp_PartnerDocsments
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBpPartnerdocsments(List<BpPartnerdocsments> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Doctype.IsEmpty(), "document type不能为空")
                .SplitError(x => x.Item.Partner.IsEmpty(), "Partner不能为空")
                .SplitError(x => x.Item.FileUrl.IsEmpty(), "FileUrl不能为空")
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
        /// 导出Bp_PartnerDocsments
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpPartnerdocsmentsDto> ExportList(BpPartnerdocsmentsQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BpPartnerdocsmentsDto()
                {
                    DoctypeLabel = it.Doctype.GetConfigValue<Model.System.SysDictData>("sql_bp_businesspartnerdoctype"),
                    PartnerLabel = it.Partner.GetConfigValue<Model.System.SysDictData>("sql_bp_businesspartner"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<BpPartnerdocsments> QueryExp(BpPartnerdocsmentsQueryDto parm)
        {
            var predicate = Expressionable.Create<BpPartnerdocsments>();

            predicate = predicate.AndIF(parm.Doctype != null, it => it.Doctype == parm.Doctype);
            predicate = predicate.AndIF(parm.Partner != null, it => it.Partner == parm.Partner);
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
