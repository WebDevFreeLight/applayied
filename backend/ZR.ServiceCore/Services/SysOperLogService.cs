using Infrastructure;
using Infrastructure.Attribute;
using ZR.Infrastructure.Constant;
using ZR.Infrastructure.Helper;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Model.System.Generate;
using ZR.Repository;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ????
    /// </summary>
    [AppService(ServiceType = typeof(ISysOperLogService), ServiceLifetime = LifeTime.Transient)]
    public class SysOperLogService : BaseService<SysOperLog>, ISysOperLogService
    {
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="operLog">????</param>
        public void InsertOperlog(SysOperLog operLog)
        {


            try
            {

                if (operLog.OperParam != null && operLog.OperParam.Length >= 1000)
                {
                    operLog.OperParam = operLog.OperParam[..1000];
                }
                //sysOperLogRepository.AddSysOperLog(operLog);
                Insert(operLog);
            }
            catch (Exception exH)
            {

                Console.WriteLine(exH.Message);
            }



        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="sysOper">??????</param>
        /// <returns>??????</returns>
        public PagedInfo<SysOperLog> SelectOperLogList(SysOperLogQueryDto sysOper)
        {
            var exp = Expressionable.Create<SysOperLog>();
            exp.AndIF(sysOper.BeginTime == null, it => it.OperTime >= DateTime.Now.ToShortDateString().ParseToDateTime());
            exp.AndIF(sysOper.BeginTime != null, it => it.OperTime >= sysOper.BeginTime && it.OperTime <= sysOper.EndTime);
            exp.AndIF(sysOper.Title.IfNotEmpty(), it => it.Title.Contains(sysOper.Title));
            exp.AndIF(sysOper.OperName.IfNotEmpty(), it => it.OperName.Contains(sysOper.OperName));
            exp.AndIF(sysOper.BusinessType != null, it => it.BusinessType == sysOper.BusinessType);
            exp.AndIF(sysOper.Status != null, it => it.Status == sysOper.Status);
            exp.AndIF(sysOper.OperParam != null, it => it.OperParam.Contains(sysOper.OperParam));
            exp.AndIF(sysOper.BusinessTypes != null, it => sysOper.BusinessTypes.Contains(it.BusinessType));

            var list = Queryable()
                .Where(exp.ToExpression())
                .OrderBy(x => x.OperId, OrderByType.Desc)
                .ToPage(sysOper);
            foreach (var item in list.Result)
            {
                if (!HttpContextExtension.HasSensitivePerm(App.HttpContext, SensitivePerms.ViewRealIP))
                {
                    item.OperIp = MaskUtil.MaskIp(item.OperIp);
                }
            }
            return list;
        }

        /// <summary>
        /// ??????
        /// </summary>
        public void CleanOperLog()
        {
            var newTableName = $"sys_oper_log_{DateTime.Now:yyyyMMdd}";
            if (Queryable().Any() && !Context.DbMaintenance.IsAnyTable(newTableName))
            {
                Context.DbMaintenance.BackupTable("sys_oper_log", newTableName);
            }

            Truncate();
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="operIds">?????????ID</param>
        /// <returns>??</returns>
        public int DeleteOperLogByIds(long[] operIds)
        {
            return Delete(operIds);
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="operId">??ID</param>
        /// <returns>??????</returns>
        public SysOperLog SelectOperLogById(long operId)
        {
            return GetById(operId);
        }
    }
}

