using Infrastructure.Converter;
using Newtonsoft.Json;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    public class TerminationQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        public string Status { get; set; }
        public string TerminationType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class TerminationRequestDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string TerminationType { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public string Reason { get; set; }
        public string ReasonCategory { get; set; }
        public int NoticePeriodDays { get; set; } = 30;
    }

    public class TerminationApprovalDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TerminationId { get; set; }
        public bool Approved { get; set; }
        public string Comments { get; set; }
    }

    public class TerminationSettlementDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TerminationId { get; set; }
        public decimal ESBAmount { get; set; }
        public decimal LeaveEncashmentAmount { get; set; }
        public decimal PendingLoanAmount { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal FinalSettlementAmount { get; set; }

        // Breakdown from ESB calculation
        public decimal BasicSalary { get; set; }
        public decimal TotalServiceYears { get; set; }
        public string EligibilityNote { get; set; }
    }

    public class TerminationChecklistDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TerminationId { get; set; }
        public bool AssetReturnCompleted { get; set; }
        public bool ITAccessRevoked { get; set; }
        public bool ExitInterviewCompleted { get; set; }
        public bool ClearanceCompleted { get; set; }
        public bool ExperienceLetterIssued { get; set; }
    }
}
