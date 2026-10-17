using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    public interface ISysOperLogService
    {
        public void InsertOperlog(SysOperLog operLog);

        /// <summary>
        /// Query system operation log collection
        /// </summary>
        /// <param name="operLog">Operation log object</param>
        /// <returns>Operation log collection</returns>
        public PagedInfo<SysOperLog> SelectOperLogList(SysOperLogQueryDto operLog);

        /// <summary>
        /// Clear operation logs
        /// </summary>
        public void CleanOperLog();

        /// <summary>
        /// Batch delete system operation logs
        /// </summary>
        /// <param name="operIds">Operation log IDs to be deleted</param>
        /// <returns>Result</returns>
        public int DeleteOperLogByIds(long[] operIds);

        /// <summary>
        /// Query operation log details
        /// </summary>
        /// <param name="operId">Operation ID</param>
        /// <returns>Operation log object</returns>
        public SysOperLog SelectOperLogById(long operId);
    }
}

