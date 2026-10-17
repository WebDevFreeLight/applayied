namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// Employee lookup DTO for dropdowns
    /// Minimal data for performance
    /// </summary>
    public class EmployeeLookupDto
    {
        /// <summary>
        /// Employee ID (Snowflake ID)
        /// </summary>
        public long EmployeeId { get; set; }

        /// <summary>
        /// Employee code (e.g., "EMP001")
        /// </summary>
        public string EmployeeCode { get; set; }

        /// <summary>
        /// Given/First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Family/Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Full Arabic name
        /// </summary>
        public string FullNameAr { get; set; }
    }
}
