using System.ComponentModel.DataAnnotations;

namespace ZR.Model.Dto
{
    /// <summary>
    /// 邮件模板查询对象
    /// </summary>
    public class EmailTplQueryDto : PagerInfo 
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// 邮件模板输入输出对象
    /// </summary>
    public class EmailTplDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Name cannot be empty")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Template content cannot be empty")]
        public string Content { get; set; }

        public string Create_by { get; set; }

        public DateTime? Create_time { get; set; }

        public string Update_by { get; set; }

        public DateTime? Update_time { get; set; }

        public string Remark { get; set; }



    }
}
