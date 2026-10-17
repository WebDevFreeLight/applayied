 
using Infrastructure.Converter;
using ZR.Model;
using ZR.Model.System;

namespace ZR.ServiceCore.Model.Dto
{
    [SugarTable("sys_post_dto", "Position table")]
    [Tenant("0")]
    public class SysPostDto : SysPost
    {
        /// <summary>
        /// ????
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserNum { get; set; }
    }

    public class SysPostQueryDto : PagerInfo
    {
        public string PostName { get; set; }
        public string Status { get; set; }
        public string PostCode { get; set; }
    }
}

