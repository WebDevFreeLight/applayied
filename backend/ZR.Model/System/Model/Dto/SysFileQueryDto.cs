namespace ZR.Model.System.Dto
{
    /// <summary>
    /// ????????
    /// </summary>
    public class SysFileDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// ?????
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// ?????? eg:/uploads/20220202
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// ???? eg:/uploads
        /// </summary>
        public string StorePath { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string FileSize { get; set; }
        /// <summary>
        /// ?????
        /// </summary>
        public string FileExt { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public string Create_by { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public DateTime? Create_time { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public int? StoreType { get; set; }
        /// <summary>
        /// ??
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        public string AccessUrl { get; set; }
        public string ClassifyType { get; set; }

        public SysFileDto() { }
        public SysFileDto(string originFileName, string fileName, string ext, string fileSize, string storePath, string accessUrl, string create_by)
        {
            StorePath = storePath;
            RealName = originFileName;
            FileName = fileName;
            FileExt = ext;
            FileSize = fileSize;
            AccessUrl = accessUrl;
            Create_by = create_by;
            Create_time = DateTime.Now;
        }
    }
    public class SysFileQueryDto : PagerInfo
    {
        /// <summary>
        /// ??
        /// </summary>
        public string ClassifyType { get; set; }
        public DateTime? BeginCreate_time { get; set; }
        public DateTime? EndCreate_time { get; set; }
        public int? StoreType { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? FileId { get; set; }
        public int? CategoryId { get; set; }
    }
}

