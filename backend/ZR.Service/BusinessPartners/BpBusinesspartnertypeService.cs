using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Repository;
using ZR.Service.BusinessPartners.IService;
using ZR.Model.BusinessPartners;

namespace ZR.Service.BusinessPartners
{
    /// <summary>
    /// BusinessPartnerTypeService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBpBusinesspartnertypeService), ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnertypeService : BaseService<BpBusinesspartnertype>, IBpBusinesspartnertypeService
    {
        /// <summary>
        /// 查询BusinessPartnerType列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnertypeDto> GetList(BpBusinesspartnertypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Typeid asc")
                .Where(predicate.ToExpression())
                .ToPage<BpBusinesspartnertype, BpBusinesspartnertypeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        public BpBusinesspartnertype GetInfo(long Typeid)
        {
            var response = Queryable()
                .Where(x => x.Typeid == Typeid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加BusinessPartnerType
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BpBusinesspartnertype AddBpBusinesspartnertype(BpBusinesspartnertype model)
        {
            // Check if code is required
            if (string.IsNullOrEmpty(model.code))
            {
                throw new ArgumentException("Code is required");
            }

            // Check if code already exists
            var exists = Queryable().Any(x => x.code == model.code);
            if (exists)
            {
                throw new InvalidOperationException($"Code '{model.code}' already exists");
            }

            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改BusinessPartnerType
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBpBusinesspartnertype(BpBusinesspartnertype model)
        {
            // Check if code is required
            if (string.IsNullOrEmpty(model.code))
            {
                throw new ArgumentException("Code is required");
            }

            // Check if code already exists for another record
            var exists = Queryable().Any(x => x.code == model.code && x.Typeid != model.Typeid);
            if (exists)
            {
                throw new InvalidOperationException($"Code '{model.code}' already exists");
            }

            // Check if any BpBusinesspartner is using this type
            var inUse = Context.Queryable<BpBusinesspartner>().Any(x => x.Type == long.Parse(model.code));
            if (inUse)
            {
                throw new InvalidOperationException($"Cannot update this type because it is being used by business partners");
            }

            return Update(model, true, "修改BusinessPartnerType");
        }

        /// <summary>
        /// 删除BusinessPartnerType
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        public int DeleteBpBusinesspartnertype(long Typeid)
        {
            // Check if any BpBusinesspartner is using this type
            var inUse = Context.Queryable<BpBusinesspartner>().Any(x => x.Type == Typeid);
            if (inUse)
            {
                throw new InvalidOperationException($"Cannot delete this type because it is being used by business partners");
            }

            return Delete(Typeid);
        }

        /// <summary>
        /// 清空BusinessPartnerType
        /// </summary>
        /// <returns></returns>
        public bool TruncateBpBusinesspartnertype()
        {
            var newTableName = $"bp_businesspartnertype_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("bp_businesspartnertype", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入BusinessPartnerType
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBpBusinesspartnertype(List<BpBusinesspartnertype> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Name.IsEmpty(), "Review  Type name不能为空")
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
        /// 导出BusinessPartnerType
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnertypeDto> ExportList(BpBusinesspartnertypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BpBusinesspartnertypeDto()
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
        private static Expressionable<BpBusinesspartnertype> QueryExp(BpBusinesspartnertypeQueryDto parm)
        {
            var predicate = Expressionable.Create<BpBusinesspartnertype>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.code), it => it.Name.Contains(parm.code));
            predicate = predicate.AndIF(parm.BeginCreateTime != null, it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null, it => it.Create_time <= parm.EndCreateTime);
            predicate = predicate.AndIF(parm.BeginUpdateTime != null, it => it.Update_time >= parm.BeginUpdateTime);
            predicate = predicate.AndIF(parm.EndUpdateTime != null, it => it.Update_time  <= parm.EndUpdateTime);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Remark), it => it.Remark.Contains(parm.Remark));
            return predicate;
        }
    }
}
