 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Approval Step DTO
    /// </summary>
    public class ApprovalStepDto
    {



        [JsonConverter(typeof(ValueToStringConverter))]
        public long ApprovalId { get; set; }



        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApproverId { get; set; }
        public string ApproverName { get; set; }
        public string ApproverRole { get; set; }
        public int Level { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Comments { get; set; }
        public bool IsEscalated { get; set; }
    }
}

