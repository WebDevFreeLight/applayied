namespace ZR.Model.System
{
    /// <summary>
    /// Department table
    /// </summary>
    [SugarTable("sys_dept", "Department configuration table")]
    [Tenant("0")]
    public class SysDept : SysBase
    {
        /// <summary>
        /// Department ID
        /// </summary>
        [SugarColumn(
            
            
            //IsIdentity = true,

            IsPrimaryKey = true


            )]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }

        /// <summary>
        /// Parent department ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ParentId { get; set; }

        /// <summary>
        /// Ancestor list
        /// </summary>
        public string Ancestors { get; set; }

        /// <summary>
        /// Department name
        /// </summary>
        [SugarColumn(Length = 150, ExtendedAttribute = ProteryConstant.NOTNULL)]
        public string DeptName { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int OrderNum { get; set; }

        /// <summary>
        /// Leader
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Leader { get; set; }

        /// <summary>
        /// Contact phone
        /// </summary>
        [SugarColumn(Length = 11)]
        public string Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Email { get; set; }

        /// <summary>
        /// Department status: 0=Normal, 1=Disabled
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int Status { get; set; }

        /// <summary>
        /// Delete flag (0=Exists, 2=Deleted)
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int DelFlag { get; set; }

        /// <summary>
        /// Child departments
        /// </summary>
        public List<SysDept> children = new();
    }
}

