using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using SqlSugar;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR
{
    /// <summary>
    /// Employee Core - Family Controller
    /// Handles: EmergencyContact + DependentLink
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/family")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeFamilyController : BaseController
    {
        private readonly IHrEmployeeEmergencyContactService _emergencyContactService;
        private readonly IHrEmployeeDependentLinkService _dependentLinkService;
        private readonly IHrPersonService _personService;
        private readonly IHrPersonNameService _personNameService;
        private readonly IHrPersonDemographicService _personDemographicService;
        private readonly IHrPersonIdentifierService _personIdentifierService;

        public HrEmployeeFamilyController(
            IHrEmployeeEmergencyContactService emergencyContactService,
            IHrEmployeeDependentLinkService dependentLinkService,
            IHrPersonService personService,
            IHrPersonNameService personNameService,
            IHrPersonDemographicService personDemographicService,
            IHrPersonIdentifierService personIdentifierService)
        {
            _emergencyContactService = emergencyContactService;
            _dependentLinkService = dependentLinkService;
            _personService = personService;
            _personNameService = personNameService;
            _personDemographicService = personDemographicService;
            _personIdentifierService = personIdentifierService;
        }

        /// <summary>
        /// List emergency contacts (paged)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:employeecore:family:list")]
        public async Task<IActionResult> List([FromQuery] HrEmployeeFamilyQueryDto dto)
        {
            var predicate = Expressionable.Create<ZR.Model.HR.EmployeeCore.HrEmployeeEmergencyContact>();
            predicate = predicate.AndIF(dto.EmployeeId.HasValue && dto.EmployeeId > 0, it => it.EmployeeId == dto.EmployeeId);
            predicate = predicate.AndIF(dto.IsActive.HasValue, it => it.IsActive == dto.IsActive.Value);

            var response = await _emergencyContactService.GetPageListAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get family details for an employee (simple composition, no complex joins)
        /// </summary>
        [HttpGet("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:family:query")]
        public async Task<IActionResult> GetInfo(long employeeId)
        {
            var emergencyContacts = await _emergencyContactService.GetListAsync(x => x.EmployeeId == employeeId);
            var dependentLinks = await _dependentLinkService.GetListAsync(x => x.EmployeeId == employeeId);

            return SUCCESS(new
            {
                EmployeeId = employeeId,
                EmergencyContacts = emergencyContacts,
                DependentLinks = dependentLinks
            });
        }

        /// <summary>
        /// Get dependents for an employee with person details
        /// </summary>
        [HttpGet("{employeeId:long}/dependents")]
        [ActionPermissionFilter("hr:employeecore:family:query")]
        public async Task<IActionResult> GetDependents(long employeeId)
        {
            var dependentLinks = await _dependentLinkService.GetListAsync(x => x.EmployeeId == employeeId && x.IsActive);
            var dependents = new List<object>();

            foreach (var link in dependentLinks)
            {
                // Get person details
                var person = await _personService.GetByIdAsync(link.DependentPersonId);
                if (person == null) continue;

                // Get primary name
                var names = await _personNameService.GetListAsync(n => n.PersonId == link.DependentPersonId && n.IsPrimary);
                var primaryName = names.FirstOrDefault();

                // Get demographic
                var demographic = await _personDemographicService.GetByIdAsync(link.DependentPersonId);

                // Get primary identifier
                var identifiers = await _personIdentifierService.GetListAsync(i => i.PersonId == link.DependentPersonId && i.IsPrimary);
                var primaryIdentifier = identifiers.FirstOrDefault();

                int? age = null;
                if (demographic?.DateOfBirth != null)
                {
                    var today = DateTime.Today;
                    age = today.Year - demographic.DateOfBirth.Value.Year;
                    if (demographic.DateOfBirth.Value.Date > today.AddYears(-age.Value)) age--;
                }

                // Build full name from parts
                string fullName = null;
                if (primaryName != null)
                {
                    if (!string.IsNullOrEmpty(primaryName.FullNameAr))
                    {
                        fullName = primaryName.FullNameAr;
                    }
                    else
                    {
                        var nameParts = new[] { primaryName.GivenName, primaryName.MiddleName, primaryName.FamilyName }
                            .Where(n => !string.IsNullOrEmpty(n));
                        fullName = string.Join(" ", nameParts);
                    }
                }

                dependents.Add(new
                {
                    EmployeeDependentLinkId = link.EmployeeDependentLinkId,
                    DependentPersonId = link.DependentPersonId,
                    FullName = fullName,
                    FullNameAr = primaryName?.FullNameAr,
                    FirstName = primaryName?.GivenName,
                    LastName = primaryName?.FamilyName,
                    DateOfBirth = demographic?.DateOfBirth,
                    Age = age,
                    Gender = demographic?.GenderDictCode,
                    Nationality = demographic?.NationalityDictCode,
                    IdNumber = primaryIdentifier?.IdentifierNumber,
                    Relationship = link.RelationshipDictCode,
                    EffectiveFrom = link.EffectiveFrom,
                    EffectiveTo = link.EffectiveTo,
                    IsActive = link.IsActive
                });
            }

            return SUCCESS(dependents);
        }

        /// <summary>
        /// Add new dependent to employee
        /// </summary>
        [HttpPost("{employeeId:long}/dependents")]
        [ActionPermissionFilter("hr:employeecore:family:add")]
        [Log(Title = "Employee Family", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddDependent(long employeeId, [FromBody] CreateDependentDto dto)
        {
            try
            {
                // Create person record
                var person = new Model.HR.EmployeeCore.HrPerson
                {
                    IsActive = true,
                    Create_by = HttpContext.GetName(),
                    Create_time = DateTime.Now
                };

                var personId = await _personService.AddAsync(person);
                Console.WriteLine($"[DEBUG DEPENDENT] Person returned ID from AddAsync: {personId}");

                // WORKAROUND: Verify the actual person ID in database (SqlSugar returns rows affected, not ID)
                var savedPerson = await _personService.GetListAsync(p =>
                    p.Create_by == HttpContext.GetName() &&
                    p.Create_time != null);
                var actualPersonId = savedPerson.OrderByDescending(p => p.PersonId).FirstOrDefault()?.PersonId ?? personId;
                Console.WriteLine($"[DEBUG DEPENDENT] Actual person ID from database query: {actualPersonId}");

                if (personId != actualPersonId)
                {
                    Console.WriteLine($"[DEBUG DEPENDENT] ⚠️  WARNING: AddAsync returned wrong ID! Using {actualPersonId} instead of {personId}");
                    personId = (int)actualPersonId;
                }

                // Create person name
                var personName = new Model.HR.EmployeeCore.HrPersonName
                {
                    PersonId = personId,
                    GivenName = dto.FirstName,
                    FamilyName = dto.LastName,
                    FullNameAr = dto.FullNameAr,
                    EffectiveFrom = DateTime.Now,
                    IsPrimary = true,
                    Create_by = HttpContext.GetName(),
                    Create_time = DateTime.Now
                };

                await _personNameService.AddAsync(personName);

                // Create demographic if provided
                if (dto.DateOfBirth.HasValue || dto.GenderDictCode.HasValue || dto.NationalityDictCode.HasValue)
                {
                    var demographic = new Model.HR.EmployeeCore.HrPersonDemographic
                    {
                        PersonId = personId,
                        DateOfBirth = dto.DateOfBirth,
                        GenderDictCode = dto.GenderDictCode,
                        NationalityDictCode = dto.NationalityDictCode,
                        Create_by = HttpContext.GetName(),
                        Create_time = DateTime.Now
                    };

                    await _personDemographicService.AddAsync(demographic);
                }

                // Create identifier if provided
                if (!string.IsNullOrEmpty(dto.IdNumber))
                {
                    var identifier = new Model.HR.EmployeeCore.HrPersonIdentifier
                    {
                        PersonId = personId,
                        IdentifierTypeDictCode = 1, // National ID/Iqama
                        IdentifierNumber = dto.IdNumber,
                        IsPrimary = true,
                        Create_by = HttpContext.GetName(),
                        Create_time = DateTime.Now
                    };

                    await _personIdentifierService.AddAsync(identifier);
                }

                // Create dependent link
                var dependentLink = new Model.HR.EmployeeCore.HrEmployeeDependentLink
                {
                    EmployeeId = employeeId,
                    DependentPersonId = personId,
                    RelationshipDictCode = dto.RelationshipDictCode,
                    EffectiveFrom = dto.EffectiveFrom == default ? DateTime.Now : dto.EffectiveFrom,
                    IsActive = true,
                    Create_by = HttpContext.GetName(),
                    Create_time = DateTime.Now
                };

                var linkId = await _dependentLinkService.AddAsync(dependentLink);

                return SUCCESS(new { linkId, personId, message = "Dependent added successfully" });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to add dependent: {ex.Message}");
            }
        }

        /// <summary>
        /// Update dependent information
        /// </summary>
        [HttpPut("{employeeId:long}/dependents/{linkId:long}")]
        [ActionPermissionFilter("hr:employeecore:family:edit")]
        [Log(Title = "Employee Family", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateDependent(long employeeId, long linkId, [FromBody] UpdateDependentDto dto)
        {
            try
            {
                var link = await _dependentLinkService.GetByIdAsync(linkId);
                if (link == null || link.EmployeeId != employeeId)
                {
                    return ToResponse(ResultCode.FAIL, "Dependent not found");
                }

                // Update link
                if (dto.RelationshipDictCode.HasValue)
                {
                    link.RelationshipDictCode = dto.RelationshipDictCode;
                }
                if (dto.EffectiveTo.HasValue)
                {
                    link.EffectiveTo = dto.EffectiveTo;
                }
                link.IsActive = dto.IsActive;
                link.Update_by = HttpContext.GetName();
                link.Update_time = DateTime.Now;

                await _dependentLinkService.UpdateAsync(link);

                // Update person name if provided
                if (!string.IsNullOrEmpty(dto.FirstName) || !string.IsNullOrEmpty(dto.LastName) || !string.IsNullOrEmpty(dto.FullNameAr))
                {
                    var names = await _personNameService.GetListAsync(n => n.PersonId == link.DependentPersonId && n.IsPrimary);
                    var primaryName = names.FirstOrDefault();

                    if (primaryName != null)
                    {
                        if (!string.IsNullOrEmpty(dto.FirstName))
                            primaryName.GivenName = dto.FirstName;
                        if (!string.IsNullOrEmpty(dto.LastName))
                            primaryName.FamilyName = dto.LastName;
                        if (!string.IsNullOrEmpty(dto.FullNameAr))
                            primaryName.FullNameAr = dto.FullNameAr;

                        primaryName.Update_by = HttpContext.GetName();
                        primaryName.Update_time = DateTime.Now;

                        await _personNameService.UpdateAsync(primaryName);
                    }
                }

                // Update demographic if provided
                if (dto.DateOfBirth.HasValue || dto.GenderDictCode.HasValue || dto.NationalityDictCode.HasValue)
                {
                    var demographic = await _personDemographicService.GetByIdAsync(link.DependentPersonId);

                    if (demographic != null)
                    {
                        if (dto.DateOfBirth.HasValue)
                            demographic.DateOfBirth = dto.DateOfBirth;
                        if (dto.GenderDictCode.HasValue)
                            demographic.GenderDictCode = dto.GenderDictCode;
                        if (dto.NationalityDictCode.HasValue)
                            demographic.NationalityDictCode = dto.NationalityDictCode;

                        demographic.Update_by = HttpContext.GetName();
                        demographic.Update_time = DateTime.Now;

                        await _personDemographicService.UpdateAsync(demographic);
                    }
                }

                return SUCCESS(new { message = "Dependent updated successfully" });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to update dependent: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove (deactivate) dependent
        /// </summary>
        [HttpDelete("{employeeId:long}/dependents/{linkId:long}")]
        [ActionPermissionFilter("hr:employeecore:family:delete")]
        [Log(Title = "Employee Family", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> RemoveDependent(long employeeId, long linkId)
        {
            try
            {
                var link = await _dependentLinkService.GetByIdAsync(linkId);
                if (link == null || link.EmployeeId != employeeId)
                {
                    return ToResponse(ResultCode.FAIL, "Dependent not found");
                }

                // Deactivate instead of hard delete
                link.IsActive = false;
                link.EffectiveTo = DateTime.Now;
                link.Update_by = HttpContext.GetName();
                link.Update_time = DateTime.Now;

                await _dependentLinkService.UpdateAsync(link);

                return SUCCESS(new { message = "Dependent removed successfully" });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to remove dependent: {ex.Message}");
            }
        }
    }
}












