 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Training Program Query DTO
    /// </summary>
    public class TrainingProgramQueryDto : PagerInfo
    {
        public string ProgramName { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Training Program Input DTO
    /// </summary>
    public class TrainingProgramDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgramId { get; set; }
        
        [Required(ErrorMessage = "Program name is required")]
        public string ProgramName { get; set; }
        
        public string ProgramNameAr { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }
        
        public string Level { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public string DurationUnit { get; set; }
        public string DeliveryMethod { get; set; }
        public string Provider { get; set; }
        public decimal Cost { get; set; }
        public string Currency { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsMandatory { get; set; }
        public string PreRequisites { get; set; }
        public string Objectives { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Training Session Query DTO
    /// </summary>
    public class TrainingSessionQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ProgramId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CourseId { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Instructor { get; set; }
    }

    /// <summary>
    /// Training Session DTO
    /// </summary>
    public class TrainingSessionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }




        
        [Required(ErrorMessage = "Program ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgramId { get; set; }
        
        // [Required(ErrorMessage = "Session name is required")]
        public string SessionName { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public string Instructor { get; set; }
        public int MaxParticipants { get; set; }
        public int EnrolledCount { get; set; }
        public string Status { get; set; }
        public string SessionUrl { get; set; }
        public string Materials { get; set; }





         

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CourseId { get; set; }

        [SugarColumn(Length = 100)]
        public string SessionCode { get; set; } 

        [SugarColumn(Length = 20)]
        public string DeliveryMode { get; set; }
         
        public int CompletedCount { get; set; }
         
        public decimal ActualCost { get; set; }
         


    }

    /// <summary>
    /// Training Enrollment DTO
    /// </summary>
    public class TrainingEnrollmentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Session ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }
        
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal? Score { get; set; }
        public string CertificateNumber { get; set; }
        public string Feedback { get; set; }
        public int? Rating { get; set; }
    }

    /// <summary>
    /// Employee Training History DTO
    /// </summary>
    public class EmployeeTrainingHistoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public List<TrainingRecordDto> TrainingRecords { get; set; }
        public int TotalTrainings { get; set; }
        public int CompletedTrainings { get; set; }
        public int InProgressTrainings { get; set; }
        public decimal TotalTrainingHours { get; set; }
        public decimal AverageScore { get; set; }
    }

    /// <summary>
    /// Training Record DTO
    /// </summary>
    public class TrainingRecordDto
    {
        public string ProgramName { get; set; }
        public string Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; }
        public decimal? Score { get; set; }
        public string CertificateNumber { get; set; }
    }

    /// <summary>
    /// Training Analytics DTO
    /// </summary>
    public class TrainingAnalyticsDto
    {
        public int TotalPrograms { get; set; }
        public int ActiveSessions { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageRating { get; set; }
        public decimal TotalCost { get; set; }
        public Dictionary<string, int> EnrollmentsByCategory { get; set; }
        public Dictionary<string, decimal> CompletionRateByDepartment { get; set; }
        public List<TopPerformersDto> TopPerformers { get; set; }
        public List<UpcomingSessionsDto> UpcomingSessions { get; set; }
    }

    /// <summary>
    /// Top Performers DTO
    /// </summary>
    public class TopPerformersDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int TrainingsCompleted { get; set; }
        public decimal AverageScore { get; set; }
    }

    /// <summary>
    /// Upcoming Sessions DTO
    /// </summary>
    public class UpcomingSessionsDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }
        public string ProgramName { get; set; }
        public DateTime StartDate { get; set; }
        public int AvailableSeats { get; set; }
    }
}

