using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISysDictService
    {
        public List<SysDictType> GetAll();
        public PagedInfo<SysDictType> SelectDictTypeList(SysDictType dictType, PagerInfo pager);

        /// <summary>
        /// Check if dictionary type name is unique
        /// </summary>
        /// <param name="dictType">Dictionary type</param>
        /// <returns></returns>
        public string CheckDictTypeUnique(SysDictType dictType);

        /// <summary>
        /// Batch delete dictionary data information
        /// </summary>
        /// <param name="dictIds"></param>
        /// <returns></returns>
        public int DeleteDictTypeByIds(long[] dictIds);

        /// <summary>
        /// Insert dictionary type
        /// </summary>
        /// <param name="sysDictType"></param>
        /// <returns></returns>
        public long InsertDictType(SysDictType sysDictType);

        /// <summary>
        /// Update dictionary type
        /// </summary>
        /// <param name="sysDictType"></param>
        /// <returns></returns>
        public int UpdateDictType(SysDictType sysDictType);

        /// <summary>
        /// Get dictionary information
        /// </summary>
        /// <param name="dictId"></param>
        /// <returns></returns>
        SysDictType GetInfo(long dictId);

        List<SysDictDataDto> SelectDictDataByCustomSql(string dictType);
    }
}

