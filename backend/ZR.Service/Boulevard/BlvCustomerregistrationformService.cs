using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;
using ZR.Repository;
using ZR.Service.Boulevard.IBoulevardService;
using Infrastructure;

namespace ZR.Service.Boulevard
{
    /// <summary>
    /// CustomerRegistrationFormService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBlvCustomerregistrationformService), ServiceLifetime = LifeTime.Transient)]
    public class BlvCustomerregistrationformService : BaseService<BlvCustomerregistrationform>, IBlvCustomerregistrationformService
    {
        /// <summary>
        /// 查询CustomerRegistrationForm列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvCustomerregistrationformDto> GetList(BlvCustomerregistrationformQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Id asc")
                .Where(predicate.ToExpression())
                .ToPage<BlvCustomerregistrationform, BlvCustomerregistrationformDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public BlvCustomerregistrationform GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加CustomerRegistrationForm
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BlvCustomerregistrationform AddBlvCustomerregistrationform(BlvCustomerregistrationform model)
        {
            // Check if Email already exists
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingEmail = Queryable().Where(x => x.Email == model.Email).Any();
                if (existingEmail)
                {
                    throw  new  CustomException(  "Email is already registered");
                }
            }

            // Check if Phone already exists
            if (!string.IsNullOrEmpty(model.Phone))
            {
                var existingPhone = Queryable().Where(x => x.Phone == model.Phone).Any();
                if (existingPhone)
                {
                    throw new CustomException(ResultCode.FAIL, "Phone number is already registered");
                }
            }

            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改CustomerRegistrationForm
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBlvCustomerregistrationform(BlvCustomerregistrationform model)
        {
            return Update(model, true, "修改CustomerRegistrationForm");
        }

        /// <summary>
        /// 清空CustomerRegistrationForm
        /// </summary>
        /// <returns></returns>
        public bool TruncateBlvCustomerregistrationform()
        {
            var newTableName = $"blv_customerregistrationform_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("blv_customerregistrationform", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入CustomerRegistrationForm
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBlvCustomerregistrationform(List<BlvCustomerregistrationform> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Id.IsEmpty(), "id不能为空")
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
        /// 导出CustomerRegistrationForm
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BlvCustomerregistrationformDto> ExportList(BlvCustomerregistrationformQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BlvCustomerregistrationformDto()
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
        private static Expressionable<BlvCustomerregistrationform> QueryExp(BlvCustomerregistrationformQueryDto parm)
        {
            var predicate = Expressionable.Create<BlvCustomerregistrationform>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name.Contains(parm.Name));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Phone), it => it.Phone.Contains(parm.Phone));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Email), it => it.Email.Contains(parm.Email));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Notes), it => it.Notes.Contains(parm.Notes));
            return predicate;
        }
    }
}
