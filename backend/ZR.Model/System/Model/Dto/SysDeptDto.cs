namespace ZR.Model.System.Dto
{
    public class SysDeptQueryDto : PagerInfo
    {
        public int? Status { get; set; }
        public int? DelFlag { get; set; }
        public string DeptName { get; set; }
    }
    public class SysDeptDto : SysBase
    {

        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }


        [JsonConverter(typeof(ValueToStringConverter))]
        public long ParentId { get; set; }

        public string Ancestors { get; set; }

        public string DeptName { get; set; }

        public int OrderNum { get; set; }

        public string Leader { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public int Status { get; set; }

        public int DelFlag { get; set; }
        public int UserNum { get; set; }
    }
}

