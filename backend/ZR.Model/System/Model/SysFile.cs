namespace ZR.Model.System
{
    [Tenant("0")]
    [SugarTable("sys_file", "File storage table")]
    public class SysFile : SysBase
    {
        /// <summary>
        /// Auto-increment ID
        /// </summary> 
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }
        /// <summary>
        /// Original file name
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// File type
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// Stored file name
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// File storage path e.g.: /uploads/20220202
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// Repository location e.g.: /uploads
        /// </summary>
        public string StorePath { get; set; }
        /// <summary>
        /// File size
        /// </summary>
        public string FileSize { get; set; }
        /// <summary>
        /// File extension
        /// </summary>
        public string FileExt { get; set; }
        /// <summary>
        /// Creator
        /// </summary>
        public string Create_by { get; set; }
        /// <summary>
        /// Upload time
        /// </summary>
        public DateTime? Create_time { get; set; }
        /// <summary>
        /// Storage type
        /// </summary>
        public int? StoreType { get; set; }
        /// <summary>
        /// Access path
        /// </summary>
        public string AccessUrl { get; set; }
        /// <summary>
        /// Directory
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// Classification
        /// </summary>
        public string ClassifyType { get; set; }
        public SysFile() { }
        public SysFile(string originFileName, string fileName, string ext, string fileSize, string storePath, string create_by)
        {
            StorePath = storePath;
            RealName = originFileName;
            FileName = fileName;
            FileExt = ext;
            FileSize = fileSize;
            Create_by = create_by;
            Create_time = DateTime.Now;
        }
    }
}

