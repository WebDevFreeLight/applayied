using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    public class PmoCostCategoryInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CategoryId { get; set; }

        [Required(ErrorMessage = "Category code is required")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        public string Name { get; set; }
    }
}
