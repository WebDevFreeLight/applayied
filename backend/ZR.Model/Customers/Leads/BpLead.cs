using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Customers.Leads
{
    /// <summary>
    /// Lead (Potential Customer) - full details
    /// </summary>
    [SugarTable("bp_lead")]
    public class BpLead : SysBase, ISoftDelete
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "leadid", IsPrimaryKey = true, IsIdentity = false)]
        public long LeadId { get; set; }

        /// <summary>
        /// Type: 1 = Company, 2 = Individual
        /// </summary>
        [SugarColumn(ColumnName = "leadtype")]
        public long LeadType { get; set; }

        /// <summary>
        /// Brand Name / Company Name
        /// </summary>
        [SugarColumn(ColumnName = "brandname")]
        public string BrandName { get; set; }

        /// <summary>
        /// Brand Image / Logo URL
        /// </summary>
        [SugarColumn(ColumnName = "brandimage", Length = 500, IsNullable = true)]
        public string BrandImage { get; set; }

        /// <summary>
        /// Owner Name
        /// </summary>
        [SugarColumn(ColumnName = "ownername")]
        public string OwnerName { get; set; }

        /// <summary>
        /// Owner Contact Number
        /// </summary>
        [SugarColumn(ColumnName = "ownerphone")]
        public string OwnerPhone { get; set; }

        /// <summary>
        /// Owner Email
        /// </summary>
        [SugarColumn(ColumnName = "owneremail", IsNullable = true)]
        public string OwnerEmail { get; set; }

        /// <summary>
        /// Assigned to Employee ID (Marketing team member)
        /// References: hr_employee_core.employee_id
        /// </summary>
        [SugarColumn(ColumnName = "assignedto")]
        public long AssignedTo { get; set; }

        /// <summary>
        /// Lead Status: New, Contacted, Qualifying, Qualified, In Negotiation, Confirmed, Converted, Lost
        /// </summary>
        [SugarColumn(ColumnName = "leadstatus")]
        public string LeadStatus { get; set; }

        /// <summary>
        /// Qualification Notes
        /// </summary>
        [SugarColumn(ColumnName = "qualificationnotes")]
        public string QualificationNotes { get; set; }

        /// <summary>
        /// Rejection Notes
        /// </summary>
        [SugarColumn(ColumnName = "rejectionnotes")]
        public string RejectionNotes { get; set; }

        /// <summary>
        /// Internal Notes / Remarks
        /// </summary>
        [SugarColumn(ColumnName = "internalnotes")]
        public string InternalNotes { get; set; }

        /// <summary>
        /// Conversion to Customer ID (FK -> bp_businesspartner)
        /// </summary>
        [SugarColumn(ColumnName = "convertedpartnerid")]
        public long? ConvertedPartnerId { get; set; }

        /// <summary>
        /// Conversion Date
        /// </summary>
        [SugarColumn(ColumnName = "conversiondate")]
        public DateTime? ConversionDate { get; set; }

        /// <summary>
        /// Lead Source: Website, Referral, Social Media, Cold Call, Event, Partner, etc.
        /// </summary>
        [SugarColumn(ColumnName = "leadsource", Length = 100)]
        public string LeadSource { get; set; }

        /// <summary>
        /// Budget Range
        /// </summary>
        [SugarColumn(ColumnName = "budgetrange", Length = 50)]
        public string BudgetRange { get; set; }

        /// <summary>
        /// Date of first contact
        /// </summary>
        [SugarColumn(ColumnName = "firstcontactdate")]
        public DateTime? FirstContactDate { get; set; }

        /// <summary>
        /// Date of last interaction
        /// </summary>
        [SugarColumn(ColumnName = "lastcontactdate")]
        public DateTime? LastContactDate { get; set; }

        /// <summary>
        /// Priority: Low, Medium, High, Urgent
        /// </summary>
        [SugarColumn(ColumnName = "priority", Length = 20)]
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Expected Revenue
        /// </summary>
        [SugarColumn(ColumnName = "expectedrevenue", DecimalDigits = 2)]
        public decimal? ExpectedRevenue { get; set; }

        /// <summary>
        /// Reason for losing the lead
        /// </summary>
        [SugarColumn(ColumnName = "lostreason", Length = 200)]
        public string LostReason { get; set; }

        [SugarColumn(ColumnName = "companyname", Length = 200, IsNullable = true)]
        public string CompanyName { get; set; }

        [SugarColumn(ColumnName = "activitytype", Length = 200, IsNullable = true)]
        public string ActivityType { get; set; }

        [SugarColumn(ColumnName = "interesting", IsNullable = true)]
        public string Interesting { get; set; }

        /// <summary>
        /// JSON array of EventLocation IDs e.g. "[123, 456]"
        /// </summary>
        [SugarColumn(ColumnName = "locationinterested", IsNullable = true)]
        public string LocationInterested { get; set; }

        /// <summary>
        /// "share" or "rent"
        /// </summary>
        [SugarColumn(ColumnName = "contracttypeinterested", Length = 50, IsNullable = true)]
        public string ContractTypeInterested { get; set; }

        /// <summary>
        /// Assigned to Sales Employee ID (set when lead is Qualified)
        /// References: hr_employee_core.employee_id
        /// </summary>
        [SugarColumn(ColumnName = "salesassignedto", IsNullable = true)]
        public long? SalesAssignedTo { get; set; }

        /// <summary>
        /// Flag indicating lead is ready for Operations to see/act on
        /// </summary>
        [SugarColumn(ColumnName = "ready_for_operations", IsNullable = true)]
        public bool ReadyForOperations { get; set; }

        [SugarColumn(ColumnName = "is_deleted", IsNullable = false)]
        public bool IsDeleted { get; set; } = false;

        [SugarColumn(ColumnName = "deleted_at", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties (not mapped to database)
        [SugarColumn(IsIgnore = true)]
        public string AssignedToName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string SalesAssignedToName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int ActivityCount { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int DaysSinceLastContact
        {
            get
            {
                if (LastContactDate.HasValue)
                {
                    return (DateTime.Now - LastContactDate.Value).Days;
                }
                return 0;
            }
        }
    }
}
