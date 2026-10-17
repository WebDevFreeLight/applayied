using Infrastructure.Attribute;
using ZR.Common;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ?????
    /// </summary>
    [AppService(ServiceType = typeof(ISysDictDataService), ServiceLifetime = LifeTime.Transient)]
    public class SysDictDataService : BaseService<SysDictData>, ISysDictDataService
    {
        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="dictData"></param>
        /// <param name="pagerInfo"></param>
        /// <returns></returns>
        public PagedInfo<SysDictData> SelectDictDataList(SysDictData dictData, PagerInfo pagerInfo)
        {
            //return SysDictDataRepository.SelectDictDataList(dictData, pagerInfo);
            var exp = Expressionable.Create<SysDictData>();
            exp.AndIF(!string.IsNullOrEmpty(dictData.DictLabel), it => it.DictLabel.Contains(dictData.DictLabel));
            exp.AndIF(!string.IsNullOrEmpty(dictData.Status), it => it.Status == dictData.Status);
            exp.AndIF(!string.IsNullOrEmpty(dictData.DictType), it => it.DictType == dictData.DictType);
            return GetPages(exp.ToExpression(), pagerInfo);
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="dictType"></param>
        /// <returns></returns>
        public List<SysDictData> SelectDictDataByType(string dictType)
        {
            string CK = $"SelectDictDataByType_{dictType}";

            var list = Queryable()
                .WithCache(CK, 60 * 10)
                .Where(f => f.Status == "0" && f.DictType == dictType)
            .OrderBy(it => it.DictSort)
            .ToList();

            return list;
        }
        public List<SysDictDataDto> SelectDictDataByTypes(string[] dictTypes)
        {
            //string CK = $"SelectDictDataByTypes_{dictTypes}";

            var list = Queryable()
            //.WithCache(CK, 60 * 30)
            .Where(f => f.Status == "0" && dictTypes.Contains(f.DictType))
            .OrderBy(it => it.DictSort)
            .Select((it) => new SysDictDataDto()
            {
                Label = it.DictLabel,
                Value = it.DictValue
            }, true)
            .ToList();

            return list;
        }
        /// <summary>
        /// ??????ID????
        /// </summary>
        /// <param name="dictCode"></param>
        /// <returns></returns>
        public SysDictData SelectDictDataById(long dictCode)
        {
            string CK = $"SelectDictDataByCode_{dictCode}";
            if (CacheHelper.GetCache(CK) is not SysDictData list)
            {
                list = GetFirst(f => f.DictCode == dictCode);
                CacheHelper.SetCache(CK, list, 5);
            }
            return list;
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public long InsertDictData(SysDictData dict)
        {
            return Insertable(dict).ExecuteReturnBigIdentity();
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public long UpdateDictData(SysDictData dict)
        {
            var result = Update(w => w.DictCode == dict.DictCode, it => new SysDictData()
            {
                Remark = dict.Remark,
                Update_time = DateTime.Now,
                DictSort = dict.DictSort,
                DictLabel = dict.DictLabel,
                DictValue = dict.DictValue,
                Status = dict.Status,
                CssClass = dict.CssClass,
                ListClass = dict.ListClass,
                LangKey = dict.LangKey,
                Extend1 = dict.Extend1,
                Extend2 = dict.Extend2,
            });

            CacheHelper.Remove($"SelectDictDataByCode_{dict.DictCode}");
            return result;
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int UpdateStatus(SysDictData data)
        {
            return Update(data, it => new { it.Status }, f => f.DictCode == data.DictCode);
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="dictCodes"></param>
        /// <returns></returns>
        public int DeleteDictDataByIds(long[] dictCodes)
        {
            return Delete(dictCodes);
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="old_dictType">?????</param>
        /// <param name="new_dictType">?????</param>
        /// <returns></returns>
        public int UpdateDictDataType(string old_dictType, string new_dictType)
        {
            //???DictType????where??
            return Context.Updateable<SysDictData>()
                .SetColumns(t => new SysDictData() { DictType = new_dictType })
                .Where(f => f.DictType == old_dictType)
                .ExecuteCommand();
        }

        /// <summary>
        /// ???????????sql
        /// </summary>
        /// <param name="sysDictType"></param>
        /// <returns></returns>
        public List<SysDictDataDto> SelectDictDataByCustomSql(SysDictType sysDictType)
        {
            return Context.Ado.SqlQuery<SysDictDataDto>(sysDictType?.CustomSql).ToList();
        }
    }
}

