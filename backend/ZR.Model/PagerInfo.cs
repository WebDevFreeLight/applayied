namespace ZR.Model
{
    public class PagerInfo
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNum { get; set; }
        /// <summary>
        /// Records per page
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Total records
        /// </summary>
        public int TotalNum { get; set; }
        /// <summary>
        /// Sort field
        /// </summary>
        public string Sort { get; set; } = string.Empty;
        /// <summary>
        /// Sort type, frontend passes "ascending" or "descending"
        /// </summary>
        public string SortType { get; set; } = string.Empty;
        public PagerInfo()
        {
            PageNum = 1;
            PageSize = 20;
        }

        public PagerInfo(int page = 1, int pageSize = 20)
        {
            PageNum = page;
            PageSize = pageSize;
        }
    }
}

