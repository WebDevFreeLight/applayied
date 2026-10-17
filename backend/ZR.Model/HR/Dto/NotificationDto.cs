 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Notification DTO
    /// </summary>
    public class NotificationDto
    {



        public NotificationDto()
        {

            
        }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecipientId { get; set; }
        public string RecipientRole { get; set; }
        public string RecipientType { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; }
        public bool ActionRequired { get; set; }
        public string ActionUrl { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public string Status { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    /// <summary>
    /// Workflow Task DTO
    /// </summary>
    public class WorkflowTaskDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }
        public string TaskName { get; set; }
        public string TaskType { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Comments { get; set; }
    }
}

