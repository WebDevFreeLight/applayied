using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.EventManagement.Dto
{

    // ==================== Event Zone ====================
    public class EventZoneQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }
        public string ZoneCode { get; set; }
        public string ZoneName { get; set; }
        public string ZoneType { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; }
    }

    public class EventZoneDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ZoneId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Event ID is required")]
        public long EventId { get; set; }
        public string ZoneCode { get; set; }

        [Required(ErrorMessage = "Zone name is required")]
        public string ZoneName { get; set; }
        public string ZoneNameAr { get; set; }
        public string Description { get; set; }
        public string ZoneType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ParentZoneId { get; set; }
        public int? MaxCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Update Zone Occupancy Request DTO
    /// </summary>
    public class UpdateOccupancyDto
    {
        [Required(ErrorMessage = "Zone ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ZoneId { get; set; }

        [Required(ErrorMessage = "Occupancy is required")]
        public int Occupancy { get; set; }
    }

}

