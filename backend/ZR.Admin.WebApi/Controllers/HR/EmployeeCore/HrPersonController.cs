using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using SqlSugar;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR
{
    /// <summary>
    /// Employee Core - Person Controller
    /// Handles: Person + Name + Demographic + Identifier + Contact + Address
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/person")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrPersonController : BaseController
    {
        private readonly IHrPersonService _personService;
        private readonly IHrPersonNameService _personNameService;
        private readonly IHrPersonDemographicService _personDemographicService;
        private readonly IHrPersonIdentifierService _personIdentifierService;
        private readonly IHrPersonContactService _personContactService;
        private readonly IHrPersonAddressService _personAddressService;

        public HrPersonController(
            IHrPersonService personService,
            IHrPersonNameService personNameService,
            IHrPersonDemographicService personDemographicService,
            IHrPersonIdentifierService personIdentifierService,
            IHrPersonContactService personContactService,
            IHrPersonAddressService personAddressService)
        {
            _personService = personService;
            _personNameService = personNameService;
            _personDemographicService = personDemographicService;
            _personIdentifierService = personIdentifierService;
            _personContactService = personContactService;
            _personAddressService = personAddressService;
        }

        /// <summary>
        /// List persons (paged)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:employeecore:person:list")]
        public async Task<IActionResult> List([FromQuery] HrPersonQueryDto dto)
        {
            var predicate = Expressionable.Create<ZR.Model.HR.EmployeeCore.HrPerson>();
            predicate = predicate.AndIF(dto.PersonId.HasValue && dto.PersonId > 0, it => it.PersonId == dto.PersonId);
            predicate = predicate.AndIF(dto.IsActive.HasValue, it => it.IsActive == dto.IsActive.Value);

            var response = await _personService.GetPageListAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get person full details (simple composition, no complex joins)
        /// </summary>
        [HttpGet("{id:long}")]
        [ActionPermissionFilter("hr:employeecore:person:query")]
        public async Task<IActionResult> GetInfo(long id)
        {
            var person = await _personService.GetByIdAsync(id);
            if (person == null)
                return ToResponse(ResultCode.FAIL, "Person not found");

            var names = await _personNameService.GetListAsync(x => x.PersonId == id);
            var demographic = await _personDemographicService.GetByIdAsync(id);
            var identifiers = await _personIdentifierService.GetListAsync(x => x.PersonId == id);
            var contacts = await _personContactService.GetListAsync(x => x.PersonId == id);
            var addresses = await _personAddressService.GetListAsync(x => x.PersonId == id);

            return SUCCESS(new
            {
                Person = person,
                Names = names,
                Demographic = demographic,
                Identifiers = identifiers,
                Contacts = contacts,
                Addresses = addresses
            });
        }

        /// <summary>
        /// Get person identifiers only
        /// </summary>
        [HttpGet("{id:long}/identifiers")]
        [ActionPermissionFilter("hr:employeecore:person:query")]
        public async Task<IActionResult> GetIdentifiers(long id)
        {
            var identifiers = await _personIdentifierService.GetListAsync(x => x.PersonId == id);
            return SUCCESS(identifiers);
        }
    }
}














