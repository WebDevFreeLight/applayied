using MiniExcelLibs.Attributes;
using SqlSugar.DbConvert;
using ZR.Model.System.Enums;

namespace ZR.Model.System.Dto
{
    public class RoleMenuExportDto
    {
        /// <summary>
        /// ?????
        /// </summary>
        [ExcelColumn(Name = "??", Width = 50)]
        public string MenuName { get; set; }
        //[ExcelColumn(Name = "???", Width = 20)]
        //public string MenuName1 { get; set; }
        //[ExcelColumn(Name = "????", Width = 20)]
        //public string MenuName2 { get; set; }
        [ExcelColumn(Name = "??", Width = 20)]
        public string Path { get; set; }
        [ExcelColumn(Name = "???", Width = 20)]
        public string Component { get; set; }
        [ExcelColumn(Name = "????", Width = 20)]
        public string Perms { get; set; }
        [ExcelColumn(Name = "????")]
        public MenuType MenuType { get; set; }
        [ExcelColumn(Name = "????")]
        public MenuStatus Status { get; set; }
    }
}

