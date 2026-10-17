using System.Collections.Generic;

namespace ZR.Model
{
    /// <summary>
    /// Paging parameters
    /// </summary>
    public class PagedInfo<T>
    {
        /// <summary>
        /// Rows per page
        /// </summary>
        public int PageSize { get; set; } = 10;
        /// <summary>
        /// Current page
        /// </summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>
        /// Total records
        /// </summary>
        public int TotalNum { get; set; }
        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPage
        {
            get
            {
                if (TotalNum > 0)
                {
                    return TotalNum % this.PageSize == 0 ? TotalNum / this.PageSize : TotalNum / this.PageSize + 1;
                }
                else
                {
                    return 0;
                }
            }
            set { }
        }
        public List<T> Result { get; set; }
        public Dictionary<string, object> Extra { get; set; } = new Dictionary<string, object>();
        public PagedInfo()
        {
        }
    }
}

