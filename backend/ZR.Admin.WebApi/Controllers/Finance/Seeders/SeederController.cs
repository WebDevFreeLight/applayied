using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Service.IService.Finance.GL;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Admin.WebApi.Controllers.Finance.Seeders
{
    /// <summary>
    /// Seeder Controller - متحكم البذر
    /// Controller for running database seeders
    /// </summary>
    [Verify]
    [Route("erp/finance/seeders")]
    [ApiExplorerSettings(GroupName = "finance")]
    public class SeederController : BaseController
    {
        private readonly IGLAccountSeeder _glAccountSeeder;
        private readonly ITreasuryGLAccountMappingSeeder _treasuryMappingSeeder;

        public SeederController(
            IGLAccountSeeder glAccountSeeder,
            ITreasuryGLAccountMappingSeeder treasuryMappingSeeder)
        {
            _glAccountSeeder = glAccountSeeder;
            _treasuryMappingSeeder = treasuryMappingSeeder;
        }

        /// <summary>
        /// Seed default GL accounts (Chart of Accounts)
        /// زراعة الحسابات الافتراضية
        /// </summary>
        /// <returns></returns>
        [HttpPost("gl-accounts")]
        [Log(Title = "Seed GL Accounts", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SeedGLAccounts()
        {
            try
            {
                var username = HttpContext.GetName();
                var companyId = HttpContext.GetCompanyId();

                // Check if already seeded
                if (await _glAccountSeeder.DefaultAccountsExistAsync(companyId))
                {
                    return ToResponse(ResultCode.CUSTOM_ERROR, "GL accounts already seeded - الحسابات موجودة بالفعل");
                }

                var count = await _glAccountSeeder.SeedDefaultAccountsAsync(companyId, username);

                return SUCCESS(new
                {
                    accountsCreated = count,
                    message = $"Successfully seeded {count} GL accounts - تم زراعة {count} حساب بنجاح"
                });
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.GLOBAL_ERROR, $"Error seeding GL accounts: {ex.Message}");
            }
        }

        /// <summary>
        /// Link existing treasury accounts to GL accounts
        /// ربط حسابات الخزينة بحسابات دفتر الأستاذ
        /// </summary>
        /// <returns></returns>
        [HttpPost("link-treasury-accounts")]
        [Log(Title = "Link Treasury Accounts to GL", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> LinkTreasuryAccountsToGL()
        {
            try
            {
                var username = HttpContext.GetName();
                var companyId = HttpContext.GetCompanyId();

                var count = await _glAccountSeeder.LinkTreasuryAccountsToGLAsync(companyId, username);

                return SUCCESS(new
                {
                    accountsLinked = count,
                    message = $"Successfully linked {count} treasury accounts - تم ربط {count} حساب خزينة بنجاح"
                });
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.GLOBAL_ERROR, $"Error linking treasury accounts: {ex.Message}");
            }
        }

        /// <summary>
        /// Seed treasury GL account mappings
        /// زراعة تعيينات حسابات الخزينة
        /// </summary>
        /// <returns></returns>
        [HttpPost("treasury-mappings")]
        [Log(Title = "Seed Treasury GL Mappings", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SeedTreasuryMappings()
        {
            try
            {
                var username = HttpContext.GetName();
                var companyId = HttpContext.GetCompanyId();

                // Check if already seeded
                if (await _treasuryMappingSeeder.DefaultMappingsExistAsync(companyId))
                {
                    return ToResponse(ResultCode.CUSTOM_ERROR, "Treasury mappings already seeded - التعيينات موجودة بالفعل");
                }

                var count = await _treasuryMappingSeeder.SeedDefaultMappingsAsync(companyId, username);

                return SUCCESS(new
                {
                    mappingsCreated = count,
                    message = $"Successfully seeded {count} treasury mappings - تم زراعة {count} تعيين بنجاح"
                });
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.GLOBAL_ERROR, $"Error seeding treasury mappings: {ex.Message}");
            }
        }

        /// <summary>
        /// Update treasury mappings with GL account information
        /// تحديث التعيينات بمعلومات حسابات دفتر الأستاذ
        /// </summary>
        /// <returns></returns>
        [HttpPut("treasury-mappings/update-gl-info")]
        [Log(Title = "Update Treasury Mappings GL Info", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateTreasuryMappingsGLInfo()
        {
            try
            {
                var username = HttpContext.GetName();
                var companyId = HttpContext.GetCompanyId();

                var count = await _treasuryMappingSeeder.UpdateMappingsWithGLInfoAsync(companyId, username);

                return SUCCESS(new
                {
                    mappingsUpdated = count,
                    message = $"Successfully updated {count} treasury mappings - تم تحديث {count} تعيين بنجاح"
                });
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.GLOBAL_ERROR, $"Error updating treasury mappings: {ex.Message}");
            }
        }

        /// <summary>
        /// Run all seeders in sequence (complete setup)
        /// تشغيل جميع عمليات الزراعة بالتسلسل
        /// </summary>
        /// <returns></returns>
        [HttpPost("run-all")]
        [Log(Title = "Run All Seeders", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> RunAllSeeders()
        {
            try
            {
                var username = HttpContext.GetName();
                var companyId = HttpContext.GetCompanyId();

                int glAccountsSeeded = 0;
                int treasuryAccountsLinked = 0;
                int treasuryMappingsSeeded = 0;
                var errors = new List<string>();

                // 1. Seed GL Accounts
                try
                {
                    if (!await _glAccountSeeder.DefaultAccountsExistAsync(companyId))
                    {
                        glAccountsSeeded = await _glAccountSeeder.SeedDefaultAccountsAsync(companyId, username);
                    }
                    else
                    {
                        errors.Add("GL accounts already exist - الحسابات موجودة بالفعل");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"GL Accounts Seeding Error: {ex.Message}");
                }

                // 2. Link Treasury Accounts to GL
                try
                {
                    treasuryAccountsLinked = await _glAccountSeeder.LinkTreasuryAccountsToGLAsync(companyId, username);
                }
                catch (Exception ex)
                {
                    errors.Add($"Treasury Linking Error: {ex.Message}");
                }

                // 3. Seed Treasury GL Mappings
                try
                {
                    if (!await _treasuryMappingSeeder.DefaultMappingsExistAsync(companyId))
                    {
                        treasuryMappingsSeeded = await _treasuryMappingSeeder.SeedDefaultMappingsAsync(companyId, username);
                    }
                    else
                    {
                        errors.Add("Treasury mappings already exist - التعيينات موجودة بالفعل");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Treasury Mappings Seeding Error: {ex.Message}");
                }

                var successMessage = $"Seeding completed. GL Accounts: {glAccountsSeeded}, Treasury Links: {treasuryAccountsLinked}, Mappings: {treasuryMappingsSeeded}";

                if (errors.Count > 0)
                {
                    successMessage += $"\n\nWarnings: {string.Join(", ", errors)}";
                }

                return SUCCESS(new
                {
                    glAccountsSeeded = glAccountsSeeded,
                    treasuryAccountsLinked = treasuryAccountsLinked,
                    treasuryMappingsSeeded = treasuryMappingsSeeded,
                    errors = errors,
                    message = successMessage
                });
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.GLOBAL_ERROR, $"Error running seeders: {ex.Message}");
            }
        }

        /// <summary>
        /// Check seeder status
        /// التحقق من حالة البذر
        /// </summary>
        /// <returns></returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetSeederStatus()
        {
            try
            {
                var companyId = HttpContext.GetCompanyId();

                var status = new
                {
                    glAccountsSeeded = await _glAccountSeeder.DefaultAccountsExistAsync(companyId),
                    treasuryMappingsSeeded = await _treasuryMappingSeeder.DefaultMappingsExistAsync(companyId)
                };

                return SUCCESS(status);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.GLOBAL_ERROR, $"Error checking seeder status: {ex.Message}");
            }
        }
    }
}
