using System;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// DTO for creating a new assignment
    /// </summary>
    public class CreateAssignmentDto
    {
        public long? DepartmentId { get; set; }
        public long? PositionId { get; set; }
        public long? ManagerEmployeeId { get; set; }
        public long? WorkLocationId { get; set; }
        public DateTime? EffectiveFrom { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing assignment
    /// </summary>
    public class UpdateAssignmentDto
    {
        public long? DepartmentId { get; set; }
        public long? PositionId { get; set; }
        public long? ManagerEmployeeId { get; set; }
        public long? WorkLocationId { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// DTO for simple current assignment update
    /// </summary>
    public class UpdateCurrentAssignmentDto
    {
        public long? DepartmentId { get; set; }
        public long? PositionId { get; set; }
        public long? ManagerId { get; set; }
    }
}
