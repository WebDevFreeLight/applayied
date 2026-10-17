using System.ComponentModel;

namespace Infrastructure.Enums
{
    /// <summary>
    /// File storage location
    /// </summary>
    public enum StoreType
    {
        /// <summary>
        /// Local
        /// </summary>
        [Description("Local")]
        LOCAL = 1,

        /// <summary>
        /// Alibaba Cloud
        /// </summary>
        [Description("Alibaba Cloud")]
        ALIYUN = 2,

        /// <summary>
        /// Tencent Cloud
        /// </summary>
        [Description("Tencent Cloud")]
        TENCENT = 3,

        /// <summary>
        /// Qiniu Cloud
        /// </summary>
        [Description("Qiniu Cloud")]
        QINIU = 4,

        /// <summary>
        /// Remote
        /// </summary>
        [Description("Remote")]
        REMOTE = 5
    }
}

