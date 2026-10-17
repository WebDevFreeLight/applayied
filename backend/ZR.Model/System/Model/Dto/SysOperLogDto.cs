using System;

namespace ZR.Model.System.Dto
{
    public class SysOperLogQueryDto : PagerInfo
    {
        /// <summary>
        /// ????
        /// </summary>
        public string OperName { get; set; }
        /// <summary>
        /// ???? 0=??,1=??,2=??,3=??,4=??,5=??,6=??,7=??,8=????,9=????
        /// </summary>
        public int? BusinessType { get; set; }
        public int[] BusinessTypes { get; set; }
        /// <summary>
        /// ??
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string OperParam { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class SysOperLogDto : SysBase
    {
        /// <summary>
        /// ??
        /// </summary>
        public int PageNum { get; set; }
        public int PageSize { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string OperName { get; set; }
        /// <summary>
        /// ???? 0=??,1=??,2=??,3=??,4=??,5=??,6=??,7=??,8=????,9=????
        /// </summary>
        public int BusinessType { get; set; } = -1;
        /// <summary>
        /// ??
        /// </summary>
        public int Status { get; set; } = -1;
        /// <summary>
        /// ????
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string OperParam { get; set; }
    }
}

