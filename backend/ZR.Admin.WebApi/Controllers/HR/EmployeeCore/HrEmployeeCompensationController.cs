using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// Employee Core - Compensation Controller
    /// Handles: CompensationPackage + CompensationComponent + BankAccount
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/compensation")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeCompensationController : BaseController
    {
        private readonly IHrEmployeeCompensationPackageService _packageService;
        private readonly IHrEmployeeCompensationComponentService _componentService;
        private readonly IHrEmployeeBankAccountService _bankAccountService;

        public HrEmployeeCompensationController(
            IHrEmployeeCompensationPackageService packageService,
            IHrEmployeeCompensationComponentService componentService,
            IHrEmployeeBankAccountService bankAccountService)
        {
            _packageService = packageService;
            _componentService = componentService;
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// List compensation packages (paged)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:employeecore:compensation:list")]
        public async Task<IActionResult> List([FromQuery] HrEmployeeCompensationQueryDto dto)
        {
            var predicate = Expressionable.Create<ZR.Model.HR.EmployeeCore.HrEmployeeCompensationPackage>();
            predicate = predicate.AndIF(dto.EmployeeId.HasValue && dto.EmployeeId > 0, it => it.EmployeeId == dto.EmployeeId);
            predicate = predicate.AndIF(dto.EffectiveFrom.HasValue, it => it.EffectiveFrom >= dto.EffectiveFrom.Value);
            predicate = predicate.AndIF(dto.EffectiveTo.HasValue, it => it.EffectiveTo <= dto.EffectiveTo.Value);

            var response = await _packageService.GetPageListAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get compensation details for an employee (simple composition, no complex joins)
        /// </summary>
        [HttpGet("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:compensation:query")]
        public async Task<IActionResult> GetInfo(long employeeId)
        {
            var packages = await _packageService.GetListAsync(x => x.EmployeeId == employeeId);
            var bankAccounts = await _bankAccountService.GetListAsync(x => x.EmployeeId == employeeId);

            // Components are tied to packages; load by each package id (no joins yet).
            var componentsByPackage = new Dictionary<long, object>();
            foreach (var pkg in packages)
            {
                var components = await _componentService.GetListAsync(x => x.CompPackageId == pkg.CompPackageId);
                componentsByPackage[pkg.CompPackageId] = components;
            }

            return SUCCESS(new
            {
                EmployeeId = employeeId,
                Packages = packages,
                ComponentsByPackage = componentsByPackage,
                BankAccounts = bankAccounts
            });
        }

        /// <summary>
        /// Get current compensation package for an employee (with components)
        /// </summary>
        [HttpGet("{employeeId:long}/current")]
        [ActionPermissionFilter("hr:employeecore:compensation:query")]
        public async Task<IActionResult> GetCurrentCompensation(long employeeId)
        {
            Console.WriteLine($"[DEBUG /current] Getting compensation for employeeId: {employeeId}");
            var packages = await _packageService.GetListAsync(x => x.EmployeeId == employeeId);
            Console.WriteLine($"[DEBUG /current] Found {packages.Count()} total packages");

            // Get current package (effective now, no end date or end date in future)
            var now =DateTime.Now;
            Console.WriteLine($"[DEBUG /current] Current datetime: {now}");
            var currentPackage = packages
                .Where(p => p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo > now))
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefault();

            if (currentPackage == null)
            {
                Console.WriteLine($"[DEBUG /current] No current package found");
                return SUCCESS(new
                {
                    EmployeeId = employeeId,
                    Package = (object)null,
                    Components = new List<object>(),
                    TotalMonthly = 0m,
                    TotalAnnual = 0m
                });
            }

            Console.WriteLine($"[DEBUG /current] Current package found - CompPackageId: {currentPackage.CompPackageId}, EffectiveFrom: {currentPackage.EffectiveFrom}");
            var components = await _componentService.GetListAsync(x => x.CompPackageId == currentPackage.CompPackageId);
            var componentsList = components.ToList();
            Console.WriteLine($"[DEBUG /current] Found {componentsList.Count} components for package {currentPackage.CompPackageId}");
            decimal totalMonthly = componentsList.Sum(c => c.Amount);

            return SUCCESS(new
            {
                EmployeeId = employeeId,
                Package = currentPackage,
                Components = componentsList,
                TotalMonthly = totalMonthly,
                TotalAnnual = totalMonthly * 12
            });
        }

        /// <summary>
        /// Get compensation history for an employee
        /// </summary>
        [HttpGet("{employeeId:long}/history")]
        [ActionPermissionFilter("hr:employeecore:compensation:query")]
        public async Task<IActionResult> GetCompensationHistory(long employeeId)
        {
            var packages = await _packageService.GetListAsync(x => x.EmployeeId == employeeId);
            var packagesList = packages.OrderByDescending(p => p.EffectiveFrom).ToList();

            var history = new List<object>();

            foreach (var pkg in packagesList)
            {
                var components = await _componentService.GetListAsync(x => x.CompPackageId == pkg.CompPackageId);
                var componentsList = components.ToList();
                decimal totalMonthly = componentsList.Sum(c => c.Amount);

                history.Add(new
                {
                    Package = pkg,
                    Components = componentsList,
                    TotalMonthly = totalMonthly,
                    TotalAnnual = totalMonthly * 12
                });
            }

            return SUCCESS(history);
        }

        /// <summary>
        /// Create new compensation package
        /// </summary>
        [HttpPost("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:compensation:add")]
        [Log(Title = "Employee Compensation", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateCompensationPackage(long employeeId, [FromBody] CreateCompensationPackageDto dto)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("======== CREATE COMPENSATION START ========");
            Console.WriteLine($"======== Employee ID: {employeeId} ========");
            Console.WriteLine("========================================");

            try
            {
                // Debug logging
                Console.WriteLine($"[DEBUG] Received CreateCompensationPackage for employeeId: {employeeId}");
                Console.WriteLine($"[DEBUG] DTO EffectiveFrom: {dto.EffectiveFrom}");
                Console.WriteLine($"[DEBUG] DTO CurrencyId: {dto.CurrencyId}");
                Console.WriteLine($"[DEBUG] DTO Components: {dto.Components?.Count ?? 0} items");

                if (dto.Components != null)
                {
                    foreach (var comp in dto.Components)
                    {
                        Console.WriteLine($"[DEBUG] Component: Type={comp.ComponentTypeDictCode}, Code={comp.ComponentCodeDictCode}, Amount={comp.Amount}");
                    }
                }

                // Validate components
                if (dto.Components == null || dto.Components.Count == 0)
                {
                    Console.WriteLine("[DEBUG] Validation FAILED: No components provided");
                    return ToResponse(ResultCode.FAIL, "At least one compensation component is required");
                }

                // Close existing current packages
                var existingPackages = await _packageService.GetListAsync(p =>
                    p.EmployeeId == employeeId &&
                    (p.EffectiveTo == null || p.EffectiveTo > DateTime.Now));

                DateTime effectiveFrom = dto.EffectiveFrom;

                foreach (var existingPkg in existingPackages)
                {
                    existingPkg.EffectiveTo = effectiveFrom.AddDays(-1);
                    existingPkg.Update_by = HttpContext.GetName();
                    existingPkg.Update_time = DateTime.Now;
                    await _packageService.UpdateAsync(existingPkg);
                }

                // Create new package
                var package = new Model.HR.EmployeeCore.HrEmployeeCompensationPackage
                {
                    EmployeeId = employeeId,
                    CurrencyId = dto.CurrencyId,
                    EffectiveFrom = effectiveFrom,
                    EffectiveTo = dto.EffectiveTo,
                    CompPackageStatusDictCode = 1,
                    Create_by = HttpContext.GetName(),
                    Create_time = DateTime.Now
                };

                var packageId = await _packageService.AddAsync(package);
                Console.WriteLine($"[DEBUG] Package returned ID from AddAsync: {packageId}");

                // Verify the actual package ID in database
                var savedPackage = await _packageService.GetListAsync(p =>
                    p.EmployeeId == employeeId &&
                    p.EffectiveFrom == effectiveFrom);
                var actualPackageId = savedPackage.OrderByDescending(p => p.CompPackageId).FirstOrDefault()?.CompPackageId ?? packageId;
                Console.WriteLine($"[DEBUG] Actual package ID from database query: {actualPackageId}");

                if (packageId != actualPackageId)
                {
                    Console.WriteLine($"[DEBUG] ⚠️  WARNING: AddAsync returned wrong ID! Using {actualPackageId} instead of {packageId}");
                    packageId = (int)actualPackageId;
                }

                // Create components
                int componentCount = 0;
                foreach (var compDto in dto.Components)
                {
                    var component = new Model.HR.EmployeeCore.HrEmployeeCompensationComponent
                    {
                        CompPackageId = packageId,
                        ComponentTypeDictCode = compDto.ComponentTypeDictCode,
                        ComponentCodeDictCode = compDto.ComponentCodeDictCode,
                        Amount = compDto.Amount,
                        Create_by = HttpContext.GetName(),
                        Create_time = DateTime.Now
                    };

                    Console.WriteLine($"[DEBUG CREATE] Creating component {componentCount + 1}:");
                    Console.WriteLine($"[DEBUG CREATE]   CompPackageId: {component.CompPackageId}");
                    Console.WriteLine($"[DEBUG CREATE]   ComponentTypeDictCode: {component.ComponentTypeDictCode}");
                    Console.WriteLine($"[DEBUG CREATE]   ComponentCodeDictCode: {component.ComponentCodeDictCode}");
                    Console.WriteLine($"[DEBUG CREATE]   Amount: {component.Amount}");

                    try
                    {
                        var componentId = await _componentService.AddAsync(component);
                        Console.WriteLine($"[DEBUG CREATE] ✓ Component created successfully with ID: {componentId}");
                        componentCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG CREATE] ✗ FAILED to create component: {ex.Message}");
                        Console.WriteLine($"[DEBUG CREATE] ✗ Stack trace: {ex.StackTrace}");
                        throw;
                    }
                }

                Console.WriteLine($"[DEBUG CREATE] Total components created: {componentCount}");

                // Verify components were saved
                var verifyComponents = await _componentService.GetListAsync(x => x.CompPackageId == packageId);
                Console.WriteLine($"[DEBUG CREATE] VERIFICATION: Found {verifyComponents.Count()} components in database for package {packageId}");

                return SUCCESS(new { packageId, message = "Compensation package created successfully" });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to create compensation package: {ex.Message}");
            }
        }
    }
}


