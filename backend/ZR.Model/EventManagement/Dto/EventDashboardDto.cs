using System;
using System.Collections.Generic;

namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Event Dashboard DTO - لوحة تحكم الفعالية
    /// </summary>
    public class EventDashboardDto
    {
        /// <summary>
        /// Event Information - معلومات الفعالية
        /// </summary>
        public EventBasicInfo EventInfo { get; set; }

        /// <summary>
        /// Visitor Statistics - إحصائيات الزوار
        /// </summary>
        public VisitorStatistics Visitors { get; set; }

        /// <summary>
        /// Gate Statistics - إحصائيات البوابات
        /// </summary>
        public List<GateStatusDto> Gates { get; set; }

        /// <summary>
        /// Zone Occupancy - إشغال المناطق
        /// </summary>
        public List<ZoneOccupancyDto> Zones { get; set; }

        /// <summary>
        /// Booth Statistics - إحصائيات الأكشاك
        /// </summary>
        public BoothStatistics Booths { get; set; }

        /// <summary>
        /// Financial Summary - الملخص المالي
        /// </summary>
        public FinancialSummary Financial { get; set; }

        /// <summary>
        /// Incident Summary - ملخص الحوادث
        /// </summary>
        public IncidentSummary Incidents { get; set; }

        /// <summary>
        /// Staff on Duty - الموظفون المناوبون
        /// </summary>
        public StaffSummary Staff { get; set; }

        /// <summary>
        /// Real-time Updates - التحديثات اللحظية
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    public class EventBasicInfo
    {
        public long EventId { get; set; }
        public string EventName { get; set; }
        public string EventNameAr { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int DaysRemaining { get; set; }
        public int TotalDays { get; set; }
    }

    public class VisitorStatistics
    {
        /// <summary>
        /// Total Registered - إجمالي المسجلين
        /// </summary>
        public int TotalRegistered { get; set; }

        /// <summary>
        /// Currently Inside - الموجودون حاليًا
        /// </summary>
        public int CurrentlyInside { get; set; }

        /// <summary>
        /// Checked In Today - دخلوا اليوم
        /// </summary>
        public int CheckedInToday { get; set; }

        /// <summary>
        /// Checked Out Today - خرجوا اليوم
        /// </summary>
        public int CheckedOutToday { get; set; }

        /// <summary>
        /// VIP Visitors - زوار VIP
        /// </summary>
        public int VIPCount { get; set; }

        /// <summary>
        /// Visitor Types Breakdown - توزيع أنواع الزوار
        /// </summary>
        public Dictionary<string, int> VisitorTypeBreakdown { get; set; }

        /// <summary>
        /// Occupancy Percentage - نسبة الإشغال
        /// </summary>
        public decimal OccupancyPercentage { get; set; }
    }

    public class GateStatusDto
    {
        public long GateId { get; set; }
        public string GateCode { get; set; }
        public string GateName { get; set; }
        public string GateType { get; set; }
        public string Status { get; set; }
        public int CurrentQueueCount { get; set; }
        public int AvgWaitTimeMinutes { get; set; }
        public int TotalCheckInsToday { get; set; }
        public int TotalCheckOutsToday { get; set; }
    }

    public class ZoneOccupancyDto
    {
        public long ZoneId { get; set; }
        public string ZoneCode { get; set; }
        public string ZoneName { get; set; }
        public string ZoneType { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public decimal OccupancyPercentage { get; set; }
        public string Status { get; set; }
    }

    public class BoothStatistics
    {
        public int TotalBooths { get; set; }
        public int RentedBooths { get; set; }
        public int AvailableBooths { get; set; }
        public int ReservedBooths { get; set; }
        public int MaintenanceBooths { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Currency { get; set; }
    }

    public class FinancialSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal TicketRevenue { get; set; }
        public decimal BoothRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public decimal OverduePayments { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public string Currency { get; set; }
    }

    public class IncidentSummary
    {
        public int TotalIncidents { get; set; }
        public int OpenIncidents { get; set; }
        public int InProgressIncidents { get; set; }
        public int ResolvedToday { get; set; }
        public int CriticalIncidents { get; set; }
        public int HighPriorityIncidents { get; set; }
        public decimal AvgResolutionTimeMinutes { get; set; }
        public Dictionary<string, int> IncidentTypeBreakdown { get; set; }
    }

    public class StaffSummary
    {
        public int TotalStaff { get; set; }
        public int OnDutyStaff { get; set; }
        public int OffDutyStaff { get; set; }
        public int SecurityStaff { get; set; }
        public int MedicalStaff { get; set; }
        public int TechnicalStaff { get; set; }
    }
}

