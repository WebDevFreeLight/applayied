using System;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Model.Finance.Zatca.Dto
{
    /// <summary>
    /// ZATCA Submission Log Query DTO - استعلام سجل الإرسال
    /// </summary>
    public class ZatcaSubmissionLogQueryDto
    {
        public Guid? InvoiceHashId { get; set; }
        public Guid? DeviceId { get; set; }
        public string OperationType { get; set; }
        public ZatcaEnvironment? Environment { get; set; }
        public bool? IsSuccess { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// ZATCA Submission Log List DTO - قائمة سجل الإرسال
    /// </summary>
    public class ZatcaSubmissionLogListDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceHashId { get; set; }
        public Guid DeviceId { get; set; }
        public string ApiEndpoint { get; set; }
        public string OperationType { get; set; }
        public ZatcaEnvironment Environment { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int? HttpStatusCode { get; set; }
        public string DispositionStatus { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public long? ResponseTimeMs { get; set; }
        public int RetryAttempt { get; set; }

        // Related info
        public string InvoiceNumber { get; set; }
        public string InvoiceUuid { get; set; }
        public string DeviceName { get; set; }
    }

    /// <summary>
    /// ZATCA Submission Log Detail DTO - تفاصيل سجل الإرسال
    /// </summary>
    public class ZatcaSubmissionLogDetailDto : ZatcaSubmissionLogListDto
    {
        public string RequestHeaders { get; set; }
        public string RequestBody { get; set; }
        public string ResponseHeaders { get; set; }
        public string ResponseBody { get; set; }
        public string ValidationSteps { get; set; }
        public string ClientIp { get; set; }
        public string UserAgent { get; set; }
    }

    /// <summary>
    /// ZATCA Dashboard Stats DTO - إحصائيات لوحة التحكم
    /// </summary>
    public class ZatcaDashboardStatsDto
    {
        // Invoice Counts
        public int TotalInvoicesSubmitted { get; set; }
        public int TotalCleared { get; set; }
        public int TotalReported { get; set; }
        public int TotalRejected { get; set; }
        public int TotalPending { get; set; }
        public int TotalFailed { get; set; }

        // Today's Stats
        public int TodaySubmitted { get; set; }
        public int TodayCleared { get; set; }
        public int TodayRejected { get; set; }

        // Success Rate
        public decimal SuccessRate { get; set; }
        public decimal AverageResponseTimeMs { get; set; }

        // Device Stats
        public int ActiveDevices { get; set; }
        public int DevicesNearExpiry { get; set; }

        // Pending Actions
        public int InvoicesPendingSubmission { get; set; }
        public int InvoicesNeedingRetry { get; set; }
    }

    /// <summary>
    /// ZATCA API Test Request DTO - طلب اختبار API
    /// </summary>
    public class ZatcaApiTestRequestDto
    {
        public Guid? DeviceId { get; set; }
        public ZatcaEnvironment Environment { get; set; }
    }

    /// <summary>
    /// ZATCA API Test Response DTO - استجابة اختبار API
    /// </summary>
    public class ZatcaApiTestResponseDto
    {
        public bool IsConnected { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public string ApiVersion { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
    }
}
