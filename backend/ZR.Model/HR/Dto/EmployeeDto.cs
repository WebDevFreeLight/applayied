 
using Infrastructure.Converter;
using System;
using System.ComponentModel.DataAnnotations;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Employee query object
    /// </summary>
    public class EmployeeQueryDto : PagerInfo
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        public string Status { get; set; }
        public string EmploymentType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }






        [SugarColumn(IsJson = true)]
        public List<long> RoleIds { get; set; } = new();
    }

    /// <summary>
    /// Employee input object
    /// </summary>
    public class EmployeeDto
    {

         
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }




        [SugarColumn(IsJson = true)]
        public List<long> RoleIds { get; set; } = new();




        public string EmployeeCode { get; set; }
        
        [Required(ErrorMessage = "Name cannot be empty")]
        public string Name { get; set; }
        
        public string NameAr { get; set; }
        
        [EmailAddress(ErrorMessage = "Email format is incorrect")]
        public string Email { get; set; }
        
        [Phone(ErrorMessage = "Phone number format is incorrect")]
        public string Phone { get; set; }
        
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerId { get; set; }
        public DateTime? JoinDate { get; set; }
        public string Status { get; set; }
        public string EmploymentType { get; set; }
        public string Nationality { get; set; }
        public string IdNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public string BankAccount { get; set; }
        public string IBAN { get; set; }

        /// <summary>
        /// Finance GL Account for employee payroll posting (FK -> gl_account.account_id)
        /// </summary>
        public Guid? GLAccountId { get; set; }

        public string BiometricId { get; set; }
        public string WorkLocation { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string GOSINumber { get; set; }
        public string MedicalInsuranceNumber { get; set; }
        public string Remark { get; set; }






        /// <summary>
        /// First name
        /// </summary>
        [SugarColumn(Length = 100)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [SugarColumn(Length = 100)]
        public string LastName { get; set; }

         
        /// <summary>
        /// Joining Date
        /// </summary>
        public DateTime? JoiningDate { get; set; }
         

        /// <summary>
        /// Bank name
        /// </summary>
        [SugarColumn(Length = 100)]
        public string BankName { get; set; }

        /// <summary>
        /// Account number
        /// </summary>
        [SugarColumn(Length = 50)]
        public string AccountNumber { get; set; } 


        /// <summary>
        /// Hire Date
        /// </summary>
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Termination Date
        /// </summary>
        public DateTime? TerminationDate { get; set; }

        /// <summary>
        /// Date of Birth
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Personal Email
        /// </summary>
        [SugarColumn(Length = 100)]
        public string PersonalEmail { get; set; }

        /// <summary>
        /// Mobile Number
        /// </summary>
        [SugarColumn(Length = 20)]
        public string MobileNumber { get; set; }

        /// <summary>
        /// Emergency Contact Phone
        /// </summary>
        [SugarColumn(Length = 20)]
        public string EmergencyContactPhone { get; set; }

        /// <summary>
        /// Shift ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShiftId { get; set; }

        /// <summary>
        /// Is Saudi
        /// </summary>
        public bool IsSaudi { get; set; }

        /// <summary>
        /// Passport Number
        /// </summary>
        [SugarColumn(Length = 50)]
        public string PassportNumber { get; set; }

        /// <summary>
        /// Passport Expiry Date
        /// </summary>
        public DateTime? PassportExpiryDate { get; set; }

        /// <summary>
        /// Visa Number
        /// </summary>
        [SugarColumn(Length = 50)]
        public string VisaNumber { get; set; }

        /// <summary>
        /// Visa Expiry Date
        /// </summary>
        public DateTime? VisaExpiryDate { get; set; }

        /// <summary>
        /// Education Level
        /// </summary>
        [SugarColumn(Length = 50)]
        public string EducationLevel { get; set; }

        /// <summary>
        /// Field of Study
        /// </summary>
        [SugarColumn(Length = 100)]
        public string FieldOfStudy { get; set; }

        /// <summary>
        /// University
        /// </summary>
        [SugarColumn(Length = 200)]
        public string University { get; set; }

        /// <summary>
        /// Graduation Year
        /// </summary>
        public int? GraduationYear { get; set; }

        /// <summary>
        /// Previous Company
        /// </summary>
        [SugarColumn(Length = 200)]
        public string PreviousCompany { get; set; }

        /// <summary>
        /// Years of Experience
        /// </summary>
        public int? YearsOfExperience { get; set; }

        /// <summary>
        /// Profile Picture
        /// </summary>
        [SugarColumn(Length = 500)]
        public string ProfilePicture { get; set; }






    }

    /// <summary>
    /// Employee statistics object
    /// </summary>
    public class EmployeeStatisticsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public int NewJoinThisMonth { get; set; }
        public int ResignedThisMonth { get; set; }
        public Dictionary<string, int> ByDepartment { get; set; }
        public Dictionary<string, int> ByEmploymentType { get; set; }
        public Dictionary<string, int> ByNationality { get; set; }
    }
}

