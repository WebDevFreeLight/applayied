using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.BusinessPartners.Dto;
using ZR.Model.BusinessPartners;
using ZR.Repository;
using ZR.Service.BusinessPartners.IService;

namespace ZR.Service.BusinessPartners
{
    /// <summary>
    /// businesspartnerService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBpBusinesspartnerService), ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnerService : BaseService<BpBusinesspartner>, IBpBusinesspartnerService
    {
        /// <summary>
        /// 查询businesspartner列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnerDto_Res> GetList(BpBusinesspartnerQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Partnerid asc")
                .Where(predicate.ToExpression())
                .ToPage<BpBusinesspartner, BpBusinesspartnerDto_Res>(parm);
            return response;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Partnerid"></param>
        /// <returns></returns>
        public BpBusinesspartner GetInfo(long Partnerid)
        {
            var response = Queryable()
                .Where(x => x.Partnerid == Partnerid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加businesspartner
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BpBusinesspartner AddBpBusinesspartner(BpBusinesspartner model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改businesspartner
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBpBusinesspartner(BpBusinesspartner model)
        {
            return Update(model, true, "修改businesspartner");
        }

        /// <summary>
        /// 清空businesspartner
        /// </summary>
        /// <returns></returns>
        public bool TruncateBpBusinesspartner()
        {
            var newTableName = $"bp_businesspartner_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("bp_businesspartner", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入businesspartner
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBpBusinesspartner(List<BpBusinesspartner> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Type.IsEmpty(), "Type不能为空")
                .SplitError(x => x.Item.Status.IsEmpty(), "Status不能为空")
                .SplitError(x => x.Item.Kind.IsEmpty(), "Kind不能为空")
                .SplitError(x => x.Item.Assigneddepartment.IsEmpty(), "Assigneddepartment不能为空")
                .SplitError(x => x.Item.Assignedto.IsEmpty(), "Assignedto不能为空")
                .SplitError(x => x.Item.Compliancestatus.IsEmpty(), "Compliancestatus不能为空")
                .SplitError(x => x.Item.Preferredcontactmethod.IsEmpty(), "Preferredcontactmethod不能为空")
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
        /// 导出businesspartner
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BpBusinesspartnerDto_Res> ExportList(BpBusinesspartnerQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BpBusinesspartnerDto_Res()
                {
                    //TypeLabel = it.Type.GetConfigValue<Model.System.SysDictData>("sql_bp_businesspartnertype"),
                    //StatusLabel = it.Status.GetConfigValue<Model.System.SysDictData>("sql_bp_businesspartnerstatus"), df
                    //KindLabel = it.Kind.GetConfigValue<Model.System.SysDictData>("sql_bp_businesspartnerkind"),
                    //AssigneddepartmentLabel = it.Assigneddepartment.GetConfigValue<Model.System.SysDictData>("sql_sys_dept"),
                    ////AssignedtoLabel = it.Assignedto.GetConfigValue<Model.System.SysDictData>("sql_hr_employee"),
                    //CompliancestatusLabel = it.Compliancestatus.GetConfigValue<Model.System.SysDictData>("sql_bp_compliancestatus"),
                    //PreferredcontactmethodLabel = it.Preferredcontactmethod.GetConfigValue<Model.System.SysDictData>("sql_bp_contactmethod"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<BpBusinesspartner> QueryExp(BpBusinesspartnerQueryDto parm)
        {
            var predicate = Expressionable.Create<BpBusinesspartner>();

            predicate = predicate.AndIF(parm.Type != null, it => it.Type == parm.Type);
            predicate = predicate.AndIF(parm.Status != null, it => it.Status == parm.Status);
            predicate = predicate.AndIF(parm.Kind != null, it => it.Kind == parm.Kind);
            predicate = predicate.AndIF(parm.Assigneddepartment != null, it => it.Assigneddepartment == parm.Assigneddepartment);
            predicate = predicate.AndIF(parm.Assignedto != null, it => it.Assignedto == parm.Assignedto);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Contactfirstname), it => it.Contactfirstname.Contains(parm.Contactfirstname));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Contactlastname), it => it.Contactlastname.Contains(parm.Contactlastname));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Businessphone), it => it.Businessphone.Contains(parm.Businessphone));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Contactphone), it => it.Contactphone.Contains(parm.Contactphone));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Businessemail), it => it.Businessemail.Contains(parm.Businessemail));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Contactemail), it => it.Contactemail.Contains(parm.Contactemail));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Mobile), it => it.Mobile.Contains(parm.Mobile));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Buildingnumber), it => it.Buildingnumber.Contains(parm.Buildingnumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Streetaddress), it => it.Streetaddress.Contains(parm.Streetaddress));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Unitnumber), it => it.Unitnumber.Contains(parm.Unitnumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.City), it => it.City.Contains(parm.City));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Province), it => it.Province.Contains(parm.Province));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Postalcode), it => it.Postalcode.Contains(parm.Postalcode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Country), it => it.Country.Contains(parm.Country));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Compliancestatus), it => it.Compliancestatus == parm.Compliancestatus);
            predicate = predicate.AndIF(parm.Preferredcontactmethod != null, it => it.Preferredcontactmethod == parm.Preferredcontactmethod);
            return predicate;
        }
    }
}
