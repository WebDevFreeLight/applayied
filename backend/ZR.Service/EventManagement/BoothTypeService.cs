using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Booth Type - أنواع الأكشاكService业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBoothTypeService), ServiceLifetime = LifeTime.Transient)]
    public class BoothTypeService : BaseService<BoothType>, IBoothTypeService
    {
        /// <summary>
        /// 查询Booth Type - أنواع الأكشاك列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BoothTypeDto> GetList(BoothTypeQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                //.OrderBy("Typeid asc")
                .Where(predicate.ToExpression())
                .ToPage<BoothType, BoothTypeDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        public BoothType GetInfo(long Typeid)
        {
            var response = Queryable()
                .Where(x => x.Typeid == Typeid)
                .First();

            return response;
        }

        /// <summary>
        /// 添加Booth Type - أنواع الأكشاك
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BoothType AddBoothType(BoothType model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        /// <summary>
        /// 修改Booth Type - أنواع الأكشاك
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBoothType(BoothType model)
        {
            return Update(model, true, "修改Booth Type - أنواع الأكشاك");
        }

        /// <summary>
        /// 清空Booth Type - أنواع الأكشاك
        /// </summary>
        /// <returns></returns>
        public bool TruncateBoothType()
        {
            var newTableName = $"event_booth_types_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("event_booth_types", newTableName);
            }
            
            return Truncate();
        }
        /// <summary>
        /// 导入Booth Type - أنواع الأكشاك
        /// </summary>
        /// <returns></returns>
        public (string, object, object) ImportBoothType(List<BoothType> list)
        {
            var x = Context.Storageable(list)
                .SplitInsert(it => !it.Any())
                .SplitError(x => x.Item.Typecode.IsEmpty(), "Type Code - رمز النوع不能为空")
                .SplitError(x => x.Item.Typename.IsEmpty(), "Type Name (English) - اسم النوع بالإنجليزية不能为空")
                .SplitError(x => x.Item.Defaultpricepersqm.IsEmpty(), "Default Price Per SQM - السعر الافتراضي للمتر المربع不能为空")
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
        /// 导出Booth Type - أنواع الأكشاك
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BoothTypeDto> ExportList(BoothTypeQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BoothTypeDto()
                {
                    IncludeselectricityLabel = it.Includeselectricity.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<BoothType> QueryExp(BoothTypeQueryDto parm)
        {
            var predicate = Expressionable.Create<BoothType>();

            predicate = predicate.AndIF(parm.Typeid != null, it => it.Typeid == parm.Typeid);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typecode), it => it.Typecode == parm.Typecode);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typename), it => it.Typename.Contains(parm.Typename));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Typenamear), it => it.Typenamear.Contains(parm.Typenamear));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Description), it => it.Description.Contains(parm.Description));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Colorcode), it => it.Colorcode.Contains(parm.Colorcode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Includeselectricity), it => it.Includeselectricity == parm.Includeselectricity);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Includesinternet), it => it.Includesinternet == parm.Includesinternet);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Includesfurniture), it => it.Includesfurniture == parm.Includesfurniture);
            predicate = predicate.AndIF(parm.Displayorder != null, it => it.Displayorder >= parm.Displayorder);
            return predicate;
        }
    }
}
